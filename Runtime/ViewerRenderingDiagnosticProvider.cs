using Deucarian.Diagnostics;

namespace Deucarian.ViewerRendering
{
    internal sealed class ViewerRenderingDiagnosticProvider :
        IDiagnosticProvider
    {
        private readonly ViewerRenderingController _controller;

        public ViewerRenderingDiagnosticProvider(
            ViewerRenderingController controller)
        {
            _controller = controller;
        }

        public string ProviderId => _controller != null
            ? "viewer-rendering." + _controller.GetInstanceID()
            : "viewer-rendering";

        public string DisplayName => "Viewer Rendering";

        public void Collect(DiagnosticReportBuilder builder)
        {
            DiagnosticSection section =
                builder.AddSection(ProviderId, DisplayName);
            if (_controller == null)
            {
                section.AddItem(
                    "controller",
                    "Controller",
                    "Missing",
                    DiagnosticSeverity.Error);
                return;
            }

            ViewerDisplaySettingsSnapshot snapshot =
                _controller.CurrentSettings;
            section
                .AddItem(
                    "mode",
                    "Rendering Mode",
                    snapshot.RenderingMode.ToString())
                .AddItem(
                    "camera_relative_light",
                    "Camera-relative Light",
                    snapshot.CameraRelativeLight.ToString())
                .AddItem(
                    "quality_tier",
                    "Viewer Quality Tier",
                    _controller.ActiveQualityTier.ToString())
                .AddItem(
                    "effects",
                    "Quality Effects",
                    snapshot.EffectsActive ? "Active" : "Reduced")
                .AddItem(
                    "settings",
                    "Reference Assets",
                    _controller.Settings != null
                    && _controller.Settings.IsComplete
                        ? "Complete"
                        : "Incomplete",
                    _controller.Settings != null
                    && _controller.Settings.IsComplete
                        ? DiagnosticSeverity.Info
                        : DiagnosticSeverity.Error)
                .AddItem(
                    "camera",
                    "Camera",
                    _controller.Camera != null
                        ? "Ready"
                        : "Missing",
                    _controller.Camera != null
                        ? DiagnosticSeverity.Info
                        : DiagnosticSeverity.Error)
                .AddItem(
                    "key_light",
                    "Key Light",
                    _controller.KeyLight != null
                        ? "Ready"
                        : "Missing",
                    _controller.KeyLight != null
                        ? DiagnosticSeverity.Info
                        : DiagnosticSeverity.Error)
                .AddItem(
                    "volume",
                    "Global Volume",
                    _controller.GlobalVolume != null
                        ? "Ready"
                        : "Missing",
                    _controller.GlobalVolume != null
                        ? DiagnosticSeverity.Info
                        : DiagnosticSeverity.Error)
                .AddItem(
                    "reflection",
                    "Neutral Reflection",
                    _controller.NeutralReflection != null
                        ? "Ready"
                        : "Missing",
                    _controller.NeutralReflection != null
                        ? DiagnosticSeverity.Info
                        : DiagnosticSeverity.Warning);
        }
    }
}
