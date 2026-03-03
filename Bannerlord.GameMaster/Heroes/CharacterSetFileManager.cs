using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.ItemCommands;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Heroes.HeroDevelopment;
using Bannerlord.GameMaster.Information;
using Bannerlord.GameMaster.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Manages character set file I/O operations for saving and loading full hero character data.
    /// Aggregates appearance, development, traits, and equipment into a single unified JSON file.
    /// Supports configurable mod folder names for use by different mods.
    /// </summary>
    public class CharacterSetFileManager
    {
        private const string BaseFolder = "Mount and Blade II Bannerlord";
        private const string ConfigFolder = "Configs";
        private const string CharacterSetsFolder = "CharacterSets";

        /// <summary>
        /// The mod-specific folder name used in the configuration path.
        /// </summary>
        public string ModFolder { get; }

        /// <summary>
        /// Default instance using "GameMaster" folder for backwards compatibility.
        /// </summary>
        public static CharacterSetFileManager Default { get; } = new("GameMaster");

        /// <summary>
        /// Creates a new CharacterSetFileManager with the specified mod folder name.
        /// </summary>
        /// <param name="modFolder">The mod folder name to use in the configuration path.</param>
        public CharacterSetFileManager(string modFolder)
        {
            if (string.IsNullOrWhiteSpace(modFolder))
                throw new ArgumentException("Mod folder cannot be null or empty.", nameof(modFolder));
            ModFolder = modFolder;
        }

        #region File Path Operations

        /// <summary>
        /// Gets the full file path for character set files.
        /// </summary>
        /// <param name="filename">The filename without extension</param>
        /// <returns>The full file path</returns>
        public string GetCharacterSetFilePath(string filename)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string basePath = Path.Combine(documentsPath, BaseFolder, ConfigFolder, ModFolder, CharacterSetsFolder);

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
        /// Gets the character sets directory path.
        /// </summary>
        /// <returns>The directory path</returns>
        public string GetCharacterSetDirectory()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documentsPath, BaseFolder, ConfigFolder, ModFolder, CharacterSetsFolder);
        }

        /// <summary>
        /// Checks if a character set file exists.
        /// </summary>
        /// <param name="filename">The filename to check</param>
        /// <returns>True if the file exists</returns>
        public bool CharacterSetFileExists(string filename)
        {
            string filepath = GetCharacterSetFilePath(filename);
            return File.Exists(filepath);
        }

        /// <summary>
        /// Lists all character set files in the directory.
        /// </summary>
        /// <returns>Array of file names without paths</returns>
        public string[] ListCharacterSetFiles()
        {
            string directory = GetCharacterSetDirectory();

            if (!Directory.Exists(directory))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(directory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
        }

        #endregion

        #region Save Operations

        /// MARK: SaveCharacterSetToFile
        /// <summary>
        /// Saves a hero's full character data to a single JSON file.
        /// Aggregates metadata, appearance, development, traits, and both equipment sets.
        /// </summary>
        /// <param name="hero">The hero whose character data is being saved</param>
        /// <param name="filepath">The full file path to save to</param>
        public void SaveCharacterSetToFile(Hero hero, string filepath)
        {
            CharacterSetData data = new()
            {
                // Hero metadata
                HeroName = hero.Name?.ToString() ?? "",
                HeroStringId = hero.StringId,
                HeroMBGUID = hero.Id.ToString(),
                IsFemale = hero.IsFemale,
                HeroType = ResolveHeroType(hero),
                Level = hero.Level,
                Age = (int)hero.Age,
                Culture = hero.Culture?.StringId ?? "",
                ClanName = hero.Clan?.Name?.ToString() ?? "",
                ClanStringId = hero.Clan?.StringId ?? "",
                SavedDate = DateTime.UtcNow.ToString("o"),
                BLGMVersion = GameEnvironment.BLGMVersion.ToString(),

                // Embedded data sections
                Appearance = CaptureAppearance(hero),
                Development = CaptureDevelopment(hero),
                Traits = CaptureTraits(hero),
                BattleEquipment = CaptureEquipment(hero, false),
                CivilianEquipment = CaptureEquipment(hero, true)
            };

            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filepath, jsonString);
        }

        #endregion

        #region Load Operations

        /// <summary>
        /// Loads character set data from a JSON file without applying to a hero.
        /// </summary>
        /// <param name="filepath">The full file path to load from</param>
        /// <returns>The deserialized character set data</returns>
        public CharacterSetData LoadCharacterSetData(string filepath)
        {
            string jsonString = File.ReadAllText(filepath);
            CharacterSetData data = JsonConvert.DeserializeObject<CharacterSetData>(jsonString);

            if (data == null)
            {
                throw new InvalidDataException("Invalid character set file format.");
            }

            return data;
        }

        /// MARK: LoadCharacterSetToHero
        /// <summary>
        /// Loads saved character data from a JSON file and applies ALL sections to an existing hero.
        /// Applies: age, culture, appearance, development, traits, battle equipment, civilian equipment.
        /// Does NOT apply: name, gender, stringId, MBGUID, type (occupation).
        /// </summary>
        /// <param name="hero">The hero to apply data to</param>
        /// <param name="filepath">The full file path to load from</param>
        /// <returns>BLGMResult with summary of applied/skipped items</returns>
        public BLGMResult LoadCharacterSetToHero(Hero hero, string filepath)
        {
            return LoadCharacterSetToHero(hero, filepath, CharacterSetApplyFlags.All);
        }

        /// MARK: LoadCharacterSetToHero (flags)
        /// <summary>
        /// Loads saved character data from a JSON file and selectively applies sections to an existing hero.
        /// Use <see cref="CharacterSetApplyFlags"/> to control which sections are applied.
        /// Does NOT apply: name, gender, stringId, MBGUID, type (occupation).
        /// </summary>
        /// <param name="hero">The hero to apply data to</param>
        /// <param name="filepath">The full file path to load from</param>
        /// <param name="flags">Flags controlling which sections to apply</param>
        /// <returns>BLGMResult with summary of applied/skipped items</returns>
        public BLGMResult LoadCharacterSetToHero(Hero hero, string filepath, CharacterSetApplyFlags flags)
        {
            if (hero == null)
            {
                return BLGMResult.Error("LoadCharacterSetToHero() failed, hero cannot be null",
                    new ArgumentNullException(nameof(hero))).Log();
            }

            CharacterSetData data = LoadCharacterSetData(filepath);
            StringBuilder resultMessage = new();
            string filename = Path.GetFileNameWithoutExtension(filepath);

            // Apply age
            if (flags.HasFlag(CharacterSetApplyFlags.Age) && data.Age >= 18)
            {
                hero.SetAge(data.Age);
                resultMessage.Append($"Age: {data.Age}. ");
            }

            // Apply culture
            if (flags.HasFlag(CharacterSetApplyFlags.Culture) && !string.IsNullOrEmpty(data.Culture))
            {
                CultureObject culture = MBObjectManager.Instance.GetObject<CultureObject>(data.Culture);
                if (culture != null)
                {
                    hero.Culture = culture;
                    resultMessage.Append($"Culture: {culture.Name}. ");
                }

                else
                {
                    resultMessage.Append($"Culture '{data.Culture}' not found (skipped). ");
                }
            }

            // Apply appearance
            if (flags.HasFlag(CharacterSetApplyFlags.Appearance) && data.Appearance != null)
            {
                BLGMResult appearanceResult = ApplyAppearance(hero, data.Appearance);
                resultMessage.Append($"Appearance: {(appearanceResult.IsSuccess ? "Applied" : "Failed")}. ");
            }

            // Apply development
            if (flags.HasFlag(CharacterSetApplyFlags.Development) && data.Development != null)
            {
                BLGMResult devResult = ApplyDevelopment(hero, data.Development);
                resultMessage.Append($"Development: {(devResult.IsSuccess ? "Applied" : "Failed")}. ");
            }

            // Apply traits
            if (flags.HasFlag(CharacterSetApplyFlags.Traits) && data.Traits != null)
            {
                BLGMResult traitResult = ApplyTraits(hero, data.Traits);
                resultMessage.Append($"Traits: {(traitResult.IsSuccess ? "Applied" : "Failed")}. ");
            }

            // Apply battle equipment
            if (flags.HasFlag(CharacterSetApplyFlags.BattleEquipment) && data.BattleEquipment != null)
            {
                BLGMResult battleResult = ApplyEquipment(hero, data.BattleEquipment, false);
                resultMessage.Append($"Battle Equipment: {(battleResult.IsSuccess ? "Applied" : "Failed")}. ");
            }

            // Apply civilian equipment
            if (flags.HasFlag(CharacterSetApplyFlags.CivilianEquipment) && data.CivilianEquipment != null)
            {
                BLGMResult civilianResult = ApplyEquipment(hero, data.CivilianEquipment, true);
                resultMessage.Append($"Civilian Equipment: {(civilianResult.IsSuccess ? "Applied" : "Failed")}. ");
            }

            return BLGMResult.Success(
                $"Loaded character set '{filename}' to {hero.Name}.\n{resultMessage}");
        }

        /// MARK: ImportCharacterSet
        /// <summary>
        /// Creates a brand new hero from saved character data.
        /// Uses HeroGenerator to create and initialize the hero, then overrides with saved data.
        /// </summary>
        /// <param name="filepath">Full path to the character set JSON file</param>
        /// <param name="clan">Target clan for the new hero (required)</param>
        /// <param name="typeOverride">Override hero type from file. If null, uses saved HeroType. Valid: "lord", "wanderer", "companion"</param>
        /// <param name="settlement">Settlement for placement. If null, auto-resolved from clan</param>
        /// <param name="withParty">For Lords only: create a party. Defaults to true</param>
        /// <returns>BLGMResult with the created hero summary</returns>
        public BLGMResult ImportCharacterSet(string filepath, Clan clan, string typeOverride = null, Settlement settlement = null, bool withParty = true)
        {
            if (clan == null)
            {
                return BLGMResult.Error("ImportCharacterSet() failed, clan cannot be null",
                    new ArgumentNullException(nameof(clan))).Log();
            }

            CharacterSetData data = LoadCharacterSetData(filepath);
            string filename = Path.GetFileNameWithoutExtension(filepath);

            // Resolve hero type
            string heroType = !string.IsNullOrEmpty(typeOverride) ? typeOverride.ToLower() : (data.HeroType?.ToLower() ?? "lord");

            // Resolve culture flags from saved culture
            CultureFlags cultureFlags = CultureFlags.AllMainCultures;
            if (!string.IsNullOrEmpty(data.Culture))
            {
                CultureFlags parsedFlags = Console.Common.Parsing.FlagParser.ParseCultureArgument(data.Culture);
                if (parsedFlags != CultureFlags.None)
                {
                    cultureFlags = parsedFlags;
                }
            }

            // Resolve gender flags from saved gender
            GenderFlags genderFlags = data.IsFemale ? GenderFlags.Female : GenderFlags.Male;

            // Select template
            CharacterTemplatePooler templatePooler = new();
            List<CharacterObject> characterPool = templatePooler.GetAllHeroTemplatesFromFlags(cultureFlags, genderFlags);
            if (characterPool == null || characterPool.Count == 0)
            {
                // Fallback to all main cultures if specific culture has no templates
                characterPool = templatePooler.GetAllHeroTemplatesFromFlags(CultureFlags.AllMainCultures, genderFlags);
            }

            if (characterPool == null || characterPool.Count == 0)
            {
                return BLGMResult.Error($"ImportCharacterSet() failed, no character templates found for culture '{data.Culture}' and gender '{(data.IsFemale ? "female" : "male")}'").Log();
            }

            int randomIndex = RandomNumberGen.Instance.NextRandomInt(characterPool.Count);
            CharacterObject template = CharacterObject.CreateFrom(characterPool[randomIndex]);

            // Create basic hero with saved identity
            TextObject nameObj = new(data.HeroName ?? "Imported Hero");
            int age = data.Age >= 18 ? data.Age : -1;

            Hero hero = HeroGenerator.CreateBasicHero(template, nameObj, age, clan, randomFactor: 0);

            // Initialize role with saved level
            int level = data.Level > 0 ? data.Level : -1;
            Settlement targetSettlement = settlement ?? hero.GetHomeOrAlternativeSettlement();

            switch (heroType)
            {
                case "lord":
                    HeroGenerator.InitializeAsLord(hero, targetSettlement, withParty, level);
                    break;

                case "wanderer":
                    hero.Clan = null;
                    HeroGenerator.InitializeAsWanderer(hero, targetSettlement, level);
                    break;

                case "companion":
                    HeroGenerator.InitializeAsCompanion(hero, level);
                    break;

                default:
                    HeroGenerator.InitializeAsLord(hero, targetSettlement, withParty, level);
                    break;
            }

            // Override with saved character data
            StringBuilder applyResults = new();

            if (data.Appearance != null)
            {
                BLGMResult appearanceResult = ApplyAppearance(hero, data.Appearance);
                if (!appearanceResult.IsSuccess)
                    applyResults.Append($"Appearance: {appearanceResult.Message}. ");
            }

            if (data.Development != null)
            {
                BLGMResult devResult = ApplyDevelopment(hero, data.Development);
                if (!devResult.IsSuccess)
                    applyResults.Append($"Development: {devResult.Message}. ");
            }

            if (data.Traits != null)
            {
                BLGMResult traitResult = ApplyTraits(hero, data.Traits);
                if (!traitResult.IsSuccess)
                    applyResults.Append($"Traits: {traitResult.Message}. ");
            }

            if (data.BattleEquipment != null)
            {
                BLGMResult battleResult = ApplyEquipment(hero, data.BattleEquipment, false);
                if (!battleResult.IsSuccess)
                    applyResults.Append($"Battle Equipment: {battleResult.Message}. ");
            }

            if (data.CivilianEquipment != null)
            {
                BLGMResult civilianResult = ApplyEquipment(hero, data.CivilianEquipment, true);
                if (!civilianResult.IsSuccess)
                    applyResults.Append($"Civilian Equipment: {civilianResult.Message}. ");
            }

            string typeDisplay = heroType.Substring(0, 1).ToUpper() + heroType.Substring(1);
            string clanInfo = hero.Clan != null ? $" in clan {hero.Clan.Name}" : "";
            string partyInfo = heroType == "lord" && withParty ? " with party" : "";
            string warnings = applyResults.Length > 0 ? $"\nWarnings: {applyResults}" : "";

            return BLGMResult.Success(
                $"Imported '{data.HeroName}' from '{filename}' as {typeDisplay}{clanInfo}{partyInfo}.\n" +
                $"Hero: {hero.Name} (ID: {hero.StringId}), Level: {hero.Level}, Age: {(int)hero.Age}" +
                warnings);
        }

        #endregion

        #region Capture Methods

        /// MARK: CaptureAppearance
        /// <summary>
        /// Captures a hero's appearance data into an AppearanceSetData object.
        /// Mirrors the field-capture logic from AppearanceFileManager.SaveAppearanceToFile().
        /// </summary>
        private AppearanceSetData CaptureAppearance(Hero hero)
        {
            StaticBodyProperties staticProps = hero.BodyProperties.StaticProperties;

            return new AppearanceSetData
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
        }

        /// MARK: CaptureDevelopment
        /// <summary>
        /// Captures a hero's development data into a DevelopmentSetData object.
        /// Mirrors the field-capture logic from DevelopmentFileManager.SaveDevelopmentToFile().
        /// </summary>
        private DevelopmentSetData CaptureDevelopment(Hero hero)
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

            // Capture attributes
            foreach (CharacterAttribute attribute in Attributes.All)
            {
                data.Attributes.Add(new AttributeData
                {
                    AttributeId = attribute.StringId,
                    Value = hero.GetAttributeValue(attribute)
                });
            }

            // Capture skills with XP and focus
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

            // Capture perk selections
            foreach (PerkObject perk in PerkObject.All)
            {
                data.Perks.Add(new PerkData
                {
                    PerkId = perk.StringId,
                    IsSelected = hero.GetPerkValue(perk)
                });
            }

            return data;
        }

        /// MARK: CaptureTraits
        /// <summary>
        /// Captures a hero's trait data into a TraitSetData object.
        /// Mirrors the field-capture logic from TraitFileManager.SaveTraitsToFile().
        /// </summary>
        private TraitSetData CaptureTraits(Hero hero)
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

            return data;
        }

        /// MARK: CaptureEquipment
        /// <summary>
        /// Captures a hero's equipment data into an EquipmentSetData object.
        /// Mirrors the field-capture logic from EquipmentFileManager.SaveEquipmentToFile().
        /// </summary>
        /// <param name="hero">The hero whose equipment is being captured</param>
        /// <param name="isCivilian">True to capture civilian equipment, false for battle</param>
        private EquipmentSetData CaptureEquipment(Hero hero, bool isCivilian)
        {
            Equipment equipment = isCivilian ? hero.CivilianEquipment : hero.BattleEquipment;

            EquipmentSetData data = new()
            {
                HeroName = hero.Name?.ToString() ?? "",
                HeroId = hero.StringId,
                SavedDate = DateTime.UtcNow.ToString("o"),
                IsCivilian = isCivilian,
                Equipment = new()
            };

            if (equipment == null)
                return data;

            for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
            {
                EquipmentIndex slot = (EquipmentIndex)i;
                EquipmentElement element = equipment[slot];

                if (!element.IsEmpty)
                {
                    data.Equipment.Add(new EquipmentSlotData
                    {
                        Slot = slot.ToString(),
                        ItemId = element.Item.StringId,
                        ModifierId = element.ItemModifier?.StringId
                    });
                }
            }

            return data;
        }

        #endregion

        #region Apply Methods

        /// MARK: ApplyAppearance
        /// <summary>
        /// Applies appearance data to a hero.
        /// Mirrors the application logic from AppearanceFileManager.LoadAppearanceToHero().
        /// Gender mismatch is allowed but warned about during character set operations.
        /// </summary>
        public BLGMResult ApplyAppearance(Hero hero, AppearanceSetData data)
        {
            if (string.IsNullOrEmpty(data.BodyPropertiesString))
            {
                return BLGMResult.Error("ApplyAppearance() failed, BodyPropertiesString is empty").Log();
            }

            if (!BodyProperties.FromString(data.BodyPropertiesString, out BodyProperties loadedProps))
            {
                return BLGMResult.Error("ApplyAppearance() failed to parse BodyPropertiesString",
                    new InvalidOperationException("BodyProperties.FromString failed")).Log();
            }

            // Apply StaticBodyProperties (facial features, hair, tattoos, skin color, height)
            hero.StaticBodyProperties = loadedProps.StaticProperties;

            // Apply Weight and Build from DynamicBodyProperties
            hero.Weight = loadedProps.DynamicProperties.Weight;
            hero.Build = loadedProps.DynamicProperties.Build;

            string warningText = "";
            if (data.IsFemale != hero.IsFemale)
            {
                string savedGender = data.IsFemale ? "female" : "male";
                string heroGender = hero.IsFemale ? "female" : "male";
                warningText = $" (Gender mismatch: saved={savedGender}, hero={heroGender})";
            }

            return BLGMResult.Success($"Applied appearance{warningText}");
        }

        /// MARK: ApplyDevelopment
        /// <summary>
        /// Applies development data to a hero using the 7-step pattern from DevelopmentFileManager.LoadDevelopmentToHero().
        /// 1. Set Attributes, 2. Set Focus, 3. Set Skill Levels,
        /// 4. Clear All Perks, 5. Re-Select Saved Perks, 6. Set Unspent Points, 7. RecalculateLevel.
        /// </summary>
        public BLGMResult ApplyDevelopment(Hero hero, DevelopmentSetData data)
        {
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
                    continue;

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

            // Step 4: Clear All Perks
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

            // Step 7: RecalculateLevel
            HeroSkillEditor editor = new(hero);
            editor.RecalculateLevel();

            StringBuilder resultMessage = new();
            resultMessage.Append($"Attributes: {attributesLoaded}, Skills: {skillsLoaded}, Perks: {perksLoaded}");

            if (skippedItems.Count > 0)
            {
                resultMessage.Append($" (Skipped {skippedItems.Count}: {string.Join(", ", skippedItems)})");
            }

            return BLGMResult.Success(resultMessage.ToString());
        }

        /// MARK: ApplyTraits
        /// <summary>
        /// Applies trait data to a hero.
        /// Mirrors the application logic from TraitFileManager.LoadTraitsToHero().
        /// </summary>
        public BLGMResult ApplyTraits(Hero hero, TraitSetData data)
        {
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
            resultMessage.Append($"{traitsLoaded} trait(s) applied");

            if (skippedTraits.Count > 0)
            {
                resultMessage.Append($" (Skipped {skippedTraits.Count}: {string.Join(", ", skippedTraits)})");
            }

            return BLGMResult.Success(resultMessage.ToString());
        }

        /// MARK: ApplyEquipment
        /// <summary>
        /// Applies equipment data to a hero.
        /// Mirrors the application logic from EquipmentFileManager.LoadEquipmentFromFile().
        /// </summary>
        /// <param name="hero">The hero to apply equipment to</param>
        /// <param name="data">The equipment set data to apply</param>
        /// <param name="isCivilian">True to apply to civilian equipment, false for battle</param>
        public BLGMResult ApplyEquipment(Hero hero, EquipmentSetData data, bool isCivilian)
        {
            Equipment equipment = isCivilian ? hero.CivilianEquipment : hero.BattleEquipment;

            if (equipment == null)
            {
                return BLGMResult.Error($"ApplyEquipment() failed, hero {(isCivilian ? "civilian" : "battle")} equipment is null").Log();
            }

            int loadedCount = 0;
            int skippedCount = 0;
            List<string> skippedItems = new();

            // Clear existing equipment
            for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
            {
                equipment[(EquipmentIndex)i] = EquipmentElement.Invalid;
            }

            // Load each equipment slot
            if (data.Equipment != null)
            {
                foreach (EquipmentSlotData slotData in data.Equipment)
                {
                    if (!Enum.TryParse<EquipmentIndex>(slotData.Slot, out EquipmentIndex slot))
                        continue;

                    // Find the item
                    ItemObject item = ItemQueries.QueryItems(slotData.ItemId)
                        .FirstOrDefault(i => i.StringId == slotData.ItemId);

                    if (item == null)
                    {
                        skippedCount++;
                        skippedItems.Add($"{slot}: {slotData.ItemId}");
                        continue;
                    }

                    // Try to find modifier
                    ItemModifier modifier = null;
                    if (!string.IsNullOrEmpty(slotData.ModifierId))
                    {
                        modifier = ItemModifierHelper.GetModifierByStringId(slotData.ModifierId);
                        if (modifier == null)
                        {
                            (ItemModifier parsedModifier, string _) = ItemModifierHelper.ParseModifier(slotData.ModifierId);
                            modifier = parsedModifier;
                        }
                    }

                    equipment[slot] = new EquipmentElement(item, modifier);
                    loadedCount++;
                }
            }

            string equipType = isCivilian ? "Civilian" : "Battle";

            if (skippedCount > 0)
            {
                return BLGMResult.Success($"{equipType}: {loadedCount} loaded, {skippedCount} skipped ({string.Join(", ", skippedItems)})");
            }

            return BLGMResult.Success($"{equipType}: {loadedCount} item(s) loaded");
        }

        #endregion

        #region Resolution Helpers

        /// <summary>
        /// Resolves the hero type string from the hero's occupation.
        /// </summary>
        private string ResolveHeroType(Hero hero)
        {
            if (hero.IsLord)
                return "Lord";

            if (hero.IsWanderer)
                return "Wanderer";

            if (hero.CompanionOf != null)
                return "Companion";

            // Fallback based on occupation
            if (hero.CharacterObject?.Occupation == Occupation.Lord)
                return "Lord";

            if (hero.CharacterObject?.Occupation == Occupation.Wanderer)
                return "Wanderer";

            return "Lord";
        }

        /// <summary>
        /// Resolves a CharacterAttribute by StringId from the live Attributes.All collection.
        /// </summary>
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
