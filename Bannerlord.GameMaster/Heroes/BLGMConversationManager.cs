using System.Data.Odbc;
using Bannerlord.GameMaster.Troops;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Heroes
{
    public class BLGMConversationManager
    {
        ConversationCharacterData playerCCD;
        ConversationCharacterData otherHeroCCD;

        /// MARK: Constructor
        /// <summary>
        /// Initialize conversation objects for player and other hero
        /// </summary>
        /// <param name="otherHero">The hero the player will be talking to</param>
        /// <param name="forceCivilian">Optional, defaults to false. If true both heroes will use civilian loadout, 
        /// otherwise appropiate loadout will be selected based on location and hero states</param>
        public BLGMConversationManager(Hero otherHero, bool forceCivilian = false)
        {
            playerCCD = CreateConversationCharacterData(Hero.MainHero, forceCivilian);
            otherHeroCCD = CreateConversationCharacterData(otherHero, forceCivilian);
        }

        /// MARK: StartConversation
        /// <summary>
        /// Start the conversation between player and other hero
        /// </summary>
        public void StartConversation()
        {
            CampaignMapConversation.OpenConversation(playerCCD, otherHeroCCD);
        }

        /// MARK: CreateCCD
        /// <summary>
        /// Create ConversationCharacterData for hero automatically selecting, civilian or battle gear and other properties 
        /// based on hero state, type, and equipment
        /// </summary>
        /// <param name="hero">The hero for which to create the ConversationCharacterData for</param>
        /// <param name="forceCivilian">Force civillian loadout for hero</param>
        /// <returns></returns>
        public static ConversationCharacterData CreateConversationCharacterData(Hero hero, bool forceCivilian = false)
        {
            bool inSettlement = false;
            if (hero.CurrentSettlement != null)
                inSettlement = true;

            CharacterObject character = hero.CharacterObject;
            
            PartyBase party = null; 
            if (hero.PartyBelongedTo != null)
                party = hero.PartyBelongedTo.Party;
            
            
            bool noHorse = !character.IsMounted || inSettlement;
            bool noWeapon = hero.IsNoncombatant || !character.HasWeaponType(ItemObject.ItemTypeEnum.OneHandedWeapon) || !character.HasWeaponType(ItemObject.ItemTypeEnum.TwoHandedWeapon);
            bool spawnAfterFight = false;
            bool isCivilianEquipmentRequiredForLeader = inSettlement || hero.IsNoncombatant;
            bool isCivilianEquipmentRequiredForBodyGuardCharacters = isCivilianEquipmentRequiredForLeader;
            bool noBodyguards = isCivilianEquipmentRequiredForLeader;

            if (forceCivilian)
            {
                noHorse = true;
                noWeapon = true;
                isCivilianEquipmentRequiredForLeader = true;
                isCivilianEquipmentRequiredForBodyGuardCharacters = true;
                noBodyguards = true;
            }

            return new(character, party, noHorse, noWeapon, spawnAfterFight, isCivilianEquipmentRequiredForLeader, isCivilianEquipmentRequiredForBodyGuardCharacters, noBodyguards);
        }
    }
}