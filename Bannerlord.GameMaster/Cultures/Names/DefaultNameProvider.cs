using Bannerlord.GameMaster.Cultures.FactionNames;
using Bannerlord.GameMaster.Cultures.HeroNames;

namespace Bannerlord.GameMaster.Cultures.Names
{
    /// <summary>
    /// Provides access to the hardcoded default name arrays.
    /// Used as fallback when no external file override exists.
    /// </summary>
    internal static class DefaultNameProvider
    {
        /// <summary>
        /// Gets default hero names for a culture and gender from hardcoded arrays
        /// </summary>
        internal static string[] GetHeroNames(string cultureId, bool isFemale)
        {
            return cultureId switch
            {
                "aserai" => isFemale ? AseraiHeroNames.FemaleNames : AseraiHeroNames.MaleNames,
                "battania" => isFemale ? BattaniaHeroNames.FemaleNames : BattaniaHeroNames.MaleNames,
                "empire" => isFemale ? EmpireHeroNames.FemaleNames : EmpireHeroNames.MaleNames,
                "khuzait" => isFemale ? KhuzaitHeroNames.FemaleNames : KhuzaitHeroNames.MaleNames,
                "nord" => isFemale ? NordHeroNames.FemaleNames : NordHeroNames.MaleNames,
                "sturgia" => isFemale ? SturgiaHeroNames.FemaleNames : SturgiaHeroNames.MaleNames,
                "vlandia" => isFemale ? VlandiaHeroNames.FemaleNames : VlandiaHeroNames.MaleNames,
                _ => null
            };
        }

        /// <summary>
        /// Gets default hero suffixes for a culture from hardcoded arrays
        /// </summary>
        internal static string[] GetHeroSuffixes(string cultureId)
        {
            return cultureId switch
            {
                "aserai" => AseraiHeroNames.HeroSuffixes,
                "battania" => BattaniaHeroNames.HeroSuffixes,
                "empire" => EmpireHeroNames.HeroSuffixes,
                "khuzait" => KhuzaitHeroNames.HeroSuffixes,
                "nord" => NordHeroNames.HeroSuffixes,
                "sturgia" => SturgiaHeroNames.HeroSuffixes,
                "vlandia" => VlandiaHeroNames.HeroSuffixes,
                _ => null
            };
        }

        /// <summary>
        /// Gets default clan names for a culture from hardcoded arrays
        /// </summary>
        internal static string[] GetClanNames(string cultureId)
        {
            return cultureId switch
            {
                "aserai" => AseraiFactionNames.ClanNames,
                "battania" => BattaniaFactionNames.ClanNames,
                "empire" => EmpireFactionNames.ClanNames,
                "khuzait" => KhuzaitFactionNames.ClanNames,
                "nord" => NordFactionNames.ClanNames,
                "sturgia" => SturgiaFactionNames.ClanNames,
                "vlandia" => VlandiaFactionNames.ClanNames,
                _ => null
            };
        }

        /// <summary>
        /// Gets default faction prefixes for a culture from hardcoded arrays
        /// </summary>
        internal static string[] GetFactionPrefixes(string cultureId)
        {
            return cultureId switch
            {
                "aserai" => AseraiFactionNames.FactionPrefixes,
                "battania" => BattaniaFactionNames.FactionPrefixes,
                "empire" => EmpireFactionNames.FactionPrefixes,
                "khuzait" => KhuzaitFactionNames.FactionPrefixes,
                "nord" => NordFactionNames.FactionPrefixes,
                "sturgia" => SturgiaFactionNames.FactionPrefixes,
                "vlandia" => VlandiaFactionNames.FactionPrefixes,
                _ => null
            };
        }

        /// <summary>
        /// Gets default kingdom names for a culture from hardcoded arrays
        /// </summary>
        internal static string[] GetKingdomNames(string cultureId)
        {
            return cultureId switch
            {
                "aserai" => AseraiFactionNames.KingdomNames,
                "battania" => BattaniaFactionNames.KingdomNames,
                "empire" => EmpireFactionNames.KingdomNames,
                "khuzait" => KhuzaitFactionNames.KingdomNames,
                "nord" => NordFactionNames.KingdomNames,
                "sturgia" => SturgiaFactionNames.KingdomNames,
                "vlandia" => VlandiaFactionNames.KingdomNames,
                _ => null
            };
        }
    }
}
