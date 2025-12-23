using TaleWorlds.CampaignSystem;

namespace Bannerlord.GameMaster.Kingdoms
{
	public static class KingdomTributeExtensions
	{
		/// <summary>
		/// Makes the kingdom pay the specified kingdom a daily tribute. Tributes between 2 kingdoms override eachother and do not accumulate<br />
		/// There can be multiple tributes between different kingdom, just not the same 2 kingdoms <br />
		/// A tribute can be canceled by setting a new tribute between the two kingdoms of 0 or by making the receiver pay the payer
		/// </summary>
		public static TributeInfo PayTribute(this Kingdom kingdom, Kingdom otherKingdom, int dailyAmount, int days)
		{
			TributeInfo tributeInfo = GetTributeInfo(kingdom, otherKingdom);
			tributeInfo.StanceLink.SetDailyTributePaid(kingdom, dailyAmount, days);
			
			return tributeInfo;
		}

		/// <summary>
		/// Returns a TributeInfo object containing information relating to any existing tributes between the two kingdoms
		/// </summary>
		/// <param name="kingdom"></param>
		/// <param name="otherKingdom"></param>
		/// <returns></returns>
		public static TributeInfo GetTributeInfo(this Kingdom kingdom, Kingdom otherKingdom)
		{
			TributeInfo tributeInfo = new(kingdom, otherKingdom);
			return tributeInfo;
		}
	}
	
	/// <summary>
	/// A struct containing tribute information between two kingdoms
	/// </summary>
	public struct TributeInfo
	{
		public Kingdom Kingdom { get; }
		public Kingdom OtherKingdom { get; }

		/// <summary>
		/// Indicates whether this TributeInfo has valid kingdoms.
		/// Invalid when: Kingdom is null, OtherKingdom is null, or both are the same kingdom.
		/// </summary>
		public bool IsValid => Kingdom != null && OtherKingdom != null && Kingdom != OtherKingdom;

		public StanceLink StanceLink => IsValid ? Kingdom.GetStanceWith(OtherKingdom) : null;

		public int KingdomToOtherDailyAmmount => IsValid ? StanceLink.GetDailyTributeToPay(Kingdom) : 0;
		public int KingdomToOtherTotalPaid => IsValid ? StanceLink.GetTotalTributePaid(Kingdom) : 0;
		public int OtherToKingdomDailyAmmount => IsValid ? StanceLink.GetDailyTributeToPay(OtherKingdom) : 0;
		public int OtherToKingdomTotalPaid => IsValid ? StanceLink.GetTotalTributePaid(OtherKingdom) : 0;
		public int RemainingDaysBoth => IsValid ? StanceLink.GetRemainingTributePaymentCount() : 0;

		/// <summary>
		/// Creates a TributeInfo for the specified kingdoms.
		/// Check IsValid property before accessing tribute data.
		/// </summary>
		public TributeInfo(Kingdom kingdom, Kingdom otherKingdom)
		{
			Kingdom = kingdom;
			OtherKingdom = otherKingdom;
		}

		/// <summary>
		/// Returns a message string showing which kingdom is paying which and how much
		/// </summary>
		/// <returns></returns>
		public string GetTributeString()
		{
			string tributeString = $"No tribute being paid between {Kingdom.Name} and {OtherKingdom.Name}";
			
			if (RemainingDaysBoth > 0)
			{	
				if (KingdomToOtherDailyAmmount > OtherToKingdomDailyAmmount)
					tributeString = $"{Kingdom.Name} is paying tribute to {OtherKingdom.Name}\n";

				else
					tributeString = $"{OtherKingdom.Name} is paying tribute to {Kingdom.Name}\n";

				tributeString += $"Daily Amount: {KingdomToOtherDailyAmmount}, Remaining Days: {RemainingDaysBoth}, Total Paid: {KingdomToOtherTotalPaid}";
			}
			
			return tributeString;
		}
	}
}