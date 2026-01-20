using System;

namespace Bannerlord.GameMaster.Characters
{
    /// <summary>
    /// Flags enum for categorizing ALL CharacterObject types across multiple dimensions.
    /// Unlike TroopTypes which is focused on combat troops only, CharacterTypes covers
    /// EVERYTHING: heroes, troops, templates, NPCs, etc.
    /// </summary>
    [Flags]
    public enum CharacterTypes : long
    {
        None = 0,

        // MARK: Character Classification (1-8)
        IsHero = 1,                // 2^0  - character.IsHero (has HeroObject)
        IsTroop = 2,               // 2^1  - !character.IsHero && IsActualTroop()
        IsTemplate = 4,            // 2^2  - StringId contains "template" or "_equipment"
        IsNPC = 8,                 // 2^3  - Non-combat NPCs (merchants, tavernkeepers, etc.)

        // MARK: Hero-Specific (16-128) - only applicable when IsHero
        IsLord = 16,               // 2^4  - HeroObject?.IsLord
        IsWanderer = 32,           // 2^5  - HeroObject?.IsWanderer
        IsNotable = 64,            // 2^6  - HeroObject?.IsNotable
        IsChild = 128,             // 2^7  - HeroObject?.IsChild OR stringId contains child/infant

        // MARK: Gender (256-512)
        IsFemale = 256,            // 2^8  - character.IsFemale
        IsMale = 512,              // 2^9  - !character.IsFemale

        // MARK: Character State (1024-2048)
        IsOriginalCharacter = 1024,// 2^10 - Not BLGM-created (no "blgm_" prefix)
        IsBlgmCreated = 2048,      // 2^11 - Has "blgm_" prefix in StringId

        // MARK: Tier-Based (4096-262144) - for non-hero characters
        Tier0 = 4096,              // 2^12 - Tier 0 characters
        Tier1 = 8192,              // 2^13 - Tier 1 characters
        Tier2 = 16384,             // 2^14 - Tier 2 characters
        Tier3 = 32768,             // 2^15 - Tier 3 characters
        Tier4 = 65536,             // 2^16 - Tier 4 characters
        Tier5 = 131072,            // 2^17 - Tier 5 characters
        Tier6Plus = 262144,        // 2^18 - Tier 6+ characters

        // MARK: Culture-Based (524288-67108864)
        Empire = 524288,           // 2^19 - Culture: Empire
        Vlandia = 1048576,         // 2^20 - Culture: Vlandia
        Sturgia = 2097152,         // 2^21 - Culture: Sturgia
        Aserai = 4194304,          // 2^22 - Culture: Aserai
        Khuzait = 8388608,         // 2^23 - Culture: Khuzait
        Battania = 16777216,       // 2^24 - Culture: Battania
        Nord = 33554432,           // 2^25 - Culture: Nord (Warsails DLC)
        Bandit = 67108864,         // 2^26 - Culture: Bandit (includes looters, etc.)
    }
}
