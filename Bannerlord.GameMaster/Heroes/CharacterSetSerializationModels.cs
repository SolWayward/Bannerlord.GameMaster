using Bannerlord.GameMaster.Console.ItemCommands;
using Bannerlord.GameMaster.Heroes.HeroDevelopment;
using Newtonsoft.Json;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Unified data class for full character set serialization to JSON.
    /// Embeds existing serialization models (AppearanceSetData, DevelopmentSetData, TraitSetData, EquipmentSetData)
    /// as nested objects for zero duplication.
    /// </summary>
    public class CharacterSetData
    {
        #region Hero Metadata

        [JsonProperty("HeroName")]
        public string HeroName { get; set; }

        [JsonProperty("HeroStringId")]
        public string HeroStringId { get; set; }

        [JsonProperty("HeroMBGUID")]
        public string HeroMBGUID { get; set; }

        [JsonProperty("IsFemale")]
        public bool IsFemale { get; set; }

        /// <summary>
        /// Hero type stored as string for readability and forward-compatibility.
        /// Valid values: "Lord", "Wanderer", "Companion"
        /// </summary>
        [JsonProperty("HeroType")]
        public string HeroType { get; set; }

        [JsonProperty("Level")]
        public int Level { get; set; }

        [JsonProperty("Age")]
        public int Age { get; set; }

        /// <summary>
        /// CultureObject.StringId of the hero's culture.
        /// </summary>
        [JsonProperty("Culture")]
        public string Culture { get; set; }

        [JsonProperty("ClanName")]
        public string ClanName { get; set; }

        [JsonProperty("ClanStringId")]
        public string ClanStringId { get; set; }

        [JsonProperty("SavedDate")]
        public string SavedDate { get; set; }

        [JsonProperty("BLGMVersion")]
        public string BLGMVersion { get; set; }

        #endregion

        #region Embedded Data Sections

        [JsonProperty("Appearance")]
        public AppearanceSetData Appearance { get; set; }

        [JsonProperty("Development")]
        public DevelopmentSetData Development { get; set; }

        [JsonProperty("Traits")]
        public TraitSetData Traits { get; set; }

        [JsonProperty("BattleEquipment")]
        public EquipmentSetData BattleEquipment { get; set; }

        [JsonProperty("CivilianEquipment")]
        public EquipmentSetData CivilianEquipment { get; set; }

        #endregion
    }
}
