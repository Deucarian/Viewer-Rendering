namespace Deucarian.ViewerRendering
{
    /// <summary>
    /// Optional generic theme roles for an authored viewer environment.
    /// Themes without these roles use the package-owned neutral studio sky.
    /// </summary>
    public static class ViewerRenderingColorRoleIds
    {
        public const string EnvironmentSkyTop =
            "deucarian.viewer.environment.sky.top";
        public const string EnvironmentSkyHorizon =
            "deucarian.viewer.environment.sky.horizon";
        public const string EnvironmentSkyBottom =
            "deucarian.viewer.environment.sky.bottom";
    }
}
