using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.BLGMDebug
{
    public static class HeroDebug
    {
        /// <summary>
        /// Compares all Heroes stringIds with their linked CharacterObject stringIds checking if they match within the BLGMObjectManager <br />
        /// Automatically writes to log and returns the result as well
        /// </summary>
        public static string CheckHeroesCharacterStringId()
        {
            string debugInfo = "Hero and hero.characterObject stringIds:";

            foreach (Hero hero in BLGMObjectManager.BlgmHeroes)
            {
                bool mbHeroRegistered = MBObjectManager.Instance.GetObject<Hero>(hero.StringId) != null;
                bool mbCharRegistered = MBObjectManager.Instance.GetObject<CharacterObject>(hero.CharacterObject.StringId) != null;
                bool mbOrpanedChar = false;

                if (hero.StringId != hero.CharacterObject.StringId)
                {
                    // Check if mb has an orphaned characterObject registered using the string id of hero if hero doesnt match character
                    mbOrpanedChar = MBObjectManager.Instance.GetObject<CharacterObject>(hero.StringId) != null;
                }

                debugInfo += $"\nHero: {hero.StringId}, Character: {hero.CharacterObject.StringId}";
                debugInfo +=$"\nMBObjectManager heroRegistered: {mbHeroRegistered}, charRegistered: {mbCharRegistered}, mbOrphanedChar: {mbOrpanedChar}";
                debugInfo += "\n";              
            }
            
            TaleWorlds.Library.Debug.Print(debugInfo);
            return debugInfo;
        }

        /// <summary>
        /// Prints a list of all perks that effect on foot troops <br />
        /// Automatically writes to log and returns the result as well
        /// </summary>
        /// <returns></returns>
        public static string AllCaptainOnFootPerks()
        {
            string debugInfo = "\n---------GetCaptainPerksForTroopUsages---------";
            debugInfo += "\nCaptain OnFoot Perks:";
            IEnumerable<PerkObject> perks = Helpers.PerkHelper.GetCaptainPerksForTroopUsages(TroopUsageFlags.OnFoot);
            foreach (PerkObject perk in perks)
            {
                debugInfo += $"\nOnFootCaptainPerk:{perk.Name}, skill:{perk.Skill}";
            }

            TaleWorlds.Library.Debug.Print(debugInfo);
            return debugInfo;
        }

        /// <summary>
        /// Get a debug formated string of perk bonuses and ratings for Hero <br />
        /// Automatically writes to log and returns the result as well
        /// </summary>
        public static string CaptainOnFootPerks(Hero hero)
        {
            string debugInfo = $"=============================={hero.Name}==============================";
            debugInfo += "\n---------GetCaptainRatingForTroopUsages---------";
            debugInfo += "\nOnfoot Perk Ratings:";
            DefaultBattleCaptainModel model = new(); model.GetCaptainRatingForTroopUsages(hero, TroopUsageFlags.OnFoot, out List<PerkObject> perks2);
            foreach (PerkObject perk in perks2)
            {
                debugInfo += $"\nOnFootCaptainPerk:{perk.Name}, skill:{perk.Skill}";
            }

            TaleWorlds.Library.Debug.Print(debugInfo);
            return debugInfo;
        }        
    }
}