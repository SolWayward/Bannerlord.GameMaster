using Bannerlord.GameMaster.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Manages trait file I/O operations for saving and loading hero trait sets.
    /// Supports configurable mod folder names for use by different mods.
    /// </summary>
    public class TraitFileManager
    {
        private const string BaseFolder = "Mount and Blade II Bannerlord";
        private const string ConfigFolder = "Configs";
        private const string TraitSetsFolder = "TraitSets";

        /// <summary>
        /// The mod-specific folder name used in the configuration path.
        /// </summary>
        public string ModFolder { get; }

        /// <summary>
        /// Default instance using "GameMaster" folder for backwards compatibility.
        /// </summary>
        public static TraitFileManager Default { get; } = new("GameMaster");

        /// <summary>
        /// Creates a new TraitFileManager with the specified mod folder name.
        /// </summary>
        /// <param name="modFolder">The mod folder name to use in the configuration path.</param>
        public TraitFileManager(string modFolder)
        {
            if (string.IsNullOrWhiteSpace(modFolder))
                throw new ArgumentException("Mod folder cannot be null or empty.", nameof(modFolder));
            ModFolder = modFolder;
        }

        #region File Path Operations

        /// <summary>
        /// Gets the full file path for trait files.
        /// </summary>
        /// <param name="filename">The filename without extension</param>
        /// <returns>The full file path</returns>
        public string GetTraitFilePath(string filename)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string basePath = Path.Combine(documentsPath, BaseFolder, ConfigFolder, ModFolder, TraitSetsFolder);

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
        /// Gets the trait sets directory path.
        /// </summary>
        /// <returns>The directory path</returns>
        public string GetTraitDirectory()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documentsPath, BaseFolder, ConfigFolder, ModFolder, TraitSetsFolder);
        }

        #endregion

        #region Save Operations

        /// <summary>
        /// Saves a hero's trait data to a JSON file.
        /// Iterates TraitObject.All to capture every trait including modded ones.
        /// </summary>
        /// <param name="hero">The hero whose traits are being saved</param>
        /// <param name="filepath">The full file path to save to</param>
        public void SaveTraitsToFile(Hero hero, string filepath)
        {
            TraitSetData data = new()
            {
                HeroName = hero.Name?.ToString() ?? "",
                HeroStringId = hero.StringId,
                HeroMBGUID = hero.Id.ToString(),
                SavedDate = DateTime.UtcNow.ToString("o")
            };

            foreach (TraitObject trait in TraitObject.All)
            {
                data.Traits.Add(new TraitData
                {
                    TraitId = trait.StringId,
                    Level = hero.GetTraitLevel(trait)
                });
            }

            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filepath, jsonString);
        }

        #endregion

        #region Load Operations

        /// <summary>
        /// Loads trait data from a JSON file without applying to a hero.
        /// </summary>
        /// <param name="filepath">The full file path to load from</param>
        /// <returns>The deserialized trait set data</returns>
        public TraitSetData LoadTraitData(string filepath)
        {
            string jsonString = File.ReadAllText(filepath);
            TraitSetData data = JsonConvert.DeserializeObject<TraitSetData>(jsonString);

            if (data == null || data.Traits == null)
            {
                throw new InvalidDataException("Invalid trait file format.");
            }

            return data;
        }

        /// MARK: LoadTraitsToHero
        /// <summary>
        /// Loads trait data from a JSON file and applies it to a hero.
        /// For each trait, resolves by StringId and calls hero.SetTraitLevel().
        /// Tracks skipped traits (removed mod traits) in the result.
        /// </summary>
        /// <param name="hero">The hero to apply traits to</param>
        /// <param name="filepath">The full file path to load from</param>
        /// <returns>BLGMResult with loaded/skipped counts</returns>
        public BLGMResult LoadTraitsToHero(Hero hero, string filepath)
        {
            if (hero == null)
            {
                return BLGMResult.Error("LoadTraitsToHero() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            TraitSetData data = LoadTraitData(filepath);

            int traitsLoaded = 0;
            List<string> skippedTraits = new();

            foreach (TraitData traitData in data.Traits)
            {
                TraitObject trait = MBObjectManager.Instance.GetObject<TraitObject>(traitData.TraitId);
                if (trait == null)
                {
                    skippedTraits.Add(traitData.TraitId);
                    continue;
                }

                hero.SetTraitLevel(trait, traitData.Level);
                traitsLoaded++;
            }

            StringBuilder resultMessage = new();
            resultMessage.Append($"Applied {traitsLoaded} trait(s) to {hero.Name} from '{Path.GetFileNameWithoutExtension(filepath)}'.");

            if (skippedTraits.Count > 0)
            {
                resultMessage.Append($"\nSkipped {skippedTraits.Count} trait(s) (not found in game): ");
                resultMessage.Append(string.Join(", ", skippedTraits));
            }

            return BLGMResult.Success(resultMessage.ToString());
        }

        /// <summary>
        /// Checks if a trait file exists.
        /// </summary>
        /// <param name="filename">The filename to check</param>
        /// <returns>True if the file exists</returns>
        public bool TraitFileExists(string filename)
        {
            string filepath = GetTraitFilePath(filename);
            return File.Exists(filepath);
        }

        /// <summary>
        /// Lists all trait files in the directory.
        /// </summary>
        /// <returns>Array of file names without paths</returns>
        public string[] ListTraitFiles()
        {
            string directory = GetTraitDirectory();

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
