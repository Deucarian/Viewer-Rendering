using Deucarian.Common;
using Deucarian.Theming;
using UnityEngine;

namespace Deucarian.ViewerRendering
{
    /// <summary>
    /// Theme-authored sky presentation. Neutral model lighting remains owned by
    /// <see cref="ViewerRenderingController"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ViewerThemedEnvironment : MonoBehaviour
    {
        public const string GradientShaderResourcesPath =
            "Deucarian/ViewerRendering/ViewerGradientSkybox";
        public const string GradientShaderName =
            "Deucarian/Viewer/GradientSkybox";

        private static readonly int TopColorId =
            Shader.PropertyToID("_TopColor");
        private static readonly int HorizonColorId =
            Shader.PropertyToID("_HorizonColor");
        private static readonly int BottomColorId =
            Shader.PropertyToID("_BottomColor");
        private static readonly int PrimaryTintId =
            Shader.PropertyToID("_PrimaryTint");
        private static readonly int PrimaryStrengthId =
            Shader.PropertyToID("_PrimaryStrength");

        private Camera _camera;
        private DeucarianThemeProvider _themeProvider;
        private DeucarianTheme _theme;
        private Shader _gradientShader;
        private ViewerRenderingEnvironmentProfile _profile;
        private Material _skyboxMaterial;
        private Material _previousSkybox;
        private bool _providerSubscribed;
        private bool _skyboxInstalled;
        private bool _fogCaptured;
        private bool _previousFog;

        public Material SkyboxMaterial => _skyboxMaterial;
        public DeucarianThemeProvider ThemeProvider => _themeProvider;

        public void Initialize(
            Camera viewerCamera,
            Shader gradientShader,
            DeucarianThemeProvider themeProvider,
            ViewerRenderingEnvironmentProfile profile)
        {
            _camera = viewerCamera;
            _profile = profile;
            if (_gradientShader != gradientShader)
            {
                RestorePresentation();
                if (_skyboxMaterial != null)
                {
                    UnityObjectUtility.DestroySafely(_skyboxMaterial);
                    _skyboxMaterial = null;
                }

                _gradientShader = gradientShader;
            }

            BindThemeProvider(themeProvider);
            EnsureMaterial();
            ApplyTheme();
        }

        private void OnEnable()
        {
            if (_gradientShader == null)
            {
                return;
            }

            BindThemeProvider(_themeProvider);
            EnsureMaterial();
            ApplyTheme();
        }

        private void OnDisable()
        {
            UnbindThemeProvider();
            RestorePresentation();
        }

        private void OnDestroy()
        {
            UnbindThemeProvider();
            RestorePresentation();
            if (_skyboxMaterial != null)
            {
                UnityObjectUtility.DestroySafely(_skyboxMaterial);
                _skyboxMaterial = null;
            }
        }

        private void BindThemeProvider(
            DeucarianThemeProvider requestedProvider)
        {
            DeucarianThemeProvider provider = requestedProvider != null
                ? requestedProvider
                : DeucarianThemeRuntimeResolver.FindProvider(this);
            if (_themeProvider == provider && _providerSubscribed)
            {
                _theme = provider != null
                    ? provider.CurrentTheme
                    : ResolveFallbackTheme();
                return;
            }

            UnbindThemeProvider();
            _themeProvider = provider;
            if (_themeProvider != null)
            {
                if (_themeProvider.CurrentThemeFamily == null
                    && _themeProvider.CurrentTheme == null)
                {
                    DeucarianViewerReferenceThemeProfile reference =
                        DeucarianViewerReferenceThemePreset.Resolve();
                    _themeProvider.SetThemeFamily(
                        reference.ThemeFamily,
                        DeucarianViewerReferenceThemePreset.DefaultMode);
                }

                _themeProvider.ThemeChanged += OnThemeChanged;
                _themeProvider.StyleChanged += OnStyleChanged;
                _providerSubscribed = true;
                _theme = _themeProvider.CurrentTheme;
            }
            else
            {
                _theme = ResolveFallbackTheme();
            }
        }

        private void UnbindThemeProvider()
        {
            if (_themeProvider != null && _providerSubscribed)
            {
                _themeProvider.ThemeChanged -= OnThemeChanged;
                _themeProvider.StyleChanged -= OnStyleChanged;
            }

            _providerSubscribed = false;
        }

        private void OnThemeChanged(DeucarianTheme theme)
        {
            _theme = theme;
            ApplyTheme();
        }

        private void OnStyleChanged(DeucarianThemeStyle style)
        {
            ApplyTheme();
        }

        private void EnsureMaterial()
        {
            if (_skyboxMaterial != null || _gradientShader == null)
            {
                return;
            }

            _skyboxMaterial = new Material(_gradientShader)
            {
                name = "Viewer Reference Gradient Skybox (Runtime)",
                hideFlags = HideFlags.DontSave
            };
        }

        private void ApplyTheme()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            EnsureMaterial();
            ResolveGradientColors(
                _theme != null ? _theme : ResolveFallbackTheme(),
                _profile,
                out Color top,
                out Color horizon,
                out Color bottom,
                out Color primary,
                out Color background);

            if (_skyboxMaterial != null)
            {
                _skyboxMaterial.SetColor(TopColorId, top);
                _skyboxMaterial.SetColor(HorizonColorId, horizon);
                _skyboxMaterial.SetColor(BottomColorId, bottom);
                _skyboxMaterial.SetColor(PrimaryTintId, primary);
                _skyboxMaterial.SetFloat(
                    PrimaryStrengthId,
                    ResolvePrimaryStrength(
                        _theme,
                        background,
                        _profile));
                InstallSkybox();
            }

            if (_camera != null)
            {
                _camera.clearFlags = _skyboxMaterial != null
                    ? CameraClearFlags.Skybox
                    : CameraClearFlags.SolidColor;
                _camera.backgroundColor = bottom;
            }

            CaptureFog();
            RenderSettings.fog = false;
        }

