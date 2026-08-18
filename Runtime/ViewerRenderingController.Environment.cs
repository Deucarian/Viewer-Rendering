using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Deucarian.ViewerRendering
{
    public sealed partial class ViewerRenderingController
    {
        private bool _runtimeStateCaptured;
        private AmbientMode _previousAmbientMode;
        private Color _previousAmbientLight;
        private Color _previousAmbientSky;
        private Color _previousAmbientEquator;
        private Color _previousAmbientGround;
        private float _previousAmbientIntensity;
        private DefaultReflectionMode _previousReflectionMode;
        private Texture _previousCustomReflection;
        private float _previousReflectionIntensity;
        private Light _previousSun;
        private RenderPipelineAsset _previousPipeline;
        private bool _previousCameraAllowHdr;
        private bool _previousCameraPostProcessing;
        private AntialiasingMode _previousCameraAntialiasing;
        private AntialiasingQuality _previousCameraAntialiasingQuality;
        private LightShadows _previousKeyLightShadows;

        private void ApplyEnvironmentPolicy()
        {
            if (_renderingMode == ViewerRenderingMode.Realistic)
            {
                RenderSettings.ambientMode = AmbientMode.Trilight;
                RenderSettings.ambientSkyColor =
                    GrayscaleSrgb(
                        _environmentProfile
                            .RealisticAmbientSkySrgb);
                RenderSettings.ambientEquatorColor =
                    GrayscaleSrgb(
                        _environmentProfile
                            .RealisticAmbientEquatorSrgb);
                RenderSettings.ambientGroundColor =
                    GrayscaleSrgb(
                        _environmentProfile
                            .RealisticAmbientGroundSrgb);
            }
            else
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight =
                    GrayscaleSrgb(
                        _environmentProfile
                            .ColorFaithfulAmbientSrgb);
            }

            RenderSettings.ambientIntensity = 1f;
            if (_neutralReflection != null)
            {
                RenderSettings.defaultReflectionMode =
                    DefaultReflectionMode.Custom;
                RenderSettings.customReflectionTexture =
                    _neutralReflection;
                RenderSettings.reflectionIntensity =
                    _effectsActive
                        ? _environmentProfile.ReflectionIntensity
                        : 0f;
            }
        }

        private static Color GrayscaleSrgb(float value)
        {
            float linear = Mathf.GammaToLinearSpace(
                Mathf.Clamp01(value));
            return new Color(linear, linear, linear, 1f);
        }

        private void CaptureRuntimeState()
        {
            if (_runtimeStateCaptured)
            {
                return;
            }

            _runtimeStateCaptured = true;
            _previousAmbientMode = RenderSettings.ambientMode;
            _previousAmbientLight = RenderSettings.ambientLight;
            _previousAmbientSky = RenderSettings.ambientSkyColor;
            _previousAmbientEquator =
                RenderSettings.ambientEquatorColor;
            _previousAmbientGround =
                RenderSettings.ambientGroundColor;
            _previousAmbientIntensity =
                RenderSettings.ambientIntensity;
            _previousReflectionMode =
                RenderSettings.defaultReflectionMode;
            _previousCustomReflection =
                RenderSettings.customReflectionTexture;
            _previousReflectionIntensity =
                RenderSettings.reflectionIntensity;
            _previousSun = RenderSettings.sun;
            _previousPipeline = QualitySettings.renderPipeline;
            if (_camera != null)
            {
                _previousCameraAllowHdr = _camera.allowHDR;
            }

            if (_cameraData != null)
            {
                _previousCameraPostProcessing =
                    _cameraData.renderPostProcessing;
                _previousCameraAntialiasing =
                    _cameraData.antialiasing;
                _previousCameraAntialiasingQuality =
                    _cameraData.antialiasingQuality;
            }

            if (_keyLight != null)
            {
                _previousKeyLightShadows = _keyLight.shadows;
            }
        }

        private void RestoreRuntimeState()
        {
            if (!_runtimeStateCaptured)
            {
                return;
            }

            RenderSettings.ambientMode = _previousAmbientMode;
            RenderSettings.ambientLight = _previousAmbientLight;
            RenderSettings.ambientSkyColor = _previousAmbientSky;
            RenderSettings.ambientEquatorColor =
                _previousAmbientEquator;
            RenderSettings.ambientGroundColor =
                _previousAmbientGround;
            RenderSettings.ambientIntensity =
                _previousAmbientIntensity;
            RenderSettings.defaultReflectionMode =
                _previousReflectionMode;
            RenderSettings.customReflectionTexture =
                _previousCustomReflection;
            RenderSettings.reflectionIntensity =
                _previousReflectionIntensity;
            if (RenderSettings.sun == _keyLight)
            {
                RenderSettings.sun = _previousSun;
            }

            if (_camera != null)
            {
                _camera.allowHDR = _previousCameraAllowHdr;
            }

            if (_cameraData != null)
            {
                _cameraData.renderPostProcessing =
                    _previousCameraPostProcessing;
                _cameraData.antialiasing =
                    _previousCameraAntialiasing;
                _cameraData.antialiasingQuality =
                    _previousCameraAntialiasingQuality;
            }

            if (_keyLight != null)
            {
                _keyLight.shadows = _previousKeyLightShadows;
            }

            if (_settings != null
                && (QualitySettings.renderPipeline
                    == _settings.LightweightPipeline
                    || QualitySettings.renderPipeline
                    == _settings.PostProcessingPipeline))
            {
                QualitySettings.renderPipeline = _previousPipeline;
            }

            if (_volume != null)
            {
                _volume.enabled = false;
            }

            _runtimeStateCaptured = false;
        }
    }
}
