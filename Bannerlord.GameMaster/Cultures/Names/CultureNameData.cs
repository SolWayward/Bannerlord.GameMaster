using Newtonsoft.Json;

namespace Bannerlord.GameMaster.Cultures.Names
{
    /// <summary>
    /// Serialization model for culture name override JSON files.
    /// All properties are optional -- null means use hardcoded default.
    /// Path: Documents/Mount and Blade II Bannerlord/Configs/GameMaster/Names/{cultureId}.json
    /// </summary>
    internal class CultureNameData
    {
        [JsonProperty("MaleHeroNames")]
        public string[] MaleHeroNames { get; set; }

        [JsonProperty("FemaleHeroNames")]
        public string[] FemaleHeroNames { get; set; }

        [JsonProperty("HeroSuffixes")]
        public string[] HeroSuffixes { get; set; }

        [JsonProperty("ClanNames")]
        public string[] ClanNames { get; set; }

        [JsonProperty("KingdomNames")]
        public string[] KingdomNames { get; set; }

        [JsonProperty("FactionPrefixes")]
        public string[] FactionPrefixes { get; set; }
    }
}
