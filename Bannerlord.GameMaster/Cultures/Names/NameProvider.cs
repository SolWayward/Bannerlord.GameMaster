using System.Collections.Concurrent;

namespace Bannerlord.GameMaster.Cultures.Names
{
    /// <summary>
    /// Provides name arrays with external JSON file override support and caching.
    /// Lookup chain: Cache -> External JSON file -> Hardcoded defaults
    /// </summary>
    public static class NameProvider
    {
        private static readonly ConcurrentDictionary<string, CultureNameData> _cache = new();
        private static volatile bool _forceDefaults;

        // Sentinel object cached when no file exists, to avoid repeated disk checks
        private static readonly CultureNameData EmptySentinel = new();

        // MARK: Hero Names

        /// <summary>
        /// Gets hero names for a culture and gender. Checks external JSON file first, falls back to defaults.
        /// </summary>
        public static string[] GetHeroNames(string cultureId, bool isFemale)
        {
            NameType nameType = isFemale ? NameType.FemaleHeroNames : NameType.MaleHeroNames;
            string[] overrideNames = GetOverrideNames(cultureId, nameType);
            return overrideNames ?? DefaultNameProvider.GetHeroNames(cultureId, isFemale);
        }

        /// <summary>
        /// Gets hero suffixes for a culture. Checks external JSON file first, falls back to defaults.
        /// </summary>
        public static string[] GetHeroSuffixes(string cultureId)
        {
            string[] overrideNames = GetOverrideNames(cultureId, NameType.HeroSuffixes);
            return overrideNames ?? DefaultNameProvider.GetHeroSuffixes(cultureId);
        }

        // MARK: Faction Names

        /// <summary>
        /// Gets clan names for a culture. Checks external JSON file first, falls back to defaults.
        /// </summary>
        public static string[] GetClanNames(string cultureId)
        {
            string[] overrideNames = GetOverrideNames(cultureId, NameType.ClanNames);
            return overrideNames ?? DefaultNameProvider.GetClanNames(cultureId);
        }

        /// <summary>
        /// Gets faction prefixes for a culture. Checks external JSON file first, falls back to defaults.
        /// </summary>
        public static string[] GetFactionPrefixes(string cultureId)
        {
            string[] overrideNames = GetOverrideNames(cultureId, NameType.FactionPrefixes);
            return overrideNames ?? DefaultNameProvider.GetFactionPrefixes(cultureId);
        }

        /// <summary>
        /// Gets kingdom names for a culture. Checks external JSON file first, falls back to defaults.
        /// </summary>
        public static string[] GetKingdomNames(string cultureId)
        {
            string[] overrideNames = GetOverrideNames(cultureId, NameType.KingdomNames);
            return overrideNames ?? DefaultNameProvider.GetKingdomNames(cultureId);
        }

        // MARK: Cache Management

        /// <summary>
        /// Whether force-defaults mode is currently active
        /// </summary>
        public static bool IsForceDefaultsActive => _forceDefaults;

        /// <summary>
        /// Clears all cached culture data and re-reads from JSON files on next access
        /// </summary>
        public static void ReloadAll()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Clears cached data for a specific culture only
        /// </summary>
        public static void ReloadCulture(string cultureId)
        {
            _cache.TryRemove(cultureId, out _);
        }

        /// <summary>
        /// Enables force-defaults mode (ignores all external files, clears cache)
        /// </summary>
        public static void ForceDefaults()
        {
            _forceDefaults = true;
            _cache.Clear();
        }

        /// <summary>
        /// Disables force-defaults mode and clears cache so files are re-checked on next access
        /// </summary>
        public static void ClearForceDefaults()
        {
            _forceDefaults = false;
            _cache.Clear();
        }

        // MARK: Internal

        /// <summary>
        /// Core lookup: loads CultureNameData from cache or file, extracts the requested name type.
        /// Returns null if the requested type is not overridden (caller falls back to DefaultNameProvider).
        /// Returns immediately from DefaultNameProvider when _forceDefaults is true.
        /// </summary>
        private static string[] GetOverrideNames(string cultureId, NameType nameType)
        {
            if (_forceDefaults)
                return null;

            CultureNameData data = _cache.GetOrAdd(cultureId, id =>
            {
                CultureNameData loaded = NameFileManager.LoadCultureNames(id);
                return loaded ?? EmptySentinel;
            });

            return ExtractNameArray(data, nameType);
        }

        /// <summary>
        /// Extracts the specific name array from a CultureNameData object by NameType
        /// </summary>
        private static string[] ExtractNameArray(CultureNameData data, NameType nameType)
        {
            return nameType switch
            {
                NameType.MaleHeroNames => data.MaleHeroNames,
                NameType.FemaleHeroNames => data.FemaleHeroNames,
                NameType.HeroSuffixes => data.HeroSuffixes,
                NameType.ClanNames => data.ClanNames,
                NameType.KingdomNames => data.KingdomNames,
                NameType.FactionPrefixes => data.FactionPrefixes,
                _ => null
            };
        }
    }
}
