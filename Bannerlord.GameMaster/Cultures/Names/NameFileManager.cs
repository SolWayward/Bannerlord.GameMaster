using Bannerlord.GameMaster.Common;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Bannerlord.GameMaster.Cultures.Names
{
    /// <summary>
    /// Manages JSON file I/O for custom name override files.
    /// Path: Documents/Mount and Blade II Bannerlord/Configs/GameMaster/Names/{cultureId}.json
    /// </summary>
    internal static class NameFileManager
    {
        private const string BaseFolder = "Mount and Blade II Bannerlord";
        private const string ConfigFolder = "Configs";
        private const string ModFolder = "GameMaster";
        private const string NamesFolder = "Names";

        /// <summary>
        /// Gets the base Names directory path
        /// </summary>
        internal static string GetNamesBaseDirectory()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documentsPath, BaseFolder, ConfigFolder, ModFolder, NamesFolder);
        }

        /// <summary>
        /// Gets the full file path for a culture's name override JSON file
        /// </summary>
        internal static string GetFilePath(string cultureId)
        {
            return Path.Combine(GetNamesBaseDirectory(), $"{cultureId}.json");
        }

        /// <summary>
        /// Checks if an override JSON file exists for the specified culture
        /// </summary>
        internal static bool FileExists(string cultureId)
        {
            return File.Exists(GetFilePath(cultureId));
        }

        /// <summary>
        /// Loads and deserializes culture name data from a JSON file.
        /// Returns null if file does not exist.
        /// Falls back to null on I/O or deserialization errors (logged via BLGMResult).
        /// </summary>
        internal static CultureNameData LoadCultureNames(string cultureId)
        {
            string filepath = GetFilePath(cultureId);

            if (!File.Exists(filepath))
                return null;

            try
            {
                string jsonString = File.ReadAllText(filepath);
                CultureNameData data = JsonConvert.DeserializeObject<CultureNameData>(jsonString);
                return data;
            }
            catch (Exception ex)
            {
                BLGMResult.Error(
                    $"LoadCultureNames() failed to read name overrides for '{cultureId}' from {filepath}",
                    ex).Log();
                return null;
            }
        }

        /// <summary>
        /// Counts how many override JSON files exist in the Names directory
        /// </summary>
        internal static int CountOverrideFiles()
        {
            string baseDir = GetNamesBaseDirectory();

            if (!Directory.Exists(baseDir))
                return 0;

            return Directory.GetFiles(baseDir, "*.json").Length;
        }
    }
}
