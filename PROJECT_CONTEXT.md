# Tactical Strike — Project Context

> This file is cumulative. Add confirmed requirements, design decisions, implementation status, test results, and known issues here after each meaningful project step. Do not record unrelated entertainment conversations or other projects here.

## Project identity and workflow

- Repository: https://github.com/CybersecurityRahbar/Tactical-Strike-Android
- Primary platform: Android.
- Engine: Godot 4.7.2 stable (do not switch to a beta engine version without a reason).
- Renderer: Compatibility renderer, initially targeting broad Android compatibility and modest hardware.
- All source, scene files, configuration, documentation, and CI build definitions live in this repository.
- The user does not want to use their own computer for development. Code changes and Android builds should happen in GitHub; GitHub Actions should validate and export APK artifacts. The user's Android phone is used to install and test APK builds.
- Work on the `main` branch, keeping the workflow straightforward and avoiding unnecessary branches.
- A GitHub APK artifact is a test build, not proof that gameplay is correct. Device testing remains necessary.

## High-level game concept

Create an original, small-scope but polished first-person tactical infantry game. It may be inspired by the broad genre of SFG2 / tactical shooters, but it must have its own name, map, interface, assets, code, and identity. The first release focuses on one large, carefully designed map rather than many small maps.

The intended experience is deliberate, long-range and positional rather than dominated by close-quarters run-and-gun combat.

## Battlefield — user's requirements

- One wide battlefield as the only initial map.
- Long sightlines. The opposing defensive position should be far away. Soldiers at long distance should naturally appear small or atmospheric silhouettes, and become more visually detailed as the player approaches.
- The terrain is a semi-level plateau with rolling, broken and irregular ground: alternating low and high ridges, rises, shallow dips and uneven approaches.
- Soil is dry, dusty brown plateau earth. It is **not** a sand-dune desert and **not** lush or green terrain.
- The attacker sector should include open approach ground and forward cover.
- The central approach should contain varied low and high ground that breaks lines of sight and creates tactical decisions.
- The defender sector should be distant and contain two separated houses/buildings, multiple trenches, sandbag positions, barriers, walls and firing positions.
- Defensive and attacker cover should include earth-filled sandbags, brick, stone and solid walls.
- Bullet impacts on the ground and cover should raise dust. Large-calibre impacts should create stronger, wider and more forceful dust effects than rifle-calibre impacts.

## Combat and AI — user's requirements

- Defenders generally hold prepared trenches and firing positions.
- Defenders should move between cover positions when their current position is under heavy fire.
- A defender may withdraw to a safer fallback position when pressure becomes too high.
- Attackers should advance slowly and cautiously, use suppressive fire, and avoid rushing into a strong defensive position.
- Attackers should try to weaken or suppress the defenders before committing to an assault.
- Attackers should advance after the defensive line becomes sufficiently weakened.
- Bot behavior should have readable tactical states such as hold, suppress, reposition, advance and retreat; it should not be random rushing.
- The human player remains free to move and choose their own approach.

## Weapons and game-ballistics — user's requirements

Initial weapon list:
- AK-74.
- M4A1.
- PKM (Russian general-purpose machine gun).
- RPG-7.
- SVD / Dragunov sniper rifle.
- DShK 12.7 mm heavy machine gun.
- Optionally include a .50-calibre heavy machine gun as a second heavy-weapon choice.

Ammunition:
- Each applicable firearm should support a normal ammunition mode and a visible red tracer mode.
- Tracer visibility and apparent size should vary by game-calibre class; do not make every projectile look identical.
- Tracer rounds are a visual gameplay feature; they are not to be described as incendiary unless a separate incendiary mechanic is explicitly implemented.

Projectile behavior:
- Each weapon/calibre needs its own configurable game values for muzzle speed, drag, gravity response, effective range, damage, penetration and firing cadence.
- Projectiles should leave the muzzle quickly; velocity should decrease gradually over distance rather than appearing slow at launch.
- Long shots should show a visible ballistic arc/drop. The player should be able to adjust aim smoothly using normal first-person look controls; later add a dedicated sight/zeroing or elevation adjustment where appropriate.
- Higher-calibre rounds should have greater game impact on cover and terrain than small-arms rounds.
- Include directional shot cracks / bullet whiz when hostile rounds pass near the player.
- Include impact sounds and occasional ricochet / deflection sounds when a round strikes at a shallow angle.
- Values in the first prototype are **gameplay tuning values**, not a claim of exact real-world firing tables. Refine them by playtesting and keep the data configurable.

