using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace Bannerlord.GameMaster.Heroes
{
    [Flags]
    public enum HeroTypes
    {
        None = 0,
        IsArtisan = 1,
        Lord = 2,
        Wanderer = 4,
        Notable = 8,
        Merchant = 16,
        Children = 32,
        Female = 64,
        Male = 128,
        ClanLeader = 256,
        KingdomRuler = 512,
        PartyLeader = 1024,
        Fugitive = 2048,
        Alive = 4096,
        Dead = 8192,
        Prisoner = 16384,
        WithoutClan = 32768,
        WithoutKingdom = 65536,
        Married = 131072,
    }

    public static class HeroExtensions
    {
        /// <summary>
        /// Gets all hero type flags for this hero
        /// </summary>
        public static HeroTypes GetHeroTypes(this Hero hero)
        {
            HeroTypes types = HeroTypes.None;

            if (hero.IsArtisan) types |= HeroTypes.IsArtisan;
            if (hero.IsLord) types |= HeroTypes.Lord;
            if (hero.IsWanderer) types |= HeroTypes.Wanderer;
            if (hero.IsNotable) types |= HeroTypes.Notable;
            if (hero.IsMerchant) types |= HeroTypes.Merchant;
            if (hero.IsChild) types |= HeroTypes.Children;
            if (hero.IsFemale) types |= HeroTypes.Female;
            if (!hero.IsFemale) types |= HeroTypes.Male;
            if (hero.Clan?.Leader == hero) types |= HeroTypes.ClanLeader;
            if (hero.Clan?.Kingdom?.Leader == hero) types |= HeroTypes.KingdomRuler;
            if (hero.PartyBelongedTo?.LeaderHero == hero) types |= HeroTypes.PartyLeader;
            if (hero.IsFugitive) types |= HeroTypes.Fugitive;
            if (hero.IsAlive) types |= HeroTypes.Alive;
            if (!hero.IsAlive) types |= HeroTypes.Dead;
            if (hero.IsPrisoner) types |= HeroTypes.Prisoner;
            if (hero.Clan == null) types |= HeroTypes.WithoutClan;
            if (hero.Clan?.Kingdom == null) types |= HeroTypes.WithoutKingdom;
            if (hero.Spouse != null) types |= HeroTypes.Married;

            return types;
        }

        /// <summary>
        /// Checks if hero has ALL specified flags
        /// </summary>
        public static bool HasAllTypes(this Hero hero, HeroTypes types)
        {
            if (types == HeroTypes.None) return true;
            var heroTypes = hero.GetHeroTypes();
            return (heroTypes & types) == types;
        }

        /// <summary>
        /// Checks if hero has ANY of the specified flags
        /// </summary>
        public static bool HasAnyType(this Hero hero, HeroTypes types)
        {
            if (types == HeroTypes.None) return true;
            var heroTypes = hero.GetHeroTypes();
            return (heroTypes & types) != HeroTypes.None;
        }

        /// <summary>
        /// Returns a formatted string containing the hero's details
        /// </summary>
        public static string FormattedDetails(this Hero hero)
        {
            return $"{hero.StringId}\t{hero.Name}\tClan: {hero.Clan?.Name}\tKingdom: {hero.Clan?.Kingdom?.Name}";
        }
    }
}