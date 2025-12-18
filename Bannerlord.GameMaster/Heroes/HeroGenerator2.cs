using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Heroes
{
    public class HeroGenerator2_DontUse
    {
        public Hero CreateHero(string name, Clan clan)
        {
            string stringId = ObjectManager.Instance.GetUniqueStringId(name, typeof(Hero));
            TextObject nameObject = new(name);
            
            CharacterObject templateCharacter = CharacterObject.Find("neutral_lord_1 ");

            Hero hero = HeroCreator.CreateSpecialHero(templateCharacter, age: 25);
            hero.StringId = stringId;
            hero.SetName(nameObject, nameObject);
            hero.PreferredUpgradeFormation = FormationClass.Cavalry;
            hero.Culture = MBObjectManager.Instance.GetObject<CultureObject>("vlandia");
            hero.Clan = clan;
            hero.Gold = 10000;
            hero.Level = 10;
            //hero.ModifyHair()
            //hero.AddPower();
            //TaleWorlds.MountAndBlade.FaceGen test; // Non static version with slightly less options. Can either be uses? static version seems better.
            //TaleWorlds.Core.FaceGen.GetRandomBodyProperties();
            //TaleWorlds.Core.FaceGen.SetBody()
            //TaleWorlds.Core.FaceGen.GenerateParentKey()
            return hero;
        }
    }
}