using UnityEngine;

namespace Deucarian.ViewerRendering
{
    /// <summary>
    /// Creates transient materials using the reference viewer shader set.
    /// Callers retain ownership of returned materials.
    /// </summary>
    public interface IViewerMaterialFactory
    {
        Material CreateTransparentUnlit(
            string name,
            Color color);

        Material CreateLit(
            string name,
            Color color);
    }
}
