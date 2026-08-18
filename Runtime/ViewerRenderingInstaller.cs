using System;
using Deucarian.Theming;
using UnityEngine;

namespace Deucarian.ViewerRendering
{
    /// <summary>
    /// Idempotent scene composition root for the canonical viewer visual baseline.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ViewerRenderingInstaller : MonoBehaviour
    {
        [SerializeField] private ViewerRenderingSettings settings;
        [SerializeField] private Camera viewerCamera;
        [SerializeField] private Light keyLight;

        public ViewerRenderingReferenceCompositionProfile Composition
        {
            get;
            private set;
        }

        public ViewerRenderingSettings Settings => settings;
        public Camera Camera => viewerCamera;
        public Light KeyLight => keyLight;
        public DeucarianThemeProvider ThemeProvider { get; private set; }
        public ViewerRenderingController Controller { get; private set; }
        public ViewerThemedEnvironment Environment { get; private set; }

        public static ViewerRenderingInstaller CreateWithReferencePreset(
            Transform parent,
            Camera camera = null,
            Light light = null,
            DeucarianThemeProvider themeProvider = null)
        {
            return ViewerRenderingReferenceComposition.Resolve().Compose(
                parent,
                camera,
                light,
                themeProvider);
        }

        internal static ViewerRenderingInstaller
            CreateWithReferenceComposition(
                Transform parent,
                Camera camera,
                Light light,
                DeucarianThemeProvider themeProvider,
                ViewerRenderingReferenceCompositionProfile composition)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            ViewerRenderingInstaller installer =
                parent.GetComponent<ViewerRenderingInstaller>();
            if (installer == null)
            {
                installer =
                    parent.gameObject.AddComponent<ViewerRenderingInstaller>();
            }

            installer.Initialize(
                composition,
                camera,
                light,
                themeProvider);
            return installer;
        }

        private void Initialize(
            ViewerRenderingReferenceCompositionProfile composition,
            Camera camera,
            Light light,
            DeucarianThemeProvider themeProvider)
        {
            Composition = composition;
            settings = composition.Settings;
            ThemeProvider = EnsureThemeProvider(
                transform,
                themeProvider,
                composition);
            viewerCamera = EnsureCamera(
                transform,
                camera,
                composition.CameraProfile);
            keyLight = EnsureKeyLight(transform, light);
            EnsureComponents();
            Controller.Initialize(
                viewerCamera,
                keyLight,
                composition);
            Environment.Initialize(
                viewerCamera,
                settings.GradientSkyboxShader,
                ThemeProvider,
                composition.EnvironmentProfile);
        }

        private void EnsureComponents()
        {
            Controller = GetComponent<ViewerRenderingController>();
            if (Controller == null)
            {
                Controller =
                    gameObject.AddComponent<ViewerRenderingController>();
            }

            Environment = GetComponent<ViewerThemedEnvironment>();
            if (Environment == null)
            {
                Environment =
                    gameObject.AddComponent<ViewerThemedEnvironment>();
            }
        }

        private static DeucarianThemeProvider EnsureThemeProvider(
            Transform parent,
            DeucarianThemeProvider authoritativeProvider,
            ViewerRenderingReferenceCompositionProfile composition)
        {
            DeucarianThemeProvider provider = authoritativeProvider != null
                ? authoritativeProvider
                : parent.GetComponent<DeucarianThemeProvider>();
            bool created = false;
            if (provider == null)
            {
                provider =
                    parent.gameObject.AddComponent<DeucarianThemeProvider>();
                created = true;
            }

            if (created
                || (provider.CurrentThemeFamily == null
                    && provider.CurrentTheme == null))
            {
                provider.SetThemeFamily(
                    composition.ThemeProfile.ThemeFamily,
                    composition.ThemeMode);
            }

            return provider;
        }

        private static Camera EnsureCamera(
            Transform parent,
            Camera suppliedCamera,
            ViewerRenderingCameraProfile profile)
        {
            Camera camera = suppliedCamera != null
                ? suppliedCamera
                : Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.transform.SetParent(parent, false);
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            profile.Apply(camera);
            return camera;
        }

        private static Light EnsureKeyLight(
            Transform parent,
            Light suppliedLight)
        {
            if (suppliedLight != null)
            {
                return suppliedLight;
            }

            Light[] lights = FindObjectsByType<Light>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null
                    && lights[i].type == LightType.Directional)
                {
                    return lights[i];
                }
            }

            GameObject lightObject =
                new GameObject("Viewer Reference Key Light");
            lightObject.transform.SetParent(parent, false);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            return light;
        }
    }
}
