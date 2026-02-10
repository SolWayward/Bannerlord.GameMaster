using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Banners
{
    /// <summary>
    /// Result of kingdom color selection containing the chosen color ID
    /// and the minimum perceptual distance to the nearest existing kingdom color.
    /// </summary>
    public struct KingdomColorResult
    {
        public int ColorId;
        public float MinDistanceToNearest;
    }

    /// <summary>
    /// Provides methods for selecting banner colors from the game's palette
    /// and finding complementary colors for banner design.
    /// </summary>
    public static class BannerColorPicker
    {
        private static readonly Random _random = new Random();

        /// MARK: GetRandomColorID
        /// <summary>
        /// Gets a random color ID from the banner palette.
        /// </summary>
        /// <returns>A valid color ID that exists in the palette.</returns>
        public static int GetRandomColorId()
        {
            MBReadOnlyDictionary<int, BannerColor> palette = BannerManager.Instance.ReadOnlyColorPalette;
            List<int> colorIds = palette.Keys.ToList();
            return colorIds[_random.Next(colorIds.Count)];
        }

        /// MARK: GetLighterColor
        /// <summary>
        /// Finds a lighter complementary color from the palette based on the given color ID.
        /// </summary>
        /// <param name="baseColorId">The base color ID to find a lighter complement for.</param>
        /// <param name="minLuminanceDifference">Minimum brightness difference (0.0 to 1.0). Default is 0.15.</param>
        /// <returns>A color ID for a lighter color, or the base color if no suitable lighter color exists.</returns>
        public static int GetLighterComplementaryColor(int baseColorId, float minLuminanceDifference = 0.15f)
        {
            MBReadOnlyDictionary<int, BannerColor> palette = BannerManager.Instance.ReadOnlyColorPalette;
            
            if (!palette.ContainsKey(baseColorId))
                return baseColorId;

            BannerColor baseColor = palette[baseColorId];
            float baseLuminance = CalculateLuminance(baseColor.Color);

            // Find colors that are lighter than the base color
            List<KeyValuePair<int, BannerColor>> lighterColors = palette
                .Where(kvp =>
                {
                    float luminance = CalculateLuminance(kvp.Value.Color);
                    return luminance > baseLuminance + minLuminanceDifference;
                })
                .ToList();

            if (!lighterColors.Any())
            {
                // If no lighter colors found with minimum difference, try without minimum
                lighterColors = palette
                    .Where(kvp => CalculateLuminance(kvp.Value.Color) > baseLuminance)
                    .ToList();
            }

            if (!lighterColors.Any())
                return baseColorId; // Return base color if no lighter colors exist

            // Find the color with the best complementary relationship
            return FindBestComplementaryColor(baseColor.Color, lighterColors, true);
        }

        /// MARK: GetDarkerColor
        /// <summary>
        /// Finds a darker complementary color from the palette based on the given color ID.
        /// </summary>
        /// <param name="baseColorId">The base color ID to find a darker complement for.</param>
        /// <param name="minLuminanceDifference">Minimum brightness difference (0.0 to 1.0). Default is 0.15.</param>
        /// <returns>A color ID for a darker color, or the base color if no suitable darker color exists.</returns>
        public static int GetDarkerComplementaryColor(int baseColorId, float minLuminanceDifference = 0.15f)
        {
            MBReadOnlyDictionary<int, BannerColor> palette = BannerManager.Instance.ReadOnlyColorPalette;
            
            if (!palette.ContainsKey(baseColorId))
                return baseColorId;

            BannerColor baseColor = palette[baseColorId];
            float baseLuminance = CalculateLuminance(baseColor.Color);

            // Find colors that are darker than the base color
            List<KeyValuePair<int, BannerColor>> darkerColors = palette
                .Where(kvp =>
                {
                    float luminance = CalculateLuminance(kvp.Value.Color);
                    return luminance < baseLuminance - minLuminanceDifference;
                })
                .ToList();

            if (!darkerColors.Any())
            {
                // If no darker colors found with minimum difference, try without minimum
                darkerColors = palette
                    .Where(kvp => CalculateLuminance(kvp.Value.Color) < baseLuminance)
                    .ToList();
            }

            if (!darkerColors.Any())
                return baseColorId; // Return base color if no darker colors exist

            // Find the color with the best complementary relationship
            return FindBestComplementaryColor(baseColor.Color, darkerColors, false);
        }

        /// MARK: GetContrastingColor
        /// <summary>
        /// Finds a contrasting color from the palette that provides maximum visibility against the base color.
        /// Prefers colors with high luminance difference and complementary hues for better emblem visibility.
        /// </summary>
        /// <param name="baseColorId">The base color ID to find a contrasting color for.</param>
        /// <param name="preferLighter">If true, returns a lighter color; if false, returns a darker color.</param>
        /// <param name="minLuminanceDifference">Minimum brightness difference (0.0 to 1.0). Default is 0.3 for stronger contrast.</param>
        /// <returns>A color ID for a contrasting color, or the base color if no suitable contrasting color exists.</returns>
        public static int GetContrastingColor(int baseColorId, bool preferLighter, float minLuminanceDifference = 0.3f)
        {
            MBReadOnlyDictionary<int, BannerColor> palette = BannerManager.Instance.ReadOnlyColorPalette;
            
            if (!palette.ContainsKey(baseColorId))
                return baseColorId;

            BannerColor baseColor = palette[baseColorId];
            float baseLuminance = CalculateLuminance(baseColor.Color);

            // Find colors with high luminance difference
            List<KeyValuePair<int, BannerColor>> contrastingColors = palette
                .Where(kvp =>
                {
                    float luminance = CalculateLuminance(kvp.Value.Color);
                    if (preferLighter)
                        return luminance > baseLuminance + minLuminanceDifference;
                    else
                        return luminance < baseLuminance - minLuminanceDifference;
                })
                .ToList();

            if (!contrastingColors.Any())
            {
                // If no colors found with minimum difference, lower the threshold
                contrastingColors = palette
                    .Where(kvp =>
                    {
                        float luminance = CalculateLuminance(kvp.Value.Color);
                        if (preferLighter)
                            return luminance > baseLuminance + 0.15f;
                        else
                            return luminance < baseLuminance - 0.15f;
                    })
                    .ToList();
            }

            if (!contrastingColors.Any())
            {
                // Last resort: any color in the right direction
                contrastingColors = palette
                    .Where(kvp =>
                    {
                        float luminance = CalculateLuminance(kvp.Value.Color);
                        return preferLighter ? luminance > baseLuminance : luminance < baseLuminance;
                    })
                    .ToList();
            }

            if (!contrastingColors.Any())
                return baseColorId;

            // Find the color with the best contrasting relationship
            return FindBestContrastingColor(baseColor.Color, contrastingColors);
        }

        /// MARK: GetColorScheme
        /// <summary>
        /// Gets a complete banner color scheme with main background, secondary background, and emblem colors.
        /// Automatically selects the best theme (standard or alternative) based on the main color's luminance.
        /// For lighter main colors, uses darker secondary and emblem. For darker main colors, uses lighter emblem.
        /// </summary>
        /// <param name="mainBackgroundId">Output: The main background color ID.</param>
        /// <param name="secondaryBackgroundId">Output: The secondary background color ID.</param>
        /// <param name="emblemColorId">Output: The emblem color ID (high contrast).</param>
        public static void GetBannerColorScheme(out int mainBackgroundId, out int secondaryBackgroundId, out int emblemColorId)
        {
            // Pick a random main background color
            mainBackgroundId = GetRandomColorId();

            // Automatically choose the best theme based on the main color
            GetBannerColorScheme(mainBackgroundId, out secondaryBackgroundId, out emblemColorId);
        }

        /// MARK: GetColorScheme
        /// <summary>
        /// Gets a complete banner color scheme with main background, secondary background, and emblem colors
        /// for a specific main color. Automatically selects the best theme based on the main color's luminance.
        /// For lighter main colors (luminance &gt; 0.6), uses lighter secondary and darker emblem.
        /// For darker main colors (luminance &lt;= 0.6), uses darker secondary and lighter emblem.
        /// </summary>
        /// <param name="mainBackgroundId">The main background color ID to use.</param>
        /// <param name="secondaryBackgroundId">Output: The secondary background color ID.</param>
        /// <param name="emblemColorId">Output: The emblem color ID (high contrast).</param>
        public static void GetBannerColorScheme(int mainBackgroundId, out int secondaryBackgroundId, out int emblemColorId)
        {
            MBReadOnlyDictionary<int, BannerColor> palette = BannerManager.Instance.ReadOnlyColorPalette;
            
            if (!palette.ContainsKey(mainBackgroundId))
            {
                // If invalid color, fall back to random
                mainBackgroundId = GetRandomColorId();
            }

            BannerColor mainColor = palette[mainBackgroundId];
            float mainLuminance = CalculateLuminance(mainColor.Color);

            // If the main color is very light (close to white), use alternative scheme (lighter secondary, darker emblem)
            // Otherwise use standard scheme (darker secondary, lighter emblem)
            if (mainLuminance > 0.6f)
            {
                // Light main color: use lighter secondary and darker emblem
                secondaryBackgroundId = GetLighterComplementaryColor(mainBackgroundId);
                emblemColorId = GetContrastingColor(mainBackgroundId, false);
            }
            else
            {
                // Dark/medium main color: use darker secondary and lighter emblem
                secondaryBackgroundId = GetDarkerComplementaryColor(mainBackgroundId);
                emblemColorId = GetContrastingColor(mainBackgroundId, true);
            }
        }

        /// MARK: GetAlternativeScheme
        /// <summary>
        /// Gets an alternative banner color scheme with lighter secondary background and darker emblem.
        /// Automatically selects a random main color and applies the alternative theme.
        /// Best for creating banners with lighter overall appearance.
        /// </summary>
        /// <param name="mainBackgroundId">Output: The main background color ID.</param>
        /// <param name="secondaryBackgroundId">Output: The secondary background color ID (lighter).</param>
        /// <param name="emblemColorId">Output: The emblem color ID (darker, high contrast).</param>
        public static void GetAlternativeBannerColorScheme(out int mainBackgroundId, out int secondaryBackgroundId, out int emblemColorId)
        {
            // Pick a random main background color
            mainBackgroundId = GetRandomColorId();

            // Apply the alternative theme
            GetAlternativeBannerColorScheme(mainBackgroundId, out secondaryBackgroundId, out emblemColorId);
        }

        /// MARK: GetAlternativeScheme
        /// <summary>
        /// Gets an alternative banner color scheme with lighter secondary background and darker emblem
        /// for a specific main color. Always uses the alternative theme regardless of luminance.
        /// Best for creating banners with lighter overall appearance.
        /// </summary>
        /// <param name="mainBackgroundId">The main background color ID to use.</param>
        /// <param name="secondaryBackgroundId">Output: The secondary background color ID (lighter).</param>
        /// <param name="emblemColorId">Output: The emblem color ID (darker, high contrast).</param>
        public static void GetAlternativeBannerColorScheme(int mainBackgroundId, out int secondaryBackgroundId, out int emblemColorId)
        {
            MBReadOnlyDictionary<int, BannerColor> palette = BannerManager.Instance.ReadOnlyColorPalette;
            
            if (!palette.ContainsKey(mainBackgroundId))
            {
                // If invalid color, fall back to random
                mainBackgroundId = GetRandomColorId();
            }

            // Get a lighter color for secondary background
            secondaryBackgroundId = GetLighterComplementaryColor(mainBackgroundId);

            // Get a high-contrast darker color for emblem visibility
            emblemColorId = GetContrastingColor(mainBackgroundId, false);
        }

        /// MARK: GetStandardScheme
        /// <summary>
        /// Gets a standard banner color scheme with darker secondary background and lighter emblem
        /// for a specific main color. Always uses the standard theme regardless of luminance.
        /// Best for creating banners with darker overall appearance.
        /// </summary>
        /// <param name="mainBackgroundId">The main background color ID to use.</param>
        /// <param name="secondaryBackgroundId">Output: The secondary background color ID (darker).</param>
        /// <param name="emblemColorId">Output: The emblem color ID (lighter, high contrast).</param>
        public static void GetStandardBannerColorScheme(int mainBackgroundId, out int secondaryBackgroundId, out int emblemColorId)
        {
            MBReadOnlyDictionary<int, BannerColor> palette = BannerManager.Instance.ReadOnlyColorPalette;
            
            if (!palette.ContainsKey(mainBackgroundId))
            {
                // If invalid color, fall back to random
                mainBackgroundId = GetRandomColorId();
            }

            // Get a darker color for secondary background
            secondaryBackgroundId = GetDarkerComplementaryColor(mainBackgroundId);

            // Get a high-contrast lighter color for emblem visibility
            emblemColorId = GetContrastingColor(mainBackgroundId, true);
        }

        /// MARK: CalculateLuminance
        /// <summary>
        /// Calculates the relative luminance of a color using the standard formula.
        /// </summary>
        /// <param name="colorValue">The uint color value in ARGB format.</param>
        /// <returns>Luminance value between 0.0 (black) and 1.0 (white).</returns>
        private static float CalculateLuminance(uint colorValue)
        {
            // Extract RGB components from uint (ARGB format)
            float r = ((colorValue >> 16) & 0xFF) / 255f;
            float g = ((colorValue >> 8) & 0xFF) / 255f;
            float b = (colorValue & 0xFF) / 255f;

            // Use standard relative luminance formula (ITU-R BT.709)
            return 0.2126f * r + 0.7152f * g + 0.0722f * b;
        }

        /// MARK: CalculateHueDiff
        /// <summary>
        /// Calculates hue similarity between two colors (0 = same hue, 180 = opposite).
        /// </summary>
        private static float CalculateHueDifference(uint color1, uint color2)
        {
            float h1 = GetHue(color1);
            float h2 = GetHue(color2);

            float diff = Math.Abs(h1 - h2);
            // Handle wrap-around (e.g., 350 and 10 are close)
            if (diff > 180f)
                diff = 360f - diff;

            return diff;
        }

        /// MARK: GetHue
        /// <summary>
        /// Gets the hue of a color in degrees (0-360).
        /// </summary>
        private static float GetHue(uint colorValue)
        {
            float r = ((colorValue >> 16) & 0xFF) / 255f;
            float g = ((colorValue >> 8) & 0xFF) / 255f;
            float b = (colorValue & 0xFF) / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            if (delta == 0)
                return 0; // Gray color, no hue

            float hue;
            if (max == r)
                hue = 60f * (((g - b) / delta) % 6);
            else if (max == g)
                hue = 60f * (((b - r) / delta) + 2);
            else
                hue = 60f * (((r - g) / delta) + 4);

            if (hue < 0)
                hue += 360f;

            return hue;
        }

        /// MARK: GetSaturation
        /// <summary>
        /// Gets the saturation of a color (0-1).
        /// </summary>
        private static float GetSaturation(uint colorValue)
        {
            float r = ((colorValue >> 16) & 0xFF) / 255f;
            float g = ((colorValue >> 8) & 0xFF) / 255f;
            float b = (colorValue & 0xFF) / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            if (max == 0)
                return 0;

            return delta / max;
        }

        /// MARK: PerceptualDistance
        /// <summary>
        /// Calculates a saturation-aware perceptual distance between two colors.
        /// Dynamically adjusts hue weight based on the minimum saturation of both colors,
        /// so that desaturated colors (grays/creams) are compared primarily by luminance
        /// rather than by their meaningless hue values.
        /// </summary>
        /// <param name="color1">First color as uint value.</param>
        /// <param name="color2">Second color as uint value.</param>
        /// <returns>Perceptual distance between 0.0 (identical) and ~1.0 (maximally different).</returns>
        public static float CalculatePerceptualDistance(uint color1, uint color2)
        {
            float luminanceDiff = Math.Abs(CalculateLuminance(color1) - CalculateLuminance(color2));
            float hueDiff = CalculateHueDifference(color1, color2) / 180f; // Normalize to 0-1
            float saturationDiff = Math.Abs(GetSaturation(color1) - GetSaturation(color2));

            // At low saturation, hue is perceptually meaningless (gray has no hue)
            // Scale hue relevance by the minimum saturation of both colors
            float minSat = Math.Min(GetSaturation(color1), GetSaturation(color2));
            float hueRelevance = MBMath.ClampFloat(minSat * 5f, 0f, 1f);

            // Dynamic weights: freed hue weight is absorbed by luminance
            float hueWeight = 0.35f * hueRelevance;
            float lumWeight = 0.50f + (0.35f - hueWeight);
            float satWeight = 0.15f;

            return (luminanceDiff * lumWeight) + (hueDiff * hueWeight) + (saturationDiff * satWeight);
        }

        /// MARK: ComplementaryColor
        /// <summary>
        /// Finds the best complementary color from a list of candidates.
        /// Prefers colors with similar hue but different brightness, avoiding harsh contrasts.
        /// </summary>
        private static int FindBestComplementaryColor(uint baseColor, List<KeyValuePair<int, BannerColor>> candidates, bool preferLighter)
        {
            if (!candidates.Any())
                return -1;

            float baseSaturation = GetSaturation(baseColor);

            // Score each candidate color
            var scoredCandidates = candidates.Select(kvp =>
            {
                float hueDiff = CalculateHueDifference(baseColor, kvp.Value.Color);
                float saturation = GetSaturation(kvp.Value.Color);
                
                // Prefer colors with similar hue (analogous colors work well for banners)
                float hueScore = 1f - (hueDiff / 180f); // 0-1, higher is better
                
                // Prefer colors with similar saturation (creates harmony)
                float saturationScore = 1f - Math.Abs(baseSaturation - saturation);
                
                // Combine scores (weighted toward hue similarity)
                float totalScore = (hueScore * 0.6f) + (saturationScore * 0.4f);

                return new { ColorId = kvp.Key, Score = totalScore };
            })
            .OrderByDescending(x => x.Score)
            .ToList();

            // Return the best scoring color, with some randomness for variety
            // 70% chance to pick the best, 20% second best, 10% third best
            int index = PickWeightedTopIndex(scoredCandidates.Count);
            return scoredCandidates[index].ColorId;
        }

        /// MARK: ContrastingColor
        /// <summary>
        /// Finds the best contrasting color from a list of candidates.
        /// Prefers colors with complementary hues and high saturation for maximum visibility.
        /// </summary>
        private static int FindBestContrastingColor(uint baseColor, List<KeyValuePair<int, BannerColor>> candidates)
        {
            if (!candidates.Any())
                return -1;

            float baseLuminance = CalculateLuminance(baseColor);

            // Score each candidate color for contrast
            var scoredCandidates = candidates.Select(kvp =>
            {
                float hueDiff = CalculateHueDifference(baseColor, kvp.Value.Color);
                float saturation = GetSaturation(kvp.Value.Color);
                float luminance = CalculateLuminance(kvp.Value.Color);
                
                // Prefer colors with complementary hues (120-180 degrees apart)
                float hueScore = 0f;
                if (hueDiff >= 120f)
                    hueScore = (hueDiff - 120f) / 60f; // Max score at 180 degrees
                else
                    hueScore = hueDiff / 120f * 0.5f; // Lower score for closer hues
                
                hueScore = Math.Min(hueScore, 1f);
                
                // Prefer high saturation (vibrant colors stand out better)
                float saturationScore = saturation;
                
                // Prefer high luminance difference
                float luminanceScore = Math.Abs(luminance - baseLuminance);
                
                // Combine scores (weighted toward luminance and hue contrast)
                float totalScore = (luminanceScore * 0.5f) + (hueScore * 0.3f) + (saturationScore * 0.2f);

                return new { ColorId = kvp.Key, Score = totalScore };
            })
            .OrderByDescending(x => x.Score)
            .ToList();

            // Return the best scoring color, with some randomness for variety
            // 70% chance to pick the best, 20% second best, 10% third best
            int index = PickWeightedTopIndex(scoredCandidates.Count);
            return scoredCandidates[index].ColorId;
        }

        /// MARK: GetColorInfo
        /// <summary>
        /// Gets color information for debugging purposes.
        /// </summary>
        /// <param name="colorId">The color ID to get information for.</param>
        /// <returns>A string containing color information, or null if the color doesn't exist.</returns>
        public static string GetColorInfo(int colorId)
        {
            MBReadOnlyDictionary<int, BannerColor> palette = BannerManager.Instance.ReadOnlyColorPalette;
            if (!palette.ContainsKey(colorId))
                return null;

            BannerColor color = palette[colorId];
            float luminance = CalculateLuminance(color.Color);
            float hue = GetHue(color.Color);
            float saturation = GetSaturation(color.Color);

            uint colorValue = color.Color;
            byte r = (byte)((colorValue >> 16) & 0xFF);
            byte g = (byte)((colorValue >> 8) & 0xFF);
            byte b = (byte)(colorValue & 0xFF);

            return $"Color ID {colorId}: RGB({r},{g},{b}) Hex:#{r:X2}{g:X2}{b:X2} " +
                   $"Luminance:{luminance:F3} Hue:{hue:F1} Saturation:{saturation:F3}";
        }

        /// MARK: AreColorsSimilar
        /// <summary>
        /// Determines if two colors are similar based on perceptual difference.
        /// Uses saturation-aware perceptual distance that dynamically adjusts hue weight
        /// based on color saturation, properly handling desaturated colors.
        /// </summary>
        /// <param name="colorId1">First color ID from banner palette.</param>
        /// <param name="colorId2">Second color ID from banner palette.</param>
        /// <param name="threshold">Perceptual similarity threshold (0.0-1.0). Lower values mean more strict matching. Default is 0.2.</param>
        /// <returns>True if colors are perceptually similar, false otherwise.</returns>
        public static bool AreColorsSimilar(int colorId1, int colorId2, float threshold = 0.2f)
        {
            // Exact match check
            if (colorId1 == colorId2)
                return true;

            MBReadOnlyDictionary<int, BannerColor> palette = BannerManager.Instance.ReadOnlyColorPalette;

            // Validate color IDs exist in palette
            if (!palette.ContainsKey(colorId1) || !palette.ContainsKey(colorId2))
                return false;

            uint color1 = palette[colorId1].Color;
            uint color2 = palette[colorId2].Color;

            float perceptualDifference = CalculatePerceptualDistance(color1, color2);
            return perceptualDifference < threshold;
        }

        /// MARK: AreColorsSimilar
        /// <summary>
        /// Determines if two colors (by uint value) are similar based on perceptual difference.
        /// Uses saturation-aware perceptual distance that dynamically adjusts hue weight
        /// based on color saturation, properly handling desaturated colors.
        /// </summary>
        /// <param name="color1">First color as uint value.</param>
        /// <param name="color2">Second color as uint value.</param>
        /// <param name="threshold">Perceptual similarity threshold (0.0-1.0). Lower values mean more strict matching. Default is 0.2.</param>
        /// <returns>True if colors are perceptually similar, false otherwise.</returns>
        public static bool AreColorsSimilar(uint color1, uint color2, float threshold = 0.2f)
        {
            // Exact match check
            if (color1 == color2)
                return true;

            float perceptualDifference = CalculatePerceptualDistance(color1, color2);
            return perceptualDifference < threshold;
        }

        /// MARK: GetUniqueClanColor
        /// <summary>
        /// Gets the most unique color ID from the banner palette compared to existing clans.
        /// Ensures the selected color is perceptually distinct from all existing clan colors.
        /// </summary>
        /// <param name="minimumThreshold">Minimum perceptual difference threshold (0.0-1.0). Default is 0.15 for good distinction.</param>
        /// <returns>A banner palette color ID that is maximally distinct from existing clan colors.</returns>
        public static int GetUniqueClanColorId(float minimumThreshold = 0.15f)
        {   
            int maxAttempts = 50; // Max attempts before color threshold is lowered

            MBReadOnlyDictionary<int, BannerColor> palette = BannerManager.Instance.ReadOnlyColorPalette;
            List<int> colorIds = palette.Keys.ToList();

            // Start with a high threshold for maximum uniqueness, then gradually decrease
            for (float similarThreshold = 0.5f; similarThreshold >= minimumThreshold; similarThreshold -= 0.05f)
            {
                // Try multiple random colors at this threshold level
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    int randomColorId = colorIds[_random.Next(colorIds.Count)];
                    bool isColorSimilar = false;

                    foreach (Clan clan in Clan.All)
                    {
                        if (clan.IsEliminated)
                            continue;

                        // Check if similar to this clan's color
                        if (AreColorsSimilar(palette[randomColorId].Color, clan.Color, similarThreshold))
                        {
                            isColorSimilar = true;
                            break;
                        }
                    }

                    // Color is sufficiently unique
                    if (!isColorSimilar)
                        return randomColorId;
                }
            }

            // If no unique color found even at minimum threshold, return a random color
            return colorIds[_random.Next(colorIds.Count)];
        }

        /// MARK: GetUniqueKingdomColor
        /// <summary>
        /// Gets the most unique color ID from the banner palette compared to existing kingdoms
        /// using deterministic max-min distance scored selection.
        /// Scores every palette color by its minimum perceptual distance to all active kingdom colors,
        /// applies an adaptive saturation floor to prefer vivid colors, and picks from the top candidates.
        /// </summary>
        /// <returns>A <see cref="KingdomColorResult"/> containing the chosen color ID and its minimum distance to the nearest kingdom.</returns>
        public static KingdomColorResult GetUniqueKingdomColorId()
        {
            MBReadOnlyDictionary<int, BannerColor> palette = BannerManager.Instance.ReadOnlyColorPalette;
            List<int> allColorIds = palette.Keys.ToList();

            // Collect active kingdom background colors
            List<uint> kingdomColors = new();
            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (!kingdom.IsEliminated)
                    kingdomColors.Add(kingdom.Color);
            }

            // If no active kingdoms, return random color with max distance
            if (kingdomColors.Count == 0)
            {
                return new KingdomColorResult
                {
                    ColorId = allColorIds[_random.Next(allColorIds.Count)],
                    MinDistanceToNearest = 1.0f
                };
            }

            // Compute adaptive saturation floor
            float minSaturation = CalculateAdaptiveSaturationFloor(kingdomColors.Count, allColorIds.Count);

            // Score candidates with saturation filter, fallback to all if none pass
            KingdomColorResult result = ScoreAndPickKingdomColor(palette, allColorIds, kingdomColors, minSaturation);

            // If filtered list yielded nothing (all below sat floor), retry without filter
            if (result.ColorId == -1)
                result = ScoreAndPickKingdomColor(palette, allColorIds, kingdomColors, 0f);

            // Final fallback (should not happen, but safety)
            if (result.ColorId == -1)
            {
                result = new KingdomColorResult
                {
                    ColorId = allColorIds[_random.Next(allColorIds.Count)],
                    MinDistanceToNearest = 0f
                };
            }

            return result;
        }

        /// MARK: ScoreAndPick
        /// <summary>
        /// Scores palette colors by minimum perceptual distance to existing kingdom colors
        /// and returns the best candidate using weighted random selection from the top 3-5.
        /// </summary>
        private static KingdomColorResult ScoreAndPickKingdomColor(
            MBReadOnlyDictionary<int, BannerColor> palette,
            List<int> allColorIds,
            List<uint> kingdomColors,
            float minSaturation)
        {
            // Filter candidates by saturation
            List<int> candidates = new();
            for (int i = 0; i < allColorIds.Count; i++)
            {
                int colorId = allColorIds[i];
                if (GetSaturation(palette[colorId].Color) >= minSaturation)
                    candidates.Add(colorId);
            }

            if (candidates.Count == 0)
                return new KingdomColorResult { ColorId = -1, MinDistanceToNearest = 0f };

            // Score each candidate: minimum distance to any existing kingdom color
            List<int> scoredIds = new(candidates.Count);
            List<float> scoredDists = new(candidates.Count);

            for (int i = 0; i < candidates.Count; i++)
            {
                int candidateId = candidates[i];
                uint candidateColor = palette[candidateId].Color;
                float minDist = float.MaxValue;

                for (int k = 0; k < kingdomColors.Count; k++)
                {
                    float dist = CalculatePerceptualDistance(candidateColor, kingdomColors[k]);
                    if (dist < minDist)
                        minDist = dist;
                }

                scoredIds.Add(candidateId);
                scoredDists.Add(minDist);
            }

            // Sort by distance descending (best candidates first)
            // Simple insertion sort is fine for this size
            for (int i = 1; i < scoredIds.Count; i++)
            {
                int tempId = scoredIds[i];
                float tempDist = scoredDists[i];
                int j = i - 1;
                while (j >= 0 && scoredDists[j] < tempDist)
                {
                    scoredIds[j + 1] = scoredIds[j];
                    scoredDists[j + 1] = scoredDists[j];
                    j--;
                }
                scoredIds[j + 1] = tempId;
                scoredDists[j + 1] = tempDist;
            }

            // Pick from top 3-5 with weighted randomness (70/20/10)
            int pickIndex = PickWeightedTopIndex(scoredIds.Count);

            return new KingdomColorResult
            {
                ColorId = scoredIds[pickIndex],
                MinDistanceToNearest = scoredDists[pickIndex]
            };
        }

        /// MARK: AdaptiveSatFloor
        /// <summary>
        /// Calculates an adaptive saturation floor based on kingdom count pressure.
        /// At low kingdom counts, enforces vivid colors only. As color space fills,
        /// gradually relaxes to allow more muted tones.
        /// </summary>
        /// <param name="activeKingdomCount">Number of non-eliminated kingdoms.</param>
        /// <param name="paletteSize">Total number of palette colors available.</param>
        /// <returns>Minimum saturation value (0.0 to 0.25).</returns>
        private static float CalculateAdaptiveSaturationFloor(int activeKingdomCount, int paletteSize)
        {
            float estimatedDistinctGroups = paletteSize / 4.0f;
            float pressure = activeKingdomCount / estimatedDistinctGroups;
            float minSaturation = Math.Max(0f, 0.25f * (1f - pressure / 0.7f));
            return minSaturation;
        }

        /// MARK: UniqueKingdomIconColor
        /// <summary>
        /// Gets a unique icon color for a kingdom banner using a dual-strategy approach.
        /// Below the pressure threshold (~12 kingdoms): focuses on aesthetic harmony and visibility
        /// against the banner background using the existing contrasting color system.
        /// Above the pressure threshold: focuses on icon uniqueness from kingdoms with similar
        /// background shades while maintaining minimum visibility.
        /// </summary>
        /// <param name="backgroundColorId">The kingdom's background (primary) color ID.</param>
        /// <param name="minDistanceToNearest">The minimum perceptual distance from the background to the nearest kingdom color.</param>
        /// <returns>A palette color ID for the icon color.</returns>
        public static int GetUniqueKingdomIconColorId(int backgroundColorId, float minDistanceToNearest)
        {
            const int KINGDOM_PRESSURE_THRESHOLD = 12;
            const float SIMILAR_BACKGROUND_THRESHOLD = 0.25f;
            const float MIN_LUMINANCE_CONTRAST = 0.25f;

            MBReadOnlyDictionary<int, BannerColor> palette = BannerManager.Instance.ReadOnlyColorPalette;

            if (!palette.ContainsKey(backgroundColorId))
                return GetRandomColorId();

            // Count active kingdoms
            int activeKingdoms = 0;
            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (!kingdom.IsEliminated)
                    activeKingdoms++;
            }

            float bgLuminance = CalculateLuminance(palette[backgroundColorId].Color);
            bool preferLighter = bgLuminance <= 0.6f;

            // LOW PRESSURE: Focus on aesthetic harmony + visibility against own background
            if (activeKingdoms <= KINGDOM_PRESSURE_THRESHOLD)
                return GetContrastingColor(backgroundColorId, preferLighter);

            // HIGH PRESSURE: Focus on icon uniqueness from kingdoms with similar backgrounds
            // Find kingdoms with similar background shade
            List<uint> similarKingdomIconColors = new();
            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (kingdom.IsEliminated)
                    continue;

                float bgDist = CalculatePerceptualDistance(palette[backgroundColorId].Color, kingdom.Color);
                if (bgDist < SIMILAR_BACKGROUND_THRESHOLD)
                    similarKingdomIconColors.Add(kingdom.Color2); // Color2 = icon color
            }

            // If no similar-background kingdoms exist, fall back to aesthetic mode
            if (similarKingdomIconColors.Count == 0)
                return GetContrastingColor(backgroundColorId, preferLighter);

            // Score each palette color as candidate icon
            List<int> candidateIds = new();
            List<float> candidateScores = new();

            foreach (int colorId in palette.Keys)
            {
                uint candidateColor = palette[colorId].Color;
                float lumDiff = Math.Abs(CalculateLuminance(candidateColor) - bgLuminance);

                // Must have minimum contrast against own background
                if (lumDiff < MIN_LUMINANCE_CONTRAST)
                    continue;

                // Minimum distance from all similar-kingdom icon colors
                float minIconDist = float.MaxValue;
                for (int i = 0; i < similarKingdomIconColors.Count; i++)
                {
                    float dist = CalculatePerceptualDistance(candidateColor, similarKingdomIconColors[i]);
                    if (dist < minIconDist)
                        minIconDist = dist;
                }

                // Composite score: 60% uniqueness from similar icons + 40% visibility against own background
                float score = (minIconDist * 0.6f) + (lumDiff * 0.4f);
                candidateIds.Add(colorId);
                candidateScores.Add(score);
            }

            // Fallback if no candidates pass visibility filter
            if (candidateIds.Count == 0)
                return GetContrastingColor(backgroundColorId, preferLighter);

            // Sort by score descending
            for (int i = 1; i < candidateIds.Count; i++)
            {
                int tempId = candidateIds[i];
                float tempScore = candidateScores[i];
                int j = i - 1;
                while (j >= 0 && candidateScores[j] < tempScore)
                {
                    candidateIds[j + 1] = candidateIds[j];
                    candidateScores[j + 1] = candidateScores[j];
                    j--;
                }
                candidateIds[j + 1] = tempId;
                candidateScores[j + 1] = tempScore;
            }

            // Pick from top 3 with weighted randomness (70/20/10)
            int pickIndex = PickWeightedTopIndex(candidateIds.Count);
            return candidateIds[pickIndex];
        }

        /// MARK: PickWeightedTop
        /// <summary>
        /// Picks an index from the top of a sorted list using weighted randomness.
        /// 70% chance for index 0, 20% for index 1, 10% for index 2.
        /// Clamps to available count.
        /// </summary>
        /// <param name="availableCount">Total number of available items in the sorted list.</param>
        /// <returns>Selected index (0, 1, or 2).</returns>
        private static int PickWeightedTopIndex(int availableCount)
        {
            if (availableCount <= 1)
                return 0;

            float roll = (float)_random.NextDouble();
            int index = 0;
            if (roll > 0.7f && availableCount > 1)
                index = 1;
            if (roll > 0.9f && availableCount > 2)
                index = 2;

            return index;
        }
    }
}
