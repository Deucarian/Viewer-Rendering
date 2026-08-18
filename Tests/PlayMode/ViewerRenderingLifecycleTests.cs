using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;

namespace Deucarian.ViewerRendering.Tests.PlayMode
{
    public sealed class ViewerRenderingLifecycleTests
    {
        [UnityTest]
        public IEnumerator DisableRestoresAndEnableReappliesRuntimeState()
        {
            RenderPipelineAsset previousPipeline =
                QualitySettings.renderPipeline;
            GameObject root = new GameObject(
                "Viewer Rendering Lifecycle");
            GameObject cameraObject = new GameObject(
                "Viewer Rendering Lifecycle Camera");
            GameObject lightObject = new GameObject(
                "Viewer Rendering Lifecycle Light");
            try
            {
                ViewerRenderingReferenceCompositionProfile composition =
                    ViewerRenderingReferenceComposition.Resolve();
                QualitySettings.renderPipeline =
                    composition.Settings.PostProcessingPipeline;
                RenderPipelineAsset baselinePipeline =
                    QualitySettings.renderPipeline;

                Camera camera = cameraObject.AddComponent<Camera>();
                camera.allowHDR = false;
                UniversalAdditionalCameraData cameraData =
                    camera.GetUniversalAdditionalCameraData();
                cameraData.renderPostProcessing = false;
                cameraData.antialiasing = AntialiasingMode.None;
                cameraData.antialiasingQuality = AntialiasingQuality.Low;
                Light light = lightObject.AddComponent<Light>();
                light.shadows = LightShadows.Hard;

                Assert.That(
                    QualitySettings.renderPipeline,
                    Is.SameAs(baselinePipeline));
                Assert.That(camera.allowHDR, Is.False);
                Assert.That(cameraData.renderPostProcessing, Is.False);
                Assert.That(
                    cameraData.antialiasing,
                    Is.EqualTo(AntialiasingMode.None));
                Assert.That(light.shadows, Is.EqualTo(LightShadows.Hard));

                ViewerRenderingController controller =
                    composition.Compose(
                        root.transform,
                        camera,
                        light,
                        null)
                        .Controller;
                controller.ApplyDisplaySettings(
                    new ViewerDisplaySettingsRequest(
                        ViewerRenderingMode.Realistic,
                        false),
                    ViewerDisplaySettingsChangeSource.Host);
                controller.ApplyQualityTier(
                    ViewerRenderingQualityTier.Lightweight,
                    ViewerDisplaySettingsChangeSource.QualityChange);

                Assert.That(
                    QualitySettings.renderPipeline,
                    Is.SameAs(controller.Settings.LightweightPipeline));
                Assert.That(light.shadows, Is.EqualTo(LightShadows.None));

                controller.enabled = false;
                yield return null;

                Assert.That(
                    QualitySettings.renderPipeline,
                    Is.SameAs(baselinePipeline));
                Assert.That(camera.allowHDR, Is.False);
                Assert.That(cameraData.renderPostProcessing, Is.False);
                Assert.That(
                    cameraData.antialiasing,
                    Is.EqualTo(AntialiasingMode.None));
                Assert.That(
                    cameraData.antialiasingQuality,
                    Is.EqualTo(AntialiasingQuality.Low));
                Assert.That(light.shadows, Is.EqualTo(LightShadows.Hard));

                controller.ApplyQualityTier(
                    ViewerRenderingQualityTier.Full,
                    ViewerDisplaySettingsChangeSource.QualityChange);

                Assert.That(controller.CurrentSettings.EffectsActive, Is.True);
                Assert.That(
                    QualitySettings.renderPipeline,
                    Is.SameAs(baselinePipeline));
                Assert.That(camera.allowHDR, Is.False);
                Assert.That(light.shadows, Is.EqualTo(LightShadows.Hard));

                controller.enabled = true;
                yield return null;

                Assert.That(
                    QualitySettings.renderPipeline,
                    Is.SameAs(controller.Settings.PostProcessingPipeline));
                Assert.That(controller.GlobalVolume.enabled, Is.True);
                Assert.That(camera.allowHDR, Is.True);
                Assert.That(cameraData.renderPostProcessing, Is.True);
                Assert.That(
                    cameraData.antialiasing,
                    Is.EqualTo(
                        AntialiasingMode
                            .SubpixelMorphologicalAntiAliasing));
                Assert.That(
                    cameraData.antialiasingQuality,
                    Is.EqualTo(AntialiasingQuality.High));
                Assert.That(light.shadows, Is.EqualTo(LightShadows.Soft));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(lightObject);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                QualitySettings.renderPipeline = previousPipeline;
            }
        }
    }
}