## Game modes and networking

- Offline single-player mode against AI.
- Local multiplayer over Wi-Fi hotspot / LAN without requiring internet access or a remote game server.
- Start with a small player-count target (2–8 local players) and validate network authority, synchronization and performance before raising it.
- The single-player and LAN modes should share the same combat, map, weapon and match systems where practical.

## Initial implementation status — 2026-11-07

### Added in this commit
- Godot 4.7.2 project scaffold using the Compatibility renderer.
- A procedural 700 m × 700 m brown-earth plateau greybox with rolling ridges, shallow trench cuts and varied elevation.
- Attacker-side and defender-side cover layouts, including sandbag lines, stone/brick barriers and two separated simple defensive buildings.
- Distant low-poly defender silhouettes to establish scale and sightline.
- First-person controller and starter desktop/mobile input bindings.
- Weapon data for AK-74, M4A1, PKM, RPG-7, SVD, DShK and .50-calibre heavy machine gun.
- Configurable projectile simulation with gravity, drag, range, damage and penetration parameters.
- Optional visible red tracer projectile streaks.
- Dust impact particles, destructible cover integrity, shallow-angle ricochet behavior, and procedurally synthesized placeholder shot/impact audio.
- Basic HUD and an Android export preset.
- GitHub Actions workflow for headless validation and Android APK artifact export.
- A headless scene smoke test.

### Explicitly not yet implemented / not yet verified
- Distant defender silhouettes are placeholders, not the final tactical AI.
- The full attacker/defender behavior tree described above is a next milestone.
- AI suppression sensing, cover selection, fallback movement and coordinated assault are not yet complete.
- Local Wi-Fi / hotspot multiplayer is not yet implemented.
- The project currently uses procedural greybox geometry; final high-quality weapon, soldier, building, sandbag and environment assets remain to be created or sourced with clear licences.
- Audio is synthesized placeholder audio, not recorded firearm or battlefield sound design.
- Android APK export and on-device performance are not confirmed until the GitHub Actions run completes and the APK is tested on an Android phone.

## Quality rules

1. Do not call a placeholder a finished feature.
2. Do not add many small maps; polish the initial wide map first.
3. Keep the distant sightline and terrain readability central to level design.
4. Keep code modular and configuration-driven.
5. Test GDScript parsing and scene startup in headless CI before exporting APK.
6. After each major step, record files changed, tests run, results, and unresolved issues in this context file.
7. Never claim a successful APK build without a successful Actions run and artifact verification.


## CI follow-up — 2026-11-07

- First Android workflow attempt stopped before project validation because the legacy Android SDK package name `tools` is no longer available from the current SDK package repository.
- The workflow was changed to use the preinstalled GitHub-hosted Android SDK and install only current packages (`platform-tools`, Android platform 36 and build-tools 36.0.0). These installation steps succeeded in the next run.
- The second attempt downloaded Godot and the Android export templates successfully, but the Godot executable was mistakenly appended to `GITHUB_PATH` as though it were a directory. The subsequent import step therefore could not find the `godot` command. The workflow is being corrected to persist the exact executable path in `GODOT_BIN` and invoke that executable explicitly.
- Android export-template archive is approximately 1.2 GB. The workflow now extracts only the three Android template files instead of unpacking templates for every platform.
- Android orientation changed to landscape to suit the first-person shooter layout.
- Mobile one-shot HUD controls now send press/release on separate button-down/button-up events so reload and weapon-cycle actions can register as discrete input transitions.
- **Validation status:** no successful Godot import, scene smoke test or Android APK export has been confirmed yet. The next CI run must pass all three before an APK can be offered as a tested build.


## Workflow parser correction — 2026-11-07

