using Bannerlord.GameMaster.Characters;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Provides the ability to modify hero appearance properties. <br />
    /// To change the target hero, create a new instance of HeroEditor instead. <br />
    /// This class is for appearance only - use HeroExtensions for game state modifications.
    /// <br /><br />
    /// Sub-editors: <br />
    /// - BodyEditor: Height, Weight, Build <br />
    /// - HeroAppearanceEditorUI: Opens native face generator UI
    /// </summary>
    public class HeroEditor
    {
        private readonly CharacterObject _template;
        public Hero Hero { get; }
        
        /// <summary>
        /// Sub-editor for body properties (Height, Weight, Build).
        /// </summary>
        public HeroBodyEditor BodyEditor { get; }
        
        /// <summary>
        /// Sub-editor for opening the native face generator UI.
        /// </summary>
        public HeroAppearanceEditorUI HeroAppearanceEditorUI { get; }
        
        /// <summary>
        /// Returns true if any sub-editor has been modified since creation or last reset.
        /// </summary>
        public bool IsDirty => BodyEditor.IsDirty;

        public HeroEditor(Hero hero)
        {
            Hero = hero;
            _template = hero.CharacterObject.OriginalCharacter ?? hero.CharacterObject;
            BodyEditor = new(hero);
            HeroAppearanceEditorUI = new(hero);
        }

        /// MARK: RandomizeApperanc
        /// <summary>
        /// Randomizes hero appearance using BodyEditor constraints.
        /// If height/weight/build fall outside constraints, new random values within range are generated.
        /// </summary>
        /// <param name="randomFactor">Amount of randomization (0-1)</param>
        public void RandomizeAppearance(float randomFactor)
        {
            if (randomFactor <= 0) return;

            int seed = RandomNumberGen.Instance.NextRandomInt();

            BodyProperties randomProperties = FaceGen.GetRandomBodyProperties(
                _template.Race,
                _template.IsFemale,
                _template.GetBodyPropertiesMin(true),
                _template.GetBodyPropertiesMax(true),
                HairCoveringType.None,
                seed,
                _template.BodyPropertyRange.HairTags,
                _template.BodyPropertyRange.BeardTags,
                _template.BodyPropertyRange.TattooTags,
                randomFactor);

            // Delegate body property application to the body editor
            // This handles height, weight, and build with proper constraint enforcement
            BodyEditor.ApplyRandomizedProperties(randomProperties);
        }

        /// MARK: Reset
        /// <summary>
        /// Resets all sub-editors to their original state when editor was created.
        /// Useful for GUI "cancel" functionality.
        /// </summary>
        public void Reset()
        {
            BodyEditor.Reset();
            // Future: EyesEditor.Reset(), NoseEditor.Reset(), etc.
        }
    }
}
