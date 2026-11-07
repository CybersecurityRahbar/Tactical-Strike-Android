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

The verified Android debug APK was rebuilt on 7 November 2026:
- [Open the successful and verified GitHub Actions run](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42405548021).
- Godot project import and the full core-script smoke test passed.
- The APK exported successfully.
- APK internal ZIP integrity and signing checks passed.
- Android manifest, DEX bytecode and ARM64 Godot native library checks passed.
- The Actions artifact includes the APK, a SHA-256 checksum file and an APK validation report.
- APK SHA-256: `566cc17eab9fdf6ca04d60b8c85fbcf45ce31636a1fb6cdd0ebb27394ac26692`.

The APK has **not yet been installed or tested on a physical Android phone**. CI success confirms build/export and file-integrity checks, not on-device gameplay quality.

## Download and installation note

GitHub Actions automatically wraps uploaded artifact files in a ZIP archive. Extract the outer artifact ZIP once; inside it is `TacticalStrike-debug.apk`, which should be installed directly rather than extracted. A partial APK (for example, only 12 MB) is incomplete and must not be installed. The APK is already a ZIP-based Android package, so recompressing it with another ZIP or 7z container provides very little additional size reduction.

## What is still a prototype

The current battlefield is procedural greybox geometry. Distant soldiers are static visual placeholders, not the final AI. Tactical defense/reposition/withdrawal, cautious attacker advances and coordinated assaults are **not implemented yet**. Local LAN / hotspot multiplayer is also **not implemented yet**. Final soldier and weapon models, animations, recorded weapon sounds, material-specific impacts, polished touch controls and device performance validation remain future milestones.

See [PROJECT_CONTEXT.md](PROJECT_CONTEXT.md) for the cumulative requirements, implementation record, test results and known limitations.
