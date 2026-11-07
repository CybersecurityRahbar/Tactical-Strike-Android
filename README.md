# Tactical Strike

**Android-first, offline-capable tactical infantry game.**

The game is being built in this repository with Godot 4.7.2 stable. Development, validation, and Android APK export are intended to run through GitHub Actions; no desktop editor is required for the planned workflow.

## Battlefield direction

One large, semi-arid plateau map with long sightlines: a relatively open attacker sector, rolling and broken ground through the middle, and a distant defender sector with two separated buildings, trenches, sandbag positions, brick and stone cover, and barriers. The ground is dusty brown earth, not desert dunes and not lush green terrain.

## Gameplay direction

- First-person Android controls.
- Single-player battle with tactical AI soldiers.
- Local Wi-Fi / hotspot multiplayer without an internet server.
- AK-74, M4A1, PKM, RPG-7, SVD/Dragunov, DShK 12.7 mm, and optional .50-calibre heavy machine gun.
- Normal and red tracer ammunition variants.
- Game-tuned projectile velocity, drag, gravity, range, penetration, and damage by weapon.
- Dust impacts, cover damage, ricochet and directional bullet audio.
- Day and night lighting.

## Current milestone

The first commit establishes the Godot project, procedural terrain and cover layout, first-person controller, configurable weapon data, projectile simulation, tracer visuals, impact dust, synthesized placeholder audio, HUD, and Android build workflow.

**Not yet complete:** tactical bot decision-making, attacker/defender coordination, finished weapon/character art and animation, real recorded sound libraries, LAN multiplayer, and performance validation on target phones. These will be implemented and tested in later milestones.

See [PROJECT_CONTEXT.md](PROJECT_CONTEXT.md) for the cumulative requirements and implementation record.
