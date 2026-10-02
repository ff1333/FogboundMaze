# 01 - Playable Vertical Slice

## Goal

Turn the tested maze algorithm into a complete level loop before expanding the
campaign. The slice must start outside the maze, require a weapon choice, open
the entry gate, spawn navigating enemies and end only at the exit or on death.

## Implemented

- Runtime maze construction with floors, walls, staging area and exit beacon
- Pistol and machete loadout choice
- CharacterController movement and sprint
- First-person and third-person camera modes with wall collision
- Hitscan pistol, magazine reload and melee overlap attack
- Pooled normal and elite zombie models
- Grid A* pursuit with wander, chase, attack and death states
- Health, kills, timer, weapon and level HUD
- Pause, retry, next-level and campaign-complete flows
- Desktop Input System controls and mobile touch-control components

## Engineering Decisions

The ten levels share one scene. Static authoring data lives in the level
catalog, while geometry is generated at runtime. This keeps level difficulty
reviewable as data and prevents scene copies from drifting apart.

Enemy navigation uses the same logical grid as maze generation. A* therefore
cannot cut through a wall and does not require baking a NavMesh after every
procedural generation. Paths are recalculated at a staggered interval rather
than every frame.

## Verification Boundary

At this stage Unity compilation and maze EditMode tests are required. Full
runtime, balance and platform verification are recorded in later release logs.
