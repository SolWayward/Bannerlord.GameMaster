using System;
using System.Collections.Generic;
using System.Reflection;
using Bannerlord.GameMaster.Common;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Banners
{
    /// <summary>
    /// Temporarily unlocks all 229 banner palette colors for the banner editor by setting
    /// PlayerCanChooseForSigil and PlayerCanChooseForBackground to true on every BannerColor entry.
    /// Backs up the original palette for restoration when the editor closes.
    /// </summary>
    public static class BannerPaletteExpander
    {
        // Cached reflection for BannerManager._colorPalette (private field)
        private static readonly FieldInfo _colorPaletteField = typeof(BannerManager).GetField(
            "_colorPalette",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private static Dictionary<int, BannerColor> _originalPalette;
        private static bool _isExpanded;

        // MARK: ExpandAllColors
        /// <summary>
        /// Expands the banner color palette so all colors are available for both sigil and background selection.
        /// Must be called before constructing BannerEditorVM so RefreshValues() sees the expanded palette.
        /// </summary>
        public static BLGMResult ExpandAllColors()
        {
            if (_isExpanded)
                return BLGMResult.Success("Palette already expanded");

            if (BannerManager.Instance == null)
            {
                return BLGMResult.Error("ExpandAllColors() failed, BannerManager.Instance is null",
                    new InvalidOperationException("BannerManager.Instance is null")).Log();
            }

            if (_colorPaletteField == null)
            {
                return BLGMResult.Error("ExpandAllColors() failed, could not resolve BannerManager._colorPalette field via reflection",
                    new MissingFieldException(nameof(BannerManager), "_colorPalette")).Log();
            }

            try
            {
                Dictionary<int, BannerColor> currentPalette =
                    (Dictionary<int, BannerColor>)_colorPaletteField.GetValue(BannerManager.Instance);

                if (currentPalette == null)
                {
                    return BLGMResult.Error("ExpandAllColors() failed, _colorPalette is null",
                        new InvalidOperationException("_colorPalette is null")).Log();
                }

                // Deep-copy the original palette for restoration
                _originalPalette = new Dictionary<int, BannerColor>(currentPalette.Count);
                foreach (KeyValuePair<int, BannerColor> entry in currentPalette)
                {
                    _originalPalette[entry.Key] = entry.Value;
                }

                // Build expanded palette with all flags set to true
                Dictionary<int, BannerColor> expandedPalette = new(currentPalette.Count);
                foreach (KeyValuePair<int, BannerColor> entry in currentPalette)
                {
                    expandedPalette[entry.Key] = new BannerColor(
                        entry.Value.Color,
                        playerCanChooseForSigil: true,
                        playerCanChooseForBackground: true);
                }

                // Write expanded palette back to private field
                _colorPaletteField.SetValue(BannerManager.Instance, expandedPalette);

                // Update the public ReadOnlyColorPalette wrapper to point to the new dictionary
                BannerManager.Instance.ReadOnlyColorPalette = expandedPalette.GetReadOnlyDictionary();

                _isExpanded = true;
                return BLGMResult.Success($"Expanded {expandedPalette.Count} banner colors for editor").Log();
            }
            catch (Exception ex)
            {
                return BLGMResult.Error($"ExpandAllColors() failed unexpectedly: {ex.Message}", ex).Log();
            }
        }

        // MARK: RestoreOriginalColors
        /// <summary>
        /// Restores the original banner color palette after the editor closes.
        /// Prevents the expanded palette from leaking into other game systems.
        /// </summary>
        public static BLGMResult RestoreOriginalColors()
        {
            if (!_isExpanded || _originalPalette == null)
                return BLGMResult.Success("Palette was not expanded, nothing to restore");

            if (BannerManager.Instance == null)
            {
                _originalPalette = null;
                _isExpanded = false;
                return BLGMResult.Error("RestoreOriginalColors() failed, BannerManager.Instance is null",
                    new InvalidOperationException("BannerManager.Instance is null")).Log();
            }

            try
            {
                // Write the original palette back to the private field
                _colorPaletteField.SetValue(BannerManager.Instance, _originalPalette);

                // Restore the ReadOnlyColorPalette wrapper
                BannerManager.Instance.ReadOnlyColorPalette = _originalPalette.GetReadOnlyDictionary();

                int restoredCount = _originalPalette.Count;
                _originalPalette = null;
                _isExpanded = false;
                return BLGMResult.Success($"Restored {restoredCount} original banner colors").Log();
            }
            catch (Exception ex)
            {
                _originalPalette = null;
                _isExpanded = false;
                return BLGMResult.Error($"RestoreOriginalColors() failed unexpectedly: {ex.Message}", ex).Log();
            }
        }
    }
}
