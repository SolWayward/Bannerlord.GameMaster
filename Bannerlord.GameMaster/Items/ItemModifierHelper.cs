using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Items
{
    /// <summary>
    /// Helper methods for working with item modifiers (quality levels)
    /// </summary>
    public static class ItemModifierHelper
    {
        /// <summary>
        /// Get all available item modifiers from the game
        /// </summary>
        public static List<ItemModifier> GetAllModifiers()
        {
            var modifiers = new List<ItemModifier>();
            
            // Get all modifiers from the game's MBObjectManager
            foreach (var modifier in MBObjectManager.Instance.GetObjectTypeList<ItemModifier>())
            {
                modifiers.Add(modifier);
            }
            
            return modifiers;
        }

        /// <summary>
        /// Find a modifier by name (case-insensitive partial match)
        /// </summary>
        public static ItemModifier GetModifierByName(string modifierName)
        {
            if (string.IsNullOrEmpty(modifierName))
                return null;

            string lowerName = modifierName.ToLower();
            var allModifiers = MBObjectManager.Instance.GetObjectTypeList<ItemModifier>();
            
            // Try exact match first
            var exactMatch = allModifiers
                .FirstOrDefault(m => m.Name.ToString().Equals(modifierName, StringComparison.OrdinalIgnoreCase));
            
            if (exactMatch != null)
                return exactMatch;
            
            // Try contains match
            var containsMatch = allModifiers
                .FirstOrDefault(m => m.Name.ToString().ToLower().Contains(lowerName));
            
            return containsMatch;
        }

        /// <summary>
        /// Find a modifier by StringId (exact match)
        /// </summary>
        public static ItemModifier GetModifierByStringId(string stringId)
        {
            if (string.IsNullOrEmpty(stringId))
                return null;

            MBReadOnlyList<ItemModifier> allModifiers = MBObjectManager.Instance.GetObjectTypeList<ItemModifier>();
            return allModifiers.FirstOrDefault(m => m.StringId == stringId);
        }

        /// <summary>
        /// Get a list of modifier names for display
        /// </summary>
        public static string GetFormattedModifierList()
        {
            var modifiers = GetAllModifiers()
                .OrderBy(m => m.Name.ToString())
                .ToList();
            
            if (modifiers.Count == 0)
                return "No modifiers available.\n";
            
            return string.Join("\n", modifiers.Select(m => 
                $"{m.StringId}\t{m.Name}\tPrice Factor: {m.PriceMultiplier:F2}")) + "\n";
        }

        /// <summary>
        /// Check if an item can have modifiers (weapons and armor typically can)
        /// </summary>
        public static bool CanHaveModifier(ItemObject item)
        {
            if (item == null)
                return false;

            // Items with weapon or armor components can typically have modifiers
            return item.WeaponComponent != null || item.ArmorComponent != null;
        }

        /// <summary>
        /// Get modifier info as formatted string
        /// </summary>
        public static string GetModifierInfo(ItemModifier modifier)
        {
            if (modifier == null)
                return "No modifier";

            return $"{modifier.Name} (ID: {modifier.StringId}, Price: x{modifier.PriceMultiplier:F2})";
        }

        /// <summary>
        /// Parse modifier name with suggestions for close matches
        /// </summary>
        public static (ItemModifier modifier, string error) ParseModifier(string modifierName)
        {
            if (string.IsNullOrEmpty(modifierName))
                return (null, null); // No modifier is valid

            var modifier = GetModifierByName(modifierName);
            
            if (modifier != null)
                return (modifier, null);

            // Provide suggestions
            var allModifiers = GetAllModifiers();
            var suggestions = allModifiers
                .Where(m => m.Name.ToString().ToLower().StartsWith(modifierName.ToLower()))
                .Take(5)
                .ToList();

            if (suggestions.Count > 0)
            {
                string suggestionList = string.Join(", ", suggestions.Select(m => m.Name.ToString()));
                return (null, $"Modifier '{modifierName}' not found. Did you mean: {suggestionList}?");
            }

            return (null, $"Modifier '{modifierName}' not found. Use gm.query.modifiers to see all available modifiers.");
        }

        /// <summary>
        /// Common quality modifiers (for convenience/quick reference)
        /// </summary>
        public static class CommonModifiers
        {
            public static ItemModifier Fine => GetModifierByName("Fine");
            public static ItemModifier Masterwork => GetModifierByName("Masterwork");
            public static ItemModifier Legendary => GetModifierByName("Legendary");
            public static ItemModifier Bent => GetModifierByName("Bent");
            public static ItemModifier Chipped => GetModifierByName("Chipped");
            public static ItemModifier Rusty => GetModifierByName("Rusty");
            public static ItemModifier Cracked => GetModifierByName("Cracked");
            public static ItemModifier Balanced => GetModifierByName("Balanced");
            public static ItemModifier Sharp => GetModifierByName("Sharp");
            public static ItemModifier Heavy => GetModifierByName("Heavy");
        }
    }
}