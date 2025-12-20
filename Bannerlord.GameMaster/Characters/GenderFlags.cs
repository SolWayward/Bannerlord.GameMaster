using System;

namespace Bannerlord.GameMaster.Characters
{
	[Flags]
    public enum GenderFlags
    {
        None = 0,
        Female = 1 << 0, 
        Male = 1 << 1,    
        Either = Female | Male  
    }
}