- GitHub's server-side workflow validation rejected the revised workflow before creating a runner job. The cause was a malformed YAML scalar in the one-line smoke-test step: its shell command contained an embedded quoted executable path. Converted that step to a YAML block scalar. Earlier zero-job runs were workflow-file validation failures, not Godot compile failures.
- Next expected check: workflow should now register its job; inspect Godot project import, scene smoke test, and Android export separately.


## GDScript smoke-test repair — 2026-11-07

- The first registered Godot run successfully downloaded Godot 4.7.2 and Android export templates and completed the project import step.
- The headless scene smoke test then exposed strict GDScript typing errors in `main_world.gd`: generic `abs` / `lerp` inference and ternary numeric expressions inferred as Variant.
- Corrected these expressions with explicit `float` annotations and float-specific `absf` / `lerpf` calls. The next run must confirm scene instantiation and then the APK export; no APK is confirmed from this failed smoke-test run.


## Android export and projectile script fixes — 2026-11-07

- The next CI attempt confirmed that the Godot import step completed and the map/player/HUD scene smoke test reached runtime, but the log still exposed a dependency parse error in `scripts/weapons/projectile.gd`: inferred Variant type for `distance_factor`. The smoke test was too permissive because it checked scene children without explicitly asserting that every required script was instantiable.
- Android export was blocked by the missing ETC2/ASTC texture-compression import setting required by Godot's Android exporter.
- Corrected the projectile explosion-distance calculations with explicit float types and clamped distance factor.
- Enabled `textures/vram_compression/import_etc2_astc=true` in the project rendering settings for Android compatibility.
- Strengthened `tests/smoke_test.gd` to load every core project script and fail when any script cannot instantiate, before creating the battlefield scene.
- **Validation status at this point:** Android SDK setup, Godot download, export-template installation, and headless project import succeeded. The previous run's scene smoke test returned exit code 0 but its log showed the dependency parse error; therefore it is not treated as a clean validation. Android export failed on ETC2/ASTC configuration. The next CI run must show a clean script smoke test and a successful APK export before a build can be shared.


## First successful cloud build — 2026-11-07

- GitHub Actions run [42402765482](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42402765482) completed with **success**.
- Godot 4.7.2 headless project import succeeded.
- The strengthened script smoke test reported: `SMOKE TEST PASSED: all required scripts compile; main scene, terrain, player and HUD initialized.`
- Android debug APK export and artifact upload both succeeded.
- Uploaded artifact: `TacticalStrike-Android-debug` (artifact ID `13585962872`), available for 14 days from the build date.
- The downloaded APK archive was inspected: it contained one APK; ZIP integrity passed; the APK contained `AndroidManifest.xml` and the ARM64 Godot native library. APK size was approximately 27 MB.
- The APK was **not installed or tested on a physical Android device**. CI success proves script/scene smoke validation and APK export, not touch-control quality, frame rate, actual device compatibility, tactical AI or LAN behavior.
- The export log also showed a non-blocking missing project-icon warning. A project icon is added in the next visual refinement pass.

## Visual refinement and repeatable cloud builds — 2026-11-07

- Added an original vector application icon and configured it in `project.godot`.
- Added a GitHub-renderable top-down map blueprint in `docs/battlefield-layout.svg` and explanatory notes in `docs/BATTLEFIELD_DESIGN.md`.
- Added subtle depth haze to support the requested long sightlines and distant-silhouette effect; day/night toggling changes the haze tint as well as the lighting.
- Added a GitHub Actions cache for the Godot Android export templates so subsequent builds can avoid re-downloading and extracting the large template archive.
- **Validation status:** these visual and workflow refinements are committed, but require their own clean CI run. The already successful earlier APK remains the only build confirmed at this point.


## Depth-haze integration syntax repair — 2026-11-07

- The follow-up build (run 42403142402) confirmed the new SVG app icon was imported and the Android SDK/Godot/template setup steps succeeded.
- The strengthened script smoke test caught a GDScript indentation error introduced while adding depth haze and day/night haze tinting in `scripts/world/main_world.gd`. It correctly failed the job before Android APK export; no artifact was produced by that run.
- Corrected the extra indentation on the environment tonemapping statement and moved the day/night environment block out from under the sun-null check. This keeps haze tint switching independent of the sun node's state.
- The next CI run must confirm all scripts compile, the main scene initializes, and Android export succeeds before the refined APK is shared.


