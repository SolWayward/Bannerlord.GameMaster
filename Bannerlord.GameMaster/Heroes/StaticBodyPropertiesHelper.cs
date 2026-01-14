using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Static utility class for bit manipulation operations on StaticBodyProperties.
    /// Used by HeroBodyEditor and future sub-editors (Eyes, Nose, Jaw, etc.) to read/write
    /// specific appearance values encoded in the KeyPart fields.
    /// </summary>
    public static class StaticBodyPropertiesHelper
    {
        #region Bit Manipulation

        /// <summary>
        /// Extracts a bit field value from a ulong key.
        /// </summary>
        /// <param name="key">The key containing the bit field</param>
        /// <param name="startBit">Starting bit position (0-based)</param>
        /// <param name="numBits">Number of bits to extract</param>
        /// <returns>The extracted value</returns>
        public static ulong GetBitsValueFromKey(ulong key, int startBit, int numBits)
        {
            // Create a mask of 'numBits' 1s, shifted to start at 'startBit'
            ulong mask = ((1UL << numBits) - 1) << startBit;

            // Apply mask and shift result back to position 0
            return (key & mask) >> startBit;
        }

        /// <summary>
        /// Sets a bit field value in a ulong key.
        /// </summary>
        /// <param name="key">The original key</param>
        /// <param name="startBit">Starting bit position (0-based)</param>
        /// <param name="numBits">Number of bits in the field</param>
        /// <param name="newValue">The new value to set (must fit within numBits)</param>
        /// <returns>The key with the modified bit field</returns>
        public static ulong SetBits(ulong key, int startBit, int numBits, int newValue)
        {
            // Validate newValue fits within numBits
            int maxValue = (1 << numBits) - 1;
            if (newValue < 0 || newValue > maxValue)
            {
                Debug.Print($"[BLGM] StaticBodyPropertiesHelper.SetBits: newValue ({newValue}) out of bounds for {numBits} bits (0-{maxValue}), clamping");
                newValue = (int)MathF.Clamp(newValue, 0, maxValue);
            }

            // Create a mask of 'numBits' 1s, shifted to start at 'startBit'
            ulong mask = ((1UL << numBits) - 1) << startBit;

            // Clear the bits at the target position
            ulong clearedKey = key & ~mask;

            // Set the new value at the target position
            return clearedKey | ((ulong)newValue << startBit);
        }

        #endregion

        #region Height Specific Operations

        // Height is stored in KeyPart8: 6 bits starting at position 19, representing 0-63 (maps to 0.0f-1.0f)
        private const int HeightStartBit = 19;
        private const int HeightNumBits = 6;
        private const int HeightMaxValue = 63;

        /// <summary>
        /// Extracts height value (0-1) from StaticBodyProperties.
        /// </summary>
        public static float GetHeight(StaticBodyProperties staticBodyProperties)
        {
            ulong keyPart8 = staticBodyProperties.KeyPart8;
            return (float)GetBitsValueFromKey(keyPart8, HeightStartBit, HeightNumBits) / HeightMaxValue;
        }

        /// <summary>
        /// Creates a new StaticBodyProperties with the specified height value (0-1).
        /// </summary>
        public static StaticBodyProperties SetHeight(StaticBodyProperties staticBodyProperties, float height)
        {
            ulong keyPart8 = staticBodyProperties.KeyPart8;

            // Convert height to integer value (0-63)
            int heightValue = (int)(height * HeightMaxValue);
            heightValue = (int)TaleWorlds.Library.MathF.Clamp(heightValue, 0, HeightMaxValue);

            // Set the new height bits
            ulong newKeyPart8 = SetBits(keyPart8, HeightStartBit, HeightNumBits, heightValue);

            return new(
                staticBodyProperties.KeyPart1,
                staticBodyProperties.KeyPart2,
                staticBodyProperties.KeyPart3,
                staticBodyProperties.KeyPart4,
                staticBodyProperties.KeyPart5,
                staticBodyProperties.KeyPart6,
                staticBodyProperties.KeyPart7,
                newKeyPart8);
        }

        #endregion
    }
}
