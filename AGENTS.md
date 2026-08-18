# Deucarian Viewer Rendering Agent Notes

Package ID: `com.deucarian.viewer-rendering`

Follow the canonical Deucarian architecture standard in
`C:/Repositories/Package-Registry/ARCHITECTURE.md`.

## Ownership

This package owns the reusable viewer visual baseline: reference camera pose,
directional-light policy, quality-tier URP selection, display-settings state,
neutral ambient/reflection environment, post-processing profiles, and
theme-derived sky presentation.

It must not own navigation, raw input, pointer capture, UI chrome, command
routing, browser transport, model loading, material import policy, report or
activity DTOs, media behavior, or product branding.

## Dependencies

- Theming owns palettes, theme families, and the shared reference viewer theme.
- Common owns runtime Unity object cleanup.
- Logging owns package diagnostics output.
- Diagnostics owns operational health reporting.
- URP owns render-pipeline, volume, and camera rendering primitives.

## Policies

- Keep one authoritative display-settings state owner and publish changes by
  event.
- Treat global render settings and pipeline changes as explicit lifecycle-owned
  mutations; restore captured state when the controller is disabled or destroyed.
- Keep camera, light, environment, and quality decisions exposed through
  immutable resolved profiles so consumers can prove parity without copying
  constants.
- Do not introduce product-specific names, role IDs, commands, DTOs, media
  concepts, or branding.
- Do not add direct Unity `Debug` calls or direct object-destruction helpers.
- Keep production source files under 500 lines.
- Validate metadata, EditMode tests, asset GUID references, and
  `git diff --check`.
