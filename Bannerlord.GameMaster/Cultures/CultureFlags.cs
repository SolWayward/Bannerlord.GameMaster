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

        // Bandit Cultures (bits 8-14)
        Looters = 1 << 8,         
        DesertBandits = 1 << 9,   
        ForestBandits = 1 << 10,  
        MountainBandits = 1 << 11,
        SteppeBandits = 1 << 12,  
        SeaRaiders = 1 << 13,     
        Corsairs = 1 << 14,       
        AllBanditCultures = Looters | DesertBandits | ForestBandits |
                            MountainBandits | SteppeBandits | SeaRaiders | Corsairs,

        // Special Cultures (bits 15-16)
        DarshiSpecial = 1 << 15,
        VakkenSpecial = 1 << 16,

        // All combinations
        AllCultures = AllMainCultures | AllBanditCultures |
                      DarshiSpecial | VakkenSpecial
    }
}