using Newtonsoft.Json;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Data class for hero appearance set serialization to JSON.
    /// Stores the full BodyProperties string plus individual KeyParts for debugging/readability.
    /// </summary>
    public class AppearanceSetData
    {
        [JsonProperty("HeroName")]
        public string HeroName { get; set; }

        [JsonProperty("HeroStringId")]
        public string HeroStringId { get; set; }

        [JsonProperty("HeroMBGUID")]
        public string HeroMBGUID { get; set; }

        [JsonProperty("Culture")]
        public string Culture { get; set; }

        [JsonProperty("IsFemale")]
        public bool IsFemale { get; set; }

        [JsonProperty("SavedDate")]
        public string SavedDate { get; set; }

        [JsonProperty("BodyPropertiesString")]
        public string BodyPropertiesString { get; set; }

        [JsonProperty("KeyPart1")]
        public string KeyPart1 { get; set; }

        [JsonProperty("KeyPart2")]
        public string KeyPart2 { get; set; }

        [JsonProperty("KeyPart3")]
        public string KeyPart3 { get; set; }

        [JsonProperty("KeyPart4")]
        public string KeyPart4 { get; set; }

        [JsonProperty("KeyPart5")]
        public string KeyPart5 { get; set; }

        [JsonProperty("KeyPart6")]
        public string KeyPart6 { get; set; }

        [JsonProperty("KeyPart7")]
        public string KeyPart7 { get; set; }

        [JsonProperty("KeyPart8")]
        public string KeyPart8 { get; set; }
    }
}
