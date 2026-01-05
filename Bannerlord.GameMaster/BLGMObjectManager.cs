using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster
{
    /// <summary>
    /// Singleton manager for generating unique identifiers for game objects
    /// </summary>
    public class BLGMObjectManager
    {
        private static readonly Lazy<BLGMObjectManager> _instance = new(() => new());
        private readonly ConcurrentDictionary<string, MBObjectBase> blgmObjects;
        private int nextId = 0;

        public static BLGMObjectManager Instance => _instance.Value;

        public int ObjectCount => blgmObjects.Count;

        // Private constructor to prevent external instantiation
        // BLGM objects for a loaded save are reregisterd here as well
        private BLGMObjectManager()
        {
            blgmObjects = new();
            try
            {
                LoadObjects();
            }
            catch (Exception ex)
            {
                InfoMessage.Warning($"Failed to load saved BLGM objects during initialization: {ex.Message}\n" +
                        "BLGM objects from previous saves may not be tracked, but game functionality is unaffected.");
            }
        }

        /// <summary>
        /// Retrieve all registered BLGM created object stringIds
        /// </summary>
        public string[] GetObjectIds()
        {
            return blgmObjects.Keys.ToArray();
        }

        /// <summary>
        /// Try to retrieve an object by its stringID with type safety
        /// </summary>
        public bool TryGetObject<T>(string stringId, out T mbObject) where T : MBObjectBase
        {
            if (blgmObjects.TryGetValue(stringId, out MBObjectBase baseObject))
            {
                mbObject = baseObject as T;
                return mbObject != null;  // Returns false if object exists but is wrong type
            }
            mbObject = null;
            return false;
        }

        /// <summary>
        /// Load objects that were created with BLGM into the dictionary when loading a save game
        /// </summary>
        private void LoadObjects()
        {
            List<MBObjectBase> legacyObjects = new();

            foreach (MBObjectBase obj in MBObjectManager.Instance.GetObjects<MBObjectBase>(
                obj => obj.StringId != null && obj.StringId.StartsWith("blgm_")))
            {
                if (obj == null)
                    continue;

                // Compare with nextID to ensure nextID will be unique when new objects are registered
                string idSuffix = obj.StringId.Substring(obj.StringId.LastIndexOf('_') + 1);

                // Try to parse as int first
                if (int.TryParse(idSuffix, out int parsedId))
                {
                    // It's an integer - compare with nextId
                    if (parsedId >= nextId)
                    {
                        nextId = parsedId + 1;
                    }

                    // Add to blgmObjects normally
                    blgmObjects[obj.StringId] = obj;
                }

                else
                {
                    // It's a GUID without dashes - save to temporary list for later processing
                    legacyObjects.Add(obj);
                }
            }

            // Convert old legacy objects with new sequential unique int
            ConvertLegacyObjectsAndRegister(legacyObjects);
        }

        /// <summary>
        /// Assign an object a unique stringID prefixed with "blgm_type_name_xxx" and register it in BLGMObjectManager and register with game as well<br/>
        /// stringId will be overwritten on object if already assigned. Assign name before calling method to include name in stringId<br/>
        /// </summary>
        /// <returns>stringId of registered Object</returns>
        public string RegisterHero(Hero hero)
        {
            string stringId = RegisterObject(hero);
            
            if (!Campaign.Current.CampaignObjectManager.AliveHeroes.Contains(hero))
                Campaign.Current.CampaignObjectManager.AliveHeroes.Add(hero);
            
            return stringId;
        }

        /// <summary>
        /// Assign an object a unique stringID prefixed with "blgm_type_name_xxx" and register it in BLGMObjectManager and register with game as well<br/>
        /// stringId will be overwritten on object if already assigned. Assign name before calling method to include name in stringId<br/>
        /// </summary>
        /// <returns>stringId of registered Object</returns>
        public string RegisterClan(Clan clan)
        {
            string stringId = RegisterObject(clan);
            
            if (!Campaign.Current.CampaignObjectManager.Clans.Contains(clan))
                Campaign.Current.CampaignObjectManager.Clans.Add(clan);
            
            return stringId;
        }

        /// <summary>
        /// Assign an object a unique stringID prefixed with "blgm_type_name_xxx" and register it in BLGMObjectManager and register with game as well<br/>
        /// stringId will be overwritten on object if already assigned. Assign name before calling method to include name in stringId<br/>
        /// </summary>
        /// <returns>stringId of registered Object</returns>
        public string RegisterKingdom(Kingdom kingdom)
        {
            string stringId = RegisterObject(kingdom);
            
            if (!Campaign.Current.CampaignObjectManager.Kingdoms.Contains(kingdom))
                Campaign.Current.CampaignObjectManager.Kingdoms.Add(kingdom);
            
            return stringId;
        }

        /// <summary>
        /// Assign an object a unique stringID prefixed with "blgm_type_name_xxx" and register it in BLGMObjectManager and register with game as well<br/>
        /// stringId will be overwritten on object if already assigned. Assign name before calling method to include name in stringId<br/>
        /// Dont call directly, Call the specific register methods for the correct types which will call this, so objects get properly added to the games type collection as well
        /// </summary>
        /// <returns>stringId of registered Object</returns>
        private string RegisterObject<T>(T mbObject) where T : MBObjectBase
        {
            if (mbObject == null)
                return null;

            // Remove old entry if object was previously registered
            if (!string.IsNullOrEmpty(mbObject.StringId))
            {
                if (mbObject.StringId.StartsWith("blgm_"))
                {
                    blgmObjects.TryRemove(mbObject.StringId, out _);
                }

                // Unregister as we changing the stringId (MBObjectManager will skip if object isnt registered)
                MBObjectManager.Instance.UnregisterObject(mbObject);
            }

            string prefix = CleanString(typeof(T).Name);

            TextObject nameObj = mbObject.GetName();
            if (nameObj != null && !string.IsNullOrEmpty(nameObj.ToString()))
            {
                prefix = $"{prefix}_{CleanString(nameObj.ToString())}";
            }

            string stringID = $"blgm_{prefix}_{Interlocked.Increment(ref nextId)}";
            
            // Change stringId and register
            mbObject.StringId = stringID;
            blgmObjects[mbObject.StringId] = mbObject;
            MBObjectManager.Instance.RegisterObject(mbObject);

            return stringID;
        }

        /// <summary>
        /// Unregister an object from BLGMObjectManager (does not unregister from MBObjectManager)
        /// </summary>
        public bool UnregisterObject(string stringId)
        {
            if (string.IsNullOrEmpty(stringId))
                return false;

            return blgmObjects.TryRemove(stringId, out _);
        }

        /// <summary>
        /// Replaces Spaces with underscores and converts all characters to lowercase
        /// </summary>
        private string CleanString(string stringToClean)
        {
            if (string.IsNullOrWhiteSpace(stringToClean))
                return string.Empty;

            return stringToClean.Trim().Replace(' ', '_').ToLower();
        }

        /// <summary>
        /// Replace legacy suffixes with new sequential integers and also fix any stringId formatting issues
        /// </summary>
        private void ConvertLegacyObjectsAndRegister(List<MBObjectBase> legacyObjects)
        {
            foreach (MBObjectBase legacyObject in legacyObjects)
            {
                RegisterLegacyObject(legacyObject);
            }
        }

        /// <summary>
        /// Assign an object a unique stringID prefixed with "blgm_type_name_xxx" and register it in BLGMObjectManager <br/>
        /// Non-generic version - for runtime type detection (Used for loading and converting LEGACY objects only)
        /// </summary>
        /// <summary>
        /// Register a legacy object with runtime type detection
        /// </summary>
        private string RegisterLegacyObject(MBObjectBase mbObject)
        {
            if (mbObject == null)
                return null;

            try
            {
                Type runtimeType = mbObject.GetType();
                MethodInfo method = typeof(BLGMObjectManager)
                    .GetMethod(nameof(RegisterObject), new[] { runtimeType });

                if (method == null)
                {
                    InfoMessage.Warning($"BLGMObjectManager: Could not find RegisterObject method for type {runtimeType.Name}. Object not registered.\n" +
                                        "Game and object will function as normal");
                    return null;
                }

                MethodInfo genericMethod = method.MakeGenericMethod(runtimeType);
                return (string)genericMethod.Invoke(this, new object[] { mbObject });
            }
            catch (Exception ex)
            {
                InfoMessage.Error($"BLGMObjectManager: Failed to register legacy object: {ex.Message}\nGame and object will function as normal");
                return null;
            }
        }
    }
}