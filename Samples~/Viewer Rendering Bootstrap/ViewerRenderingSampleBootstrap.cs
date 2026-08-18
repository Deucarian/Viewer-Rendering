using UnityEngine;

namespace Deucarian.ViewerRendering.Samples.Bootstrap
{
    /// <summary>Minimal explicit reference-rendering composition root.</summary>
    public sealed class ViewerRenderingSampleBootstrap : MonoBehaviour
    {
        public ViewerRenderingInstaller Installer { get; private set; }

        private void Awake()
        {
            Installer =
                ViewerRenderingReferenceComposition.Resolve()
                    .Compose(transform);
        }
    }
}