        private void InstallSkybox()
        {
            if (_skyboxMaterial == null)
            {
                return;
            }

            if (!_skyboxInstalled)
            {
                _previousSkybox = RenderSettings.skybox;
            }

            RenderSettings.skybox = _skyboxMaterial;
            _skyboxInstalled = true;
        }

        private void CaptureFog()
        {
            if (_fogCaptured)
            {
                return;
            }

            _previousFog = RenderSettings.fog;
            _fogCaptured = true;
        }

        private void RestorePresentation()
        {
            if (_skyboxInstalled)
            {
                if (RenderSettings.skybox == _skyboxMaterial)
                {
                    RenderSettings.skybox = _previousSkybox;
                }

                _previousSkybox = null;
                _skyboxInstalled = false;
            }

            if (_fogCaptured)
            {
                RenderSettings.fog = _previousFog;
                _fogCaptured = false;
            }
        }

        public static void ResolveGradientColors(
            DeucarianTheme theme,
            ViewerRenderingEnvironmentProfile profile,
            out Color top,
            out Color horizon,
            out Color bottom,
            out Color primary,
            out Color background)
        {
            DeucarianTheme resolvedTheme =
                theme != null ? theme : ResolveFallbackTheme();
            background = Opaque(ResolveColor(
                resolvedTheme,
                DeucarianBuiltinColorRoleIds.Background,
                new Color(0.07f, 0.08f, 0.09f, 1f)));
            primary = Opaque(ResolveColor(
                resolvedTheme,
                DeucarianBuiltinColorRoleIds.Primary,
                new Color(0.3882353f, 0.25882354f, 0.5882353f, 1f)));
            top = ResolveColor(
                resolvedTheme,
                ViewerRenderingColorRoleIds.EnvironmentSkyTop,
                profile.ReferenceSkyTop);
            horizon = ResolveColor(
                resolvedTheme,
                ViewerRenderingColorRoleIds.EnvironmentSkyHorizon,
                profile.ReferenceSkyHorizon);
            bottom = ResolveColor(
                resolvedTheme,
                ViewerRenderingColorRoleIds.EnvironmentSkyBottom,
                profile.ReferenceSkyBottom);
        }

