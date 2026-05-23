using System;
using System.Reflection;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;

namespace Bannerlord.GameMaster.Information
{
    public static class GameEnvironment
    {
        private const string WarsailsDlcId = "NavalDLC";
        private static ApplicationVersion? bannerlordAppVersion;
        private static string bannerlordVersionString;
        private static Version blgmVersion;
        private static string[] loadedModules;

        /// <summary>
        /// Checks if the Warsails DLC is loaded.
        /// </summary>
        public static bool IsWarsailsDlcLoaded => ModuleHelper.IsModuleActive(WarsailsDlcId);

        /// <summary>
        /// Gets the current Bannerlord ApplicationVersion.
        /// </summary>
        public static ApplicationVersion BannerlordAppVersion
        {
            get
            {
                bannerlordAppVersion ??= ApplicationVersion.FromParametersFile();
                return bannerlordAppVersion.Value;
            }
        }

        /// <summary>
        /// Gets the current Bannerlord game version as a display string.
        /// </summary>
        public static string BannerlordVersion
        {
            get
            {
                bannerlordVersionString ??= BannerlordAppVersion.ToString();
                return bannerlordVersionString;
            }
        }

        /// <summary>
        /// Checks if the current Bannerlord version is equal to or greater than the specified version.
        /// </summary>
        public static bool BannerlordIsVersionOrGreater(int major, int minor, int revision, int changeSet = 0)
        {
            return BannerlordAppVersion >= new ApplicationVersion(
                BannerlordAppVersion.ApplicationVersionType, major, minor, revision, changeSet);
        }

        /// <summary>
        /// Gets the current Bannerlord.GameMaster mod version.
        /// </summary>
        public static Version BLGMVersion
        {
            get
            {
                blgmVersion ??= Assembly.GetExecutingAssembly().GetName().Version;
                return blgmVersion;
            }
        }

        /// <summary>
        /// Checks if the current BLGM version is equal to or greater than the specified version.
        /// </summary>
        public static bool BLGMIsVersionOrGreater(int major, int minor, int build, int revision = 0)
        {
            return BLGMVersion >= new Version(major, minor, build, revision);
        }

        /// <summary>
        /// Checks if the current BLGM version exactly matches the specified version.
        /// </summary>
        public static bool BLGMIsVersion(int major, int minor, int build, int revision = 0)
        {
            return BLGMVersion == new Version(major, minor, build, revision);
        }

        /// <summary>
        /// Returns an array of currently loaded Mod Ids
        /// </summary>
        public static string[] LoadedModules
        {
            get
            {
                loadedModules ??= TaleWorlds.Engine.Utilities.GetModulesNames();
                return loadedModules;
            }
        }
    }
}