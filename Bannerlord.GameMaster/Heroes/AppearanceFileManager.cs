using Bannerlord.GameMaster.Common;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Manages appearance file I/O operations for saving and loading hero appearance sets.
    /// Supports configurable mod folder names for use by different mods.
    /// </summary>
    public class AppearanceFileManager
    {
        private const string BaseFolder = "Mount and Blade II Bannerlord";
        private const string ConfigFolder = "Configs";
        private const string AppearanceSetsFolder = "AppearanceSets";

        /// <summary>
        /// The mod-specific folder name used in the configuration path.
        /// </summary>
        public string ModFolder { get; }

        /// <summary>
        /// Default instance using "GameMaster" folder for backwards compatibility.
        /// </summary>
        public static AppearanceFileManager Default { get; } = new("GameMaster");

        /// <summary>
        /// Creates a new AppearanceFileManager with the specified mod folder name.
        /// </summary>
        /// <param name="modFolder">The mod folder name to use in the configuration path.</param>
        public AppearanceFileManager(string modFolder)
        {
            if (string.IsNullOrWhiteSpace(modFolder))
                throw new ArgumentException("Mod folder cannot be null or empty.", nameof(modFolder));
            ModFolder = modFolder;
        }

        #region File Path Operations

        /// <summary>
        /// Gets the full file path for appearance files.
        /// </summary>
        /// <param name="filename">The filename without extension</param>
        /// <returns>The full file path</returns>
        public string GetAppearanceFilePath(string filename)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string basePath = Path.Combine(documentsPath, BaseFolder, ConfigFolder, ModFolder, AppearanceSetsFolder);

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".json";
            }

            return Path.Combine(basePath, filename);
        }

        /// <summary>
        /// Gets the appearance sets directory path.
        /// </summary>
        /// <returns>The directory path</returns>
        public string GetAppearanceDirectory()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documentsPath, BaseFolder, ConfigFolder, ModFolder, AppearanceSetsFolder);
        }

        #endregion

        #region Save Operations

        /// <summary>
        /// Saves a hero's appearance data to a JSON file.
        /// </summary>
        /// <param name="hero">The hero whose appearance is being saved</param>
        /// <param name="filepath">The full file path to save to</param>
        public void SaveAppearanceToFile(Hero hero, string filepath)
        {
            StaticBodyProperties staticProps = hero.BodyProperties.StaticProperties;

            AppearanceSetData appearanceData = new()
            {
                HeroName = hero.Name?.ToString() ?? "",
                HeroStringId = hero.StringId,
                HeroMBGUID = hero.Id.ToString(),
                Culture = hero.Culture?.StringId ?? "",
                IsFemale = hero.IsFemale,
                SavedDate = DateTime.UtcNow.ToString("o"),
                BodyPropertiesString = hero.BodyProperties.ToString(),
                KeyPart1 = staticProps.KeyPart1.ToString("X16"),
                KeyPart2 = staticProps.KeyPart2.ToString("X16"),
                KeyPart3 = staticProps.KeyPart3.ToString("X16"),
                KeyPart4 = staticProps.KeyPart4.ToString("X16"),
                KeyPart5 = staticProps.KeyPart5.ToString("X16"),
                KeyPart6 = staticProps.KeyPart6.ToString("X16"),
                KeyPart7 = staticProps.KeyPart7.ToString("X16"),
                KeyPart8 = staticProps.KeyPart8.ToString("X16")
            };

            string jsonString = JsonConvert.SerializeObject(appearanceData, Formatting.Indented);
            File.WriteAllText(filepath, jsonString);
        }

        #endregion

        #region Load Operations

        /// <summary>
        /// Loads appearance data from a JSON file without applying to a hero.
        /// </summary>
        /// <param name="filepath">The full file path to load from</param>
        /// <returns>The deserialized appearance set data</returns>
        public AppearanceSetData LoadAppearanceData(string filepath)
        {
            string jsonString = File.ReadAllText(filepath);
            AppearanceSetData appearanceData = JsonConvert.DeserializeObject<AppearanceSetData>(jsonString);

            if (appearanceData == null || string.IsNullOrEmpty(appearanceData.BodyPropertiesString))
            {
                throw new InvalidDataException("Invalid appearance file format.");
            }

            return appearanceData;
        }

        /// MARK: LoadAppearanceToHero
        /// <summary>
        /// Loads appearance data from a JSON file and applies it to a hero.
        /// Parses the BodyPropertiesString to extract StaticBodyProperties, Weight, and Build.
        /// Age is NOT applied (tied to hero birth date/timeline).
        /// Gender mismatch is blocked by default unless forceGenderMismatch is true.
        /// </summary>
        /// <param name="hero">The hero to apply appearance to</param>
        /// <param name="filepath">The full file path to load from</param>
        /// <param name="forceGenderMismatch">If true, allows applying appearance across genders</param>
        /// <returns>BLGMResult indicating success or failure</returns>
        public BLGMResult LoadAppearanceToHero(Hero hero, string filepath, bool forceGenderMismatch = false)
        {
            if (hero == null)
            {
                return BLGMResult.Error("LoadAppearanceToHero() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            AppearanceSetData data = LoadAppearanceData(filepath);

            // Gender mismatch check
            if (data.IsFemale != hero.IsFemale && !forceGenderMismatch)
            {
                string savedGender = data.IsFemale ? "female" : "male";
                string heroGender = hero.IsFemale ? "female" : "male";
                return BLGMResult.Error(
                    $"Gender mismatch: Saved appearance is {savedGender} but {hero.Name} is {heroGender}. " +
                    $"Use force:true to override this check.").Log();
            }

            // Parse BodyProperties from the saved string
            if (!BodyProperties.FromString(data.BodyPropertiesString, out BodyProperties loadedProps))
            {
                return BLGMResult.Error(
                    $"LoadAppearanceToHero() failed to parse BodyPropertiesString from file",
                    new InvalidOperationException("BodyProperties.FromString failed")).Log();
            }

            // Apply StaticBodyProperties (facial features, hair, tattoos, skin color, height)
            hero.StaticBodyProperties = loadedProps.StaticProperties;

            // Apply Weight and Build from DynamicBodyProperties
            hero.Weight = loadedProps.DynamicProperties.Weight;
            hero.Build = loadedProps.DynamicProperties.Build;
            // Age is NOT applied - it is tied to hero birth date/timeline

            string warningText = "";
            if (data.IsFemale != hero.IsFemale && forceGenderMismatch)
            {
                string savedGender = data.IsFemale ? "female" : "male";
                string heroGender = hero.IsFemale ? "female" : "male";
                warningText = $" (WARNING: Gender mismatch - saved: {savedGender}, hero: {heroGender})";
            }

            return BLGMResult.Success(
                $"Applied appearance to {hero.Name} from '{Path.GetFileNameWithoutExtension(filepath)}'" +
                $"{warningText}. Applied: StaticBodyProperties, Weight ({hero.Weight:F2}), Build ({hero.Build:F2}). " +
                $"Age was not applied (tied to timeline).");
        }

        /// <summary>
        /// Checks if an appearance file exists.
        /// </summary>
        /// <param name="filename">The filename to check</param>
        /// <returns>True if the file exists</returns>
        public bool AppearanceFileExists(string filename)
        {
            string filepath = GetAppearanceFilePath(filename);
            return File.Exists(filepath);
        }

        /// <summary>
        /// Lists all appearance files in the directory.
        /// </summary>
        /// <returns>Array of file names without paths</returns>
        public string[] ListAppearanceFiles()
        {
            string directory = GetAppearanceDirectory();

            if (!Directory.Exists(directory))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(directory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
        }

        #endregion
    }
}
