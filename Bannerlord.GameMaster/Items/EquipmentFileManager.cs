using Bannerlord.GameMaster.Console.ItemCommands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Items
{
    /// <summary>
    /// Manages equipment file I/O operations for saving and loading hero equipment sets.
    /// Supports configurable mod folder names for use by different mods.
    /// </summary>
    public class EquipmentFileManager
    {
        private const string BaseFolder = "Mount and Blade II Bannerlord";
        private const string ConfigFolder = "Configs";
        private const string HeroSetsFolder = "HeroSets";
        private const string CivilianFolder = "civilian";

        /// <summary>
        /// The mod-specific folder name used in the configuration path.
        /// </summary>
        public string ModFolder { get; }

        /// <summary>
        /// Default instance using "GameMaster" folder for backwards compatibility.
        /// </summary>
        public static EquipmentFileManager Default { get; } = new("GameMaster");

        /// <summary>
        /// Creates a new EquipmentFileManager with the specified mod folder name.
        /// </summary>
        /// <param name="modFolder">The mod folder name to use in the configuration path.</param>
        /// <exception cref="ArgumentException">Thrown when modFolder is null or whitespace.</exception>
        public EquipmentFileManager(string modFolder)
        {
            if (string.IsNullOrWhiteSpace(modFolder))
                throw new ArgumentException("Mod folder cannot be null or empty.", nameof(modFolder));
            ModFolder = modFolder;
        }

        #region File Path Operations

        /// <summary>
        /// Gets the full file path for equipment files
        /// </summary>
        /// <param name="filename">The filename without extension</param>
        /// <param name="isCivilian">Whether this is civilian equipment</param>
        /// <returns>The full file path</returns>
        public string GetEquipmentFilePath(string filename, bool isCivilian)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string basePath = Path.Combine(documentsPath, BaseFolder, ConfigFolder, ModFolder, HeroSetsFolder);

            if (isCivilian)
            {
                basePath = Path.Combine(basePath, CivilianFolder);
            }

            // Ensure directory exists
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            // Add .json extension if not present
            if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".json";
            }

            return Path.Combine(basePath, filename);
        }

        /// <summary>
        /// Gets the equipment sets directory path
        /// </summary>
        /// <param name="isCivilian">Whether to get the civilian equipment folder</param>
        /// <returns>The directory path</returns>
        public string GetEquipmentDirectory(bool isCivilian)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string basePath = Path.Combine(documentsPath, BaseFolder, ConfigFolder, ModFolder, HeroSetsFolder);

            if (isCivilian)
            {
                basePath = Path.Combine(basePath, CivilianFolder);
            }

            return basePath;
        }

        #endregion

        #region Save Operations

        /// <summary>
        /// Saves equipment to a JSON file
        /// </summary>
        /// <param name="hero">The hero whose equipment is being saved</param>
        /// <param name="equipment">The equipment set to save</param>
        /// <param name="filepath">The full file path to save to</param>
        /// <param name="isCivilian">Whether this is civilian equipment</param>
        public void SaveEquipmentToFile(Hero hero, Equipment equipment, string filepath, bool isCivilian)
        {
            EquipmentSetData equipmentData = new()
            {
                HeroName = hero.Name?.ToString() ?? "",
                HeroId = hero.StringId,
                SavedDate = DateTime.UtcNow.ToString("o"),
                IsCivilian = isCivilian,
                Equipment = new()
            };

            // Save each equipment slot (only non-empty slots)
            for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
            {
                EquipmentIndex slot = (EquipmentIndex)i;
                EquipmentElement element = equipment[slot];

                if (!element.IsEmpty)
                {
                    equipmentData.Equipment.Add(new EquipmentSlotData
                    {
                        Slot = slot.ToString(),
                        ItemId = element.Item.StringId,
                        ModifierId = element.ItemModifier?.StringId
                    });
                }
            }

            // Serialize to JSON with indentation
            string jsonString = JsonConvert.SerializeObject(equipmentData, Formatting.Indented);

            // Write to file
            File.WriteAllText(filepath, jsonString);
        }

        /// <summary>
        /// Saves both battle and civilian equipment to separate files
        /// </summary>
        /// <param name="hero">The hero whose equipment is being saved</param>
        /// <param name="filename">The base filename (without extension)</param>
        /// <param name="battleEquipment">The battle equipment set</param>
        /// <param name="civilianEquipment">The civilian equipment set</param>
        /// <returns>Tuple with battle and civilian file paths</returns>
        public (string battlePath, string civilianPath) SaveBothEquipmentSets(
            Hero hero,
            string filename,
            Equipment battleEquipment,
            Equipment civilianEquipment)
        {
            string battlePath = GetEquipmentFilePath(filename, false);
            SaveEquipmentToFile(hero, battleEquipment, battlePath, false);

            string civilianPath = GetEquipmentFilePath(filename, true);
            SaveEquipmentToFile(hero, civilianEquipment, civilianPath, true);

            return (battlePath, civilianPath);
        }

        #endregion

        #region Load Operations

        /// <summary>
        /// Loads equipment data from a JSON file without applying to a hero
        /// </summary>
        /// <param name="filepath">The full file path to load from</param>
        /// <returns>The deserialized equipment set data</returns>
        public EquipmentSetData LoadEquipmentData(string filepath)
        {
            string jsonString = File.ReadAllText(filepath);
            EquipmentSetData equipmentData = JsonConvert.DeserializeObject<EquipmentSetData>(jsonString);

            if (equipmentData == null || equipmentData.Equipment == null)
            {
                throw new InvalidDataException("Invalid equipment file format.");
            }

            return equipmentData;
        }

        /// <summary>
        /// Loads equipment from a JSON file and applies it to a hero
        /// </summary>
        /// <param name="hero">The hero to apply equipment to</param>
        /// <param name="filepath">The full file path to load from</param>
        /// <param name="isCivilian">Whether to load to civilian equipment</param>
        /// <returns>Tuple with (loadedCount, skippedCount, skippedItems)</returns>
        public (int loadedCount, int skippedCount, List<SkippedItemInfo> skippedItems) LoadEquipmentFromFile(
            Hero hero,
            string filepath,
            bool isCivilian)
        {
            EquipmentSetData equipmentData = LoadEquipmentData(filepath);

            Equipment equipment = isCivilian ? hero.CivilianEquipment : hero.BattleEquipment;

            int loadedCount = 0;
            int skippedCount = 0;
            List<SkippedItemInfo> skippedItems = new();

            // Clear existing equipment
            for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; i++)
            {
                equipment[(EquipmentIndex)i] = EquipmentElement.Invalid;
            }

            // Load each equipment slot
            foreach (EquipmentSlotData slotData in equipmentData.Equipment)
            {
                if (!Enum.TryParse<EquipmentIndex>(slotData.Slot, out EquipmentIndex slot))
                {
                    continue; // Skip invalid slot
                }

                // Find the item
                ItemObject item = ItemQueries.QueryItems(slotData.ItemId)
                    .FirstOrDefault(i => i.StringId == slotData.ItemId);

                if (item == null)
                {
                    skippedCount++;
                    string modifierInfo = !string.IsNullOrEmpty(slotData.ModifierId) 
                        ? $"(modifier: {slotData.ModifierId})" 
                        : "";
                    skippedItems.Add(new SkippedItemInfo
                    {
                        Slot = slot.ToString(),
                        ItemId = slotData.ItemId,
                        ModifierInfo = modifierInfo
                    });
                    continue; // Skip if item not found
                }

                // Try to find modifier by StringId first (for saved equipment), then by Name (for backwards compatibility)
                ItemModifier modifier = null;
                if (!string.IsNullOrEmpty(slotData.ModifierId))
                {
                    // Try StringId lookup first (exact match for saved equipment)
                    modifier = ItemModifierHelper.GetModifierByStringId(slotData.ModifierId);
                    
                    // Fall back to name lookup (for backwards compatibility or manual JSON edits)
                    if (modifier == null)
                    {
                        (ItemModifier parsedModifier, string _) = ItemModifierHelper.ParseModifier(slotData.ModifierId);
                        modifier = parsedModifier;
                    }
                }

                // Equip the item
                equipment[slot] = new EquipmentElement(item, modifier);
                loadedCount++;
            }

            return (loadedCount, skippedCount, skippedItems);
        }

        /// <summary>
        /// Checks if an equipment file exists
        /// </summary>
        /// <param name="filename">The filename to check</param>
        /// <param name="isCivilian">Whether to check civilian equipment folder</param>
        /// <returns>True if the file exists</returns>
        public bool EquipmentFileExists(string filename, bool isCivilian)
        {
            string filepath = GetEquipmentFilePath(filename, isCivilian);
            return File.Exists(filepath);
        }

        /// <summary>
        /// Lists all equipment files in the specified folder
        /// </summary>
        /// <param name="isCivilian">Whether to list civilian equipment files</param>
        /// <returns>Array of file names without paths</returns>
        public string[] ListEquipmentFiles(bool isCivilian)
        {
            string directory = GetEquipmentDirectory(isCivilian);

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
