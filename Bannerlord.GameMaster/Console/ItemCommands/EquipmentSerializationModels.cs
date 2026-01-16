using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord.GameMaster.Console.ItemCommands
{
    /// <summary>
    /// Data class for equipment set serialization to JSON
    /// </summary>
    public class EquipmentSetData
    {
        [JsonProperty("HeroName")]
        public string HeroName { get; set; }

        [JsonProperty("HeroId")]
        public string HeroId { get; set; }

        [JsonProperty("SavedDate")]
        public string SavedDate { get; set; }

        [JsonProperty("IsCivilian")]
        public bool IsCivilian { get; set; }

        [JsonProperty("Equipment")]
        public List<EquipmentSlotData> Equipment { get; set; }

        public EquipmentSetData()
        {
            Equipment = new();
        }
    }

    /// <summary>
    /// Data class for individual equipment slot serialization
    /// </summary>
    public class EquipmentSlotData
    {
        [JsonProperty("Slot")]
        public string Slot { get; set; }

        [JsonProperty("ItemId")]
        public string ItemId { get; set; }

        [JsonProperty("ModifierId")]
        public string ModifierId { get; set; }
    }

    /// <summary>
    /// Helper class for displaying equipment items in command output
    /// </summary>
    public class EquipmentItemInfo
    {
        public string Slot { get; set; }
        public string ItemName { get; set; }
        public string ModifierText { get; set; }
    }

    /// <summary>
    /// Helper class for tracking skipped items during equipment load
    /// </summary>
    public class SkippedItemInfo
    {
        public string Slot { get; set; }
        public string ItemId { get; set; }
        public string ModifierInfo { get; set; }
    }
}
