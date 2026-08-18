# Deucarian Viewer Rendering

`com.deucarian.viewer-rendering` owns the reusable visual baseline for Deucarian
3D viewers: the canonical camera pose, directional light, URP quality policy,
neutral ambient/reflection environment, post-processing profiles, and themed
gradient sky.

Current package version: `0.1.0`. Unity `6000.0` or newer is required because
the packaged render-pipeline assets target URP `17.3.0`.

## Reference composition

Compose the exact reference setup from an application composition root:

```csharp
ViewerRenderingReferenceCompositionProfile rendering =
    ViewerRenderingReferenceComposition.Resolve();
ViewerRenderingInstaller installer = rendering.Compose(transform);
```

Existing camera, key-light, and theme-provider instances can be made
authoritative while retaining the same resolved policy:

```csharp
ViewerRenderingInstaller installer = rendering.Compose(
    transform,
    viewerCamera,
    keyLight,
    themeProvider);
```

An injected provider keeps its already selected family and persisted light/dark
mode. The reference family/default mode is assigned only when the installer has
to create or initialize an empty provider.

The installer applies the packaged reference camera pose, configures the light,
installs the shared theme family, creates the global post-processing volume,
and binds the theme-authored sky. Repeated composition is idempotent.

## Display settings contract

`IViewerRenderingController` exposes one immutable
`ViewerDisplaySettingsSnapshot`, an observable `SettingsChanged` event, and
an atomic `ApplyDisplaySettings` request. The package defines only generic
rendering state. Browser payloads, commands, report data, media, and product
branding remain consumer-owned.

Rendering capability is selected through the consumer-neutral
`ViewerRenderingQualityTier` contract. The reference composition defaults to
`Full` on both desktop and WebGL, so every consumer starts with the complete
reference look regardless of how its Unity project names or orders quality
levels. Runtime code deliberately does not read Unity quality-level indices.

Hosts that need an explicit reduced-cost mode can request `Lightweight` through
`IViewerRenderingQualityController.ApplyQualityTier`. Tier changes use the same
observable settings event and are lifecycle-owned: package mutations are
restored while the controller is disabled and reapplied when it is enabled.

`ViewerRenderingReferenceComposition.Resolve()` also exposes immutable camera,
light, and environment profiles. Consumers can compare these values directly in
parity tests instead of duplicating constants.

`ViewerDisplaySettingsPayload` provides stable generic wire values and parsing
for consumers that expose the state through commands or browser events.

`IViewerMaterialFactory` and `ViewerMaterialFactory` create transient lit and
transparent-unlit materials from the same packaged shader references. Product
markers and model semantics remain consumer-owned.

## Theme integration

The package consumes `DeucarianViewerReferenceThemePreset` from
`com.deucarian.theming`. Background and primary colors drive the restrained
gradient fallback. A custom theme may optionally author the generic environment
roles exposed by `ViewerRenderingColorRoleIds`; no product-specific role IDs
are used.

## Asset ownership

The package owns both reference URP pipelines and renderer-data assets, the two
volume profiles, the neutral studio reflection cubemap, and the gradient sky
shader. Consumers should not copy those assets into application projects.

The migrated assets intentionally have package-owned GUIDs. Reusing the donor
GUIDs would create duplicate-GUID collisions during the transition period in
which both copies are installed.

## Validation

EditMode tests cover the resolved reference values, semantic quality and shadow
policy, independence from consumer Unity quality indices, explicit tier
switching and lifecycle restoration, asset graph, composition idempotency,
camera/light setup, display notifications, neutral reflection fallback, and
theme-derived gradient behavior. An architecture assertion prevents runtime
code from reintroducing `QualitySettings.GetQualityLevel` coupling.

Run the shared package validator before publication:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

## License

Released under the MIT License. See `LICENSE.md`.
