using System;

namespace Bannerlord.GameMaster.Cultures
{
    [Flags]
    public enum CultureFlags
    {
        None = 0,

        // Main Cultures (bits 0-7)
        Calradian = 1 << 0, 
        Aserai = 1 << 1,    
        Battania = 1 << 2,  
        Empire = 1 << 3,    
        Khuzait = 1 << 4,   
        Nord = 1 << 5,      
        Sturgia = 1 << 6,   
        Vlandia = 1 << 7,   
        AllMainCultures = Calradian | Aserai | Battania | Empire |
                          Khuzait | Nord | Sturgia | Vlandia,

        // Bandit Cultures (bits 8-15)
        Looters = 1 << 8,
        Deserters = 1 << 9,
        DesertBandits = 1 << 10,
        ForestBandits = 1 << 11,
        MountainBandits = 1 << 12,
        SteppeBandits = 1 << 13,
        SeaRaiders = 1 << 14,
        Corsairs = 1 << 15,
        AllBanditCultures = Looters | Deserters | DesertBandits | ForestBandits |
                            MountainBandits | SteppeBandits | SeaRaiders | Corsairs,

        // Special Cultures (bits 16-17)
        DarshiSpecial = 1 << 16,
        VakkenSpecial = 1 << 17,

        // All combinations
        AllCultures = AllMainCultures | AllBanditCultures |
                      DarshiSpecial | VakkenSpecial
    }
}