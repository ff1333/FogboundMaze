# Fogbound Maze - Game Design Baseline

## Player Promise

Choose a weapon, enter an unfamiliar maze, read the light and sound around you,
and survive long enough to find the exit.

## Core Loop

1. Spawn in the staging area outside the maze.
2. Choose the pistol or machete.
3. Enter through the start gate.
4. Navigate while managing health, enemies and environmental pressure.
5. Reach the exit gate alive to unlock the next level.
6. Retry or continue until all ten levels are cleared.

## Control Scheme

| Action | Desktop | Mobile |
| --- | --- | --- |
| Move | WASD | Left virtual stick |
| Look | Mouse | Drag right half |
| Attack | Left mouse | Attack button |
| Reload | R | Reload button |
| Sprint | Left Shift | Sprint button |
| Camera | V | Camera button |
| Pause | Escape | Pause button |

## Difficulty Curve

- Levels 1-3 teach navigation and combat.
- Level 4 introduces meaningful fog.
- Level 5 introduces elite zombies.
- Level 6 introduces a day/night cycle.
- Level 7 introduces miasma and safe-light zones.
- Levels 8-10 combine every system with larger mazes and higher pressure.

Every level uses a different deterministic maze seed and always has a valid
path. The generator evaluates multiple candidates using route length, turns,
dead ends and junctions; later levels use larger grids and more candidate
selection so complexity increases as part of the difficulty curve.
Enemy spawns exclude the start area and cells immediately around the player.
Safe lights are placed along the solved route so miasma never creates an
unavoidable failure state.

## Release Boundary

Version 1.0.0 includes the complete ten-level single-player campaign. Online
multiplayer, inventory crafting, live-service progression and paid assets are
explicitly outside the release scope.
