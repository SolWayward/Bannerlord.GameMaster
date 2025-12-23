using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Kingdoms;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Console
{
    /// <summary>
    /// Used for debugging and displaying useful info
    /// </summary>
	public static class MainCommand
    {
        /// <summary>
        /// Used for debugging and displaying useful info
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("info", "gm")]
		public static string GmCommand(List<string> args)
        {
            if (args.Count < 2)
                return "Error: must provide 2 arguments (kingdom and otherKingdom)";

            //needs logic to make sure kingdom exists otherwise crashes. other commands should have this follow that
            Kingdom kingdom = KingdomQueries.QueryKingdoms(args[0]).First(); //Crashes here if kingdom doesnt exist
            Kingdom otherKingdom = KingdomQueries.QueryKingdoms(args[1]).First(); 

            if (kingdom == null) // Not sufficient to prevent crash if kingdom wasnt found
                return $"Error: Unable to find kingdom matching '{args[0]}'";

            if (otherKingdom == null)
                return $"Error: Unable to find kingdom matching '{args[1]}'";

            kingdom.MakeTradeAgreement(otherKingdom);

            return $"Succesfully started a trade agreement between {kingdom} and {otherKingdom}";  
        }
    }
}