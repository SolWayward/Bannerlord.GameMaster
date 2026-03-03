using System;

namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Flags controlling which sections of a character set file are applied to a hero
    /// during <see cref="CharacterSetFileManager.LoadCharacterSetToHero(TaleWorlds.CampaignSystem.Hero, string, CharacterSetApplyFlags)"/>.
    /// </summary>
    [Flags]
    public enum CharacterSetApplyFlags
    {
        None = 0,
        Age = 1 << 0,
        Culture = 1 << 1,
        Appearance = 1 << 2,
        Development = 1 << 3,
        Traits = 1 << 4,
        BattleEquipment = 1 << 5,
        CivilianEquipment = 1 << 6,

        /// <summary>Both battle and civilian equipment.</summary>
        Equipment = BattleEquipment | CivilianEquipment,

        /// <summary>Everything except age and culture (appearance, development, traits, equipment).</summary>
        CharacterOnly = Appearance | Development | Traits | Equipment,

        /// <summary>All sections (default behaviour of the original overload).</summary>
        All = Age | Culture | Appearance | Development | Traits | Equipment
    }
}