        public static float ResolvePrimaryStrength(
            DeucarianTheme theme,
            Color background,
            ViewerRenderingEnvironmentProfile profile)
        {
            if (HasNearNeutralResolvedSky(theme, profile))
            {
                return profile.NeutralSkyPrimaryStrength;
            }

            return ResolveThemeMode(theme, background)
                   == DeucarianThemeMode.Light
                ? profile.LightThemePrimaryStrength
                : profile.DarkThemePrimaryStrength;
        }

        private static Color ResolveColor(
            DeucarianTheme theme,
            string roleId,
            Color fallback)
        {
            if (theme != null
                && theme.TryGetColorById(roleId, out Color color)
                && !IsMissingColor(color))
            {
                return Opaque(color);
            }

            return Opaque(fallback);
        }

        private static bool HasNearNeutralResolvedSky(
            DeucarianTheme theme,
            ViewerRenderingEnvironmentProfile profile)
        {
            DeucarianTheme resolvedTheme =
                theme != null ? theme : ResolveFallbackTheme();
            Color top = ResolveColor(
                resolvedTheme,
                ViewerRenderingColorRoleIds.EnvironmentSkyTop,
                profile.ReferenceSkyTop);
            Color horizon = ResolveColor(
                resolvedTheme,
                ViewerRenderingColorRoleIds.EnvironmentSkyHorizon,
                profile.ReferenceSkyHorizon);
            Color bottom = ResolveColor(
                resolvedTheme,
                ViewerRenderingColorRoleIds.EnvironmentSkyBottom,
                profile.ReferenceSkyBottom);

            return CalculateSaturation(top)
                       <= profile.NeutralSkySaturationThreshold
                   && CalculateSaturation(horizon)
                       <= profile.NeutralSkySaturationThreshold
                   && CalculateSaturation(bottom)
                       <= profile.NeutralSkySaturationThreshold;
        }

        private static bool IsMissingColor(Color color)
        {
            Color missing = DeucarianColorPalette.MissingColor;
            return Mathf.Abs(color.r - missing.r) <= 0.0001f
                   && Mathf.Abs(color.g - missing.g) <= 0.0001f
                   && Mathf.Abs(color.b - missing.b) <= 0.0001f
                   && Mathf.Abs(color.a - missing.a) <= 0.0001f;
        }

        private static float CalculateSaturation(Color color)
        {
            float maximum =
                Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            float minimum =
                Mathf.Min(color.r, Mathf.Min(color.g, color.b));
            return maximum <= Mathf.Epsilon
                ? 0f
                : (maximum - minimum) / maximum;
        }

        private static DeucarianThemeMode ResolveThemeMode(
            DeucarianTheme theme,
            Color background)
        {
            DeucarianColorPalette palette =
                theme != null ? theme.ColorPalette : null;
            if (palette != null && palette.HasThemeMode)
            {
                return palette.ThemeMode;
            }

            return CalculateLuminance(background) < 0.5f
                ? DeucarianThemeMode.Dark
                : DeucarianThemeMode.Light;
        }

        private static float CalculateLuminance(Color color)
        {
            Color linear = color.linear;
            return 0.2126f * linear.r
                   + 0.7152f * linear.g
                   + 0.0722f * linear.b;
        }

        private static Color Opaque(Color color)
        {
            color.a = 1f;
            return color;
        }

        private static DeucarianTheme ResolveFallbackTheme() =>
            DeucarianViewerReferenceThemePreset.Resolve().DefaultTheme;
    }
}
