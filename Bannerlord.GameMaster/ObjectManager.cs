using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Bannerlord.GameMaster
{
    /// <summary>
    /// Singleton manager for generating unique identifiers for game objects
    /// </summary>
    public class ObjectManager
    {
        private static readonly Lazy<ObjectManager> _instance = new(() => new());

        public static ObjectManager Instance => _instance.Value;
        public List<string> ObjectStringIds  { get; private set; }

        // Private constructor to prevent external instantiation
        private ObjectManager()
        {
            ObjectStringIds = new();
        }

        private string CreateUniqueIdentifier()
        {
            return Guid.NewGuid().ToString("N");
        }

        private string CleanString(string stringToClean)
        {
            if (stringToClean != null && !stringToClean.IsEmpty())
                stringToClean = stringToClean.Trim().Replace(' ', '_');
            
            return stringToClean.ToLower();
        }

        /// <summary>
        /// Return a unique string to use as a stringID for objects
        /// </summary>
        public string GetUniqueStringId()
        {
            string stringID = $"object_{CreateUniqueIdentifier()}";

            ObjectStringIds.Add(stringID);
            return stringID;
        }

        /// <summary>
        /// Return a unique string to use as a stringID for objects prefixed with the objects type
        /// </summary>
        public string GetUniqueStringId(Type type)
        {
            string typeName = CleanString(type.Name);
            string stringID = $"{typeName}_{CreateUniqueIdentifier()}";

            ObjectStringIds.Add(stringID);
            return stringID;
        }

        /// <summary>
        /// Return a unique string containing the provided name to use as a stringID for objects
        /// </summary>
        public string GetUniqueStringId(TextObject nameObj)
        {
            string name = CleanString(nameObj.ToString());
            string stringID = $"object_{name}_{CreateUniqueIdentifier()}";

            ObjectStringIds.Add(stringID);
            return stringID;
        }

        /// <summary>
        /// Return a unique string containing the provided name and prefixed with the objects type to use as a stringID for objects
        /// </summary>
        public string GetUniqueStringId(TextObject nameObj, Type type)
        {
            string typeName = CleanString(type.Name);
            string name = CleanString(nameObj.ToString());

            string stringID = $"{typeName}_{name}_{CreateUniqueIdentifier()}";

            ObjectStringIds.Add(stringID);
            return stringID;
        }
    }
}