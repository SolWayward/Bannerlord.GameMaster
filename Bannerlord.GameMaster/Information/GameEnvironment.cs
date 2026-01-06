using System;
using System.Reflection;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;

namespace Bannerlord.GameMaster.Information
{
    public static class GameEnvironment
    {
        private const string WarsailsDlcId = "NavalDLC";
        private static string bannerlordVersion;
        private static string blgmVersion;
        private static string[] loadedModules;

        /// <summary>
        /// Checks if the Warsails DLC is loaded.
        /// </summary>
        public static bool IsWarsailsDlcLoaded => ModuleHelper.IsModuleActive(WarsailsDlcId);
        
        /// <summary>
        /// Gets the current Bannerlord game version.
        /// </summary>
        public static string BannerlordVersion
        {
            get
            {
                bannerlordVersion ??= ApplicationVersion.FromParametersFile().ToString();
                return bannerlordVersion;
            }
        }

        /// <summary>
        /// Gets the current Bannerlord.GameMaster mod version.
        /// </summary>
        public static string BLGMVersion
        {
            get
            {
                blgmVersion ??= Assembly.GetExecutingAssembly()
                  .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                  .InformationalVersion ?? "Unknown";
                return blgmVersion;
            }
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