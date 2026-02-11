using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Banners
{
    /// <summary>
    /// Extension methods for Banner objects.
    /// </summary>
    public static class BannerExtensions
    {
        /// MARK: SetAllIconColorIds
        /// <summary>
        /// Sets the icon color for ALL icon layers in the banner (indices 1..N).
        /// Fixes the native SetIconColorId() which only updates index 1.
        /// </summary>
        /// <param name="banner">The banner to update.</param>
        /// <param name="colorId">The palette color ID to apply to all icon layers.</param>
        public static void SetAllIconColorIds(this Banner banner, int colorId)
        {
            int count = banner.GetBannerDataListCount();
            for (int i = 1; i < count; i++)
            {
                BannerData data = banner.GetBannerDataAtIndex(i);
                if (data != null)
                {
                    data.ColorId = colorId;
                    data.ColorId2 = colorId;
                }
            }
        }

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
            banner.SetAllIconColorIds(iconColorId);

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
            banner.SetAllIconColorIds(iconColorId);

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
            banner.SetAllIconColorIds(iconColorId);

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
            banner.SetAllIconColorIds(iconColorId);

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
            banner.SetAllIconColorIds(iconColorId);

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
            banner.SetAllIconColorIds(iconColorId);

            return banner;
        }

        /// MARK: ApplyUniqueKingdomScheme
        /// <summary>
        /// Applies a unique color scheme to the banner that is distinct from existing kingdom background colors.
        /// Uses deterministic max-min distance scored selection for the background color,
        /// adaptive saturation floor to prefer vivid colors, and pressure-aware icon color selection
        /// that switches between aesthetic harmony (low kingdom count) and uniqueness (high kingdom count).
        /// </summary>
        /// <param name="banner">The banner to apply colors to.</param>
        /// <returns>The banner instance for method chaining.</returns>
        public static Banner ApplyUniqueKingdomColorScheme(this Banner banner)
        {
            KingdomColorResult result = BannerColorPicker.GetUniqueKingdomColorId();

            // Get secondary background color (ignore generic icon color, we use pressure-aware selection)
            BannerColorPicker.GetBannerColorScheme(result.ColorId, out int secondaryColorId, out int _);

            // Get pressure-aware icon color
            int iconColorId = BannerColorPicker.GetUniqueKingdomIconColorId(result.ColorId, result.MinDistanceToNearest);

            banner.SetPrimaryColorId(result.ColorId);
            banner.SetSecondaryColorId(secondaryColorId);
            banner.SetAllIconColorIds(iconColorId);

            return banner;
        }

        /// MARK: ConvertToSingleIcon
        /// <summary>
        /// Strips all icon layers except the last one (the main/primary icon).
        /// The last icon is typically the primary/main icon that sits on top of all others.
        /// Does not modify icon position or rotation -- use ResetIconTransforms() for that.
        /// </summary>
        /// <param name="banner">The banner to convert.</param>
        /// <returns>The original serialized banner code before stripping, or null if the banner
        /// already had only one icon (or no icons).</returns>
        public static string ConvertToSingleIcon(this Banner banner)
        {
            int count = banner.GetBannerDataListCount();

            // count <= 2 means background + 0 or 1 icon -- nothing to strip
            if (count <= 2)
                return null;

            string originalCode = banner.Serialize();

            // Copy the last icon data (the primary/main icon on top)
            BannerData lastIcon = banner.GetBannerDataAtIndex(count - 1);
            BannerData copiedIcon = new(lastIcon);

            // ClearAllIcons keeps only background at index 0, then add our single icon
            banner.ClearAllIcons();
            banner.AddIconData(copiedIcon);

            return originalCode;
        }

        /// MARK: ResetIconTransforms
        /// <summary>
        /// Resets position to center (764, 764) and rotation to 0 for ALL icon layers.
        /// Useful for fixing rotated or off-center icons. Does not strip any icons --
        /// use ConvertToSingleIcon() for that.
        /// 764 = BannerFullSize / 2 = 1528 / 2.
        /// </summary>
        /// <param name="banner">The banner to reset transforms on.</param>
        /// <returns>The banner instance for method chaining.</returns>
        public static Banner ResetIconTransforms(this Banner banner)
        {
            int count = banner.GetBannerDataListCount();

            // Start at index 1 to skip background layer at index 0
            for (int i = 1; i < count; i++)
            {
                BannerData data = banner.GetBannerDataAtIndex(i);
                if (data != null)
                {
                    data.Position = new Vec2(764f, 764f);
                    data.RotationValue = 0f;
                }
            }

            return banner;
        }

        /// MARK: RebuildWithoutStroke
        /// <summary>
        /// Rebuilds icon layers that have DrawStroke=true as new BannerData objects
        /// with DrawStroke=false, preserving all other properties (MeshId, Size,
        /// Position, ColorId, Mirror, Rotation). Sets ColorId2=ColorId to prevent
        /// the stroke-same-as-icon-color blurring artifact. Uses serialize-deserialize
        /// cycle to create fresh BannerData objects and invalidate the visual cache
        /// (_bannerVisual=null forces regeneration on next access).
        /// </summary>
        /// <param name="banner">The banner to rebuild.</param>
        /// <returns>The banner instance for method chaining.</returns>
        public static Banner RebuildWithoutStroke(this Banner banner)
        {
            int count = banner.GetBannerDataListCount();
            bool hasStroke = false;
            for (int i = 1; i < count; i++)
            {
                BannerData data = banner.GetBannerDataAtIndex(i);
                if (data != null && data.DrawStroke)
                {
                    data.DrawStroke = false;
                    data.ColorId2 = data.ColorId;
                    hasStroke = true;
                }
            }

            if (hasStroke)
            {
                // Serialize modified data, then Deserialize to create fresh objects
                // and null _bannerVisual (forces visual regeneration)
                string fixedCode = banner.Serialize();
                banner.Deserialize(fixedCode);
            }

            return banner;
        }
    }
}
