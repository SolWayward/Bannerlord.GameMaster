using Bannerlord.GameMaster.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Heroes.HeroDevelopment
{
    /// <summary>
    /// Manages development file I/O operations for saving and loading hero development sets.
    /// Supports configurable mod folder names for use by different mods.
    /// </summary>
    public class DevelopmentFileManager
    {
        private const string BaseFolder = "Mount and Blade II Bannerlord";
        private const string ConfigFolder = "Configs";
        private const string DevelopmentSetsFolder = "DevelopmentSets";

        /// <summary>
        /// The mod-specific folder name used in the configuration path.
        /// </summary>
        public string ModFolder { get; }

        /// <summary>
        /// Default instance using "GameMaster" folder for backwards compatibility.
        /// </summary>
        public static DevelopmentFileManager Default { get; } = new("GameMaster");

        /// <summary>
        /// Creates a new DevelopmentFileManager with the specified mod folder name.
        /// </summary>
        /// <param name="modFolder">The mod folder name to use in the configuration path.</param>
        public DevelopmentFileManager(string modFolder)
        {
            if (string.IsNullOrWhiteSpace(modFolder))
                throw new ArgumentException("Mod folder cannot be null or empty.", nameof(modFolder));
            ModFolder = modFolder;
        }

        #region File Path Operations

        /// <summary>
        /// Gets the full file path for development files.
        /// </summary>
        /// <param name="filename">The filename without extension</param>
        /// <returns>The full file path</returns>
        public string GetDevelopmentFilePath(string filename)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string basePath = Path.Combine(documentsPath, BaseFolder, ConfigFolder, ModFolder, DevelopmentSetsFolder);

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
        /// Gets the development sets directory path.
        /// </summary>
        /// <returns>The directory path</returns>
        public string GetDevelopmentDirectory()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documentsPath, BaseFolder, ConfigFolder, ModFolder, DevelopmentSetsFolder);
        }

        #endregion

        #region Save Operations

        /// <summary>
        /// Saves a hero's full development data to a JSON file.
        /// Captures all skills, attributes, perks, focus, XP, and level data.
        /// </summary>
        /// <param name="hero">The hero whose development is being saved</param>
        /// <param name="filepath">The full file path to save to</param>
        public void SaveDevelopmentToFile(Hero hero, string filepath)
        {
            HeroDeveloper developer = hero.HeroDeveloper;

            DevelopmentSetData data = new()
            {
                HeroName = hero.Name?.ToString() ?? "",
                HeroStringId = hero.StringId,
                HeroMBGUID = hero.Id.ToString(),
                SavedDate = DateTime.UtcNow.ToString("o"),
                Level = hero.Level,
                TotalXp = developer.TotalXp,
                UnspentAttributePoints = developer.UnspentAttributePoints,
                UnspentFocusPoints = developer.UnspentFocusPoints
            };

            // Save attributes
            foreach (CharacterAttribute attribute in Attributes.All)
            {
                data.Attributes.Add(new AttributeData
                {
                    AttributeId = attribute.StringId,
                    Value = hero.GetAttributeValue(attribute)
                });
            }

            // Save skills with XP and focus
            foreach (SkillObject skill in Skills.All)
            {
                data.Skills.Add(new SkillData
                {
                    SkillId = skill.StringId,
                    Level = hero.GetSkillValue(skill),
                    Xp = developer.GetSkillXp(skill),
                    Focus = developer.GetFocus(skill)
                });
            }

            // Save perk selections
            foreach (PerkObject perk in PerkObject.All)
            {
                data.Perks.Add(new PerkData
                {
                    PerkId = perk.StringId,
                    IsSelected = hero.GetPerkValue(perk)
                });
            }

            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filepath, jsonString);
        }

        #endregion

        #region Load Operations

        /// <summary>
        /// Loads development data from a JSON file without applying to a hero.
        /// </summary>
        /// <param name="filepath">The full file path to load from</param>
        /// <returns>The deserialized development set data</returns>
        public DevelopmentSetData LoadDevelopmentData(string filepath)
        {
            string jsonString = File.ReadAllText(filepath);
            DevelopmentSetData data = JsonConvert.DeserializeObject<DevelopmentSetData>(jsonString);

            if (data == null)
            {
                throw new InvalidDataException("Invalid development file format.");
            }

            return data;
        }

        /// MARK: LoadDevelopmentToHero
        /// <summary>
        /// Loads development data from a JSON file and applies it to a hero.
        /// Follows the HeroSkillSnapshot.RestoreTo pattern:
        /// 1. Set Attributes, 2. Set Focus, 3. Set Skill Levels,
        /// 4. Clear All Perks, 5. Re-Select Saved Perks, 6. Set Unspent Points,
        /// 7. RecalculateLevel.
        /// </summary>
        /// <param name="hero">The hero to apply development to</param>
        /// <param name="filepath">The full file path to load from</param>
        /// <returns>BLGMResult with details including skipped items</returns>
        public BLGMResult LoadDevelopmentToHero(Hero hero, string filepath)
        {
            if (hero == null)
            {
                return BLGMResult.Error("LoadDevelopmentToHero() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            DevelopmentSetData data = LoadDevelopmentData(filepath);
            HeroDeveloper developer = hero.HeroDeveloper;

            List<string> skippedItems = new();
            int attributesLoaded = 0;
            int skillsLoaded = 0;
            int perksLoaded = 0;

            // Step 1: Set Attributes
            foreach (AttributeData attrData in data.Attributes)
            {
                CharacterAttribute attribute = ResolveAttribute(attrData.AttributeId);
                if (attribute == null)
                {
                    skippedItems.Add($"Attribute: {attrData.AttributeId}");
                    continue;
                }

                AttributeEditor.SetAttribute(hero, attribute, attrData.Value);
                attributesLoaded++;
            }

            // Step 2: Set Focus Points
            foreach (SkillData skillData in data.Skills)
            {
                SkillObject skill = MBObjectManager.Instance.GetObject<SkillObject>(skillData.SkillId);
                if (skill == null)
                {
                    // Will be counted in step 3 as well, skip here silently
                    continue;
                }

                FocusEditor.SetFocus(hero, skill, skillData.Focus);
            }

            // Step 3: Set Skill Levels
            foreach (SkillData skillData in data.Skills)
            {
                SkillObject skill = MBObjectManager.Instance.GetObject<SkillObject>(skillData.SkillId);
                if (skill == null)
                {
                    skippedItems.Add($"Skill: {skillData.SkillId}");
                    continue;
                }

                developer.SetInitialSkillLevel(skill, skillData.Level);
                skillsLoaded++;
            }

            // Step 4: Clear All Perks (with permanent bonus handling)
            PerkEditor.ClearAllPerks(hero);

            // Step 5: Re-Select Saved Perks
            foreach (PerkData perkData in data.Perks)
            {
                if (!perkData.IsSelected)
                    continue;

                PerkObject perk = MBObjectManager.Instance.GetObject<PerkObject>(perkData.PerkId);
                if (perk == null)
                {
                    skippedItems.Add($"Perk: {perkData.PerkId}");
                    continue;
                }

                developer.AddPerk(perk);
                perksLoaded++;
            }

            // Step 6: Set Unspent Points
            developer.UnspentFocusPoints = data.UnspentFocusPoints;
            developer.UnspentAttributePoints = data.UnspentAttributePoints;

            // Step 7: RecalculateLevel from loaded skills
            HeroSkillEditor editor = new(hero);
            BLGMResult levelResult = editor.RecalculateLevel();

            StringBuilder resultMessage = new();
            resultMessage.Append($"Applied development to {hero.Name} from '{Path.GetFileNameWithoutExtension(filepath)}'. ");
            resultMessage.Append($"Level: {hero.Level}, ");
            resultMessage.Append($"Attributes: {attributesLoaded}, Skills: {skillsLoaded}, Perks: {perksLoaded}. ");
            resultMessage.Append($"Unspent Attr: {developer.UnspentAttributePoints}, Unspent Focus: {developer.UnspentFocusPoints}.");

            if (skippedItems.Count > 0)
            {
                resultMessage.Append($"\nSkipped {skippedItems.Count} item(s): ");
                resultMessage.Append(string.Join(", ", skippedItems));
            }

            return BLGMResult.Success(resultMessage.ToString());
        }

        /// <summary>
        /// Checks if a development file exists.
        /// </summary>
        /// <param name="filename">The filename to check</param>
        /// <returns>True if the file exists</returns>
        public bool DevelopmentFileExists(string filename)
        {
            string filepath = GetDevelopmentFilePath(filename);
            return File.Exists(filepath);
        }

        /// <summary>
        /// Lists all development files in the directory.
        /// </summary>
        /// <returns>Array of file names without paths</returns>
        public string[] ListDevelopmentFiles()
        {
            string directory = GetDevelopmentDirectory();

            if (!Directory.Exists(directory))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(directory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
        }

        #endregion

        #region Resolution Helpers

        /// <summary>
        /// Resolves a CharacterAttribute by StringId from the live Attributes.All collection.
        /// </summary>
        /// <param name="attributeId">The StringId of the attribute to resolve</param>
        /// <returns>The resolved CharacterAttribute, or null if not found</returns>
        private static CharacterAttribute ResolveAttribute(string attributeId)
        {
            foreach (CharacterAttribute attribute in Attributes.All)
            {
                if (attribute.StringId == attributeId)
                    return attribute;
            }

            return null;
        }

        #endregion
    }
}
