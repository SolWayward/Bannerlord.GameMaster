using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster
{
    /// <summary>
    /// Singleton manager for generating unique identifiers for and manager game objects created with BLGM
    /// </summary>
    public class BLGMObjectManager
    {
        // Note: Heroes are not registered in MBObjectManager, only their CharacterObject is
        //       Heroes are instead registered in TaleWorlds.CampaignSystem.Hero.AllAliveHeroes
        //       Attempting to register heroes in MBObjectManager is safe and will just silently fail

        #region Fields

        private static readonly Lazy<BLGMObjectManager> _instance = new(() => new());
        private ConcurrentDictionary<string, MBObjectBase> blgmObjects;
        private int nextId = 0;

        // Cache fields for frequently accessed filtered lists
        private MBList<Hero> _cachedHeroes;
        private MBList<Clan> _cachedClans;
        private MBList<Kingdom> _cachedKingdoms;

        // Separate validity flags for each cache type to prevent cross-type cache corruption
        private bool _heroesValid;
        private bool _clansValid;
        private bool _kingdomsValid;

        // Lock for thread-safe cache building
        private readonly object _cacheLock = new object();

        // Object count fields - maintained on register/unregister for O(1) access
        private int _heroCount = 0;
        private int _clanCount = 0;
        private int _kingdomCount = 0;

        #endregion

        #region Properties

        public static BLGMObjectManager Instance => _instance.Value;

        public static MBList<Hero> BlgmHeroes => Instance.GetObjects<Hero>();
        public static MBList<Clan> BlgmClans => Instance.GetObjects<Clan>();
        public static MBList<Kingdom> BlgmKingdoms => Instance.GetObjects<Kingdom>();

        public int ObjectCount => blgmObjects?.Count ?? 0;

        public static int BlgmHeroCount => Instance._heroCount;
        public static int BlgmClanCount => Instance._clanCount;
        public static int BlgmKingdomCount => Instance._kingdomCount;

        #endregion

        #region Constructor

        private BLGMObjectManager()
        {
            // Initalize is called from BLGMObjectManagerBehaviour
        }

        #endregion

        #region Initialization

        internal void Initialize()
        {
            blgmObjects = new();
            nextId = 0;

            InvalidateListCaches();
            _heroCount = 0;
            _clanCount = 0;
            _kingdomCount = 0;

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

        #endregion

        #region Query Methods

        /// <summary>
        /// Get all BLGM objects of a specific type with caching for better performance, Only type of Heroes, Clans, and Kingdoms are cached, other types will be non cached access.
        /// </summary>
        public MBList<T> GetObjects<T>() where T : MBObjectBase
        {
            if (blgmObjects == null)
                return new MBList<T>();

            // Try to get from cache without locking
            if (TryGetFromCache<T>(out MBList<T> cachedList))
                return cachedList;

            // Lock and try again
            lock (_cacheLock)
            {
                // Another thread might have built the cache while we waited for the lock
                if (TryGetFromCache<T>(out cachedList))
                    return cachedList;

                // Cache Miss We need to build the list
                MBList<T> typedObjects = new();

                // Iterate dictionary values
                foreach (MBObjectBase obj in blgmObjects.Values)
                {
                    if (obj is T typedObj)
                        typedObjects.Add(typedObj);
                }

                // Update the cache for next time
                UpdateCache(typedObjects);

                return typedObjects;
            }
        }

        /// <summary>
        /// Retrieve all registered BLGM created object stringIds
        /// </summary>
        public string[] GetObjectIds()
        {
            if (blgmObjects == null)
                return Array.Empty<string>();

            string[] result = new string[blgmObjects.Count];
            blgmObjects.Keys.CopyTo(result, 0);
            return result;
        }

        /// <summary>
        /// Try to retrieve an object by its stringID with type safety
        /// </summary>
        public bool TryGetObject<T>(string stringId, out T mbObject) where T : MBObjectBase
        {
            if (blgmObjects == null || !blgmObjects.TryGetValue(stringId, out MBObjectBase baseObject))
            {
                mbObject = null;
                return false;
            }

            mbObject = baseObject as T;
            return mbObject != null;  // Returns false if object exists but is wrong type
        }

        #endregion

        #region Registration Methods

        /// <summary>
        /// Assign an object a unique stringID prefixed with "blgm_type_name_xxx" and register it in BLGMObjectManager and register with game as well<br/>
        /// stringId will be overwritten on object if already assigned. Assign name before calling method to include name in stringId, otherwise name will be randomly assigned
        /// </summary>
        /// <returns>stringId of registered Object</returns>
        public static string RegisterHero(Hero hero)
        {
            if (hero == null)
                return null;

            // Name should be pre assigned but just incase.
            if (string.IsNullOrWhiteSpace(hero.Name.ToString()))
            {
                string randomName = CultureLookup.GetUniqueRandomHeroName(hero.Culture, hero.IsFemale);
                hero.SetStringName(randomName);
            }

            string stringId = Instance.RegisterObject(hero);

            // Always unregister CharacterObject first (safe even if not registered)
            // Must unregister with OLD StringId still on object
            MBObjectManager.Instance.UnregisterObject(hero.CharacterObject);

            // Update CharacterObject StringId to match Hero
            hero.CharacterObject.StringId = stringId;

            // Now register with new StringId
            MBObjectManager.Instance.RegisterObject(hero.CharacterObject);

            Instance.InvalidateListCaches();
            Interlocked.Increment(ref Instance._heroCount);

            if (!Campaign.Current.CampaignObjectManager.AliveHeroes.Contains(hero))
                Campaign.Current.CampaignObjectManager.AliveHeroes.Add(hero);

            return stringId;
        }

        /// <summary>
        /// Assign an object a unique stringID prefixed with "blgm_type_name_xxx" and register it in BLGMObjectManager and register with game as well<br/>
        /// stringId will be overwritten on object if already assigned. Assign name before calling method to include name in stringId, otherwise name will be randomly assigned
        /// </summary>
        /// <returns>stringId of registered Object</returns>
        public static string RegisterClan(Clan clan)
        {
            if (clan == null)
                return null;

            // Name should be pre assigned but just incase.
            if (string.IsNullOrWhiteSpace(clan.Name.ToString()))
            {
                string randomName = CultureLookup.GetUniqueRandomClanName(clan.Culture);
                clan.SetStringName(randomName);
            }

            string stringId = Instance.RegisterObject(clan);
            Instance.InvalidateListCaches();
            Interlocked.Increment(ref Instance._clanCount);

            if (!Campaign.Current.CampaignObjectManager.Clans.Contains(clan))
                Campaign.Current.CampaignObjectManager.Clans.Add(clan);

            return stringId;
        }

        /// <summary>
        /// Assign an object a unique stringID prefixed with "blgm_type_name_xxx" and register it in BLGMObjectManager and register with game as well<br/>
        /// stringId will be overwritten on object if already assigned. Assign name before calling method to include name in stringId, otherwise name will be randomly assigned
        /// </summary>
        /// <returns>stringId of registered Object</returns>
        public static string RegisterKingdom(Kingdom kingdom)
        {
            if (kingdom == null)
                return null;

            // Name should be pre assigned but just incase.
            if (string.IsNullOrWhiteSpace(kingdom.Name.ToString()))
            {
                string randomName = CultureLookup.GetUniqueRandomKingdomName(kingdom.Culture);
                kingdom.ChangeKingdomName(new TextObject(randomName), new TextObject(randomName));
            }

            string stringId = Instance.RegisterObject(kingdom);
            Instance.InvalidateListCaches();
            Interlocked.Increment(ref Instance._kingdomCount);

            if (!Campaign.Current.CampaignObjectManager.Kingdoms.Contains(kingdom))
                Campaign.Current.CampaignObjectManager.Kingdoms.Add(kingdom);

            return stringId;
        }

        #endregion

        #region Unregistration Methods

        /// <summary>
        /// Unregister a hero from BLGMObjectManager
        /// </summary>
        public static bool UnregisterHero(string stringId)
        {
            if (!Instance.blgmObjects.TryRemove(stringId, out _))
                return false;

            Instance.InvalidateListCaches();
            Interlocked.Decrement(ref Instance._heroCount);
            return true;
        }

        /// <summary>
        /// Unregister a clan from BLGMObjectManager
        /// </summary>
        public static bool UnregisterClan(string stringId)
        {
            if (!Instance.blgmObjects.TryRemove(stringId, out _))
                return false;

            Instance.InvalidateListCaches();
            Interlocked.Decrement(ref Instance._clanCount);
            return true;
        }

        /// <summary>
        /// Unregister a kingdom from BLGMObjectManager
        /// </summary>
        public static bool UnregisterKingdom(string stringId)
        {
            if (!Instance.blgmObjects.TryRemove(stringId, out _))
                return false;

            Instance.InvalidateListCaches();
            Interlocked.Decrement(ref Instance._kingdomCount);
            return true;
        }

        #endregion

        #region Private Methods

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
            if (nameObj != null && !string.IsNullOrWhiteSpace(nameObj.ToString()))
            {
                prefix = $"{prefix}_{CleanString(nameObj.ToString())}";
            }

            string stringID = $"blgm_{prefix}_{Interlocked.Increment(ref nextId)}";

            // Change stringId and register
            mbObject.StringId = stringID;
            blgmObjects[mbObject.StringId] = mbObject;

            // This will silently fail or do nothing for Heroes because MBObjectManager doesn't have a type registration for Hero. 
            // It only has registrations for CharacterObject, ItemObject, CultureObject, etc.            
            MBObjectManager.Instance.RegisterObject(mbObject);

            return stringID;
        }

        /// <summary>
        /// Load objects that were created with BLGM into the dictionary when loading a save game and convert any legacy objects to new format
        /// </summary>
        private void LoadObjects()
        {
            if (Campaign.Current == null)
                return;

            List<MBObjectBase> legacyObjects = new();

            // Find objects with stringIds starting with blgm_ in each of the below lists and add to BLGMObjectManager dictionary. 
            // Also Store any found legacy objects in legacyObjects in legacyObjects so they can be converted to new stringID format and then loaded
            LoadObjectsOfType(Campaign.Current.CampaignObjectManager.AliveHeroes, legacyObjects);
            LoadObjectsOfType(Campaign.Current.CampaignObjectManager.Clans, legacyObjects);
            LoadObjectsOfType(Campaign.Current.CampaignObjectManager.Kingdoms, legacyObjects);

            // Convert old legacy objects with new sequential unique int
            if (!legacyObjects.IsEmpty())
                ConvertLegacyObjectsAndRegister(legacyObjects);
        }

        /// <summary>
        /// Process a specific object type list for BLGM-created objects <br />
        /// Dont call directly, called from LoadObjects()
        /// </summary>
        /// <param name="objectList">MBList to check for objects with blgm_ stringIds</param>
        /// <param name="legacyObjects">List to store any legacy objects found in objectList (So it can be handled or converted later)</param>
        private void LoadObjectsOfType<T>(MBReadOnlyList<T> objectList, List<MBObjectBase> legacyObjects) where T : MBObjectBase
        {
            if (objectList == null)
                return;

            foreach (T obj in objectList)
            {
                // Filter for BLGM objects only
                if (obj == null || obj.StringId == null || !obj.StringId.StartsWith("blgm_"))
                    continue;

                // Compare with nextID to ensure nextID will be unique when new objects are registered
                string idSuffix = obj.StringId.Substring(obj.StringId.LastIndexOf('_') + 1);

                // Try to parse as int first (new format)
                if (int.TryParse(idSuffix, out int parsedId))
                {
                    // It's an integer - compare with nextId
                    if (parsedId >= nextId)
                    {
                        nextId = parsedId + 1;
                    }

                    // Add to blgmObjects normally and increment appropriate count
                    blgmObjects[obj.StringId] = obj;
                    if (obj is Hero)
                        _heroCount++;
                    else if (obj is Clan)
                        _clanCount++;
                    else if (obj is Kingdom)
                        _kingdomCount++;

                    // CRITICAL FIX: Check if Hero's CharacterObject StringId needs updating
                    if (obj is Hero hero)
                    {
                        if (hero.CharacterObject.StringId != hero.StringId)
                        {
                            // CharacterObject has mismatched StringId - add to legacy for reprocessing
                            legacyObjects.Add(obj);
                            // Don't increment count yet - will be handled during conversion
                            _heroCount--;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Replace legacy suffixes with new sequential integers and also fix any stringId formatting issues
        /// </summary>
        private void ConvertLegacyObjectsAndRegister(List<MBObjectBase> legacyObjects)
        {
            InfoMessage.Status($"[GameMaster] Converting {legacyObjects.Count} Legacy Objects");
            foreach (MBObjectBase legacyObject in legacyObjects)
            {
                RegisterLegacyObject(legacyObject);
            }
        }

        /// <summary>
        /// Register a legacy object with runtime type detection
        /// </summary>
        private string RegisterLegacyObject(MBObjectBase mbObject)
        {
            if (mbObject == null)
                return null;

            try
            {
                // Call the appropriate public registration method based on runtime type
                Type runtimeType = mbObject.GetType();

                if (runtimeType == typeof(Hero))
                    return RegisterHero((Hero)mbObject);
                else if (runtimeType == typeof(Clan))
                    return RegisterClan((Clan)mbObject);
                else if (runtimeType == typeof(Kingdom))
                    return RegisterKingdom((Kingdom)mbObject);
                else
                {
                    InfoMessage.Warning($"BLGMObjectManager: Unsupported type {runtimeType.Name} for legacy object. Object not registered.\n" +
                                        "Game and object will function as normal");
                    return null;
                }
            }
            catch (Exception ex)
            {
                InfoMessage.Error($"BLGMObjectManager: Failed to register legacy object: {ex.Message}\nObject may not be tracked by blgm, but game functionality is unaffected.");
                return null;
            }
        }

        /// <summary>
        /// Invalidate the cached filtered lists so they will be rebuilt on next access
        /// </summary>
        private void InvalidateListCaches()
        {
            lock (_cacheLock)
            {
                _heroesValid = false;
                _clansValid = false;
                _kingdomsValid = false;
                _cachedHeroes = null;
                _cachedClans = null;
                _cachedKingdoms = null;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Replaces Spaces with underscores and converts all characters to lowercase, Performant for processing hundreds of objects at once
        /// </summary>
        private string CleanString(string stringToClean)
        {
            if (string.IsNullOrWhiteSpace(stringToClean))
                return string.Empty;

            // Trim first to avoid processing leading/trailing whitespace
            ReadOnlySpan<char> trimmed = stringToClean.AsSpan().Trim();

            if (trimmed.IsEmpty)
                return string.Empty;

            StringBuilder sb = new(trimmed.Length);

            foreach (char c in trimmed)
            {
                // char.IsWhiteSpace covers spaces (' '), tabs ('\t'), newlines, etc.
                if (char.IsWhiteSpace(c))
                {
                    // Only add underscore if the builder isn't empty and the last char wasn't already an underscore
                    if (sb.Length > 0 && sb[sb.Length - 1] != '_')
                    {
                        sb.Append('_');
                    }
                }
                
                else
                {
                    sb.Append(char.ToLowerInvariant(c));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Helper to safely retrieve the correct cache list based on type T.
        /// Returns true if a valid cache exists.
        /// </summary>
        private bool TryGetFromCache<T>(out MBList<T> list) where T : MBObjectBase
        {
            list = null;

            // Check Hero Cache
            if (typeof(T) == typeof(Hero) && _heroesValid)
            {
                // Double cast is required because C# generics are invariant
                list = (MBList<T>)(object)_cachedHeroes;
                return true;
            }

            // Check Clan Cache
            if (typeof(T) == typeof(Clan) && _clansValid)
            {
                list = (MBList<T>)(object)_cachedClans;
                return true;
            }

            // Check Kingdom Cache
            if (typeof(T) == typeof(Kingdom) && _kingdomsValid)
            {
                list = (MBList<T>)(object)_cachedKingdoms;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Helper to update the correct cache list and set the valid flag.
        /// </summary>
        private void UpdateCache<T>(MBList<T> list) where T : MBObjectBase
        {
            if (typeof(T) == typeof(Hero))
            {
                _cachedHeroes = (MBList<Hero>)(object)list;
                _heroesValid = true;
            }
            
            else if (typeof(T) == typeof(Clan))
            {
                _cachedClans = (MBList<Clan>)(object)list;
                _clansValid = true;
            }
            
            else if (typeof(T) == typeof(Kingdom))
            {
                _cachedKingdoms = (MBList<Kingdom>)(object)list;
                _kingdomsValid = true;
            }
        }

        #endregion
    }
}