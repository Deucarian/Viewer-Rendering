using System;
using Deucarian.Theming;
using UnityEngine;

namespace Deucarian.ViewerRendering
{
    /// <summary>
    /// Resolves the canonical reusable rendering preset and its shared theme.
    /// </summary>
    public static class ViewerRenderingReferenceComposition
    {
        public static ViewerRenderingReferenceCompositionProfile Resolve()
        {
            ViewerRenderingSettings settings =
                ViewerRenderingSettings.LoadReferencePreset();
            if (settings == null)
            {
                const string message =
                    "Viewer rendering reference settings were not found in Resources.";
                ViewerRenderingLog.Rendering.Error(message);
                throw new InvalidOperationException(message);
            }

            DeucarianViewerReferenceThemeProfile theme =
                DeucarianViewerReferenceThemePreset.Resolve();
            return new ViewerRenderingReferenceCompositionProfile(
                settings,
                new ViewerRenderingCameraProfile(
                    settings.CameraPosition,
                    settings.CameraLookTarget),
                new ViewerRenderingLightProfile(
                    settings.FixedLightEuler,
                    settings.KeyLightIntensity,
                    settings.RealisticShadowStrength,
                    settings.ShadowBias,
                    settings.ShadowNormalBias,
                    settings.ShadowNearPlane,
                    settings.CameraRelativeLightEuler,
                    settings.CameraRelativeDampingSeconds),
                new ViewerRenderingEnvironmentProfile(
                    settings.ReflectionCubemapSize,
                    settings.ReflectionIntensity,
                    settings.ColorFaithfulAmbientSrgb,
                    settings.RealisticAmbientSkySrgb,
                    settings.RealisticAmbientEquatorSrgb,
                    settings.RealisticAmbientGroundSrgb,
                    settings.ReferenceSkyTop,
                    settings.ReferenceSkyHorizon,
                    settings.ReferenceSkyBottom,
                    settings.DarkThemePrimaryStrength,
                    settings.LightThemePrimaryStrength,
                    settings.NeutralSkyPrimaryStrength,
                    settings.NeutralSkySaturationThreshold),
                new ViewerRenderingQualityProfile(
                    settings.DesktopDefaultQualityTier,
                    settings.WebGlDefaultQualityTier),
                theme,
                DeucarianViewerReferenceThemePreset.DefaultMode);
        }
    }

    /// <summary>
    /// Fully resolved reference rendering composition shared by viewer consumers.
    /// </summary>
    public readonly struct ViewerRenderingReferenceCompositionProfile
    {
        internal ViewerRenderingReferenceCompositionProfile(
            ViewerRenderingSettings settings,
            ViewerRenderingCameraProfile cameraProfile,
            ViewerRenderingLightProfile lightProfile,
            ViewerRenderingEnvironmentProfile environmentProfile,
            ViewerRenderingQualityProfile qualityProfile,
            DeucarianViewerReferenceThemeProfile themeProfile,
            DeucarianThemeMode themeMode)
        {
            Settings = settings
                ? settings
                : throw new ArgumentNullException(nameof(settings));
            ThemeProfile = themeProfile
                ?? throw new ArgumentNullException(nameof(themeProfile));
            CameraProfile = cameraProfile;
            LightProfile = lightProfile;
            EnvironmentProfile = environmentProfile;
            QualityProfile = qualityProfile;
            ThemeMode = themeMode;
        }

        public ViewerRenderingSettings Settings { get; }
        public ViewerRenderingCameraProfile CameraProfile { get; }
        public ViewerRenderingLightProfile LightProfile { get; }
        public ViewerRenderingEnvironmentProfile EnvironmentProfile { get; }
        public ViewerRenderingQualityProfile QualityProfile { get; }
        public DeucarianViewerReferenceThemeProfile ThemeProfile { get; }
        public DeucarianThemeMode ThemeMode { get; }

        public ViewerDisplaySettingsSnapshot ResolveDefaultDisplaySettings(
            bool isWebGl) =>
            new ViewerDisplaySettingsSnapshot(
                Settings.DefaultRenderingMode,
                Settings.DefaultCameraRelativeLight,
                QualityProfile.ShouldEnableEffects(
                    QualityProfile.ResolveDefaultTier(isWebGl)));

        public ViewerRenderingInstaller Compose(Transform parent) =>
            ViewerRenderingInstaller.CreateWithReferenceComposition(
                parent,
                null,
                null,
                null,
                this);

        public ViewerRenderingInstaller Compose(
            Transform parent,
            Camera camera,
            Light keyLight,
            DeucarianThemeProvider themeProvider) =>
            ViewerRenderingInstaller.CreateWithReferenceComposition(
                parent,
                camera,
                keyLight,
                themeProvider,
                this);
    }
}
