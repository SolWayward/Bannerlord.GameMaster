using System;

namespace Bannerlord.GameMaster.Items
{
    /// <summary>
    /// Flags enum representing weapon types and weapon groups for equipment generation.
    /// Used by the HeroOutfitter system to specify weapon preferences when building equipment pools.
    /// </summary>
    [Flags]
    public enum WeaponTypeFlags : long
    {
        /// <summary>No weapon type specified.</summary>
        None = 0,

        // MARK: Individual Weapon Types

        /// <summary>One-handed swords.</summary>
        OneHandedSword = 1L << 0,

        /// <summary>Two-handed swords.</summary>
        TwoHandedSword = 1L << 1,

        /// <summary>One-handed axes.</summary>
        OneHandedAxe = 1L << 2,

        /// <summary>Two-handed axes.</summary>
        TwoHandedAxe = 1L << 3,

        /// <summary>One-handed maces.</summary>
        OneHandedMace = 1L << 4,

        /// <summary>Two-handed maces.</summary>
        TwoHandedMace = 1L << 5,

        /// <summary>One-handed polearms (short spears, javelins used as melee).</summary>
        OneHandedPolearm = 1L << 6,

        /// <summary>Two-handed polearms (pikes, lances, long spears).</summary>
        TwoHandedPolearm = 1L << 7,

        /// <summary>Daggers and knives.</summary>
        Dagger = 1L << 8,

        /// <summary>Throwing axes.</summary>
        ThrowingAxe = 1L << 9,

        /// <summary>Throwing knives.</summary>
        ThrowingKnife = 1L << 10,

        /// <summary>Javelins and throwing spears.</summary>
        Javelin = 1L << 11,

        /// <summary>Bows (standard bows, longbows).</summary>
        Bow = 1L << 12,

        /// <summary>Crossbows.</summary>
        Crossbow = 1L << 13,

        /// <summary>Arrows for bows.</summary>
        Arrow = 1L << 14,

        /// <summary>Bolts for crossbows.</summary>
        Bolt = 1L << 15,

        /// <summary>Shields (all types).</summary>
        Shield = 1L << 16,

        /// <summary>Stones (throwing).</summary>
        Stone = 1L << 17,

        /// <summary>Muskets and firearms.</summary>
        Musket = 1L << 18,

        /// <summary>Pistols.</summary>
        Pistol = 1L << 19,

        /// <summary>Bullets for firearms.</summary>
        Bullet = 1L << 20,

        /// <summary>Banner items.</summary>
        Banner = 1L << 21,

        // MARK: Future Expansion Types (Heroes should not receive these)

        /// <summary>Slings (ranged weapon type in native).</summary>
        Sling = 1L << 22,

        /// <summary>Sling stones (ammo for slings).</summary>
        SlingStone = 1L << 23,

        /// <summary>Rifles (for mods with firearms).</summary>
        Rifle = 1L << 24,

        /// <summary>Siege boulders (siege weapon ammo).</summary>
        Boulder = 1L << 25,

        /// <summary>Ballista bolts (siege weapon ammo).</summary>
        BallistaBolt = 1L << 26,

        // MARK: Weapon Type Groups

        /// <summary>All one-handed melee weapons (swords, axes, maces, polearms, daggers).</summary>
        AllOneHanded = OneHandedSword | OneHandedAxe | OneHandedMace | OneHandedPolearm | Dagger,

        /// <summary>All two-handed melee weapons (swords, axes, maces, polearms).</summary>
        AllTwoHanded = TwoHandedSword | TwoHandedAxe | TwoHandedMace | TwoHandedPolearm,

        /// <summary>All swords (one-handed and two-handed).</summary>
        AllSwords = OneHandedSword | TwoHandedSword,

        /// <summary>All axes (one-handed and two-handed, not throwing).</summary>
        AllAxes = OneHandedAxe | TwoHandedAxe,

        /// <summary>All maces (one-handed and two-handed).</summary>
        AllMaces = OneHandedMace | TwoHandedMace,

        /// <summary>All polearms (one-handed and two-handed).</summary>
        AllPolearms = OneHandedPolearm | TwoHandedPolearm,

        /// <summary>All throwing weapons (axes, knives, javelins, stones).</summary>
        AllThrowing = ThrowingAxe | ThrowingKnife | Javelin | Stone,

        /// <summary>All ranged weapons (bows, crossbows, slings).</summary>
        AllRanged = Bow | Crossbow | Sling,

        /// <summary>All ammunition (arrows, bolts, bullets, sling stones).</summary>
        AllAmmo = Arrow | Bolt | Bullet | SlingStone,

        /// <summary>All siege weapon ammo (boulders, ballista bolts).</summary>
        AllSiegeAmmo = Boulder | BallistaBolt,

        /// <summary>All firearms (muskets, pistols, rifles).</summary>
        AllFirearms = Musket | Pistol | Rifle,

        /// <summary>All melee weapons (one-handed and two-handed).</summary>
        AllMelee = AllOneHanded | AllTwoHanded,

        /// <summary>All distance weapons (ranged, throwing, firearms).</summary>
        AllDistance = AllRanged | AllThrowing | AllFirearms,

        /// <summary>All weapon types combined.</summary>
        All = AllMelee | AllDistance | Shield | AllAmmo | Banner
    }
}
