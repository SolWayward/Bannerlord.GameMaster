using System;
using TaleWorlds.CampaignSystem;

namespace Bannerlord.GameMaster.Kingdoms
{
    public static class KingdomColorPicker
    {
        /// <summary>
        /// Get a random color that is not used by another kingdom, or the players clan if the proposed color is not available and is also not too similar.
        /// </summary>
        /// <param name="proposedColor">Preferred Color if available</param>
        /// <returns>an unsigned integer representing a unique color with full opacity</returns>
        public static uint GetUniqueKingdomColor(uint proposedColor)
        {
            const int maxAttempts = 100; // Prevent infinite loops
            uint uniqueColor = proposedColor;
            int attempts = 0;

            while (IsColorInUse(uniqueColor) && attempts < maxAttempts)
            {
                uniqueColor = RandomNumberGen.Instance.NextRandomRGBColor;
                attempts++;
            }

            // If still not unique after max attempts, use it anyway
            // (very unlikely with 16M+ color combinations)
            return uniqueColor;
        }

        private static bool IsColorInUse(uint color)
        {
            // Check player's clan/kingdom colors (player clan color becomes kingdom color if they found a kingdom)
            Clan playerClan = Clan.PlayerClan;
            if (playerClan != null)
            {
                // If player has a kingdom, check kingdom colors
                if (playerClan.Kingdom != null)
                {
                    if (ColorsAreSimilar(color, playerClan.Kingdom.Color))
                        return true;
                    if (ColorsAreSimilar(color, playerClan.Kingdom.Color2))
                        return true;
                }
                // If player doesn't have a kingdom yet, check their clan banner colors
                // (these will become their kingdom colors when they found one)
                else if (playerClan.Banner != null)
                {
                    uint playerColor = playerClan.Banner.GetPrimaryColor();
                    if (ColorsAreSimilar(color, playerColor))
                        return true;
                    uint playerColor2 = playerClan.Banner.GetSecondaryColor();
                    if (ColorsAreSimilar(color, playerColor2))
                        return true;
                }
            }

            // Check all existing kingdoms' background colors (Color and Color2)
            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (kingdom != null)
                {
                    // Kingdom.Color is the primary background color displayed in-game
                    if (ColorsAreSimilar(color, kingdom.Color))
                        return true;
                    // Kingdom.Color2 is the secondary background color
                    if (ColorsAreSimilar(color, kingdom.Color2))
                        return true;
                }
            }

            return false;
        }

        private static bool ColorsAreSimilar(uint color1, uint color2)
        {
            // Exact match check
            if (color1 == color2)
                return true;

            // to prevent colors that are too similar
            return AreColorsTooSimilar(color1, color2, threshold: 30);
        }

        private static bool AreColorsTooSimilar(uint color1, uint color2, int threshold)
        {
            // Extract RGB components (ignoring alpha channel)
            int r1 = (int)((color1 >> 16) & 0xFF);
            int g1 = (int)((color1 >> 8) & 0xFF);
            int b1 = (int)(color1 & 0xFF);

            int r2 = (int)((color2 >> 16) & 0xFF);
            int g2 = (int)((color2 >> 8) & 0xFF);
            int b2 = (int)(color2 & 0xFF);

            // Calculate Euclidean distance in RGB space
            int rDiff = r1 - r2;
            int gDiff = g1 - g2;
            int bDiff = b1 - b2;

            double distance = Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);

            return distance < threshold;
        }

        /// <summary>
        /// Returns a darker shade of the given color
        /// </summary>
        /// <param name="color">Original color</param>
        /// <param name="factor">Darkening factor (0.0-1.0, where 0.7 = 30% darker)</param>
        public static uint GetDarkerShade(uint color, float factor = 0.7f)
        {
            byte r = (byte)((color >> 16) & 0xFF);
            byte g = (byte)((color >> 8) & 0xFF);
            byte b = (byte)(color & 0xFF);

            r = (byte)Math.Max(0, Math.Min(255, (int)(r * factor)));
            g = (byte)Math.Max(0, Math.Min(255, (int)(g * factor)));
            b = (byte)Math.Max(0, Math.Min(255, (int)(b * factor)));

            return 0xFF000000u | ((uint)r << 16) | ((uint)g << 8) | b;
        }


        /// <summary>
        /// Returns a lighter shade of the given color
        /// </summary>
        /// <param name="color">Original color</param>
        /// <param name="factor">Lightening factor (0.0-1.0, where 0.3 = 30% lighter)</param>
        public static uint GetLighterShade(uint color, float factor = 0.3f)
        {
            byte r = (byte)((color >> 16) & 0xFF);
            byte g = (byte)((color >> 8) & 0xFF);
            byte b = (byte)(color & 0xFF);

            r = (byte)Math.Max(0, Math.Min(255, r + (int)((255 - r) * factor)));
            g = (byte)Math.Max(0, Math.Min(255, g + (int)((255 - g) * factor)));
            b = (byte)Math.Max(0, Math.Min(255, b + (int)((255 - b) * factor)));

            return 0xFF000000u | ((uint)r << 16) | ((uint)g << 8) | b;
        }
    }
}