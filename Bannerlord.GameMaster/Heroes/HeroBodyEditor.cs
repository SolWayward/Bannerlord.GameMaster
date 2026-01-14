using Bannerlord.GameMaster.Common;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Sub-editor for hero body properties: Height, Weight, and Build.
    /// Owns BodyConstraints and tracks its own dirty state for reset functionality.
    /// </summary>
    public class HeroBodyEditor
    {
        // MARK: - Private Fields
        private readonly Hero _hero;
        private readonly BodyProperties _originalBodyProperties;
        private bool _isDirty;

        // MARK: - Public Properties
        public BodyConstraints BodyConstraints { get; set; }
        public bool IsDirty => _isDirty;

        #region Appearance Properties

        /// <summary>
        /// Gets or sets hero height (0-1). Automatically applies BodyConstraints.
        /// </summary>
        public float Height
        {
            get => GetHeight();
            set
            {
                float constrained = TaleWorlds.Library.MathF.Clamp(value,
                    BodyConstraints.Height.Minimum,
                    BodyConstraints.Height.Maximum);
                SetHeight(constrained);
                _isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets hero weight (0-1). Automatically applies BodyConstraints.
        /// </summary>
        public float Weight
        {
            get => GetWeight();
            set
            {
                float constrained = TaleWorlds.Library.MathF.Clamp(value,
                    BodyConstraints.Weight.Minimum,
                    BodyConstraints.Weight.Maximum);
                SetWeight(constrained);
                _isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets hero build/muscle (0-1). Automatically applies BodyConstraints.
        /// </summary>
        public float Build
        {
            get => GetBuild();
            set
            {
                float constrained = TaleWorlds.Library.MathF.Clamp(value,
                    BodyConstraints.Build.Minimum,
                    BodyConstraints.Build.Maximum);
                SetBuild(constrained);
                _isDirty = true;
            }
        }

        #endregion

        // MARK: - Constructor
        public HeroBodyEditor(Hero hero)
        {
            _hero = hero;
            _originalBodyProperties = new(hero.BodyProperties.DynamicProperties,
                                          hero.StaticBodyProperties);
            BodyConstraints = new();
            _isDirty = false;
        }

        // MARK: - Public Methods

        /// <summary>
        /// Sets an appearance property by name. Useful for console commands.
        /// </summary>
        /// <param name="propertyName">Property name (height, weight, build/muscle)</param>
        /// <param name="value">Value to set (0-1)</param>
        /// <returns>Result indicating success/failure</returns>
        public BLGMResult SetProperty(string propertyName, float value)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "height":
                    Height = value;
                    return new(true, $"Set height to {Height:F2}");
                case "weight":
                    Weight = value;
                    return new(true, $"Set weight to {Weight:F2}");
                case "build":
                case "muscle":
                    Build = value;
                    return new(true, $"Set build to {Build:F2}");
                default:
                    return new(false, $"Unknown body property: {propertyName}");
            }
        }

        /// <summary>
        /// Gets an appearance property value by name. Useful for console commands.
        /// </summary>
        /// <param name="propertyName">Property name (height, weight, build/muscle)</param>
        /// <returns>Property value or null if not found</returns>
        public float? GetProperty(string propertyName)
        {
            return propertyName.ToLowerInvariant() switch
            {
                "height" => Height,
                "weight" => Weight,
                "build" or "muscle" => Build,
                _ => null
            };
        }

        /// <summary>
        /// Resets body properties to original state when editor was created.
        /// </summary>
        public void Reset()
        {
            _hero.StaticBodyProperties = _originalBodyProperties.StaticProperties;
            _hero.Weight = _originalBodyProperties.Weight;
            _hero.Build = _originalBodyProperties.Build;
            _isDirty = false;
        }

        /// <summary>
        /// Re-applies current BodyConstraints to current appearance values.
        /// Clamps any out-of-range values.
        /// </summary>
        public void ApplyConstraints()
        {
            Height = Height;  // Re-sets through property, applying constraint
            Weight = Weight;
            Build = Build;
        }

        /// <summary>
        /// Applies body properties from randomization, enforcing constraints.
        /// If values are outside constraints, generates new random values within bounds.
        /// </summary>
        /// <param name="randomProperties">The randomly generated body properties</param>
        public void ApplyRandomizedProperties(BodyProperties randomProperties)
        {
            // Apply the base random properties first (includes facial features, hair, etc.)
            // Then override specific body properties (height, weight, build) with constrained values
            _hero.StaticBodyProperties = randomProperties.StaticProperties;

            // Apply height constraints - if outside range, generate random within bounds
            float randomHeight = StaticBodyPropertiesHelper.GetHeight(randomProperties.StaticProperties);
            if (randomHeight < BodyConstraints.Height.Minimum || randomHeight > BodyConstraints.Height.Maximum)
            {
                randomHeight = GenerateRandomInRange(BodyConstraints.Height.Minimum, BodyConstraints.Height.Maximum);
            }
            _hero.StaticBodyProperties = StaticBodyPropertiesHelper.SetHeight(_hero.StaticBodyProperties, randomHeight);

            // Apply weight constraints - if outside range, generate random within bounds
            float randomWeight = randomProperties.Weight;
            if (randomWeight < BodyConstraints.Weight.Minimum || randomWeight > BodyConstraints.Weight.Maximum)
            {
                randomWeight = GenerateRandomInRange(BodyConstraints.Weight.Minimum, BodyConstraints.Weight.Maximum);
            }
            _hero.Weight = randomWeight;

            // Apply build constraints - if outside range, generate random within bounds
            float randomBuild = randomProperties.Build;
            if (randomBuild < BodyConstraints.Build.Minimum || randomBuild > BodyConstraints.Build.Maximum)
            {
                randomBuild = GenerateRandomInRange(BodyConstraints.Build.Minimum, BodyConstraints.Build.Maximum);
            }
            _hero.Build = randomBuild;

            _isDirty = true;
        }

        #region Private Getters/Setters

        private float GetHeight()
        {
            return StaticBodyPropertiesHelper.GetHeight(_hero.StaticBodyProperties);
        }

        private void SetHeight(float height)
        {
            _hero.StaticBodyProperties = StaticBodyPropertiesHelper.SetHeight(_hero.StaticBodyProperties, height);
        }

        private float GetWeight()
        {
            return _hero.Weight;
        }

        private void SetWeight(float weight)
        {
            _hero.Weight = weight;
        }

        private float GetBuild()
        {
            return _hero.Build;
        }

        private void SetBuild(float build)
        {
            _hero.Build = build;
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Generates a random float value within the specified range.
        /// </summary>
        private float GenerateRandomInRange(float min, float max)
        {
            // Ensure valid range
            min = TaleWorlds.Library.MathF.Clamp(min, 0f, 1f);
            max = TaleWorlds.Library.MathF.Clamp(max, 0f, 1f);
            if (min > max)
            {
                TaleWorlds.Library.Debug.Print($"[BLGM] HeroBodyEditor: Constraint min ({min}) > max ({max}), swapping values");
                (min, max) = (max, min);
            }

            return min + (float)RandomNumberGen.Instance.NextRandomFloat() * (max - min);
        }

        #endregion
    }
}
