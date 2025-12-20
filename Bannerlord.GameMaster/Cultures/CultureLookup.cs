using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Common.Interfaces;
using TaleWorlds.ObjectSystem;
using TaleWorlds.Localization;

namespace Bannerlord.GameMaster.Characters
{
    public static class CultureLookup
    {
        public static CultureObject Aserai => MBObjectManager.Instance.GetObject<CultureObject>("aserai");
        public static CultureObject Battania => MBObjectManager.Instance.GetObject<CultureObject>("battania");
        public static CultureObject Empire => MBObjectManager.Instance.GetObject<CultureObject>("empire");
        public static CultureObject Khuzait => MBObjectManager.Instance.GetObject<CultureObject>("khuzait");
        public static CultureObject Nord => MBObjectManager.Instance.GetObject<CultureObject>("nord");
        public static CultureObject Sturgia => MBObjectManager.Instance.GetObject<CultureObject>("sturgia");
        public static CultureObject Vlandia => MBObjectManager.Instance.GetObject<CultureObject>("vlandia");
     
        public static CultureObject DesertBandits => MBObjectManager.Instance.GetObject<CultureObject>("desert_bandits");
        public static CultureObject ForestBandits => MBObjectManager.Instance.GetObject<CultureObject>("forest_bandits");
        public static CultureObject Looters => MBObjectManager.Instance.GetObject<CultureObject>("looters");
        public static CultureObject MountainBandits => MBObjectManager.Instance.GetObject<CultureObject>("mountain_bandits");
        public static CultureObject SeaRaiders => MBObjectManager.Instance.GetObject<CultureObject>("sea_raiders");
        public static CultureObject Corsairs => MBObjectManager.Instance.GetObject<CultureObject>("southern_pirates");
        public static CultureObject SteppeBandits => MBObjectManager.Instance.GetObject<CultureObject>("steppe_bandits");
              
        public static CultureObject DarshiSpecial => MBObjectManager.Instance.GetObject<CultureObject>("darshi");
        public static CultureObject VakkenSpecial => MBObjectManager.Instance.GetObject<CultureObject>("vakken");
        
        public static CultureObject CalradianNeutral => MBObjectManager.Instance.GetObject<CultureObject>("neutral_culture");

        public static List<CultureObject> AllCultures => MBObjectManager.Instance.GetObjectTypeList<CultureObject>();
        
        public static List<CultureObject> MainCultures
        {
            get
            {
                List<CultureObject> _mainCultures = new();
                foreach(CultureObject culture in AllCultures)
                {
                    if (culture.IsMainCulture)
                        _mainCultures.Add(culture);
                }

                return _mainCultures;
            }
        }

        public static List<CultureObject> BanditCultures
        {   get
            {
                List<CultureObject> _banditCultures = new();
                foreach(CultureObject culture in AllCultures)
                {
                    if (culture.IsBandit)
                        _banditCultures.Add(culture);
                }

                return _banditCultures;
            }
        }

        public static TextObject GetRandomName(CultureObject culture, bool isFemale)
        {
            List<TextObject> names;

            if (isFemale)
            {
                if (culture == null)
                    names = CalradianNeutral.FemaleNameList;
                else
                    names = culture.FemaleNameList;
            }

            else
            {
                if (culture == null)
                    names = CalradianNeutral.MaleNameList;
                else
                    names = culture.MaleNameList;              
            } 

            int randomIndex = RandomNumberGen.Instance.NextRandomInt(names.Count);
            return names[randomIndex];          
        }
    }
}