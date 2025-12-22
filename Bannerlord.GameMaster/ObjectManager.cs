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
        private List<string> objectStringIds;

        public static ObjectManager Instance => _instance.Value;

        // Private constructor to prevent external instantiation
        private ObjectManager()
        {
            objectStringIds = new();
        }

        /// <summary>
        /// Makes objects created with blgm easily identifiable by adding blgm_ prefix and adding to object list
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string RegisterObject(string input)
        {
            input = $"blgm_{input}";
            objectStringIds.Add(input);

            return input;
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

        public string[] GetObjectIds()
        {
            return objectStringIds.ToArray();
        }

        /// <summary>
        /// Return a unique string to use as a stringID for objects
        /// </summary>
        public string GetUniqueStringId()
        {
            string stringID = $"object_{CreateUniqueIdentifier()}";

            return RegisterObject(stringID);
        }

        /// <summary>
        /// Return a unique string to use as a stringID for objects prefixed with the objects type
        /// </summary>
        public string GetUniqueStringId(Type type)
        {
            string typeName = CleanString(type.Name);
            string stringID = $"{typeName}_{CreateUniqueIdentifier()}";

            return RegisterObject(stringID);
        }

        /// <summary>
        /// Return a unique string containing the provided name to use as a stringID for objects
        /// </summary>
        public string GetUniqueStringId(TextObject nameObj)
        {
            string name = CleanString(nameObj.ToString());
            string stringID = $"object_{name}_{CreateUniqueIdentifier()}";

            return RegisterObject(stringID);
        }

        /// <summary>
        /// Return a unique string containing the provided name and prefixed with the objects type to use as a stringID for objects
        /// </summary>
        public string GetUniqueStringId(TextObject nameObj, Type type)
        {
            string typeName = CleanString(type.Name);
            string name = CleanString(nameObj.ToString());

            string stringID = $"{typeName}_{name}_{CreateUniqueIdentifier()}";


            return RegisterObject(stringID);
        }
    }
}