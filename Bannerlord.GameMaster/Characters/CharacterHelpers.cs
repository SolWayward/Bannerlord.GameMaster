using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.GameMaster.Characters
{
    public static class CharacterHelpers
    {
        /// <summary>
        /// Builds Character code the same way as CampaignUIHelper.GetCharacterCode except has an option to bypass the check for IsHeroInformationHidden
        /// Use forceShowHidden = true to even show characters that are hidden or unknown to player
        /// </summary>
        public static CharacterCode BuildCharacterCode(CharacterObject character, bool useCivilian, bool forceShowHidden)
        {
            if (character == null)
                return CharacterCode.CreateEmpty();


            if (forceShowHidden == false && character.IsHero && CampaignUIHelper.IsHeroInformationHidden(character.HeroObject, out TextObject _))
                return CharacterCode.CreateEmpty();

            Hero heroObject = character.HeroObject;
            uint color1 = heroObject?.MapFaction?.Color ?? character.Culture?.Color ?? Color.White.ToUnsignedInteger();
            uint color2 = heroObject?.MapFaction?.Color2 ?? character.Culture?.Color2 ?? Color.White.ToUnsignedInteger();

            if (!useCivilian && heroObject?.IsNoncombatant == true)
                useCivilian = true;

            BodyProperties bodyProperties = character.GetBodyProperties(character.Equipment, -1);
            Equipment equipment = (useCivilian && character.FirstCivilianEquipment != null)
                ? character.FirstCivilianEquipment.Clone(false)
                : character.FirstBattleEquipment.Clone(false);

            return CharacterCode.CreateFrom(
                equipment.CalculateEquipmentCode(),
                bodyProperties,
                character.IsFemale,
                character.IsHero,
                color1, color2,
                character.DefaultFormationClass,
                character.Race);
        }
    }
}