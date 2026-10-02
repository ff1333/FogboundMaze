# Fogbound Maze

Fogbound Maze is a Unity 6 third-person and first-person survival maze game.
The player chooses a pistol or machete outside a procedurally generated maze,
enters through the start gate, survives zombie attacks, and reaches the exit.

## Portfolio Scope

- Ten deterministic, progressively harder maze levels
- Runtime maze generation with guaranteed start-to-exit connectivity
- Switchable first-person and third-person cameras
- Ranged and melee weapons with distinct risk/reward
- Grid A* navigation and explicit zombie AI states
- Normal and elite zombie variants backed by object pooling
- Fog, day/night cycling, miasma damage and safe-light zones
- Keyboard/mouse and Android touch controls
- Windows, WebGL and Android release targets

## Technology

- Unity `6000.3.18f1`
- C# and the Unity Input System
- Unity Test Framework

Development evidence, architecture notes, tests and release instructions live
under `docs/`.

## Release Status

- Version: `v1.0.0`
- EditMode tests: `8/8 PASS`
- PlayMode tests: `5/5 PASS`
- Builds: Windows, WebGL and Android (`0 errors`, `0 warnings`)
- Start here: [`docs/README.md`](docs/README.md)
- Release checklist: [`docs/04_BUILD_AND_RELEASE.md`](docs/04_BUILD_AND_RELEASE.md)
