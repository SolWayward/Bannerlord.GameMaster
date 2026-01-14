using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Kingdoms;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands
{
	[CommandLineFunctionality.CommandLineArgumentFunction("kingdom", "gm")]
	public static class KingdomDiplomacyCommands
	{
		#region War and Peace

		/// <summary>
		/// Declare war between two kingdoms
		/// Usage: gm.kingdom.declare_war [kingdom1] [kingdom2]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("declare_war", "gm.kingdom")]
		public static string DeclareWar(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignState(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
									"gm.kingdom.declare_war", "<kingdom1> <kingdom2>",
									"Declares war between two kingdoms.\n" +
									"Supports named arguments: kingdom1:empire kingdom2:battania",
									"gm.kingdom.declare_war empire battania");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("kingdom1", true),
					new CommandBase.ArgumentDefinition("kingdom2", true)
				);

				// Validate
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 2)
					return usageMessage;

				// Parse kingdom1
				string kingdom1Arg = parsedArgs.GetArgument("kingdom1", 0);
				if (kingdom1Arg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'kingdom1'.");

				var (kingdom1, kingdom1Error) = CommandBase.FindSingleKingdom(kingdom1Arg);
				if (kingdom1Error != null) return kingdom1Error;

				// Parse kingdom2
				string kingdom2Arg = parsedArgs.GetArgument("kingdom2", 1);
				if (kingdom2Arg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'kingdom2'.");

				var (kingdom2, kingdom2Error) = CommandBase.FindSingleKingdom(kingdom2Arg);
				if (kingdom2Error != null) return kingdom2Error;

				if (kingdom1 == kingdom2)
					return CommandBase.FormatErrorMessage("A kingdom cannot declare war on itself.");

				if (FactionManager.IsAtWarAgainstFaction(kingdom1, kingdom2))
					return CommandBase.FormatErrorMessage($"{kingdom1.Name} and {kingdom2.Name} are already at war.");

				// Build display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "kingdom1", kingdom1.Name.ToString() },
					{ "kingdom2", kingdom2.Name.ToString() }
				};
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("declare_war", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
							{
					DeclareWarAction.ApplyByDefault(kingdom1, kingdom2);
					return argumentDisplay + CommandBase.FormatSuccessMessage($"War declared between {kingdom1.Name} and {kingdom2.Name}.");
				}, "Failed to declare war");
			});
		}

		/// <summary>
		/// Make peace between two kingdoms
		/// Usage: gm.kingdom.make_peace [kingdom1] [kingdom2]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("make_peace", "gm.kingdom")]
		public static string MakePeace(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignState(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
									"gm.kingdom.make_peace", "<kingdom1> <kingdom2>",
									"Makes peace between two kingdoms.\n" +
									"Supports named arguments: kingdom1:empire kingdom2:battania",
									"gm.kingdom.make_peace empire battania");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("kingdom1", true),
					new CommandBase.ArgumentDefinition("kingdom2", true)
				);

				// Validate
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 2)
					return usageMessage;

				// Parse kingdom1
				string kingdom1Arg = parsedArgs.GetArgument("kingdom1", 0);
				if (kingdom1Arg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'kingdom1'.");

				var (kingdom1, kingdom1Error) = CommandBase.FindSingleKingdom(kingdom1Arg);
				if (kingdom1Error != null) return kingdom1Error;

				// Parse kingdom2
				string kingdom2Arg = parsedArgs.GetArgument("kingdom2", 1);
				if (kingdom2Arg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'kingdom2'.");

				var (kingdom2, kingdom2Error) = CommandBase.FindSingleKingdom(kingdom2Arg);
				if (kingdom2Error != null) return kingdom2Error;

				if (kingdom1 == kingdom2)
					return CommandBase.FormatErrorMessage("A kingdom cannot make peace with itself.");

				if (!FactionManager.IsAtWarAgainstFaction(kingdom1, kingdom2))
					return CommandBase.FormatErrorMessage($"{kingdom1.Name} and {kingdom2.Name} are not at war.");

				// Build display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "kingdom1", kingdom1.Name.ToString() },
					{ "kingdom2", kingdom2.Name.ToString() }
				};
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("make_peace", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
							{
					MakePeaceAction.Apply(kingdom1, kingdom2);
					return argumentDisplay + CommandBase.FormatSuccessMessage($"Peace established between {kingdom1.Name} and {kingdom2.Name}.");
				}, "Failed to make peace");
			});
		}

		#endregion

		#region Alliances

		/// <summary>
		/// Declare alliance between two kingdoms
		/// Usage: gm.kingdom.declare_alliance [proposingKingdom] [receivingKingdom] [callToWar]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("declare_alliance", "gm.kingdom")]
		public static string DeclareAlliance(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignState(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
									"gm.kingdom.declare_alliance", "<proposingKingdom> <receivingKingdom> [callToWar]",
									"Declares an alliance between two kingdoms.\n" +
									"callToWar (optional): If true, receiving kingdom declares war on proposing kingdom's enemies (default: true)\n" +
									"Supports named arguments: proposingKingdom:empire receivingKingdom:battania callToWar:false",
									"gm.kingdom.declare_alliance empire battania\n" +
									"gm.kingdom.declare_alliance proposingKingdom:empire receivingKingdom:battania callToWar:false");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("proposingKingdom", true),
					new CommandBase.ArgumentDefinition("receivingKingdom", true),
					new CommandBase.ArgumentDefinition("callToWar", false, "true")
				);

				// Validate
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 2)
					return usageMessage;

				// Parse proposingKingdom
				string proposingKingdomArg = parsedArgs.GetArgument("proposingKingdom", 0);
				if (proposingKingdomArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'proposingKingdom'.");

				var (proposingKingdom, proposingError) = CommandBase.FindSingleKingdom(proposingKingdomArg);
				if (proposingError != null) return proposingError;

				// Parse receivingKingdom
				string receivingKingdomArg = parsedArgs.GetArgument("receivingKingdom", 1);
				if (receivingKingdomArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'receivingKingdom'.");

				var (receivingKingdom, receivingError) = CommandBase.FindSingleKingdom(receivingKingdomArg);
				if (receivingError != null) return receivingError;

				// Parse optional callToWar
				bool callToWar = true;
				string callToWarArg = parsedArgs.GetArgument("callToWar", 2);
				if (callToWarArg != null)
				{
					if (!bool.TryParse(callToWarArg, out callToWar))
						return CommandBase.FormatErrorMessage($"Invalid value for 'callToWar': '{callToWarArg}'. Must be true or false.");
				}

				if (proposingKingdom == receivingKingdom)
					return CommandBase.FormatErrorMessage("A kingdom cannot form an alliance with itself.");

				if (FactionManager.IsAtWarAgainstFaction(proposingKingdom, receivingKingdom))
					return CommandBase.FormatErrorMessage($"{proposingKingdom.Name} and {receivingKingdom.Name} are at war. Make peace first.");

				if (proposingKingdom.IsAllyWith(receivingKingdom))
					return CommandBase.FormatErrorMessage($"{proposingKingdom.Name} and {receivingKingdom.Name} are already allies.");

				// Build display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "proposingKingdom", proposingKingdom.Name.ToString() },
					{ "receivingKingdom", receivingKingdom.Name.ToString() },
					{ "callToWar", callToWar.ToString() }
				};
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("declare_alliance", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					proposingKingdom.DeclareAlliance(receivingKingdom, callToWar);
					
					string message = $"Alliance formed between {proposingKingdom.Name} and {receivingKingdom.Name}.";
					if (callToWar && proposingKingdom.FactionsAtWarWith.Count > 0)
						message += $"\n{receivingKingdom.Name} called to war against {proposingKingdom.Name}'s enemies.";
					
					return argumentDisplay + CommandBase.FormatSuccessMessage(message);
				}, "Failed to declare alliance");
			});
		}

		/// <summary>
		/// Call ally to war against enemies
		/// Usage: gm.kingdom.call_ally_to_war [proposingKingdom] [allyKingdom] [enemyKingdom]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("call_ally_to_war", "gm.kingdom")]
		public static string CallAllyToWar(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignState(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
									"gm.kingdom.call_ally_to_war", "<proposingKingdom> <allyKingdom> [enemyKingdom]",
									"Calls ally kingdom to war against specified enemy or all enemies.\n" +
									"If enemyKingdom is omitted, ally declares war on all of proposer's enemies.\n" +
									"Supports named arguments: proposingKingdom:empire allyKingdom:battania enemyKingdom:sturgia",
									"gm.kingdom.call_ally_to_war empire battania sturgia\n" +
									"gm.kingdom.call_ally_to_war empire battania");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("proposingKingdom", true),
					new CommandBase.ArgumentDefinition("allyKingdom", true),
					new CommandBase.ArgumentDefinition("enemyKingdom", false, "All enemies")
				);

				// Validate
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 2)
					return usageMessage;

				// Parse proposingKingdom
				string proposingKingdomArg = parsedArgs.GetArgument("proposingKingdom", 0);
				if (proposingKingdomArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'proposingKingdom'.");

				var (proposingKingdom, proposingError) = CommandBase.FindSingleKingdom(proposingKingdomArg);
				if (proposingError != null) return proposingError;

				// Parse allyKingdom
				string allyKingdomArg = parsedArgs.GetArgument("allyKingdom", 1);
				if (allyKingdomArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'allyKingdom'.");

				var (allyKingdom, allyError) = CommandBase.FindSingleKingdom(allyKingdomArg);
				if (allyError != null) return allyError;

				if (proposingKingdom == allyKingdom)
					return CommandBase.FormatErrorMessage("A kingdom cannot call itself to war.");

				if (!proposingKingdom.IsAllyWith(allyKingdom))
					return CommandBase.FormatErrorMessage($"{proposingKingdom.Name} and {allyKingdom.Name} are not allies.");

				// Parse optional enemyKingdom
				string enemyKingdomArg = parsedArgs.GetArgument("enemyKingdom", 2);
				Kingdom enemyKingdom = null;
				
				if (enemyKingdomArg != null)
				{
					var (enemy, enemyError) = CommandBase.FindSingleKingdom(enemyKingdomArg);
					if (enemyError != null) return enemyError;
					enemyKingdom = enemy;

					if (!FactionManager.IsAtWarAgainstFaction(proposingKingdom, enemyKingdom))
						return CommandBase.FormatErrorMessage($"{proposingKingdom.Name} is not at war with {enemyKingdom.Name}.");

					if (FactionManager.IsAtWarAgainstFaction(allyKingdom, enemyKingdom))
						return CommandBase.FormatErrorMessage($"{allyKingdom.Name} is already at war with {enemyKingdom.Name}.");
				}
				else
				{
					// Check if proposing kingdom has any enemies
					if (proposingKingdom.FactionsAtWarWith.Count == 0)
						return CommandBase.FormatErrorMessage($"{proposingKingdom.Name} is not at war with any kingdoms.");
				}

				// Build display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "proposingKingdom", proposingKingdom.Name.ToString() },
					{ "allyKingdom", allyKingdom.Name.ToString() },
					{ "enemyKingdom", enemyKingdom != null ? enemyKingdom.Name.ToString() : "All enemies" }
				};
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("call_ally_to_war", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					if (enemyKingdom != null)
					{
						// Call to war against specific enemy
						proposingKingdom.ProposeCallAllyToWarForceAccept(allyKingdom, enemyKingdom);
						return argumentDisplay + CommandBase.FormatSuccessMessage(
							$"{allyKingdom.Name} called to war against {enemyKingdom.Name}.");
					}
					else
					{
						// Call to war against all enemies
						var enemies = proposingKingdom.FactionsAtWarWith
							.Where(f => f.IsKingdomFaction)
							.Cast<Kingdom>()
							.ToList();
						
						proposingKingdom.ProposeCallAllyToWarForceAccept(allyKingdom);
						
						string enemyList = string.Join(", ", enemies.Select(k => k.Name.ToString()));
						return argumentDisplay + CommandBase.FormatSuccessMessage(
							$"{allyKingdom.Name} called to war against all enemies: {enemyList}");
					}
				}, "Failed to call ally to war");
			});
		}

		#endregion

		#region Trade and Tribute

		/// <summary>
		/// Make trade agreement between two kingdoms
		/// Usage: gm.kingdom.trade_agreement [proposingKingdom] [receivingKingdom]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("trade_agreement", "gm.kingdom")]
		public static string TradeAgreement(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignState(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
									"gm.kingdom.trade_agreement", "<proposingKingdom> <receivingKingdom>",
									"Creates a trade agreement between two kingdoms.\n" +
									"Supports named arguments: proposingKingdom:empire receivingKingdom:battania",
									"gm.kingdom.trade_agreement empire battania");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("proposingKingdom", true),
					new CommandBase.ArgumentDefinition("receivingKingdom", true)
				);

				// Validate
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 2)
					return usageMessage;

				// Parse proposingKingdom
				string proposingKingdomArg = parsedArgs.GetArgument("proposingKingdom", 0);
				if (proposingKingdomArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'proposingKingdom'.");

				var (proposingKingdom, proposingError) = CommandBase.FindSingleKingdom(proposingKingdomArg);
				if (proposingError != null) return proposingError;

				// Parse receivingKingdom
				string receivingKingdomArg = parsedArgs.GetArgument("receivingKingdom", 1);
				if (receivingKingdomArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'receivingKingdom'.");

				var (receivingKingdom, receivingError) = CommandBase.FindSingleKingdom(receivingKingdomArg);
				if (receivingError != null) return receivingError;

				if (proposingKingdom == receivingKingdom)
					return CommandBase.FormatErrorMessage("A kingdom cannot make a trade agreement with itself.");

				if (FactionManager.IsAtWarAgainstFaction(proposingKingdom, receivingKingdom))
					return CommandBase.FormatErrorMessage($"{proposingKingdom.Name} and {receivingKingdom.Name} are at war. Make peace first.");

				// Build display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "proposingKingdom", proposingKingdom.Name.ToString() },
					{ "receivingKingdom", receivingKingdom.Name.ToString() }
				};
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("trade_agreement", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					proposingKingdom.MakeTradeAgreement(receivingKingdom);
					return argumentDisplay + CommandBase.FormatSuccessMessage(
						$"Trade agreement established between {proposingKingdom.Name} and {receivingKingdom.Name}.");
				}, "Failed to make trade agreement");
			});
		}

		/// <summary>
		/// Make kingdom pay tribute to another kingdom
		/// Usage: gm.kingdom.pay_tribute [payingKingdom] [receivingKingdom] [dailyAmount] [days]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("pay_tribute", "gm.kingdom")]
		public static string PayTribute(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignState(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
									"gm.kingdom.pay_tribute", "<payingKingdom> <receivingKingdom> <dailyAmount> <days>",
									"Makes one kingdom pay tribute to another kingdom.\n" +
									"dailyAmount: Amount of gold paid per day\n" +
									"days: Number of days the tribute will be paid\n" +
									"Supports named arguments: payingKingdom:battania receivingKingdom:empire dailyAmount:100 days:30",
									"gm.kingdom.pay_tribute battania empire 100 30");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("payingKingdom", true),
					new CommandBase.ArgumentDefinition("receivingKingdom", true),
					new CommandBase.ArgumentDefinition("dailyAmount", true),
					new CommandBase.ArgumentDefinition("days", true)
				);

				// Validate
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 4)
					return usageMessage;

				// Parse payingKingdom
				string payingKingdomArg = parsedArgs.GetArgument("payingKingdom", 0);
				if (payingKingdomArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'payingKingdom'.");

				var (payingKingdom, payingError) = CommandBase.FindSingleKingdom(payingKingdomArg);
				if (payingError != null) return payingError;

				// Parse receivingKingdom
				string receivingKingdomArg = parsedArgs.GetArgument("receivingKingdom", 1);
				if (receivingKingdomArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'receivingKingdom'.");

				var (receivingKingdom, receivingError) = CommandBase.FindSingleKingdom(receivingKingdomArg);
				if (receivingError != null) return receivingError;

				if (payingKingdom == receivingKingdom)
					return CommandBase.FormatErrorMessage("A kingdom cannot pay tribute to itself.");

				// Parse dailyAmount
				string dailyAmountArg = parsedArgs.GetArgument("dailyAmount", 2);
				if (dailyAmountArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'dailyAmount'.");

				if (!int.TryParse(dailyAmountArg, out int dailyAmount))
					return CommandBase.FormatErrorMessage($"Invalid value for 'dailyAmount': '{dailyAmountArg}'. Must be an integer.");

				if (dailyAmount < 0)
					return CommandBase.FormatErrorMessage("Daily amount cannot be negative.");

				// Parse days
				string daysArg = parsedArgs.GetArgument("days", 3);
				if (daysArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'days'.");

				if (!int.TryParse(daysArg, out int days))
					return CommandBase.FormatErrorMessage($"Invalid value for 'days': '{daysArg}'. Must be an integer.");

				if (days <= 0)
					return CommandBase.FormatErrorMessage("Days must be greater than 0.");

				// Build display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "payingKingdom", payingKingdom.Name.ToString() },
					{ "receivingKingdom", receivingKingdom.Name.ToString() },
					{ "dailyAmount", dailyAmount.ToString() },
					{ "days", days.ToString() }
				};
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("pay_tribute", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					TributeInfo tributeInfo = payingKingdom.PayTribute(receivingKingdom, dailyAmount, days);
					
					string message = $"{payingKingdom.Name} will pay {dailyAmount} gold per day to {receivingKingdom.Name} for {days} days.\n" +
									$"Total tribute: {dailyAmount * days} gold";
					
					return argumentDisplay + CommandBase.FormatSuccessMessage(message);
				}, "Failed to establish tribute");
			});
		}

		/// <summary>
		/// Get tribute information between two kingdoms
		/// Usage: gm.kingdom.get_tribute_info [kingdomA] [kingdomB]
		/// </summary>
		[CommandLineFunctionality.CommandLineArgumentFunction("get_tribute_info", "gm.kingdom")]
		public static string GetTributeInfo(List<string> args)
		{
			return Cmd.Run(args, () =>
			{
				if (!CommandBase.ValidateCampaignState(out string error))
					return error;

				var usageMessage = CommandValidator.CreateUsageMessage(
									"gm.kingdom.get_tribute_info", "<kingdomA> <kingdomB>",
									"Displays tribute information between two kingdoms.\n" +
									"Supports named arguments: kingdomA:empire kingdomB:battania",
									"gm.kingdom.get_tribute_info empire battania");

				// Parse arguments
				var parsedArgs = CommandBase.ParseArguments(args);

				// Define valid arguments
				parsedArgs.SetValidArguments(
					new CommandBase.ArgumentDefinition("kingdomA", true),
					new CommandBase.ArgumentDefinition("kingdomB", true)
				);

				// Validate
				string validationError = parsedArgs.GetValidationError();
				if (validationError != null)
					return CommandBase.FormatErrorMessage(validationError);

				if (parsedArgs.TotalCount < 2)
					return usageMessage;

				// Parse kingdomA
				string kingdomAArg = parsedArgs.GetArgument("kingdomA", 0);
				if (kingdomAArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'kingdomA'.");

				var (kingdomA, kingdomAError) = CommandBase.FindSingleKingdom(kingdomAArg);
				if (kingdomAError != null) return kingdomAError;

				// Parse kingdomB
				string kingdomBArg = parsedArgs.GetArgument("kingdomB", 1);
				if (kingdomBArg == null)
					return CommandBase.FormatErrorMessage("Missing required argument 'kingdomB'.");

				var (kingdomB, kingdomBError) = CommandBase.FindSingleKingdom(kingdomBArg);
				if (kingdomBError != null) return kingdomBError;

				if (kingdomA == kingdomB)
					return CommandBase.FormatErrorMessage("Cannot get tribute info for a kingdom with itself.");

				// Build display
				var resolvedValues = new Dictionary<string, string>
				{
					{ "kingdomA", kingdomA.Name.ToString() },
					{ "kingdomB", kingdomB.Name.ToString() }
				};
				string argumentDisplay = parsedArgs.FormatArgumentDisplay("get_tribute_info", resolvedValues);

				return CommandBase.ExecuteWithErrorHandling(() =>
				{
					TributeInfo tributeInfo = kingdomA.GetTributeInfo(kingdomB);
					string tributeString = tributeInfo.GetTributeString();
					
					return argumentDisplay + tributeString + "\n";
				}, "Failed to get tribute info");
			});
		}

		#endregion
	}
}
