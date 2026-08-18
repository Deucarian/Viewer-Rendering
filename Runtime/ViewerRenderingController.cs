using System;
using Deucarian.Common;
using Deucarian.Diagnostics;
using Deucarian.ViewerRendering.Environment;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Deucarian.ViewerRendering
{
    /// <summary>
    /// Authoritative display state and runtime quality policy for a viewer.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed partial class ViewerRenderingController :
        MonoBehaviour,
        IViewerRenderingController,
        IViewerRenderingQualityController
    {
        public const string VolumeObjectName =
            "Deucarian Viewer Post Processing";
        public const string ReflectionCubemapName =
            "Deucarian Viewer Neutral Studio Reflection";

        private Camera _camera;
        private Light _keyLight;
        private UniversalAdditionalCameraData _cameraData;
        private ViewerRenderingReferenceCompositionProfile _composition;
        private ViewerRenderingSettings _settings;
        private ViewerRenderingLightProfile _lightProfile;
        private ViewerRenderingEnvironmentProfile _environmentProfile;
        private ViewerRenderingQualityProfile _qualityProfile;
        private Volume _volume;
        private Cubemap _neutralReflection;
        private bool _ownsNeutralReflection;
        private ViewerRenderingMode _renderingMode =
            ViewerRenderingMode.ColorFaithful;
        private bool _cameraRelativeLight;
        private bool _effectsActive;
        private ViewerRenderingQualityTier _activeQualityTier;
        private bool _initialized;
        private DiagnosticProviderRegistration _diagnosticRegistration;

        public event Action<
            ViewerDisplaySettingsSnapshot,
            ViewerDisplaySettingsChangeSource> SettingsChanged;

        public ViewerDisplaySettingsSnapshot CurrentSettings =>
            new ViewerDisplaySettingsSnapshot(
                _renderingMode,
                _cameraRelativeLight,
                _effectsActive);

        public ViewerRenderingReferenceCompositionProfile Composition =>
            _composition;
        public ViewerRenderingSettings Settings => _settings;
        public Cubemap NeutralReflection => _neutralReflection;
        public Light KeyLight => _keyLight;
        public Camera Camera => _camera;
        public Volume GlobalVolume => _volume;
        public ViewerRenderingQualityTier ActiveQualityTier =>
            _activeQualityTier;

        public static bool ShouldEnableEffects(
            ViewerRenderingQualityTier tier) =>
            ViewerRenderingReferenceComposition.Resolve()
                .QualityProfile
                .ShouldEnableEffects(tier);

        public static bool ShouldEnableShadows(
            ViewerRenderingQualityTier tier,
            ViewerRenderingMode mode) =>
            ViewerRenderingReferenceComposition.Resolve()
                .QualityProfile
                .ShouldEnableShadows(tier, mode);

        public static Quaternion FixedLightRotation =>
            ViewerRenderingReferenceComposition.Resolve()
                .LightProfile
                .FixedRotation;

        public static Quaternion CameraRelativeLightOffset =>
            ViewerRenderingReferenceComposition.Resolve()
                .LightProfile
                .CameraRelativeOffset;

        public static Quaternion ResolveCameraRelativeTarget(
            Quaternion cameraRotation) =>
            ViewerRenderingReferenceComposition.Resolve()
                .LightProfile
                .ResolveCameraRelativeTarget(cameraRotation);

        public static float ResolveDampingFactor(
            float unscaledDeltaTime) =>
            ViewerRenderingReferenceComposition.Resolve()
                .LightProfile
                .ResolveDampingFactor(unscaledDeltaTime);

        public static Cubemap CreateNeutralStudioReflection(
            bool makeNoLongerReadable = true)
        {
            ViewerRenderingEnvironmentProfile profile =
                ViewerRenderingReferenceComposition.Resolve()
                    .EnvironmentProfile;
            return ViewerStudioReflectionFactory.Create(
                profile.ReflectionCubemapSize,
                ReflectionCubemapName,
                makeNoLongerReadable);
        }

        public static Color ResolveStudioReflectionColor(
            Vector3 direction) =>
            ViewerStudioReflectionFactory.ResolveColor(direction);

        public void Initialize(
            Camera viewerCamera,
            Light keyLight,
            ViewerRenderingReferenceCompositionProfile composition)
        {
            bool preserveDisplayState = _initialized;
            RestoreRuntimeState();
            _composition = composition;
            _settings = composition.Settings;
            _lightProfile = composition.LightProfile;
            _environmentProfile = composition.EnvironmentProfile;
            _qualityProfile = composition.QualityProfile;
            _camera = viewerCamera;
            _keyLight = keyLight;
            _cameraData = _camera != null
                ? _camera.GetUniversalAdditionalCameraData()
                : null;
            if (!preserveDisplayState)
            {
                _renderingMode = _settings.DefaultRenderingMode;
                _cameraRelativeLight =
                    _settings.DefaultCameraRelativeLight;
                _activeQualityTier =
                    _qualityProfile.ResolveDefaultTier(IsWebGlRuntime());
            }

            _effectsActive =
                _qualityProfile.ShouldEnableEffects(_activeQualityTier);
            _initialized = true;
            EnsureVolume();
            EnsureNeutralReflection();
            if (isActiveAndEnabled)
            {
                ActivateRuntimeState(true);
            }
        }

        public void ApplyDisplaySettings(
            ViewerDisplaySettingsRequest request,
            ViewerDisplaySettingsChangeSource source)
        {
            if (request.RenderingMode.HasValue)
            {
                _renderingMode = request.RenderingMode.Value;
            }

            if (request.CameraRelativeLight.HasValue)
            {
                _cameraRelativeLight =
                    request.CameraRelativeLight.Value;
            }

            ApplyCurrentPolicy();
            SettingsChanged?.Invoke(CurrentSettings, source);
        }

        public void ApplyDisplaySettings(
            ViewerRenderingMode? renderingMode,
            bool? cameraRelativeLight,
            ViewerDisplaySettingsChangeSource source)
        {
            ApplyDisplaySettings(
                new ViewerDisplaySettingsRequest(
                    renderingMode,
                    cameraRelativeLight),
                source);
        }

        public void ApplyQualityTier(
            ViewerRenderingQualityTier tier,
            ViewerDisplaySettingsChangeSource source)
        {
            ViewerRenderingQualityProfile.EnsureDefined(
                tier,
                nameof(tier));
            if (!_initialized)
            {
                throw new InvalidOperationException(
                    "Viewer rendering is not initialized.");
            }

            bool changed = _activeQualityTier != tier;
            _activeQualityTier = tier;
            ApplyCurrentPolicy();
            if (changed)
            {
                SettingsChanged?.Invoke(CurrentSettings, source);
            }
        }

        private void OnEnable()
        {
            if (!_initialized)
            {
                return;
            }

            EnsureVolume();
            EnsureNeutralReflection();
            ActivateRuntimeState(true);
        }

        private void Update()
        {
            if (_initialized
                && _runtimeStateCaptured
                && !IsCurrentPolicyApplied())
            {
                ApplyPolicy();
            }
        }

        private void LateUpdate()
        {
            if (_initialized)
            {
                UpdateKeyLightRotation(Time.unscaledDeltaTime);
            }
        }

        private void OnDisable()
        {
            _diagnosticRegistration?.Dispose();
            _diagnosticRegistration = null;
            RestoreRuntimeState();
        }

        private void OnDestroy()
        {
            _diagnosticRegistration?.Dispose();
            _diagnosticRegistration = null;
            RestoreRuntimeState();
            if (_ownsNeutralReflection && _neutralReflection != null)
            {
                UnityObjectUtility.DestroySafely(_neutralReflection);
            }

            _neutralReflection = null;
            _ownsNeutralReflection = false;
        }

        private void EnsureVolume()
        {
            if (_volume == null)
            {
                Transform existing = transform.Find(VolumeObjectName);
                if (existing != null)
                {
                    _volume = existing.GetComponent<Volume>();
                }
            }

            if (_volume == null)
            {
                GameObject volumeObject =
                    new GameObject(VolumeObjectName);
                volumeObject.transform.SetParent(transform, false);
                _volume = volumeObject.AddComponent<Volume>();
            }

            _volume.isGlobal = true;
            _volume.priority = 0f;
            _volume.weight = 1f;
            _volume.sharedProfile =
                _settings.ResolveVolumeProfile(_renderingMode);
        }

        private void EnsureNeutralReflection()
        {
            if (_neutralReflection != null)
            {
                return;
            }

            if (_settings.NeutralStudioReflection != null)
            {
                _neutralReflection =
                    _settings.NeutralStudioReflection;
                _ownsNeutralReflection = false;
                return;
            }

            _neutralReflection =
                ViewerStudioReflectionFactory.Create(
                    _environmentProfile.ReflectionCubemapSize,
                    ReflectionCubemapName,
                    true);
            _ownsNeutralReflection = true;
        }

        private void ActivateRuntimeState(bool resetLightRotation)
        {
            CaptureRuntimeState();
            ConfigureKeyLight(resetLightRotation);
            ApplyCurrentPolicy();
            RegisterDiagnostics();
        }

        private void ApplyCurrentPolicy()
        {
            _effectsActive =
                _qualityProfile.ShouldEnableEffects(_activeQualityTier);
            if (_initialized
                && isActiveAndEnabled
                && _runtimeStateCaptured)
            {
                ApplyPolicy();
            }
        }

        private void ApplyPolicy()
        {
            if (_settings == null)
            {
                ViewerRenderingLog.Rendering.Error(
                    "Viewer rendering settings are not initialized.",
                    this);
                return;
            }

            RenderPipelineAsset pipeline = _effectsActive
                ? _settings.PostProcessingPipeline
                : _settings.LightweightPipeline;
            if (pipeline != null
                && QualitySettings.renderPipeline != pipeline)
            {
                QualitySettings.renderPipeline = pipeline;
            }

            if (_volume != null)
            {
                _volume.sharedProfile =
                    _settings.ResolveVolumeProfile(_renderingMode);
                _volume.enabled = _effectsActive;
            }

            if (_camera != null)
            {
                _camera.allowHDR = _effectsActive;
            }

            if (_cameraData != null)
            {
                _cameraData.renderPostProcessing = _effectsActive;
                _cameraData.antialiasing = _effectsActive
                    ? AntialiasingMode
                        .SubpixelMorphologicalAntiAliasing
                    : AntialiasingMode.None;
                _cameraData.antialiasingQuality = _effectsActive
                    ? AntialiasingQuality.High
                    : AntialiasingQuality.Low;
            }

            ApplyEnvironmentPolicy();
            ConfigureKeyLight(false);
            if (_keyLight != null)
            {
                _keyLight.shadows =
                    _qualityProfile.ShouldEnableShadows(
                        _activeQualityTier,
                        _renderingMode)
                        ? LightShadows.Soft
                        : LightShadows.None;
            }
        }

        private bool IsCurrentPolicyApplied()
        {
            if (_settings == null)
            {
                return true;
            }

            RenderPipelineAsset expectedPipeline = _effectsActive
                ? _settings.PostProcessingPipeline
                : _settings.LightweightPipeline;
            if (QualitySettings.renderPipeline != expectedPipeline)
            {
                return false;
            }

            if (_volume != null
                && (_volume.enabled != _effectsActive
                    || _volume.sharedProfile
                    != _settings.ResolveVolumeProfile(_renderingMode)))
            {
                return false;
            }

            if (_camera != null && _camera.allowHDR != _effectsActive)
            {
                return false;
            }

            if (_cameraData != null
                && (_cameraData.renderPostProcessing != _effectsActive
                    || _cameraData.antialiasing
                    != (_effectsActive
                        ? AntialiasingMode
                            .SubpixelMorphologicalAntiAliasing
                        : AntialiasingMode.None)))
            {
                return false;
            }

            LightShadows expectedShadows =
                _qualityProfile.ShouldEnableShadows(
                    _activeQualityTier,
                    _renderingMode)
                    ? LightShadows.Soft
                    : LightShadows.None;
            return _keyLight == null || _keyLight.shadows == expectedShadows;
        }

        private static bool IsWebGlRuntime() =>
            Application.platform == RuntimePlatform.WebGLPlayer;

        private void ConfigureKeyLight(bool resetRotation)
        {
            if (_keyLight == null)
            {
                return;
            }

            _keyLight.type = LightType.Directional;
            _keyLight.color = Color.white;
            _keyLight.intensity = _lightProfile.Intensity;
            _keyLight.shadowStrength = _lightProfile.ShadowStrength;
            _keyLight.shadowBias = _lightProfile.ShadowBias;
            _keyLight.shadowNormalBias =
                _lightProfile.ShadowNormalBias;
            _keyLight.shadowNearPlane = _lightProfile.ShadowNearPlane;
            RenderSettings.sun = _keyLight;
            if (resetRotation)
            {
                _keyLight.transform.rotation =
                    _lightProfile.FixedRotation;
            }
        }

        public void UpdateKeyLightRotation(float unscaledDeltaTime)
        {
            if (_keyLight == null)
            {
                return;
            }

            Quaternion target = _lightProfile.FixedRotation;
            if (_cameraRelativeLight && _camera != null)
            {
                target =
                    _lightProfile.ResolveCameraRelativeTarget(
                        _camera.transform.rotation);
            }

            _keyLight.transform.rotation = Quaternion.Slerp(
                _keyLight.transform.rotation,
                target,
                _lightProfile.ResolveDampingFactor(
                    unscaledDeltaTime));
        }

        private void RegisterDiagnostics()
        {
            if (_diagnosticRegistration == null && _initialized)
            {
                _diagnosticRegistration =
                    DiagnosticProviderRegistry.Register(
                        new ViewerRenderingDiagnosticProvider(this));
            }
        }
    }
}
