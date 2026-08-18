using System;

namespace Deucarian.ViewerRendering
{
    /// <summary>Read/write port for authoritative viewer display settings.</summary>
    public interface IViewerRenderingController
    {
        event Action<
            ViewerDisplaySettingsSnapshot,
            ViewerDisplaySettingsChangeSource> SettingsChanged;

        ViewerDisplaySettingsSnapshot CurrentSettings { get; }

        void ApplyDisplaySettings(
            ViewerDisplaySettingsRequest request,
            ViewerDisplaySettingsChangeSource source);
    }
}