## User-reported Android artifact extraction failure — 2026-11-07

- User reported that the successful fog/refinement build downloaded from GitHub Actions was approximately 27.20 MB, but Android archive extraction stopped with an error and left only a partial APK of around 12 MB.
- Inspected the exact successful Actions artifact from run [42403356849](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42403356849), artifact ID `13586278101`, uploaded from commit `8e353a2b5ef01aaded93af81535834b693e53d92`.
- Downloaded the artifact through the connected GitHub integration and verified its complete ZIP CRC table with `unzip -t`: **PASS**, no corrupt entries. The archive contains exactly one `TacticalStrike-debug.apk` entry, 28,523,024 bytes.
- Extracted that exact entry separately. The APK was identified as an Android package, contains 125 ZIP entries including `AndroidManifest.xml`, `classes.dex`, and `lib/arm64-v8a/libgodot_android.so`; a full APK CRC check also passed with no errors.
- Conclusion: the stored GitHub artifact and the APK inside it are not corrupt. The 12 MB output reported by the user is consistent with an interrupted/partial extraction or a damaged/incomplete local download, but the precise device-side cause cannot be identified without the actual local archive/error screenshot. No evidence indicates that the Godot build itself produced a 12 MB APK.
- Compression analysis: the outer GitHub ZIP is 28,309,482 bytes versus the inner APK at 28,523,024 bytes, a saving of only 213,542 bytes (0.75%). This is expected because APK is itself a ZIP container whose contents are already compressed. A local XZ level-9 test reduced the APK by only about 1.4%, so switching to 7z is not expected to materially speed up download.
- For this conversation, extracted the verified APK from the exact current successful artifact as a standalone file so it can be installed directly without first extracting the GitHub artifact ZIP.
- Hardened the GitHub Actions workflow: after export it must test APK ZIP integrity, verify the Android signature with `apksigner`, confirm the manifest/classes/ARM64 Godot library, and generate a SHA-256 checksum before the artifact is uploaded. The artifact now includes the APK and a small validation/checksum report, with maximum ZIP compression. This workflow change requires a fresh CI run; it is not treated as passed until that run succeeds.


## Verified rebuild after extraction report — 2026-11-07

- Added a mandatory post-export validation step to `.github/workflows/android.yml`.
- New successful run: [42405392723](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42405392723), commit `79333aca0d7247c9350fbab8306319bbf092049b`.
- All required pipeline stages succeeded: Godot project import, full core-script smoke test, Android APK export, APK ZIP/CRC test, `apksigner verify`, Android manifest/classes checks, ARM64 native-library check and artifact upload.
- New artifact: `TacticalStrike-Android-verified`, artifact ID `13587750085`, GitHub Actions run 42405392723.
- Artifact ZIP size: 28,309,886 bytes. Inner APK size: 28,523,024 bytes.
- The exact uploaded artifact was downloaded again and independently checked in the working environment: outer ZIP passed `unzip -t`; the extracted APK passed a complete ZIP/CRC test and was identified as an Android package.
- APK SHA-256: `1d3efd4c492828791bf1d9f355e1d49c60154032af5f212d50c168110106e51f`.
- GitHub Actions produced a matching `TacticalStrike-debug.apk.sha256` sidecar and `apk-validation.txt` report. All checks in the report are PASS.
- A standalone copy of this verified APK was generated for the user in the conversation so installation does not require extracting the GitHub Actions outer ZIP.
- Artifact ZIP compression is set to level 9. The APK itself is a ZIP-based Android package with already-compressed contents, so the outer archive is expected to be only about 0.75% smaller than the APK. Stronger 7z/LZMA compression is not expected to materially improve download size; a local high-compression XZ test improved size by only about 1.4%.
- The prior partial 12 MB file is not the build output. Do not install partial files. The standalone APK and checksum above refer to the independently verified complete APK.
- **Remaining limitation:** CI validates file integrity, signature and target ABI, but no physical Android device installation or in-game performance test has yet been carried out.
