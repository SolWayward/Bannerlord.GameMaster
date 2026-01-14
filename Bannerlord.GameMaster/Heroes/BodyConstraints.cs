using System;

namespace Bannerlord.GameMaster.Heroes
{
    public struct BodyConstraints
    {
        /// <summary> Min and Max Height values ranging from 0 to 1</summary>
        public Constraint Height;

        /// <summary> Min and Max Weight values ranging from 0 to 1</summary>
        public Constraint Weight;

        /// <summary> Min and Max Build (Muscle Mass/Tone) values ranging from 0 to 1</summary>
        public Constraint Build;

        public BodyConstraints()
        {
            Height = new();
            Weight = new();
            Build = new();
        }

        /// <summary>
        /// Returns constraints optimized for specified gender <br />
        /// FemaleHeight: 0.3 to 0.8 - MaleHeight: 0.5 to 1 <br />
        /// FemaleWeight: 0 to 0.5 --- MaleWeight: 0 to 1 <br />
        /// FemaleBuild: 0 to 0.5 ---- MaleBuild: 0 to 1 <br />
        /// </summary>
        public static BodyConstraints GenderConstraints(bool isFemale)
        {
            if (isFemale)
                return FemaleConstraints;
            else
                return MaleConstraints;
        }

        /// <summary>
        /// Returns constraints optimized for females <br />
        /// Height: 0.3 to 0.8 <br />
        /// Weight: 0 to 0.5 <br />
        /// Build: 0 to 0.5 <br />
        /// </summary>
        public static BodyConstraints FemaleConstraints{
            get
            {
                BodyConstraints constraints = new()
                {
                    Height = new(0.3f, 0.8f),
                    Weight = new(0, 0.5f),
                    Build = new(0, 0.5f),
                };
                
                return constraints;
            }}

        /// <summary>
        /// Returns constraints optimized for males <br />
        /// Height: 0.5 to 1 <br />
        /// Weight: 0 to 1 <br />
        /// Build: 0 to 1 <br />
        /// </summary>
        public static BodyConstraints MaleConstraints{
            get
            {            
                BodyConstraints constraints = new()
                {
                    Height = new(0.5f, 1),
                    Weight = new(0, 1),
                    Build = new(0, 1),
                };

                return constraints;
            }}
        

        public struct Constraint
        {
            float _minimum;
            float _maximum;

            public float Minimum
            {
                get => _minimum;
                set => _minimum = TaleWorlds.Library.MathF.Clamp(value, 0, 1);
            }

            public float Maximum
            {
                get => _maximum;
                set => _maximum = TaleWorlds.Library.MathF.Clamp(value, 0, 1);
            }

            public Constraint()
            {
                _minimum = 0;
                _maximum = 1;
            }

            public Constraint(float min, float max)
            {
                // Clamp values to valid range
                min = TaleWorlds.Library.MathF.Clamp(min, 0, 1);
                max = TaleWorlds.Library.MathF.Clamp(max, 0, 1);

                // Enforce min <= max, swap if needed
                if (min > max)
                {
                    TaleWorlds.Library.Debug.Print($"[BLGM] BodyConstraints.Constraint: min ({min}) > max ({max}), swapping values");
                    (_minimum, _maximum) = (max, min);
                }
                else
                {
                    _minimum = min;
                    _maximum = max;
                }
            }
        }
    }
}