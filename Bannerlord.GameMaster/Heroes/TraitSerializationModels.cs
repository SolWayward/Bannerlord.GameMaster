using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Data class for hero trait set serialization to JSON.
    /// Stores all traits (personality, persona, political, role/skill) with their levels.
    /// </summary>
    public class TraitSetData
    {
        [JsonProperty("HeroName")]
        public string HeroName { get; set; }

        [JsonProperty("HeroStringId")]
        public string HeroStringId { get; set; }

        [JsonProperty("HeroMBGUID")]
        public string HeroMBGUID { get; set; }

        [JsonProperty("SavedDate")]
        public string SavedDate { get; set; }

        [JsonProperty("Traits")]
        public List<TraitData> Traits { get; set; }

        public TraitSetData()
        {
            Traits = new();
        }
    }

    /// <summary>
    /// Data class for individual trait serialization.
    /// </summary>
    public class TraitData
    {
        [JsonProperty("TraitId")]
        public string TraitId { get; set; }

        [JsonProperty("Level")]
        public int Level { get; set; }
    }
}
