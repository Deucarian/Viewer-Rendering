using Deucarian.Theming;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Deucarian.ViewerRendering.Tests.EditMode
{
    public sealed class ViewerRenderingReferenceCompositionTests
    {
        [Test]
        public void PackagedSettingsOwnTheCompleteReferenceAssetGraph()
        {
            ViewerRenderingSettings settings =
                ViewerRenderingSettings.LoadReferencePreset();

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.IsComplete, Is.True);
            Assert.That(settings.LightweightPipeline, Is.Not.Null);
            Assert.That(settings.PostProcessingPipeline, Is.Not.Null);
            Assert.That(settings.GlobalSettings, Is.Not.Null);
            Assert.That(settings.ColorFaithfulProfile, Is.Not.Null);
            Assert.That(settings.RealisticProfile, Is.Not.Null);
            Assert.That(settings.NeutralStudioReflection, Is.Not.Null);
            Assert.That(settings.GradientSkyboxShader, Is.Not.Null);
            Assert.That(
                settings.GradientSkyboxShader.name,
                Is.EqualTo(ViewerThemedEnvironment.GradientShaderName));
            Assert.That(settings.UnlitShader, Is.Not.Null);
            Assert.That(settings.LitShader, Is.Not.Null);
        }

        [Test]
        public void ResolvedProfilesMatchTheProvenReferenceTuning()
        {
            ViewerRenderingReferenceCompositionProfile composition =
                ViewerRenderingReferenceComposition.Resolve();
            ViewerRenderingCameraProfile camera =
                composition.CameraProfile;
            ViewerRenderingLightProfile light =
                composition.LightProfile;
            ViewerRenderingEnvironmentProfile environment =
                composition.EnvironmentProfile;
            ViewerRenderingQualityProfile quality =
                composition.QualityProfile;

            Assert.That(
                camera.Position,
                Is.EqualTo(new Vector3(4f, 3f, -6f)));
            Assert.That(
                camera.LookTarget,
                Is.EqualTo(new Vector3(0f, 1f, 0f)));
            Assert.That(
                Quaternion.Angle(
                    camera.Rotation,
                    Quaternion.LookRotation(
                        new Vector3(0f, 1f, 0f)
                        - new Vector3(4f, 3f, -6f))),
                Is.EqualTo(0f).Within(0.001f));

            Assert.That(light.FixedEuler, Is.EqualTo(
                new Vector3(45f, -35f, 0f)));
            Assert.That(light.Intensity, Is.EqualTo(1.05f));
            Assert.That(light.ShadowStrength, Is.EqualTo(0.75f));
            Assert.That(light.ShadowBias, Is.EqualTo(0.05f));
            Assert.That(light.ShadowNormalBias, Is.EqualTo(0.35f));
            Assert.That(light.ShadowNearPlane, Is.EqualTo(0.2f));
            Assert.That(light.CameraRelativeEuler, Is.EqualTo(
                new Vector3(8f, -12f, 0f)));
            Assert.That(light.DampingSeconds, Is.EqualTo(0.35f));

            Assert.That(environment.ReflectionCubemapSize, Is.EqualTo(64));
            Assert.That(environment.ReflectionIntensity, Is.EqualTo(0.6f));
            Assert.That(
                environment.ColorFaithfulAmbientSrgb,
                Is.EqualTo(0.58f));
            Assert.That(
                environment.RealisticAmbientSkySrgb,
                Is.EqualTo(0.68f));
            Assert.That(
                environment.RealisticAmbientEquatorSrgb,
                Is.EqualTo(0.52f));
            Assert.That(
                environment.RealisticAmbientGroundSrgb,
                Is.EqualTo(0.38f));
            Assert.That(
                environment.DarkThemePrimaryStrength,
                Is.EqualTo(0.12f));
            Assert.That(
                environment.LightThemePrimaryStrength,
                Is.EqualTo(0.06f));
            Assert.That(
                environment.NeutralSkyPrimaryStrength,
                Is.EqualTo(0f));
            Assert.That(
                environment.NeutralSkySaturationThreshold,
                Is.EqualTo(0.02f));
            Assert.That(
                quality.DesktopDefaultTier,
                Is.EqualTo(ViewerRenderingQualityTier.Full));
            Assert.That(
                quality.WebGlDefaultTier,
                Is.EqualTo(ViewerRenderingQualityTier.Full));

            DeucarianViewerReferenceThemeProfile theme =
                DeucarianViewerReferenceThemePreset.Resolve();
            Assert.That(composition.ThemeProfile, Is.SameAs(theme));
            Assert.That(
                composition.ThemeMode,
                Is.EqualTo(DeucarianThemeMode.Dark));
        }

        [TestCase(ViewerRenderingQualityTier.Lightweight, false)]
        [TestCase(ViewerRenderingQualityTier.Full, true)]
        public void EffectsPolicyUsesTheSemanticViewerTier(
            ViewerRenderingQualityTier tier,
            bool expected)
        {
            ViewerRenderingQualityProfile quality =
                ViewerRenderingReferenceComposition.Resolve()
                    .QualityProfile;

            Assert.That(
                quality.ShouldEnableEffects(tier),
                Is.EqualTo(expected));
        }

        [TestCase(
            ViewerRenderingQualityTier.Lightweight,
            ViewerRenderingMode.Realistic,
            false)]
        [TestCase(
            ViewerRenderingQualityTier.Full,
            ViewerRenderingMode.ColorFaithful,
            false)]
        [TestCase(
            ViewerRenderingQualityTier.Full,
            ViewerRenderingMode.Realistic,
            true)]
        public void ShadowPolicyRequiresEffectsAndRealisticMode(
            ViewerRenderingQualityTier tier,
            ViewerRenderingMode mode,
            bool expected)
        {
            ViewerRenderingQualityProfile quality =
                ViewerRenderingReferenceComposition.Resolve()
                    .QualityProfile;

            Assert.That(
                quality.ShouldEnableShadows(tier, mode),
                Is.EqualTo(expected));
        }

        [Test]
        public void DisplayPayloadPreservesTheStableGenericWireContract()
        {
            Assert.That(
                ViewerDisplaySettingsPayload.ToPayloadValue(
                    ViewerRenderingMode.ColorFaithful),
                Is.EqualTo("color_faithful"));
            Assert.That(
                ViewerDisplaySettingsPayload.ToPayloadValue(
                    ViewerRenderingMode.Realistic),
                Is.EqualTo("realistic"));
            Assert.That(
                ViewerDisplaySettingsPayload.ToPayloadValue(
                    ViewerDisplaySettingsChangeSource.ViewerUi),
                Is.EqualTo("viewer_ui"));
            Assert.That(
                ViewerDisplaySettingsPayload.ToPayloadValue(
                    ViewerDisplaySettingsChangeSource.Host),
                Is.EqualTo("host"));
            Assert.That(
                ViewerDisplaySettingsPayload.ToPayloadValue(
                    ViewerDisplaySettingsChangeSource.QualityChange),
                Is.EqualTo("quality_change"));
            Assert.That(
                ViewerDisplaySettingsPayload.TryParseRenderingMode(
                    " REALISTIC ",
                    out ViewerRenderingMode mode),
                Is.True);
            Assert.That(mode, Is.EqualTo(ViewerRenderingMode.Realistic));
            Assert.That(
                ViewerDisplaySettingsPayload.TryParseRenderingMode(
                    "unknown",
                    out _),
                Is.False);
        }

        [Test]
        public void RepeatedResolutionKeepsCanonicalAssetAndThemeIdentity()
        {
            ViewerRenderingReferenceCompositionProfile first =
                ViewerRenderingReferenceComposition.Resolve();
            ViewerRenderingReferenceCompositionProfile second =
                ViewerRenderingReferenceComposition.Resolve();

            Assert.That(second.Settings, Is.SameAs(first.Settings));
            Assert.That(second.ThemeProfile, Is.SameAs(first.ThemeProfile));
            Assert.That(
                second.CameraProfile,
                Is.EqualTo(first.CameraProfile));
            Assert.That(
                second.LightProfile,
                Is.EqualTo(first.LightProfile));
            Assert.That(
                second.EnvironmentProfile,
                Is.EqualTo(first.EnvironmentProfile));
            Assert.That(
                second.QualityProfile,
                Is.EqualTo(first.QualityProfile));
            Assert.That(
                first.ResolveDefaultDisplaySettings(false),
                Is.EqualTo(new ViewerDisplaySettingsSnapshot(
                    ViewerRenderingMode.ColorFaithful,
                    false,
                    true)));
            Assert.That(
                first.ResolveDefaultDisplaySettings(true),
                Is.EqualTo(first.ResolveDefaultDisplaySettings(false)));
        }

        [Test]
        public void InstallerAppliesTheExactCompositionAndIsIdempotent()
        {
            RenderPipelineAsset previousPipeline =
                QualitySettings.renderPipeline;
            Material previousSkybox = RenderSettings.skybox;
            GameObject root =
                new GameObject("Viewer Rendering Composition Test");
            GameObject cameraObject =
                new GameObject("Viewer Rendering Test Camera");
            GameObject lightObject =
                new GameObject("Viewer Rendering Test Light");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                Light light = lightObject.AddComponent<Light>();
                DeucarianThemeProvider provider =
                    root.AddComponent<DeucarianThemeProvider>();
                ViewerRenderingReferenceCompositionProfile composition =
                    ViewerRenderingReferenceComposition.Resolve();

                ViewerRenderingInstaller first = composition.Compose(
                    root.transform,
                    camera,
                    light,
                    provider);
                ViewerRenderingInstaller second = composition.Compose(
                    root.transform,
                    camera,
                    light,
                    provider);

                Assert.That(second, Is.SameAs(first));
                Assert.That(first.Camera, Is.SameAs(camera));
                Assert.That(first.KeyLight, Is.SameAs(light));
                Assert.That(first.ThemeProvider, Is.SameAs(provider));
                Assert.That(first.Controller, Is.Not.Null);
                Assert.That(first.Environment, Is.Not.Null);
                Assert.That(
                    camera.transform.position,
                    Is.EqualTo(composition.CameraProfile.Position));
                Assert.That(
                    Quaternion.Angle(
                        camera.transform.rotation,
                        composition.CameraProfile.Rotation),
                    Is.EqualTo(0f).Within(0.001f));
                Assert.That(light.type, Is.EqualTo(LightType.Directional));
                Assert.That(light.color, Is.EqualTo(Color.white));
                Assert.That(
                    light.intensity,
                    Is.EqualTo(composition.LightProfile.Intensity));
                Assert.That(
                    light.shadowStrength,
                    Is.EqualTo(composition.LightProfile.ShadowStrength));
                Assert.That(
                    Quaternion.Angle(
                        light.transform.rotation,
                        composition.LightProfile.FixedRotation),
                    Is.EqualTo(0f).Within(0.001f));
                Assert.That(
                    provider.CurrentThemeFamily,
                    Is.SameAs(composition.ThemeProfile.ThemeFamily));
                Assert.That(
                    provider.ThemeMode,
                    Is.EqualTo(composition.ThemeMode));
                Assert.That(first.Controller.GlobalVolume, Is.Not.Null);
                Assert.That(
                    first.Controller.NeutralReflection,
                    Is.SameAs(
                        composition.Settings.NeutralStudioReflection));
                Assert.That(
                    first.Environment.SkyboxMaterial.shader,
                    Is.SameAs(
                        composition.Settings.GradientSkyboxShader));
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(lightObject);
                Object.DestroyImmediate(cameraObject);
                QualitySettings.renderPipeline = previousPipeline;
                RenderSettings.skybox = previousSkybox;
            }
        }

        [Test]
        public void ControllerPublishesAtomicDisplaySettingsChanges()
        {
            RenderPipelineAsset previousPipeline =
                QualitySettings.renderPipeline;
            GameObject root =
                new GameObject("Viewer Display State Test");
            GameObject cameraObject =
                new GameObject("Viewer Display State Camera");
            GameObject lightObject =
                new GameObject("Viewer Display State Light");
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
                ViewerDisplaySettingsSnapshot observed = default;
                ViewerDisplaySettingsChangeSource observedSource = default;
                int notifications = 0;
                controller.SettingsChanged += (snapshot, source) =>
                {
                    observed = snapshot;
                    observedSource = source;
                    notifications++;
                };

                controller.ApplyDisplaySettings(
                    new ViewerDisplaySettingsRequest(
                        ViewerRenderingMode.Realistic,
                        true),
                    ViewerDisplaySettingsChangeSource.Host);

                Assert.That(notifications, Is.EqualTo(1));
                Assert.That(
                    observed.RenderingMode,
                    Is.EqualTo(ViewerRenderingMode.Realistic));
                Assert.That(observed.CameraRelativeLight, Is.True);
                Assert.That(
                    observedSource,
                    Is.EqualTo(
                        ViewerDisplaySettingsChangeSource.Host));
                Assert.That(
                    controller.GlobalVolume.sharedProfile,
                    Is.SameAs(
                        controller.Settings.RealisticProfile));
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(lightObject);
                Object.DestroyImmediate(cameraObject);
                QualitySettings.renderPipeline = previousPipeline;
            }
        }

        [Test]
        public void InjectedAuthoritativeThemeProviderKeepsItsPersistedMode()
        {
            RenderPipelineAsset previousPipeline =
                QualitySettings.renderPipeline;
            GameObject root =
                new GameObject("Viewer Theme Authority Test");
            GameObject cameraObject =
                new GameObject("Viewer Theme Authority Camera");
            GameObject lightObject =
                new GameObject("Viewer Theme Authority Light");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                Light light = lightObject.AddComponent<Light>();
                DeucarianViewerReferenceThemeProfile theme =
                    DeucarianViewerReferenceThemePreset.Resolve();
                DeucarianThemeProvider provider =
                    root.AddComponent<DeucarianThemeProvider>();
                provider.SetThemeFamily(
                    theme.ThemeFamily,
                    DeucarianThemeMode.Light);

                ViewerRenderingInstaller installer =
                    ViewerRenderingReferenceComposition.Resolve()
                        .Compose(
                            root.transform,
                            camera,
                            light,
                            provider);

                Assert.That(
                    installer.ThemeProvider,
                    Is.SameAs(provider));
                Assert.That(
                    provider.CurrentThemeFamily,
                    Is.SameAs(theme.ThemeFamily));
                Assert.That(
                    provider.ThemeMode,
                    Is.EqualTo(DeucarianThemeMode.Light));
                Assert.That(
                    provider.CurrentTheme,
                    Is.SameAs(theme.LightTheme));
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(lightObject);
                Object.DestroyImmediate(cameraObject);
                QualitySettings.renderPipeline = previousPipeline;
            }
        }

        [Test]
        public void NeutralReflectionAndThemeFallbackStayColorNeutral()
        {
            ViewerRenderingReferenceCompositionProfile composition =
                ViewerRenderingReferenceComposition.Resolve();
            Cubemap packaged =
                composition.Settings.NeutralStudioReflection;
            Assert.That(packaged.width, Is.EqualTo(64));
            Assert.That(packaged.mipmapCount, Is.EqualTo(7));
            Assert.That(packaged.isReadable, Is.False);

            Cubemap fallback =
                ViewerRenderingController.CreateNeutralStudioReflection(
                    false);
            try
            {
                foreach (Vector3 direction in new[]
                         {
                             Vector3.up,
                             Vector3.down,
                             Vector3.forward,
                             new Vector3(-0.45f, 0.72f, -0.53f)
                         })
                {
                    Color color =
                        ViewerRenderingController
                            .ResolveStudioReflectionColor(direction);
                    Assert.That(
                        color.r,
                        Is.EqualTo(color.g).Within(0.000001f));
                    Assert.That(
                        color.g,
                        Is.EqualTo(color.b).Within(0.000001f));
                }

                ViewerThemedEnvironment.ResolveGradientColors(
                    composition.ThemeProfile.DarkTheme,
                    composition.EnvironmentProfile,
                    out Color top,
                    out Color horizon,
                    out Color bottom,
                    out Color primary,
                    out Color background);
                AssertColor(
                    Color.Lerp(background, primary, 0.14f),
                    top);
                AssertColor(
                    Color.Lerp(background, primary, 0.06f),
                    horizon);
                AssertColor(background, bottom);
                Assert.That(
                    ViewerThemedEnvironment.ResolvePrimaryStrength(
                        composition.ThemeProfile.DarkTheme,
                        background,
                        composition.EnvironmentProfile),
                    Is.EqualTo(0.12f));
            }
            finally
            {
                Object.DestroyImmediate(fallback);
            }
        }

        [Test]
        public void MaterialFactoryUsesThePackagedReferenceShaders()
        {
            ViewerRenderingSettings settings =
                ViewerRenderingSettings.LoadReferencePreset();
            ViewerMaterialFactory factory =
                ViewerMaterialFactory.CreateFromReferenceSettings();
            Color color = new Color(0.2f, 0.4f, 0.6f, 0.5f);
            Material transparent = null;
            Material lit = null;
            try
            {
                transparent =
                    factory.CreateTransparentUnlit(
                        "Transparent Test",
                        color);
                lit = factory.CreateLit("Lit Test", color);

                Assert.That(
                    transparent.shader,
                    Is.SameAs(settings.UnlitShader));
                Assert.That(
                    transparent.renderQueue,
                    Is.EqualTo((int)RenderQueue.Transparent));
                Assert.That(transparent.GetInt("_ZWrite"), Is.EqualTo(0));
                Assert.That(lit.shader, Is.SameAs(settings.LitShader));
            }
            finally
            {
                if (transparent != null)
                {
                    Object.DestroyImmediate(transparent);
                }

                if (lit != null)
                {
                    Object.DestroyImmediate(lit);
                }
            }
        }

        [Test]
        public void PackagedVolumeProfilesPreserveTheReferenceLook()
        {
            ViewerRenderingSettings settings =
                ViewerRenderingSettings.LoadReferencePreset();

            Assert.That(
                settings.ColorFaithfulProfile.components,
                Has.Count.EqualTo(1));
            Assert.That(
                settings.ColorFaithfulProfile.TryGet(
                    out Tonemapping neutral),
                Is.True);
            Assert.That(
                neutral.mode.value,
                Is.EqualTo(TonemappingMode.Neutral));

            Assert.That(
                settings.RealisticProfile.components,
                Has.Count.EqualTo(5));
            Assert.That(
                settings.RealisticProfile.TryGet(
                    out Tonemapping realistic),
                Is.True);
            Assert.That(
                realistic.mode.value,
                Is.EqualTo(TonemappingMode.ACES));
            Assert.That(
                settings.RealisticProfile.TryGet(out Bloom bloom),
                Is.True);
            Assert.That(bloom.intensity.value, Is.EqualTo(0.12f));

            UniversalRenderPipelineAsset pipeline =
                settings.PostProcessingPipeline
                    as UniversalRenderPipelineAsset;
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(pipeline.supportsHDR, Is.True);
            Assert.That(
                pipeline.hdrColorBufferPrecision,
                Is.EqualTo(HDRColorBufferPrecision._64Bits));
            Assert.That(
                pipeline.colorGradingMode,
                Is.EqualTo(ColorGradingMode.HighDynamicRange));
            Assert.That(pipeline.colorGradingLutSize, Is.EqualTo(64));
        }

        private static void AssertColor(Color expected, Color actual)
        {
            Assert.That(
                actual.r,
                Is.EqualTo(expected.r).Within(0.000001f));
            Assert.That(
                actual.g,
                Is.EqualTo(expected.g).Within(0.000001f));
            Assert.That(
                actual.b,
                Is.EqualTo(expected.b).Within(0.000001f));
            Assert.That(
                actual.a,
                Is.EqualTo(expected.a).Within(0.000001f));
        }
    }
}
