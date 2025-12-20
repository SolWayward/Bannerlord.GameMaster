using System;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster
{
    /// <summary>
    /// Singleton Wrapper for System.Random, ensuring the same Random instance is used to prevent issues of constantly instantiating new Random
    /// </summary>
    public class RandomNumberGen
    {
        private static readonly Lazy<RandomNumberGen> _instance = new(() => new());
        public static RandomNumberGen Instance => _instance.Value;

        Random random;

        public RandomNumberGen()
        {
            random = new(Environment.TickCount);
        }

        /// <summary>Returns a non-negative random integer.</summary>
        public int NextRandomInt() => random.Next();      

        /// <summary>Returns a non-negative random integer that is less, but not equal to the specified maximum.</summary>
        public int NextRandomInt(int max) => random.Next(max);

        /// <summary>Returns a signed integer greater than or equal to min Value and less than max Value</summary>  
        public int NextRandomInt(int min, int max) => random.Next(min, max);

        /// <summary>A floating point number that is greater than or equal to 0f, and less than 1f.</summary>
        public float NextRandomFloat() => (float)random.NextDouble();    
        
		/// <summary>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</summary>
        public double NextRandomDouble() => random.NextDouble();  
        
		/// <summary>Fills the elements of a specified array of bytes with random numbers.</summary>  
        public void NextRandomBytes(byte[] buffer) => random.NextBytes(buffer);
    }
}