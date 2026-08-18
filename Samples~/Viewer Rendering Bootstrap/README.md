# Viewer Rendering Bootstrap

Add `ViewerRenderingSampleBootstrap` to an empty GameObject. On `Awake`, the
component resolves and composes the canonical reference camera, key light,
quality policy, neutral reflection environment, theme provider, and gradient
sky.

Production applications may instead pass their existing camera, directional
light, and authoritative `DeucarianThemeProvider` to the four-argument
`Compose` overload.
