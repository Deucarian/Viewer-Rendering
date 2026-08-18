using UnityEngine;

namespace Deucarian.ViewerRendering.Environment
{
    internal static class ViewerStudioReflectionFactory
    {
        private static readonly CubemapFace[] Faces =
        {
            CubemapFace.PositiveX,
            CubemapFace.NegativeX,
            CubemapFace.PositiveY,
            CubemapFace.NegativeY,
            CubemapFace.PositiveZ,
            CubemapFace.NegativeZ
        };

        public static Cubemap Create(
            int size,
            string name,
            bool makeNoLongerReadable)
        {
            var cubemap = new Cubemap(
                size,
                TextureFormat.RGBA32,
                true,
                true)
            {
                name = name,
                hideFlags = HideFlags.DontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear,
                anisoLevel = 0
            };

            Color[] pixels = new Color[size * size];
            foreach (CubemapFace face in Faces)
            {
                for (int y = 0; y < size; y++)
                {
                    float v =
                        (((y + 0.5f) / size) * 2f) - 1f;
                    for (int x = 0; x < size; x++)
                    {
                        float u =
                            (((x + 0.5f) / size) * 2f) - 1f;
                        Vector3 direction = ResolveDirection(
                            face,
                            u,
                            v).normalized;
                        pixels[(y * size) + x] =
                            ResolveColor(direction);
                    }
                }

                cubemap.SetPixels(pixels, face);
            }

            cubemap.Apply(true, makeNoLongerReadable);
            return cubemap;
        }

        public static Color ResolveColor(Vector3 direction)
        {
            direction = direction.sqrMagnitude > 0f
                ? direction.normalized
                : Vector3.up;
            float vertical = Mathf.Clamp01(
                (direction.y * 0.5f) + 0.5f);
            float baseSrgb = Mathf.Lerp(0.20f, 0.70f, vertical);
            Vector3 panelDirection =
                new Vector3(-0.45f, 0.72f, -0.53f).normalized;
            float panel = Mathf.Pow(
                Mathf.Clamp01(
                    Vector3.Dot(direction, panelDirection)),
                18f) * 0.28f;
            float linear = Mathf.GammaToLinearSpace(
                Mathf.Clamp01(baseSrgb + panel));
            return new Color(linear, linear, linear, 1f);
        }

        private static Vector3 ResolveDirection(
            CubemapFace face,
            float u,
            float v)
        {
            switch (face)
            {
                case CubemapFace.PositiveX:
                    return new Vector3(1f, -v, -u);
                case CubemapFace.NegativeX:
                    return new Vector3(-1f, -v, u);
                case CubemapFace.PositiveY:
                    return new Vector3(u, 1f, v);
                case CubemapFace.NegativeY:
                    return new Vector3(u, -1f, -v);
                case CubemapFace.PositiveZ:
                    return new Vector3(u, -v, 1f);
                default:
                    return new Vector3(-u, -v, -1f);
            }
        }
    }
}
