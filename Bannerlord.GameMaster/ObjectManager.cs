using System;
using System.Collections.Generic;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster
{
    /// <summary>
    /// Singleton manager for generating unique identifiers for game objects
    /// </summary>
    public class ObjectManager
    {
        private static readonly Lazy<ObjectManager> _instance = new Lazy<ObjectManager>(() => new ObjectManager());

        public static ObjectManager Instance => _instance.Value;
        public List<string> ObjectStringIds  { get; private set; }

        // Private constructor to prevent external instantiation
        private ObjectManager()
        {
            ObjectStringIds = new();
        }

        private string CreateUniqueIdentifier()
        {
            string guid = Guid.NewGuid().ToString();
            return guid.ToLower().Replace('-', '_');
        }

        private string CleanString(string stringToClean)
        {
            if (stringToClean != null && !stringToClean.IsEmpty())
                stringToClean.Trim().Replace(' ', '_');
            
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
            string stringID = $"{CleanString(type.Name)}_{CreateUniqueIdentifier()}";

            ObjectStringIds.Add(stringID);
            return stringID;
        }

        /// <summary>
        /// Return a unique string containing the provided name to use as a stringID for objects
        /// </summary>
        public string GetUniqueStringId(string name)
        {
            string stringID = $"object_{CleanString(name)}_{CreateUniqueIdentifier()}";

            ObjectStringIds.Add(stringID);
            return stringID;
        }

        /// <summary>
        /// Return a unique string containing the provided name and prefixed with the objects type to use as a stringID for objects
        /// </summary>
        public string GetUniqueStringId(string name, Type type)
        {
            string stringID = $"{CleanString(type.Name)}_{CleanString(name)}_{CreateUniqueIdentifier()}";

            ObjectStringIds.Add(stringID);
            return stringID;
        }
    }
}