using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Heroes.FaceGenerator
{
    /// <summary>
    /// Custom GameState that stores the target Hero for face generation.
    /// Used with HeroFaceGeneratorScreen for editing appearance of any hero
    /// Similar to native BarberState pattern - parameters passed via constructor.
    /// </summary>
    public class HeroFaceGeneratorState : GameState
    {
        private Hero _targetHero;
        private IFaceGeneratorCustomFilter _filter;
        private Action _onEndAction;

        public override bool IsMenuState => true;

        /// <summary>
        /// DO NOT USE 
        /// Parameterless constructor required by CreateState generic constraint.
        /// DO NOT USE - use the parameterized constructor via CreateState(params).
        /// </summary>
        public HeroFaceGeneratorState()
        {
        }

        /// <summary>
        /// Creates a new face generator state for the specified hero.
        /// Parameters must be passed via constructor because the screen is created
        /// during CreateState() before any Initialize() method could be called.
        /// </summary>
        /// <param name="targetHero">The hero whose appearance will be edited</param>
        /// <param name="filter">Optional filter to restrict available options (null for full editor)</param>
        /// <param name="onEndAction">Optional callback when editor closes</param>
        public HeroFaceGeneratorState(Hero targetHero, IFaceGeneratorCustomFilter filter, Action onEndAction)
        {
            _targetHero = targetHero;
            _filter = filter;
            _onEndAction = onEndAction;
        }

        /// <summary>
        /// Gets the target hero being edited.
        /// </summary>
        public Hero GetHero() => _targetHero;

        /// <summary>
        /// Gets the CharacterObject for the target hero.
        /// </summary>
        public CharacterObject GetCharacter() => _targetHero?.CharacterObject;

        /// <summary>
        /// Gets the face generator filter (null means full editor).
        /// </summary>
        public IFaceGeneratorCustomFilter GetFilter() => _filter;

        protected override void OnFinalize()
        {
            base.OnFinalize();
            _onEndAction?.Invoke();
        }
    }
}
