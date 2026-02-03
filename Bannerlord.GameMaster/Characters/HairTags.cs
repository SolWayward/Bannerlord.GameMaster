namespace Bannerlord.GameMaster.Characters
{
    /// <summary>
    /// Determines which hair styles are allowed <br />
    /// Multiple tags can be combined to allow a combination (ex: "tag1;tag2;tag3"), some pre-combined tags are provided
    /// </summary>
    public static class HairTags
    {
        /// <summary>
        /// Empty String: All Hair styles allowed
        /// </summary>
        public static readonly string All = "";        
        
        /// <summary>
        /// "bald" : Forces bald/no hair
        /// </summary>
        public static readonly string Bald = "bald";

        /// <summary>
        /// "shaved" : Shaved/Buzzed
        /// </summary>
        public static readonly string Shaved = "shaved";         
        
        /// <summary>
        /// "short" : Short hairstyles only
        /// </summary>
        public static readonly string Short = "short";

        /// <summary>
        /// "long" : Long hairstyles only
        /// </summary>
        public static readonly string Long = "long";             
        
        /// <summary>
        /// "braided" : Braided styles
        /// </summary>
        public static readonly string Braided = "braided";       
        
        /// <summary>
        /// "ponytail" : Ponytail styles
        /// </summary>
        public static readonly string Ponytails = "ponytail";

        /// <summary>
        /// "tied" : Tied styles
        /// </summary>
        public static readonly string Tied = "tied";

        /// <summary>
        /// "long;ponytail" : Normal attractive female hairstyles for normal people
        /// </summary>
        public static readonly string NormalFemale = "long;ponytail";

        /// <summary>
        /// "bald;shaved;short" : Normal common male hairstyles, may want to add long or others
        /// </summary>
        public static readonly string NormalMale = "bald;shaved;short";

        /// <summary>
        /// "shaved;short;long;ponytail;tied;braided" : Everything but bald
        /// </summary>
        public static readonly string NotBald = "shaved;short;long;ponytail;tied;braided";

        /// <summary>
        /// "short;long;ponytail;tied;braided" : Everything but bald and shaved
        /// </summary>
        public static readonly string NotBaldOrShaved = "short;long;ponytail;tied;braided";

        /// <summary>
        /// "long;ponytail;tied;braided" : All long hairstyles
        /// </summary>
        public static readonly string AllLong = "long;ponytail;tied;braided";

        /// <summary>
        /// bald;short;shaved;tied;braided : More viking stylish hair (Although viking can really be anything)
        /// </summary>
        public static readonly string Viking = "bald;short;shaved;tied;braided";

        /// <summary>
        /// "shaved;short;long;ponytail;tied;bald" : Everything but braided
        /// </summary>
        public static readonly string NotBraided = "shaved;short;long;ponytail;tied;bald";
    }
}