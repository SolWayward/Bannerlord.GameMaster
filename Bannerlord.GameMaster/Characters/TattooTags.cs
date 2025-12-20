namespace Bannerlord.GameMaster.Characters
{
    /// <summary>
    /// Filters which Tattoos / face paints are allowed </br>
    /// Multiple tags can be combined to allow a combination (ex: "tag1;tag2;tag3"), some pre-combined tags are provided
    /// </summary>
    public static class TattooTags
    {
        /// <summary>
        /// Empty String: All tattoos and face paints allowed
        /// </summary>
        public static readonly string All = "";        
        
        /// <summary>
        /// "none" : None allowed
        /// </summary>
        public static readonly string None = "none";       
        
        /// <summary>
        /// "tribal" : Tribal styled tattoos
        /// </summary>
        public static readonly string Tribal = "tribal";

        /// <summary>
        /// "war_paint" : War Paint styles only
        /// </summary>
        public static readonly string WarPaint = "war_paint";             
        
        /// <summary>
        /// "battanian" : Battanian culutural specific styles
        /// </summary>
        public static readonly string Battanian = "battanian";       
        
        /// <summary>
        /// "sturgian" : Sturgian culutural specific styles allowed
        /// </summary>
        public static readonly string Sturgian = "sturgian";

        /// <summary>
        /// "aserai" : Aserai culutural specific styles allowed
        /// </summary>
        public static readonly string Aserai = "aserai";

        /// <summary>
        /// "khuzait" : Khuzait culutural specific styles allowed
        /// </summary>
        public static readonly string Khuzait = "khuzait";

        /// <summary>
        /// "vlandian" : Vlandian culutural specific styles allowed (minimal)
        /// </summary>
        public static readonly string Vlandian = "vlandian";

        /// <summary>
        /// "empire" : Empire culutural specific styles allowed (minimal)
        /// </summary>
        public static readonly string Empire = "empire";
    }
}