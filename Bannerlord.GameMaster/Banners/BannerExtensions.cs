using TaleWorlds.Core;

namespace Bannerlord.GameMaster.Banners
{
    /// <summary>
    /// Extension methods for Banner objects.
    /// </summary>
    public static class BannerExtensions
    {
        /// MARK: ApplyRandomScheme
        /// <summary>
        /// Applies a random color scheme to the banner with high-contrast emblem.
        /// Automatically selects the best theme based on the randomly chosen main color's luminance.
        /// For lighter main colors, uses lighter secondary and darker emblem.
        /// For darker main colors, uses darker secondary and lighter emblem.
        /// </summary>
        /// <param name="banner">The banner to apply colors to.</param>
        /// <returns>The banner instance for method chaining.</returns>
        public static Banner ApplyRandomColorScheme(this Banner banner)
        {
            BannerColorPicker.GetBannerColorScheme(
                out int primaryColorId,
                out int secondaryColorId,
                out int iconColorId);

            banner.SetPrimaryColorId(primaryColorId);
            banner.SetSecondaryColorId(secondaryColorId);
            banner.SetIconColorId(iconColorId);

            return banner;
        }

        /// MARK: ApplyColorScheme
        /// <summary>
        /// Applies a color scheme to the banner using a specific primary color with high-contrast emblem.
        /// Automatically selects the best theme based on the provided main color's luminance.
        /// For lighter main colors (luminance &gt; 0.6), uses lighter secondary and darker emblem.
        /// For darker main colors (luminance &lt;= 0.6), uses darker secondary and lighter emblem.
        /// </summary>
        /// <param name="banner">The banner to apply colors to.</param>
        /// <param name="primaryColorId">The primary (main background) color ID to use.</param>
        /// <returns>The banner instance for method chaining.</returns>
        public static Banner ApplyColorScheme(this Banner banner, int primaryColorId)
        {
            BannerColorPicker.GetBannerColorScheme(
                primaryColorId,
                out int secondaryColorId,
                out int iconColorId);

            banner.SetPrimaryColorId(primaryColorId);
            banner.SetSecondaryColorId(secondaryColorId);
            banner.SetIconColorId(iconColorId);

            return banner;
        }

        /// MARK: ApplyAltColorScheme
        /// <summary>
        /// Applies an alternative random color scheme to the banner with high-contrast emblem.
        /// Always uses lighter secondary background and darker emblem regardless of main color luminance.
        /// Best for creating banners with lighter overall appearance.
        /// </summary>
        /// <param name="banner">The banner to apply colors to.</param>
        /// <returns>The banner instance for method chaining.</returns>
        public static Banner ApplyAlternativeColorScheme(this Banner banner)
        {
            BannerColorPicker.GetAlternativeBannerColorScheme(
                out int primaryColorId,
                out int secondaryColorId,
                out int iconColorId);

            banner.SetPrimaryColorId(primaryColorId);
            banner.SetSecondaryColorId(secondaryColorId);
            banner.SetIconColorId(iconColorId);

            return banner;
        }

        /// MARK: ApplyAltColorScheme 
        /// <summary>
        /// Applies an alternative color scheme to the banner using a specific primary color with high-contrast emblem.
        /// Always uses lighter secondary background and darker emblem regardless of main color luminance.
        /// Best for creating banners with lighter overall appearance.
        /// </summary>
        /// <param name="banner">The banner to apply colors to.</param>
        /// <param name="primaryColorId">The primary (main background) color ID to use.</param>
        /// <returns>The banner instance for method chaining.</returns>
        public static Banner ApplyAlternativeColorScheme(this Banner banner, int primaryColorId)
        {
            BannerColorPicker.GetAlternativeBannerColorScheme(
                primaryColorId,
                out int secondaryColorId,
                out int iconColorId);

            banner.SetPrimaryColorId(primaryColorId);
            banner.SetSecondaryColorId(secondaryColorId);
            banner.SetIconColorId(iconColorId);

            return banner;
        }

        /// MARK: ApplyStandardScheme
        /// <summary>
        /// Applies a standard color scheme to the banner using a specific primary color with high-contrast emblem.
        /// Always uses darker secondary background and lighter emblem regardless of main color luminance.
        /// Best for creating banners with darker overall appearance.
        /// </summary>
        /// <param name="banner">The banner to apply colors to.</param>
        /// <param name="primaryColorId">The primary (main background) color ID to use.</param>
        /// <returns>The banner instance for method chaining.</returns>
        public static Banner ApplyStandardColorScheme(this Banner banner, int primaryColorId)
        {
            BannerColorPicker.GetStandardBannerColorScheme(
                primaryColorId,
                out int secondaryColorId,
                out int iconColorId);

            banner.SetPrimaryColorId(primaryColorId);
            banner.SetSecondaryColorId(secondaryColorId);
            banner.SetIconColorId(iconColorId);

            return banner;
        }

        /// MARK: ApplyUniqueScheme
        /// <summary>
        /// Applies a unique color scheme to the banner that is distinct from existing clan colors.
        /// Automatically finds a unique primary color from the banner palette that differs from all existing clans,
        /// then applies the best theme (standard or alternative) based on that color's luminance.
        /// Ideal for creating new clans with visually distinct banners.
        /// </summary>
        /// <param name="banner">The banner to apply colors to.</param>
        /// <param name="minimumThreshold">Minimum perceptual difference from existing clans (0.0-1.0). Default is 0.15 for good distinction.</param>
        /// <returns>The banner instance for method chaining.</returns>
        public static Banner ApplyUniqueColorScheme(this Banner banner, float minimumThreshold = 0.15f)
        {
            int uniquePrimaryColorId = BannerColorPicker.GetUniqueClanColorId(minimumThreshold);

            BannerColorPicker.GetBannerColorScheme(
                uniquePrimaryColorId,
                out int secondaryColorId,
                out int iconColorId);

            banner.SetPrimaryColorId(uniquePrimaryColorId);
            banner.SetSecondaryColorId(secondaryColorId);
            banner.SetIconColorId(iconColorId);

            return banner;
        }
    }
}
