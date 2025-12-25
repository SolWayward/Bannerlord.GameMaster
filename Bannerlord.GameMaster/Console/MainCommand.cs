using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Kingdoms;
using Bannerlord.GameMaster.Settlements;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
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
			if (args.Count < 1)
				return "Require args: <settlement>";

			Settlement settlement = SettlementQueries.QuerySettlements(args[0]).First();


			Kingdom kingdom = KingdomGenerator.CreateKingdom(settlement);

			if (kingdom != null)
				return kingdom.FormattedDetails();
			else
				return "Failed to create kingdom";
		}
	}
}