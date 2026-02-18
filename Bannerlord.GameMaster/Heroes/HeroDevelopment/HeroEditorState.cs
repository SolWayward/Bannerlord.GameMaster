using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Heroes.HeroDevelopment
{
    /// <summary>
    /// Custom GameState that stores the target Hero and a HeroSkillEditor for character development editing.
    /// Used with Commander's CharacterEditorScreen for editing skills, attributes, focus, and perks of any hero.
    /// Similar to HeroFaceGeneratorState pattern - parameters passed via constructor.
    /// </summary>
    public class HeroEditorState : GameState
    {
        private Hero _targetHero;
        private Action _onCloseCallback;
        private HeroSkillEditor _editor;

        public override bool IsMenuState => true;

        /// <summary>
        /// DO NOT USE
        /// Parameterless constructor required by CreateState generic constraint.
        /// DO NOT USE - use the parameterized constructor via CreateState(params).
        /// </summary>
        public HeroEditorState()
        {
        }

        /// <summary>
        /// Creates a new hero editor state for the specified hero.
        /// Parameters must be passed via constructor because the screen is created
        /// during CreateState() before any Initialize() method could be called.
        /// The HeroSkillEditor is created immediately, capturing the hero's current
        /// development state as a snapshot for reset/cancel support.
        /// </summary>
        /// <param name="targetHero">The hero whose character development will be edited</param>
        /// <param name="onCloseCallback">Optional callback when editor closes</param>
        public HeroEditorState(Hero targetHero, Action onCloseCallback)
        {
            _targetHero = targetHero;
            _onCloseCallback = onCloseCallback;
            _editor = new HeroSkillEditor(targetHero);
        }

        /// MARK: GetHero
        /// <summary>
        /// Gets the target hero being edited.
        /// </summary>
        public Hero GetHero() => _targetHero;

        /// MARK: GetEditor
        /// <summary>
        /// Gets the HeroSkillEditor instance for modifying the target hero's character development.
        /// The editor was created at state construction time with an initial snapshot already captured.
        /// </summary>
        public HeroSkillEditor GetEditor() => _editor;

        protected override void OnFinalize()
        {
            base.OnFinalize();
            _onCloseCallback?.Invoke();
        }
    }
}
