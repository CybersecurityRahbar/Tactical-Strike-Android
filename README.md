# Tactical Strike

**An original Android-first tactical infantry game.**

The project is built with Godot 4.7.2 and maintained entirely in this GitHub repository. GitHub Actions performs project validation and exports the Android APK; the developer's local computer is not part of the required workflow.

## Battlefield direction

The first and only initial map is a wide 700 m × 700 m plateau battlefield with long sightlines. The attacker begins on one side of the map; the distant defensive sector is roughly 500 m away along the main firing direction. The terrain uses dusty brown earth, rolling high and low ridges, shallow cuts and open approach ground—not sand dunes and not lush green terrain.

The defensive sector is built around two separated buildings, main and fallback trench lines, sandbag positions, brick walls, stone cover and barriers. The middle ground uses irregular ridges and scattered firing cover to create long-range engagements.

See the [battlefield layout blueprint](docs/BATTLEFIELD_DESIGN.md) and its [top-down SVG map](docs/battlefield-layout.svg).

## Combat direction

- First-person Android controls.
- Offline single-player against tactical AI soldiers.
- Local Wi-Fi / hotspot multiplayer without an internet server.
- AK-74, M4A1, PKM, RPG-7, SVD/Dragunov, DShK 12.7 mm and .50-calibre heavy machine gun.
- Standard and red tracer ammunition variants.
- Configurable game-ballistics: muzzle velocity, drag, gravity, drop, effective range, damage and penetration.
- Dust impacts, cover integrity, shallow-angle ricochet and directional near-miss audio.
- Day and night environment settings.

## Latest verified Android build

The latest Android debug build was verified on 8 November 2026:
- [Open successful GitHub Actions run 42418460738](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42418460738).
- The asset preparation stage imports a skinned AK-74M first-person model with arms and authored idle/draw/walk/run/shot/reload animations.
- The mobile acceptance test injects actual HUD touch events and verifies movement, fire, aim, ammunition switching and reload handling.
- Terrain collision, upward-facing mesh normals, tangents, PBR diffuse/normal maps and height relief pass the strengthened checks.
- Android APK export, ZIP integrity, signature, Android manifest, DEX and ARM64 native-library checks all passed.
- The verified Actions artifact includes the APK, SHA-256 checksum and validation report.
- APK SHA-256: `b4501b67ab7a481dbecfbe76dbe2fb36916693ab21b88a265305a94882b60778`.

This is **not yet a physical-phone or screenshot-quality sign-off**. The visual weapon framing, landscape appearance, actual touch feel, recorded sound playback and performance still require a real-device test. The battlefield remains a procedural prototype rather than a finished commercial environment. Tactical bots and LAN multiplayer are intentionally deferred.

## Download and installation note

GitHub Actions automatically wraps uploaded artifact files in a ZIP archive. Extract the outer artifact ZIP once; inside it is `TacticalStrike-debug.apk`, which should be installed directly rather than extracted. A partial APK (for example, only 12 MB) is incomplete and must not be installed. The APK is already a ZIP-based Android package, so recompressing it with another ZIP or 7z container provides very little additional size reduction.

## What is still a prototype

The first-person player now uses an imported skinned AK-74M with authored idle/draw/walk/run/shot/reload clips, recorded rifle takes, sand footsteps, magazine/reserve ammunition and FMJ/tracer/AP modes. The battlefield now uses a procedural heightfield with rocky-gravel PBR maps, ridges/gullies, native heightmap collision and rock outcrops. It is still an early prototype: the environment is not a finished authored commercial map, only the AK-74M is enabled with an imported animated asset, exact firearm recordings and material-specific impact audio remain incomplete, and on-device visual/touch/performance sign-off is outstanding. Tactical defense/reposition/withdrawal, cautious attacker advances and coordinated assaults are **not implemented yet**. Local LAN / hotspot multiplayer is also **not implemented yet**.

See [PROJECT_CONTEXT.md](PROJECT_CONTEXT.md) for the cumulative requirements, implementation record, test results and known limitations.
