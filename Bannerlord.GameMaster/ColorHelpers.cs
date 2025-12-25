using System;

namespace Bannerlord.GameMaster
{
    public static class ColorHelpers
    {
        public static bool AreColorsSimilar(uint color1, uint color2, int threshold)
        {
            // Exact match check
            if (color1 == color2)
                return true;

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