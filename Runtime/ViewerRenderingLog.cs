using Deucarian.Logging;

namespace Deucarian.ViewerRendering
{
    internal static class ViewerRenderingLog
    {
        public static readonly DLog Rendering =
            DLog.For("ViewerRendering.Rendering");
        public static readonly DLog Environment =
            DLog.For("ViewerRendering.Environment");
    }
}
