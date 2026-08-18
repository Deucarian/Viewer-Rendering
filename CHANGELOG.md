# Changelog

All notable changes to this package are documented in this file.

## [0.1.0] - 2026-08-18

### Added

- Consumer-neutral reference camera, light, ambient, reflection, URP, and
  post-processing composition extracted from the proven viewer baseline.
- Immutable display-settings snapshot/request contract and observable rendering
  controller.
- Stable generic display-settings wire-value mapping and reference shader-backed
  material factory contract.
- Shared reference composition and idempotent scene installer.
- Theme-aware gradient environment with package-owned generic color-role IDs.
- Package-owned URP pipelines, renderer data, volume profiles, neutral reflection,
  and gradient sky shader.
- Operational diagnostics and EditMode parity coverage.
- Consumer-neutral `Lightweight` and `Full` rendering tiers, with the complete
  reference look selected by default on both desktop and WebGL.
- Explicit observable tier switching with lifecycle-safe restoration, plus an
  architecture guard against consumer-owned Unity quality-level indices.
