using System;
using UnityEngine;

namespace Deucarian.ViewerRendering
{
    /// <summary>Resolved canonical camera pose.</summary>
    public readonly struct ViewerRenderingCameraProfile :
        IEquatable<ViewerRenderingCameraProfile>
    {
        public ViewerRenderingCameraProfile(
            Vector3 position,
            Vector3 lookTarget)
        {
            Position = position;
            LookTarget = lookTarget;
        }

        public Vector3 Position { get; }
        public Vector3 LookTarget { get; }
        public Quaternion Rotation =>
            Quaternion.LookRotation(LookTarget - Position);

        public void Apply(Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            camera.transform.position = Position;
            camera.transform.rotation = Rotation;
        }

        public bool Equals(ViewerRenderingCameraProfile other) =>
            Position == other.Position && LookTarget == other.LookTarget;

        public override bool Equals(object obj) =>
            obj is ViewerRenderingCameraProfile other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 397)
                       ^ LookTarget.GetHashCode();
            }
        }
    }

    /// <summary>Resolved canonical directional-light policy.</summary>
    public readonly struct ViewerRenderingLightProfile :
        IEquatable<ViewerRenderingLightProfile>
    {
        public ViewerRenderingLightProfile(
            Vector3 fixedEuler,
            float intensity,
            float shadowStrength,
            float shadowBias,
            float shadowNormalBias,
            float shadowNearPlane,
            Vector3 cameraRelativeEuler,
            float dampingSeconds)
        {
            FixedEuler = fixedEuler;
            Intensity = intensity;
            ShadowStrength = shadowStrength;
            ShadowBias = shadowBias;
            ShadowNormalBias = shadowNormalBias;
            ShadowNearPlane = shadowNearPlane;
            CameraRelativeEuler = cameraRelativeEuler;
            DampingSeconds = dampingSeconds;
        }

        public Vector3 FixedEuler { get; }
        public Quaternion FixedRotation => Quaternion.Euler(FixedEuler);
        public float Intensity { get; }
        public float ShadowStrength { get; }
        public float ShadowBias { get; }
        public float ShadowNormalBias { get; }
        public float ShadowNearPlane { get; }
        public Vector3 CameraRelativeEuler { get; }
        public Quaternion CameraRelativeOffset =>
            Quaternion.Euler(CameraRelativeEuler);
        public float DampingSeconds { get; }

        public Quaternion ResolveCameraRelativeTarget(
            Quaternion cameraRotation) =>
            cameraRotation * CameraRelativeOffset;

        public float ResolveDampingFactor(float unscaledDeltaTime)
        {
            if (unscaledDeltaTime <= 0f)
            {
                return 0f;
            }

            return 1f - Mathf.Exp(
                -unscaledDeltaTime / Mathf.Max(DampingSeconds, 0.0001f));
        }

        public bool Equals(ViewerRenderingLightProfile other)
        {
            return FixedEuler == other.FixedEuler
                   && Intensity.Equals(other.Intensity)
                   && ShadowStrength.Equals(other.ShadowStrength)
                   && ShadowBias.Equals(other.ShadowBias)
                   && ShadowNormalBias.Equals(other.ShadowNormalBias)
                   && ShadowNearPlane.Equals(other.ShadowNearPlane)
                   && CameraRelativeEuler == other.CameraRelativeEuler
                   && DampingSeconds.Equals(other.DampingSeconds);
        }

        public override bool Equals(object obj) =>
            obj is ViewerRenderingLightProfile other && Equals(other);

        public override int GetHashCode() => FixedEuler.GetHashCode();
    }

    /// <summary>Resolved ambient, reflection, and sky policy.</summary>
    public readonly struct ViewerRenderingEnvironmentProfile :
        IEquatable<ViewerRenderingEnvironmentProfile>
    {
        public ViewerRenderingEnvironmentProfile(
            int reflectionCubemapSize,
            float reflectionIntensity,
            float colorFaithfulAmbientSrgb,
            float realisticAmbientSkySrgb,
            float realisticAmbientEquatorSrgb,
            float realisticAmbientGroundSrgb,
            float darkThemePrimaryStrength,
            float lightThemePrimaryStrength,
            float neutralSkyPrimaryStrength,
            float neutralSkySaturationThreshold)
            : this(
                reflectionCubemapSize,
                reflectionIntensity,
                colorFaithfulAmbientSrgb,
                realisticAmbientSkySrgb,
                realisticAmbientEquatorSrgb,
                realisticAmbientGroundSrgb,
                ViewerRenderingSettings.DefaultReferenceSkyTop,
                ViewerRenderingSettings.DefaultReferenceSkyHorizon,
                ViewerRenderingSettings.DefaultReferenceSkyBottom,
                darkThemePrimaryStrength,
                lightThemePrimaryStrength,
                neutralSkyPrimaryStrength,
                neutralSkySaturationThreshold)
        {
        }

        public ViewerRenderingEnvironmentProfile(
            int reflectionCubemapSize,
            float reflectionIntensity,
            float colorFaithfulAmbientSrgb,
            float realisticAmbientSkySrgb,
            float realisticAmbientEquatorSrgb,
            float realisticAmbientGroundSrgb,
            Color referenceSkyTop,
            Color referenceSkyHorizon,
            Color referenceSkyBottom,
            float darkThemePrimaryStrength,
            float lightThemePrimaryStrength,
            float neutralSkyPrimaryStrength,
            float neutralSkySaturationThreshold)
        {
            ReflectionCubemapSize = reflectionCubemapSize;
            ReflectionIntensity = reflectionIntensity;
            ColorFaithfulAmbientSrgb = colorFaithfulAmbientSrgb;
            RealisticAmbientSkySrgb = realisticAmbientSkySrgb;
            RealisticAmbientEquatorSrgb = realisticAmbientEquatorSrgb;
            RealisticAmbientGroundSrgb = realisticAmbientGroundSrgb;
            ReferenceSkyTop = referenceSkyTop;
            ReferenceSkyHorizon = referenceSkyHorizon;
            ReferenceSkyBottom = referenceSkyBottom;
            DarkThemePrimaryStrength = darkThemePrimaryStrength;
            LightThemePrimaryStrength = lightThemePrimaryStrength;
            NeutralSkyPrimaryStrength = neutralSkyPrimaryStrength;
            NeutralSkySaturationThreshold = neutralSkySaturationThreshold;
        }

        public int ReflectionCubemapSize { get; }
        public float ReflectionIntensity { get; }
        public float ColorFaithfulAmbientSrgb { get; }
        public float RealisticAmbientSkySrgb { get; }
        public float RealisticAmbientEquatorSrgb { get; }
        public float RealisticAmbientGroundSrgb { get; }
        public Color ReferenceSkyTop { get; }
        public Color ReferenceSkyHorizon { get; }
        public Color ReferenceSkyBottom { get; }
        public float DarkThemePrimaryStrength { get; }
        public float LightThemePrimaryStrength { get; }
        public float NeutralSkyPrimaryStrength { get; }
        public float NeutralSkySaturationThreshold { get; }

        public bool Equals(ViewerRenderingEnvironmentProfile other)
        {
            return ReflectionCubemapSize == other.ReflectionCubemapSize
                   && ReflectionIntensity.Equals(other.ReflectionIntensity)
                   && ColorFaithfulAmbientSrgb.Equals(
                       other.ColorFaithfulAmbientSrgb)
                   && RealisticAmbientSkySrgb.Equals(
                       other.RealisticAmbientSkySrgb)
                   && RealisticAmbientEquatorSrgb.Equals(
                       other.RealisticAmbientEquatorSrgb)
                   && RealisticAmbientGroundSrgb.Equals(
                       other.RealisticAmbientGroundSrgb)
                   && ReferenceSkyTop.Equals(other.ReferenceSkyTop)
                   && ReferenceSkyHorizon.Equals(other.ReferenceSkyHorizon)
                   && ReferenceSkyBottom.Equals(other.ReferenceSkyBottom)
                   && DarkThemePrimaryStrength.Equals(
                       other.DarkThemePrimaryStrength)
                   && LightThemePrimaryStrength.Equals(
                       other.LightThemePrimaryStrength)
                   && NeutralSkyPrimaryStrength.Equals(
                       other.NeutralSkyPrimaryStrength)
                   && NeutralSkySaturationThreshold.Equals(
                       other.NeutralSkySaturationThreshold);
        }

        public override bool Equals(object obj) =>
            obj is ViewerRenderingEnvironmentProfile other
            && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (ReflectionCubemapSize * 397)
                       ^ ReflectionIntensity.GetHashCode()
                       ^ ReferenceSkyTop.GetHashCode()
                       ^ ReferenceSkyHorizon.GetHashCode()
                       ^ ReferenceSkyBottom.GetHashCode();
            }
        }
    }
}
