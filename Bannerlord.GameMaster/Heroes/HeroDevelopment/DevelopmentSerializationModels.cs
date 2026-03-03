using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord.GameMaster.Heroes.HeroDevelopment
{
    /// <summary>
    /// Data class for hero development set serialization to JSON.
    /// Stores skills, attributes, perks, focus, XP, and level data.
    /// </summary>
    public class DevelopmentSetData
    {
        [JsonProperty("HeroName")]
        public string HeroName { get; set; }

        [JsonProperty("HeroStringId")]
        public string HeroStringId { get; set; }

        [JsonProperty("HeroMBGUID")]
        public string HeroMBGUID { get; set; }

        [JsonProperty("SavedDate")]
        public string SavedDate { get; set; }

        [JsonProperty("Level")]
        public int Level { get; set; }

        [JsonProperty("TotalXp")]
        public int TotalXp { get; set; }

        [JsonProperty("UnspentAttributePoints")]
        public int UnspentAttributePoints { get; set; }

        [JsonProperty("UnspentFocusPoints")]
        public int UnspentFocusPoints { get; set; }

        [JsonProperty("Attributes")]
        public List<AttributeData> Attributes { get; set; }

        [JsonProperty("Skills")]
        public List<SkillData> Skills { get; set; }

        [JsonProperty("Perks")]
        public List<PerkData> Perks { get; set; }

        public DevelopmentSetData()
        {
            Attributes = new();
            Skills = new();
            Perks = new();
        }
    }

    /// <summary>
    /// Data class for individual attribute serialization.
    /// </summary>
    public class AttributeData
    {
        [JsonProperty("AttributeId")]
        public string AttributeId { get; set; }

        [JsonProperty("Value")]
        public int Value { get; set; }
    }

    /// <summary>
    /// Data class for individual skill serialization.
    /// </summary>
    public class SkillData
    {
        [JsonProperty("SkillId")]
        public string SkillId { get; set; }

        [JsonProperty("Level")]
        public int Level { get; set; }

        [JsonProperty("Xp")]
        public float Xp { get; set; }

        [JsonProperty("Focus")]
        public int Focus { get; set; }
    }

    /// <summary>
    /// Data class for individual perk serialization.
    /// </summary>
    public class PerkData
    {
        [JsonProperty("PerkId")]
        public string PerkId { get; set; }

        [JsonProperty("IsSelected")]
        public bool IsSelected { get; set; }
    }
}
