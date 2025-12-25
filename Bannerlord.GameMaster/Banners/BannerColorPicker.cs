using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Banners
{
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
            var palette = BannerManager.Instance.ReadOnlyColorPalette;
            var colorIds = palette.Keys.ToList();
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
            var palette = BannerManager.Instance.ReadOnlyColorPalette;
            
            if (!palette.ContainsKey(baseColorId))
                return baseColorId;

            var baseColor = palette[baseColorId];
            float baseLuminance = CalculateLuminance(baseColor.Color);

            // Find colors that are lighter than the base color
            var lighterColors = palette
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
            var palette = BannerManager.Instance.ReadOnlyColorPalette;
            
            if (!palette.ContainsKey(baseColorId))
                return baseColorId;

            var baseColor = palette[baseColorId];
            float baseLuminance = CalculateLuminance(baseColor.Color);

            // Find colors that are darker than the base color
            var darkerColors = palette
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
            var palette = BannerManager.Instance.ReadOnlyColorPalette;
            
            if (!palette.ContainsKey(baseColorId))
                return baseColorId;

            var baseColor = palette[baseColorId];
            float baseLuminance = CalculateLuminance(baseColor.Color);

            // Find colors with high luminance difference
            var contrastingColors = palette
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
        /// For lighter main colors (luminance > 0.6), uses lighter secondary and darker emblem.
        /// For darker main colors (luminance <= 0.6), uses darker secondary and lighter emblem.
        /// </summary>
        /// <param name="mainBackgroundId">The main background color ID to use.</param>
        /// <param name="secondaryBackgroundId">Output: The secondary background color ID.</param>
        /// <param name="emblemColorId">Output: The emblem color ID (high contrast).</param>
        public static void GetBannerColorScheme(int mainBackgroundId, out int secondaryBackgroundId, out int emblemColorId)
        {
            var palette = BannerManager.Instance.ReadOnlyColorPalette;
            
            if (!palette.ContainsKey(mainBackgroundId))
            {
                // If invalid color, fall back to random
                mainBackgroundId = GetRandomColorId();
            }

            var mainColor = palette[mainBackgroundId];
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
            var palette = BannerManager.Instance.ReadOnlyColorPalette;
            
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
            var palette = BannerManager.Instance.ReadOnlyColorPalette;
            
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
            // Handle wrap-around (e.g., 350° and 10° are close)
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

        /// MARK: ComplementaryColor
        /// <summary>
        /// Finds the best complementary color from a list of candidates.
        /// Prefers colors with similar hue but different brightness, avoiding harsh contrasts.
        /// </summary>
        private static int FindBestComplementaryColor(uint baseColor, List<KeyValuePair<int, BannerColor>> candidates, bool preferLighter)
        {
            if (!candidates.Any())
                return -1;

            float baseHue = GetHue(baseColor);
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
            int index = 0;
            float roll = (float)_random.NextDouble();
            if (roll > 0.7f && scoredCandidates.Count > 1)
                index = 1;
            if (roll > 0.9f && scoredCandidates.Count > 2)
                index = 2;

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

            float baseHue = GetHue(baseColor);
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
            int index = 0;
            float roll = (float)_random.NextDouble();
            if (roll > 0.7f && scoredCandidates.Count > 1)
                index = 1;
            if (roll > 0.9f && scoredCandidates.Count > 2)
                index = 2;

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
            var palette = BannerManager.Instance.ReadOnlyColorPalette;
            if (!palette.ContainsKey(colorId))
                return null;

            var color = palette[colorId];
            float luminance = CalculateLuminance(color.Color);
            float hue = GetHue(color.Color);
            float saturation = GetSaturation(color.Color);

            uint colorValue = color.Color;
            byte r = (byte)((colorValue >> 16) & 0xFF);
            byte g = (byte)((colorValue >> 8) & 0xFF);
            byte b = (byte)(colorValue & 0xFF);

            return $"Color ID {colorId}: RGB({r},{g},{b}) Hex:#{r:X2}{g:X2}{b:X2} " +
                   $"Luminance:{luminance:F3} Hue:{hue:F1}° Saturation:{saturation:F3}";
        }

        /// MARK: AreColorsSimilar
        /// <summary>
        /// Determines if two colors are similar based on perceptual difference.
        /// Uses luminance, hue, and saturation to calculate color similarity.
        /// </summary>
        /// <param name="colorId1">First color ID from banner palette.</param>
        /// <param name="colorId2">Second color ID from banner palette.</param>
        /// <param name="threshold">Perceptual similarity threshold (0.0-1.0). Lower values mean more strict matching. Default is 0.2.</param>
        /// <returns>True if colors are perceptually similar, false otherwise.</returns>
        public static bool AreColorsSimilar(int colorId1, int colorId2, float threshold = 0.2f)
        {
            var palette = BannerManager.Instance.ReadOnlyColorPalette;
            
            // Exact match check
            if (colorId1 == colorId2)
                return true;

            // Validate color IDs exist in palette
            if (!palette.ContainsKey(colorId1) || !palette.ContainsKey(colorId2))
                return false;

            uint color1 = palette[colorId1].Color;
            uint color2 = palette[colorId2].Color;

            // Calculate perceptual differences
            float luminance1 = CalculateLuminance(color1);
            float luminance2 = CalculateLuminance(color2);
            float luminanceDiff = Math.Abs(luminance1 - luminance2);

            float hue1 = GetHue(color1);
            float hue2 = GetHue(color2);
            float hueDiff = CalculateHueDifference(color1, color2) / 180f; // Normalize to 0-1

            float saturation1 = GetSaturation(color1);
            float saturation2 = GetSaturation(color2);
            float saturationDiff = Math.Abs(saturation1 - saturation2);

            // Calculate weighted perceptual difference
            // Luminance is most important for distinguishing colors, then hue, then saturation
            float perceptualDifference = (luminanceDiff * 0.5f) + (hueDiff * 0.35f) + (saturationDiff * 0.15f);

            return perceptualDifference < threshold;
        }

        /// MARK: AreColorsSimilar
        /// <summary>
        /// Determines if two colors (by uint value) are similar based on perceptual difference.
        /// This overload is for compatibility with existing code using raw color values.
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

            // Calculate perceptual differences
            float luminance1 = CalculateLuminance(color1);
            float luminance2 = CalculateLuminance(color2);
            float luminanceDiff = Math.Abs(luminance1 - luminance2);

            float hueDiff = CalculateHueDifference(color1, color2) / 180f; // Normalize to 0-1

            float saturation1 = GetSaturation(color1);
            float saturation2 = GetSaturation(color2);
            float saturationDiff = Math.Abs(saturation1 - saturation2);

            // Calculate weighted perceptual difference
            float perceptualDifference = (luminanceDiff * 0.5f) + (hueDiff * 0.35f) + (saturationDiff * 0.15f);

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
            int maxAttempts = 50; //Max attempts before color threshold is lowered

            var palette = BannerManager.Instance.ReadOnlyColorPalette;
            var colorIds = palette.Keys.ToList();

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
    }
}
