using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Deucarian.ViewerRendering.Tests.EditMode
{
    public sealed class ViewerRenderingQualityPolicyTests
    {
        [Test]
        public void ReferenceDefaultsAreFullOnDesktopAndWebGl()
        {
            ViewerRenderingReferenceCompositionProfile composition =
                ViewerRenderingReferenceComposition.Resolve();
            ViewerRenderingQualityProfile quality =
                composition.QualityProfile;

            Assert.That(
                quality.ResolveDefaultTier(false),
                Is.EqualTo(ViewerRenderingQualityTier.Full));
            Assert.That(
                quality.ResolveDefaultTier(true),
                Is.EqualTo(ViewerRenderingQualityTier.Full));
            Assert.That(
                composition.ResolveDefaultDisplaySettings(false)
                    .EffectsActive,
                Is.True);
            Assert.That(
                composition.ResolveDefaultDisplaySettings(true),
                Is.EqualTo(
                    composition.ResolveDefaultDisplaySettings(false)));
        }

        [Test]
        public void ControllerDoesNotFollowConsumerQualityIndices()
        {
            int previousQualityLevel = QualitySettings.GetQualityLevel();
            RenderPipelineAsset previousPipeline =
                QualitySettings.renderPipeline;
            GameObject root = new GameObject(
                "Semantic Quality Index Independence");
            GameObject cameraObject = new GameObject(
                "Semantic Quality Index Camera");
            GameObject lightObject = new GameObject(
                "Semantic Quality Index Light");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                Light light = lightObject.AddComponent<Light>();
                ViewerRenderingController controller =
                    ViewerRenderingReferenceComposition.Resolve()
                        .Compose(
                            root.transform,
                            camera,
                            light,
                            null)
                        .Controller;
                int notifications = 0;
                controller.SettingsChanged += (_, __) => notifications++;

                if (QualitySettings.names.Length > 1)
                {
                    int alternate = previousQualityLevel == 0
                        ? QualitySettings.names.Length - 1
                        : 0;
                    QualitySettings.SetQualityLevel(alternate, false);
                }

                InvokeUpdate(controller);

                Assert.That(
                    controller.ActiveQualityTier,
                    Is.EqualTo(ViewerRenderingQualityTier.Full));
                Assert.That(controller.CurrentSettings.EffectsActive, Is.True);
                Assert.That(
                    QualitySettings.renderPipeline,
                    Is.SameAs(controller.Settings.PostProcessingPipeline));
                Assert.That(notifications, Is.Zero);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(lightObject);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                if (QualitySettings.names.Length > previousQualityLevel)
                {
                    QualitySettings.SetQualityLevel(
                        previousQualityLevel,
                        false);
                }

                QualitySettings.renderPipeline = previousPipeline;
            }
        }

        [Test]
        public void ExplicitTierSwitchingIsObservableAndLifecycleSafe()
        {
            RenderPipelineAsset previousPipeline =
                QualitySettings.renderPipeline;
            GameObject root = new GameObject(
                "Explicit Semantic Quality Tier");
            GameObject cameraObject = new GameObject(
                "Explicit Semantic Quality Camera");
            GameObject lightObject = new GameObject(
                "Explicit Semantic Quality Light");
            try
            {
                QualitySettings.renderPipeline = null;
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.allowHDR = false;
                UniversalAdditionalCameraData cameraData =
                    camera.GetUniversalAdditionalCameraData();
                cameraData.renderPostProcessing = false;
                cameraData.antialiasing = AntialiasingMode.None;
                cameraData.antialiasingQuality = AntialiasingQuality.Low;
                Light light = lightObject.AddComponent<Light>();
                light.shadows = LightShadows.Hard;

                ViewerRenderingController controller =
                    ViewerRenderingReferenceComposition.Resolve()
                        .Compose(
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

                ViewerDisplaySettingsSnapshot observed = default;
                ViewerDisplaySettingsChangeSource observedSource = default;
                int notifications = 0;
                controller.SettingsChanged += (snapshot, source) =>
                {
                    observed = snapshot;
                    observedSource = source;
                    notifications++;
                };

                controller.ApplyQualityTier(
                    ViewerRenderingQualityTier.Lightweight,
                    ViewerDisplaySettingsChangeSource.QualityChange);

                Assert.That(
                    controller.ActiveQualityTier,
                    Is.EqualTo(ViewerRenderingQualityTier.Lightweight));
                Assert.That(controller.CurrentSettings.EffectsActive, Is.False);
                Assert.That(
                    QualitySettings.renderPipeline,
                    Is.SameAs(controller.Settings.LightweightPipeline));
                Assert.That(controller.GlobalVolume.enabled, Is.False);
                Assert.That(camera.allowHDR, Is.False);
                Assert.That(cameraData.renderPostProcessing, Is.False);
                Assert.That(
                    cameraData.antialiasing,
                    Is.EqualTo(AntialiasingMode.None));
                Assert.That(light.shadows, Is.EqualTo(LightShadows.None));
                Assert.That(notifications, Is.EqualTo(1));
                Assert.That(observed.EffectsActive, Is.False);
                Assert.That(
                    observedSource,
                    Is.EqualTo(
                        ViewerDisplaySettingsChangeSource.QualityChange));

                controller.enabled = false;

                Assert.That(QualitySettings.renderPipeline, Is.Null);
                Assert.That(camera.allowHDR, Is.False);
                Assert.That(cameraData.renderPostProcessing, Is.False);
                Assert.That(
                    cameraData.antialiasing,
                    Is.EqualTo(AntialiasingMode.None));
                Assert.That(light.shadows, Is.EqualTo(LightShadows.Hard));

                controller.ApplyQualityTier(
                    ViewerRenderingQualityTier.Full,
                    ViewerDisplaySettingsChangeSource.QualityChange);

                Assert.That(controller.CurrentSettings.EffectsActive, Is.True);
                Assert.That(QualitySettings.renderPipeline, Is.Null);
                Assert.That(camera.allowHDR, Is.False);
                Assert.That(light.shadows, Is.EqualTo(LightShadows.Hard));
                Assert.That(notifications, Is.EqualTo(2));

                controller.enabled = true;

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
                Assert.That(light.shadows, Is.EqualTo(LightShadows.Soft));

                controller.ApplyQualityTier(
                    ViewerRenderingQualityTier.Full,
                    ViewerDisplaySettingsChangeSource.QualityChange);
                Assert.That(notifications, Is.EqualTo(2));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(lightObject);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                QualitySettings.renderPipeline = previousPipeline;
            }
        }

        [Test]
        public void RecompositionPreservesAnExplicitTier()
        {
            RenderPipelineAsset previousPipeline =
                QualitySettings.renderPipeline;
            GameObject root = new GameObject(
                "Semantic Quality Recomposition");
            GameObject cameraObject = new GameObject(
                "Semantic Quality Recomposition Camera");
            GameObject lightObject = new GameObject(
                "Semantic Quality Recomposition Light");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                Light light = lightObject.AddComponent<Light>();
                ViewerRenderingReferenceCompositionProfile composition =
                    ViewerRenderingReferenceComposition.Resolve();
                ViewerRenderingInstaller first = composition.Compose(
                    root.transform,
                    camera,
                    light,
                    null);
                first.Controller.ApplyQualityTier(
                    ViewerRenderingQualityTier.Lightweight,
                    ViewerDisplaySettingsChangeSource.QualityChange);

                ViewerRenderingInstaller second = composition.Compose(
                    root.transform,
                    camera,
                    light,
                    first.ThemeProvider);

                Assert.That(second, Is.SameAs(first));
                Assert.That(
                    second.Controller.ActiveQualityTier,
                    Is.EqualTo(ViewerRenderingQualityTier.Lightweight));
                Assert.That(
                    second.Controller.CurrentSettings.EffectsActive,
                    Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(lightObject);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                QualitySettings.renderPipeline = previousPipeline;
            }
        }

        [Test]
        public void RuntimeNeverReadsConsumerOwnedQualityIndices()
        {
            UnityEditor.PackageManager.PackageInfo package =
                UnityEditor.PackageManager.PackageInfo.FindForAssembly(
                    typeof(ViewerRenderingController).Assembly);
            Assert.That(package, Is.Not.Null);
            string runtimeRoot = Path.Combine(package.resolvedPath, "Runtime");

            string[] violations = Directory
                .EnumerateFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories)
                .Where(path => File.ReadAllText(path).Contains(
                    "QualitySettings.GetQualityLevel"))
                .Select(path => path.Substring(runtimeRoot.Length + 1))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            Assert.That(
                violations,
                Is.Empty,
                "Viewer Rendering must use its semantic quality tier, not " +
                "consumer-owned Unity quality indices:\n" +
                string.Join("\n", violations));
        }

        [Test]
        public void InvalidQualityTierIsRejectedWithoutChangingState()
        {
            RenderPipelineAsset previousPipeline =
                QualitySettings.renderPipeline;
            GameObject root = new GameObject("Invalid Semantic Quality Tier");
            try
            {
                ViewerRenderingController controller =
                    ViewerRenderingReferenceComposition.Resolve()
                        .Compose(root.transform)
                        .Controller;
                ViewerRenderingQualityTier before =
                    controller.ActiveQualityTier;

                Assert.That(
                    () => controller.ApplyQualityTier(
                        (ViewerRenderingQualityTier)999,
                        ViewerDisplaySettingsChangeSource.QualityChange),
                    Throws.TypeOf<ArgumentOutOfRangeException>());
                Assert.That(controller.ActiveQualityTier, Is.EqualTo(before));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                QualitySettings.renderPipeline = previousPipeline;
            }
        }

        private static void InvokeUpdate(
            ViewerRenderingController controller)
        {
            MethodInfo update = typeof(ViewerRenderingController).GetMethod(
                "Update",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(update, Is.Not.Null);
            update.Invoke(controller, null);
        }
    }
}
