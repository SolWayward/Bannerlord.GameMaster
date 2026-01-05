using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bannerlord.GameMaster.Information;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster
{
    /// <summary>
    /// Singleton manager for generating unique identifiers for game objects
    /// </summary>
    public class BLGMObjectManager
    {
        private static readonly Lazy<BLGMObjectManager> _instance = new(() => new());
        private ConcurrentDictionary<string, MBObjectBase> blgmObjects;
        private int nextId = 0;

        public static BLGMObjectManager Instance => _instance.Value;

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
                InfoMessage.Warning($"Failed to register saved BLGM objects: {ex.Message}\n " +
                    "Everything ingame Will still function correctly, just the BLGMObjectManager wont have a reference to preexisting objects created with blgm");
            }
        }

        /// <summary>
        /// Retrieve all registered object stringIds
        /// </summary>
        public string[] GetObjectIds()
        {
            return blgmObjects.Keys.ToArray();
        }

        /// <summary>
        /// Retrieve an object by its stringID
        /// </summary>
        public MBObjectBase GetObject(string stringId)
        {
            if (blgmObjects.TryGetValue(stringId, out MBObjectBase mbObject))
            {
                return mbObject;
            }

            return null;
        }

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
        /// Assign an object a unique stringID prefixed with "blgm_type_name_xxx" and register it in BLGMObjectManager
        /// </summary>
        /// <returns>stringId of registered Object</returns>
        public string RegisterObject(MBObjectBase mbObject)
        {   
            if (mbObject == null)
                return null;

            // Remove old entry if object was previously registered
            if (!string.IsNullOrEmpty(mbObject.StringId) && 
                mbObject.StringId.StartsWith("blgm_"))
            {
                blgmObjects.TryRemove(mbObject.StringId, out _);
            }

            //Add Inherrited type
            string prefix = CleanString(mbObject.GetType().Name);

            //Add Object Name if it has one
            if(mbObject.GetName() != null)
            {
                string name = mbObject.GetName().ToString();
                if (!string.IsNullOrEmpty(name))
                    prefix = $"{prefix}_{CleanString(name)}";

            }

            // Add blgm prefix and sequential int suffix
            string stringID = $"blgm_{prefix}_{Interlocked.Increment(ref nextId)}"; // Thread safe atomic increment

            // Register
            mbObject.StringId = stringID;
            blgmObjects[mbObject.StringId] = mbObject;

            return stringID;
        }

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
                RegisterObject(legacyObject);
            }
        }
    }
}