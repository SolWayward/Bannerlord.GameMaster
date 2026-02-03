namespace Bannerlord.GameMaster.Characters
{
    /// <summary>
    /// Filters which beards are allowed <br />
    /// Female Character should ignore any beard tags <br />
    /// Multiple tags can be combined to allow a combination (ex: "tag1;tag2;tag3"), some pre-combined tags are provided
    /// </summary>
    public static class BeardTags
    {
        /// <summary>
        /// Empty String: All Beard styles allowed
        /// </summary>
        public static readonly string All = "";        
        
        /// <summary>
        /// "clean_shaven" : Forces no beard
        /// </summary>
        public static readonly string NoBeard = "clean_shaven";       
        
        /// <summary>
        /// "full_beard" : Full beard styles only
        /// </summary>
        public static readonly string FullBeard = "full_beard";

        /// <summary>
        /// "goatee" : Goatee styles only
        /// </summary>
        public static readonly string Goatee = "goatee";             
        
        /// <summary>
        /// "mustache" : Mustache styles
        /// </summary>
        public static readonly string Mustache = "mustache";       
        
        /// <summary>
        /// "stubble" : Stubble styles
        /// </summary>
        public static readonly string Stubble = "stubble";

        /// <summary>
        /// "chin_beard" : Chin Beards
        /// </summary>
        public static readonly string ChinBeard = "chin_beard";

        /// <summary>
        /// "mutton_chops" : Sideburns / Mutton chops
        /// </summary>
        public static readonly string MuttonChops = "mutton_chops";
    }
}