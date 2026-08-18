using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Deucarian.ViewerRendering
{
    /// <summary>
    /// Reference shader-backed factory for transient viewer materials.
    /// </summary>
    public sealed class ViewerMaterialFactory :
        IViewerMaterialFactory
    {
        private static readonly int BaseColorId =
            Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId =
            Shader.PropertyToID("_Color");

        private readonly Shader _unlitShader;
        private readonly Shader _litShader;

        public ViewerMaterialFactory(
            Shader unlitShader,
            Shader litShader)
        {
            _unlitShader = unlitShader
                ? unlitShader
                : throw new ArgumentNullException(
                    nameof(unlitShader));
            _litShader = litShader
                ? litShader
                : throw new ArgumentNullException(
                    nameof(litShader));
        }

        public static ViewerMaterialFactory
            CreateFromReferenceSettings()
        {
            ViewerRenderingSettings settings =
                ViewerRenderingSettings.LoadReferencePreset();
            if (settings == null)
            {
                throw new InvalidOperationException(
                    "Viewer rendering reference settings are missing from Resources.");
            }

            return new ViewerMaterialFactory(
                settings.UnlitShader,
                settings.LitShader);
        }

        public Material CreateTransparentUnlit(
            string name,
            Color color)
        {
            var material = new Material(_unlitShader)
            {
                name = name,
                renderQueue = (int)RenderQueue.Transparent,
                hideFlags = HideFlags.DontSave
            };
            ApplyColor(material, color);
            material.SetInt(
                "_SrcBlend",
                (int)BlendMode.SrcAlpha);
            material.SetInt(
                "_DstBlend",
                (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_ALPHABLEND_ON");
            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            if (material.HasProperty("_Cull"))
            {
                material.SetFloat("_Cull", 0f);
            }

            return material;
        }

        public Material CreateLit(
            string name,
            Color color)
        {
            var material = new Material(_litShader)
            {
                name = name,
                hideFlags = HideFlags.DontSave
            };
            ApplyColor(material, color);
            return material;
        }

        private static void ApplyColor(
            Material material,
            Color color)
        {
            if (material.HasProperty(BaseColorId))
            {
                material.SetColor(BaseColorId, color);
            }

            if (material.HasProperty(ColorId))
            {
                material.SetColor(ColorId, color);
            }
        }
    }
}
