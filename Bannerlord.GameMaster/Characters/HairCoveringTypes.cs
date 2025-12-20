namespace Bannerlord.GameMaster.Characters
{
    /// <summary>
    /// Dertimines how hair is rendered depending on the type of headgear worn
    /// </summary>
    public static class HairCoveringType
    {
        /// <summary>
        /// 0: No hair covering - full hair visible (default for no helmet)
        /// </summary>
        public static readonly int None = 0;        
        
        /// <summary>
        /// 1: Light coverage (circlets, tiaras, open helmets)
        /// </summary>
        public static readonly int Light = 1;       
        
        /// <summary>
        /// 2: Medium coverage (half-helmets, hoods showing some hair)
        /// </summary>
        public static readonly int Medium = 2;           
        
        /// <summary>
        /// 3: Heavy coverage (most helmets, coifs)
        /// </summary>
        public static readonly int Heavy = 3;       
        
        /// <summary>
        /// 4: Full coverage (full enclosed helmets, complete head wraps)
        /// </summary>
        public static readonly int Full = 4;

        /// <summary>
        /// 5: Special case (specific cultural headgear)
        /// </summary>
        public static readonly int Special = 5;
    }
}