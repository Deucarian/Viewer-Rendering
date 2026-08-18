using System;

namespace Deucarian.ViewerRendering
{
    /// <summary>Visual treatment applied to the reference viewer.</summary>
    public enum ViewerRenderingMode
    {
        ColorFaithful = 0,
        Realistic = 1
    }

    /// <summary>Origin of an observable display-settings change.</summary>
    public enum ViewerDisplaySettingsChangeSource
    {
        Initialization = 0,
        ViewerUi = 1,
        Host = 2,
        QualityChange = 3
    }

    /// <summary>Atomic display-settings mutation request.</summary>
    public readonly struct ViewerDisplaySettingsRequest
    {
        public ViewerDisplaySettingsRequest(
            ViewerRenderingMode? renderingMode,
            bool? cameraRelativeLight)
        {
            RenderingMode = renderingMode;
            CameraRelativeLight = cameraRelativeLight;
        }

        public ViewerRenderingMode? RenderingMode { get; }
        public bool? CameraRelativeLight { get; }
        public bool HasChanges =>
            RenderingMode.HasValue || CameraRelativeLight.HasValue;
    }

    /// <summary>Immutable current state published by the rendering controller.</summary>
    public readonly struct ViewerDisplaySettingsSnapshot :
        IEquatable<ViewerDisplaySettingsSnapshot>
    {
        public ViewerDisplaySettingsSnapshot(
            ViewerRenderingMode renderingMode,
            bool cameraRelativeLight,
            bool effectsActive)
        {
            RenderingMode = renderingMode;
            CameraRelativeLight = cameraRelativeLight;
            EffectsActive = effectsActive;
        }

        public ViewerRenderingMode RenderingMode { get; }
        public bool CameraRelativeLight { get; }
        public bool EffectsActive { get; }

        public bool Equals(ViewerDisplaySettingsSnapshot other)
        {
            return RenderingMode == other.RenderingMode
                   && CameraRelativeLight == other.CameraRelativeLight
                   && EffectsActive == other.EffectsActive;
        }

        public override bool Equals(object obj)
        {
            return obj is ViewerDisplaySettingsSnapshot other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int)RenderingMode;
                hashCode = (hashCode * 397) ^ CameraRelativeLight.GetHashCode();
                hashCode = (hashCode * 397) ^ EffectsActive.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(
            ViewerDisplaySettingsSnapshot left,
            ViewerDisplaySettingsSnapshot right) => left.Equals(right);

        public static bool operator !=(
            ViewerDisplaySettingsSnapshot left,
            ViewerDisplaySettingsSnapshot right) => !left.Equals(right);
    }

    /// <summary>
    /// Stable consumer-neutral wire values for display settings. Transport and
    /// command ownership remain outside this package.
    /// </summary>
    public static class ViewerDisplaySettingsPayload
    {
        public const string ColorFaithful = "color_faithful";
        public const string Realistic = "realistic";

        public static string ToPayloadValue(ViewerRenderingMode mode)
        {
            return mode == ViewerRenderingMode.Realistic
                ? Realistic
                : ColorFaithful;
        }

        public static string ToPayloadValue(
            ViewerDisplaySettingsChangeSource source)
        {
            switch (source)
            {
                case ViewerDisplaySettingsChangeSource.ViewerUi:
                    return "viewer_ui";
                case ViewerDisplaySettingsChangeSource.Host:
                    return "host";
                case ViewerDisplaySettingsChangeSource.QualityChange:
                    return "quality_change";
                default:
                    return "initialization";
            }
        }

        public static bool TryParseRenderingMode(
            string value,
            out ViewerRenderingMode mode)
        {
            mode = ViewerRenderingMode.ColorFaithful;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case ColorFaithful:
                    mode = ViewerRenderingMode.ColorFaithful;
                    return true;
                case Realistic:
                    mode = ViewerRenderingMode.Realistic;
                    return true;
                default:
                    return false;
            }
        }
    }
}
