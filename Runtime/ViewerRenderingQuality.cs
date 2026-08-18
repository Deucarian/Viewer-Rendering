using System;

namespace Deucarian.ViewerRendering
{
    /// <summary>
    /// Consumer-neutral quality intent for the reusable viewer rendering stack.
    /// It is deliberately independent of Unity project quality-level indices.
    /// </summary>
    public enum ViewerRenderingQualityTier
    {
        Lightweight = 0,
        Full = 1
    }

    /// <summary>
    /// Immutable package-owned platform defaults and quality behavior.
    /// </summary>
    public readonly struct ViewerRenderingQualityProfile :
        IEquatable<ViewerRenderingQualityProfile>
    {
        public ViewerRenderingQualityProfile(
            ViewerRenderingQualityTier desktopDefaultTier,
            ViewerRenderingQualityTier webGlDefaultTier)
        {
            EnsureDefined(desktopDefaultTier, nameof(desktopDefaultTier));
            EnsureDefined(webGlDefaultTier, nameof(webGlDefaultTier));
            DesktopDefaultTier = desktopDefaultTier;
            WebGlDefaultTier = webGlDefaultTier;
        }

        public ViewerRenderingQualityTier DesktopDefaultTier { get; }
        public ViewerRenderingQualityTier WebGlDefaultTier { get; }

        public ViewerRenderingQualityTier ResolveDefaultTier(bool isWebGl) =>
            isWebGl ? WebGlDefaultTier : DesktopDefaultTier;

        public bool ShouldEnableEffects(ViewerRenderingQualityTier tier)
        {
            EnsureDefined(tier, nameof(tier));
            return tier == ViewerRenderingQualityTier.Full;
        }

        public bool ShouldEnableShadows(
            ViewerRenderingQualityTier tier,
            ViewerRenderingMode mode) =>
            mode == ViewerRenderingMode.Realistic
            && ShouldEnableEffects(tier);

        public bool Equals(ViewerRenderingQualityProfile other) =>
            DesktopDefaultTier == other.DesktopDefaultTier
            && WebGlDefaultTier == other.WebGlDefaultTier;

        public override bool Equals(object obj) =>
            obj is ViewerRenderingQualityProfile other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)DesktopDefaultTier * 397)
                       ^ (int)WebGlDefaultTier;
            }
        }

        public static bool IsDefined(ViewerRenderingQualityTier tier) =>
            tier == ViewerRenderingQualityTier.Lightweight
            || tier == ViewerRenderingQualityTier.Full;

        internal static void EnsureDefined(
            ViewerRenderingQualityTier tier,
            string parameterName)
        {
            if (!IsDefined(tier))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    tier,
                    "Viewer rendering quality tier is not supported.");
            }
        }
    }

    /// <summary>
    /// Explicit quality-tier mutation port. Project quality indices are not an
    /// input to this contract.
    /// </summary>
    public interface IViewerRenderingQualityController
    {
        ViewerRenderingQualityTier ActiveQualityTier { get; }

        void ApplyQualityTier(
            ViewerRenderingQualityTier tier,
            ViewerDisplaySettingsChangeSource source);
    }
}

