# 02 - Campaign Feedback And Pooling

## Changes

- Constrained each level around an increasing target route length.
- Added deterministic ten-level complexity evidence.
- Added red and purple enemy spawn telegraphs.
- Added pistol tracers, impact flashes and generated weapon audio.
- Persisted the selected and unlocked campaign level.
- Changed miasma HUD feedback between safe light and exposed states.
- Prewarmed the enemy pool to each level's maximum active count.

## Verification

- EditMode: 8/8 PASS before this log update.
- PlayMode: 3/3 PASS before pool prewarm assertion was added.
- Windows development build: succeeded with zero build warnings.
- Windows staging and gameplay Player screenshots: captured.

The test suite is rerun after this commit before release status is assigned.
