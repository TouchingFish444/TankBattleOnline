# TankBattleOnline Assets

This folder keeps only the sprites and sounds used by the current gameplay.
Release builds embed the required files into the exe, so the green zip does not
depend on an external `assets/` directory.

## Kept Assets

- `img/tank/`: player, enemy, and upgraded tank direction sprites. Names are resolved dynamically.
- `img/wall/`: wall, steel, grass, and water terrain sprites.
- `img/fire/tankmissile.gif`: bullet sprite.
- `img/other/born*.gif`, `star.gif`, `over.gif`: spawn, upgrade, and game-over sprites.
- `music/start.wav`, `fire.wav`: lobby/game start and bullet firing sounds.

## Maintenance

- `TankBattleOnline.csproj` uses an embedded-resource whitelist. Add new assets there only after code starts using them.
- Keep existing file names stable because `GameResourceManager` resolves many resources by path.
