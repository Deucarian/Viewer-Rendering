using UnityEngine;
using UnityEngine.Rendering;

namespace Deucarian.ViewerRendering
{
    /// <summary>
    /// Package-owned assets and authored values for the canonical viewer baseline.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ViewerRenderingReferenceSettings",
        menuName = "Deucarian/Viewer Rendering/Reference Settings")]
    public sealed class ViewerRenderingSettings : ScriptableObject
    {
        public const string ReferencePresetResourcesPath =
            "Deucarian/ViewerRendering/ViewerRenderingReferenceSettings";

        public static readonly Color DefaultReferenceSkyTop =
            new Color(0.1882353f, 0.1882353f, 0.1882353f, 1f);
        public static readonly Color DefaultReferenceSkyHorizon =
            new Color(0.16078432f, 0.16078432f, 0.16078432f, 1f);
        public static readonly Color DefaultReferenceSkyBottom =
            new Color(0.1254902f, 0.1254902f, 0.1254902f, 1f);

        [Header("Render assets")]
        [SerializeField] private RenderPipelineAsset _lightweightPipeline;
        [SerializeField] private RenderPipelineAsset _postProcessingPipeline;
        [SerializeField] private RenderPipelineGlobalSettings
            _globalSettings;
        [SerializeField] private VolumeProfile _colorFaithfulProfile;
        [SerializeField] private VolumeProfile _realisticProfile;
        [SerializeField] private Cubemap _neutralStudioReflection;
        [SerializeField] private Shader _gradientSkyboxShader;
        [SerializeField] private Shader _unlitShader;
        [SerializeField] private Shader _litShader;

        [Header("Camera")]
        [SerializeField] private Vector3 _cameraPosition =
            new Vector3(4f, 3f, -6f);
        [SerializeField] private Vector3 _cameraLookTarget =
            new Vector3(0f, 1f, 0f);

        [Header("Key light")]
        [SerializeField] private Vector3 _fixedLightEuler =
            new Vector3(45f, -35f, 0f);
        [SerializeField] private float _keyLightIntensity = 1.05f;
        [SerializeField] private float _realisticShadowStrength = 0.75f;
        [SerializeField] private float _shadowBias = 0.05f;
        [SerializeField] private float _shadowNormalBias = 0.35f;
        [SerializeField] private float _shadowNearPlane = 0.2f;
        [SerializeField] private Vector3 _cameraRelativeLightEuler =
            new Vector3(8f, -12f, 0f);
        [SerializeField] private float _cameraRelativeDampingSeconds = 0.35f;

        [Header("Environment")]
        [SerializeField] private int _reflectionCubemapSize = 64;
        [SerializeField] private float _reflectionIntensity = 0.6f;
        [SerializeField] private float _colorFaithfulAmbientSrgb = 0.58f;
        [SerializeField] private float _realisticAmbientSkySrgb = 0.68f;
        [SerializeField] private float _realisticAmbientEquatorSrgb = 0.52f;
        [SerializeField] private float _realisticAmbientGroundSrgb = 0.38f;
        [SerializeField] private Color _referenceSkyTop =
            new Color(0.1882353f, 0.1882353f, 0.1882353f, 1f);
        [SerializeField] private Color _referenceSkyHorizon =
            new Color(0.16078432f, 0.16078432f, 0.16078432f, 1f);
        [SerializeField] private Color _referenceSkyBottom =
            new Color(0.1254902f, 0.1254902f, 0.1254902f, 1f);
        [SerializeField] private float _darkThemePrimaryStrength = 0.12f;
        [SerializeField] private float _lightThemePrimaryStrength = 0.06f;
        [SerializeField] private float _neutralSkyPrimaryStrength;
        [SerializeField] private float _neutralSkySaturationThreshold = 0.02f;

        [Header("Quality policy")]
        [SerializeField] private ViewerRenderingQualityTier
            _desktopDefaultQualityTier = ViewerRenderingQualityTier.Full;
        [SerializeField] private ViewerRenderingQualityTier
            _webGlDefaultQualityTier = ViewerRenderingQualityTier.Full;
        [SerializeField] private ViewerRenderingMode _defaultRenderingMode =
            ViewerRenderingMode.ColorFaithful;
        [SerializeField] private bool _defaultCameraRelativeLight;

        public RenderPipelineAsset LightweightPipeline =>
            _lightweightPipeline;
        public RenderPipelineAsset PostProcessingPipeline =>
            _postProcessingPipeline;
        public RenderPipelineGlobalSettings GlobalSettings =>
            _globalSettings;
        public VolumeProfile ColorFaithfulProfile => _colorFaithfulProfile;
        public VolumeProfile RealisticProfile => _realisticProfile;
        public Cubemap NeutralStudioReflection => _neutralStudioReflection;
        public Shader GradientSkyboxShader => _gradientSkyboxShader;
        public Shader UnlitShader => _unlitShader;
        public Shader LitShader => _litShader;
        public Vector3 CameraPosition => _cameraPosition;
        public Vector3 CameraLookTarget => _cameraLookTarget;
        public Vector3 FixedLightEuler => _fixedLightEuler;
        public float KeyLightIntensity => _keyLightIntensity;
        public float RealisticShadowStrength => _realisticShadowStrength;
        public float ShadowBias => _shadowBias;
        public float ShadowNormalBias => _shadowNormalBias;
        public float ShadowNearPlane => _shadowNearPlane;
        public Vector3 CameraRelativeLightEuler =>
            _cameraRelativeLightEuler;
        public float CameraRelativeDampingSeconds =>
            _cameraRelativeDampingSeconds;
        public int ReflectionCubemapSize => _reflectionCubemapSize;
        public float ReflectionIntensity => _reflectionIntensity;
        public float ColorFaithfulAmbientSrgb =>
            _colorFaithfulAmbientSrgb;
        public float RealisticAmbientSkySrgb =>
            _realisticAmbientSkySrgb;
        public float RealisticAmbientEquatorSrgb =>
            _realisticAmbientEquatorSrgb;
        public float RealisticAmbientGroundSrgb =>
            _realisticAmbientGroundSrgb;
        public Color ReferenceSkyTop => _referenceSkyTop;
        public Color ReferenceSkyHorizon => _referenceSkyHorizon;
        public Color ReferenceSkyBottom => _referenceSkyBottom;
        public float DarkThemePrimaryStrength =>
            _darkThemePrimaryStrength;
        public float LightThemePrimaryStrength =>
            _lightThemePrimaryStrength;
        public float NeutralSkyPrimaryStrength =>
            _neutralSkyPrimaryStrength;
        public float NeutralSkySaturationThreshold =>
            _neutralSkySaturationThreshold;
        public ViewerRenderingQualityTier DesktopDefaultQualityTier =>
            _desktopDefaultQualityTier;
        public ViewerRenderingQualityTier WebGlDefaultQualityTier =>
            _webGlDefaultQualityTier;
        public ViewerRenderingMode DefaultRenderingMode =>
            _defaultRenderingMode;
        public bool DefaultCameraRelativeLight =>
            _defaultCameraRelativeLight;

        public bool IsComplete =>
            _lightweightPipeline != null
            && _postProcessingPipeline != null
            && _globalSettings != null
            && _colorFaithfulProfile != null
            && _realisticProfile != null
            && _neutralStudioReflection != null
            && _gradientSkyboxShader != null
            && _unlitShader != null
            && _litShader != null;

        public static ViewerRenderingSettings LoadReferencePreset()
        {
            return Resources.Load<ViewerRenderingSettings>(
                ReferencePresetResourcesPath);
        }

        public VolumeProfile ResolveVolumeProfile(ViewerRenderingMode mode)
        {
            return mode == ViewerRenderingMode.Realistic
                ? _realisticProfile
                : _colorFaithfulProfile;
        }
    }
}
