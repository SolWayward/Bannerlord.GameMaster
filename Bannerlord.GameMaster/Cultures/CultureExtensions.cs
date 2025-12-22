using TaleWorlds.CampaignSystem;

namespace Bannerlord.GameMaster.Cultures
{
    public static class CultureExtensions
    {
        /// <summary>
        /// Gets the CultureFlag enum value that corresponds to this culture object.
        /// </summary>
        /// <param name="culture">The culture object to convert.</param>
        /// <returns>The corresponding CultureFlag value, or CultureFlags.None if not found or null.</returns>
        public static CultureFlags ToCultureFlag(this CultureObject culture)
        {
            return CultureLookup.GetCultureFlag(culture);
        }
    }
}
