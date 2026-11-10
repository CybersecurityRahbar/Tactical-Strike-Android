# Tactical Strike — Project Context

> This file is cumulative. Add confirmed requirements, design decisions, implementation status, test results, and known issues here after each meaningful project step. Do not record unrelated entertainment conversations or other projects here.

## Current authoritative project state — 2026-11-10

**This section overrides contradictory engine, renderer and map descriptions in the older dated history below.**

- The active project is now **Unity 6000.3.28f1 (Unity 6.3 LTS)** with **URP 17.3.0**, in `UnityProject/`. It is not currently a Godot 4.7.2 project.
- The Godot 4.7.2 entries dated 2026-11-07 below document an earlier prototype/history retained for continuity; they are not instructions for the active build and must not be used to describe the current engine.
- The active gameplay map is **Northwind Valley / Sector 01**. Its intended environment is a temperate rural highland with muted olive vegetation, earth, stone and exposed rock. This supersedes the older brown-plateau landscape wording where the two conflict.
- Active automation: Unity Android APK build and Unity WebGL browser preview/deployment through GitHub Actions. Main branch only.
- Do not treat an old Godot workflow result, a successful Unity compile, or an SVG plan as proof of current playable-game acceptance. Verify the current Unity build, WebGL deployment and physical-device behaviour separately.
- Cumulative record rule: retain dated history, but add corrections rather than letting an outdated entry override the current authoritative state.

## Project identity and workflow

- Repository: https://github.com/CybersecurityRahbar/Tactical-Strike-Android
- Primary platform: Android.
- Engine: Unity 6000.3.28f1 (Unity 6.3 LTS).
- Renderer: Universal Render Pipeline (URP) 17.3.0.
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

## Archived initial implementation status — 2026-11-07 (Godot prototype; historical only)

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


## Final re-verification after the user's extraction issue — 2026-11-07

- Rebuilt the same main-branch game code after the README/context update using GitHub Actions run [42405548021](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42405548021), head commit `eb066f6de38bdf9d44158ba5d9bb4913597935d8`.
- Run completed **successfully**. Godot import, script smoke test, APK export, APK integrity/signature/ARM64 validation and upload all passed.
- Verified artifact: `TacticalStrike-Android-verified`, artifact ID `13586967951`, size 28,309,889 bytes.
- Independently downloaded the exact artifact again. The outer ZIP passed `unzip -t`; the extracted APK passed its complete ZIP/CRC test and was identified as an Android APK.
- Final standalone APK SHA-256: `566cc17eab9fdf6ca04d60b8c85fbcf45ce31636a1fb6cdd0ebb27394ac26692`.
- APK size: 28,523,024 bytes. CI validation report confirms archive integrity, signature, manifest, DEX and ARM64 native library: all PASS.
- Added `paths-ignore` for README, PROJECT_CONTEXT and documentation-only pushes so future context bookkeeping does not needlessly rebuild the unchanged Android APK. Manual workflow dispatch and actual code pushes continue to build.


## Current final verified build artifact — 2026-11-07

- Latest successful run after the workflow trigger cleanup: [42405706021](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42405706021), commit `61de784136b37a83ed4e8142bda269657aa50d5b`.
- Result: **success** across Godot import, core-script smoke test, APK export, ZIP integrity, APK signature, Android manifest/classes/ARM64 checks, and artifact upload.
- Artifact ID: `13587581782`; artifact name: `TacticalStrike-Android-verified`; size: 28,309,884 bytes.
- Exact artifact downloaded and independently rechecked: outer ZIP passes CRC/integrity test; extracted APK passes its own full ZIP/CRC test and is identified as an Android package.
- Final APK size: 28,523,024 bytes.
- Final APK SHA-256: `118252b53a0ea561c7c9c908da1d2cc9ac6d985a1928893caaa4a916029dcccc`.
- The standalone copy supplied in the conversation and its `.sha256` sidecar were extracted from this exact final artifact. The small validation report confirms archive integrity, APK signature, AndroidManifest.xml, classes.dex and ARM64 native library all PASS.
- Later documentation-only commits do not trigger another build because the workflow now ignores README, PROJECT_CONTEXT and `docs/**`. Code changes and manual workflow dispatches still trigger a build.


## User playtest feedback and first-person usability repair — 2026-11-07

### User-reported result
- User installed the Android APK and reported that the first screen looked like a largely empty beige plane with a very thin dark horizon, black side bars, clipped status text, and almost no visible controls.
- User could rotate the view but could not move, fire, reload, switch weapons, or access settings. The result did not feel like a playable game.
- This feedback is accepted as a real usability failure; a successful APK export and a headless scene-start test did not prove that the Android touch experience was usable.

### Source review findings
- The Android viewport was fixed at 1280×720 but did not specify an expanding aspect policy. On a wider phone display this can preserve a 16:9 game area and leave unused side regions.
- The battlefield used one flat earth material, a 100×100 terrain grid, only 160 small rock instances, and relatively strong early distance haze. At the player's long-range spawn point this produced very weak visible surface detail and hid the defensive sector too much.
- Weapon definitions and projectile code existed, but there was no first-person weapon mesh attached to the camera. Therefore a weapon name/ammunition count in the HUD did not look or feel like holding a weapon.
- The mobile HUD was only a few fixed-position buttons and had no pause, armory, or settings menus. The previous smoke test checked script/scene initialization, not the presence of a usable first-person weapon model or game UI.
- AI combatants, final quality assets and local multiplayer remain separate incomplete milestones; they must not be described as already complete.

### Repair committed for validation
- Changed the mobile stretch aspect policy to `expand` so the game viewport can fill wider landscape displays without fixed 16:9 side pillars.
- Increased terrain tessellation and height variation; added per-vertex dry-earth color variation, a visible dusty approach track, more ground stones and lighter, more distant haze.
- Added a low-poly first-person weapon-and-arms view model, weapon switching model refresh, recoil movement and a muzzle flash aligned with the visible weapon.
- Rebuilt the HUD with a pause menu, settings panel, weapon armory, night/day toggle, sensitivity controls, and explicit mobile movement/fire/reload/sprint/crouch/jump controls. Screen dragging is restricted to the open look region so movement/action controls do not intentionally rotate the camera.
- Wired HUD selection/settings signals to the player, weapon controller and day/night system.
- Expanded the headless smoke test to require the first-person weapon model and pause/armory/settings controls in addition to the existing world/player/HUD checks.
- **Validation boundary:** these changes are pending the next GitHub Actions run. Even after CI passes, an on-device Android playtest is still required to verify that touch buttons render in the expected positions and register correctly. These changes do not complete tactical AI, final 3D art or LAN multiplayer.


## CI-discovered duplicate terrain function correction — 2026-11-07

- The first CI run for the mobile usability repair, run [42407576305](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42407576305), correctly stopped in the strengthened headless smoke test before APK export.
- Exact error: duplicate declaration of `_build_terrain()` in `scripts/world/main_world.gd`. The code-replacement range had inserted the new vertex-coloured terrain function but accidentally retained the old terrain function farther down in the file.
- Removed the obsolete second implementation while retaining the new higher-detail terrain and approach-track mesh. A source-level check confirms exactly one `_build_terrain()` declaration remains.
- The follow-up commit also retains the HUD signal connection and approach-mesh winding corrections.
- **Build status:** the fix is committed; it is not considered validated until the next complete CI run passes project import, script/scene smoke test, Android export, APK signature/integrity checks and artifact upload.


## Successful CI validation of the usability repair — 2026-11-07

- Corrected the duplicate `_build_terrain()` declaration found by the first repair CI run. The corrected file contains exactly one terrain-builder declaration.
- The new main-branch commit is `41eba7c0e3e0337d1d415a4e8c767b249d297562`.
- GitHub Actions run [42407702148](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42407702148) completed with **success**.
- Passed stages: Godot project import, strengthened headless scene smoke test (including camera weapon view-model and pause/armory/settings UI presence), Android debug APK export, ZIP archive validation, Android APK signature verification, manifest/classes checks, ARM64 Godot native-library check, and artifact upload.
- Verified artifact ID: `13588008097`, artifact name `TacticalStrike-Android-verified`.
- Downloaded the exact artifact and independently checked the outer ZIP and inner APK; both passed full ZIP/CRC tests with no errors.
- APK size: `28,535,312` bytes. SHA-256: `32c0358640d1c6112d3e308ca4a071364829bc04f278fc6a01e296da7f042c75`.
- The artifact validation report records PASS for archive integrity, APK signature, `AndroidManifest.xml`, `classes.dex`, and the ARM64 Godot native library.
- **Important test boundary:** this is a successful source/scene/Android-export validation, not a physical-device playtest. The newly built APK has not yet been installed and interacted with on the user's phone. Touch-button placement, real movement/fire response, visual appearance on the phone, and performance still need the user's next playtest. The terrain and weapon shown are still procedural/low-poly prototype geometry, not final AAA-quality assets. Tactical AI and LAN multiplayer are not implemented.

## Second user device playtest: acceptance failure and reference-led rework — 2026-11-07

### User's actual phone feedback
- The user installed the previously validated usability-repair APK and reported that the terrain looked essentially unchanged, the weapons still looked like crude primitive shapes, the muzzle glow was poor, firearm audio remained synthetic, and walking still did not work.
- The user explicitly rejected that build as failing the intended game design. This supersedes any implication that the earlier CI/UI changes provided a playable or visually acceptable game.
- The issue is now treated as a product-level failure, not a request to make small cosmetic adjustments. A successful APK export did not establish game quality; the previous source/scene checks were insufficient.

### Reference research completed
- Reviewed Special Forces Group 2 mobile control descriptions and gameplay screenshots. Its practical mobile pattern is movement input on the left, look/aim on the right, and separate right-side combat/action controls. Its control layout and sensitivity are customizable. This project uses those interaction principles as a reference only; no proprietary game code, models, textures, sounds or branding are copied. Reference: https://special-forces-group-2.en.uptodown.com/android
- Reviewed Insurgency: Sandstorm and Squad / Kohat terrain screenshots as environmental art references. The target direction is a dry, rocky, rolling plateau with irregular ridgelines, gullies, exposed rock, sparse dry vegetation, tracks and defensible compounds—not a beige plane and not a dune desert. Reference imagery includes the Insurgency Sandstorm Outskirts preview and Squad Kohat outpost: https://steamcommunity.com/games/581320/announcements/detail/1616141061517089867 and https://joinsquad.com/
- Selected Poly Haven's Rocky Gravel CC0 PBR material for the ground: diffuse/albedo, OpenGL normal and roughness maps. It depicts reddish-brown gravel with dense small rocks and dirt. Source: https://polyhaven.com/a/rocky_gravel
- Selected two authored CC0 firearm meshes for the first model-integration milestone: loafbrr_1's AK (Godot-ready archive, 1K textures, approximately 2,670 triangles) and nisu / 3DModelsCC0's M4A1 model. These are real authored weapon meshes rather than hand-assembled box primitives. The AK mesh is a family stand-in, not an exact factory AK-74 mesh. Sources: https://opengameart.org/content/ak and https://opengameart.org/content/m4a1-assault-rifle
- Selected the CC0 Free Firearm Sound Library's recorded AK-47 and AR-15 takes as the closest available recorded audio for the first two weapons, plus CC0 Fantozzi sand footsteps. These are actual recorded firearm samples, but they are not exact AK-74 and M4A1 recordings. The impact/ricochet layer remains a separate synthetic placeholder. Sources: https://opengameart.org/content/the-free-firearm-sound-library and https://opengameart.org/content/fantozzis-footsteps-grasssand-stone
- Licensing was checked on the sources above. The build script pins the downloaded sample recordings to a specific public asset-kit commit and converts them to OGG for the game package. The phone does not need network access while playing.

### Code and build changes prepared on main
- Replaced the old four-button movement D-pad with a true analog virtual joystick on the left. The right half of the screen is reserved for free-look touch-drag; the right-side action cluster has fire, aim, sprint, jump, crouch, reload and next-weapon buttons.
- Reworked the player controller around smooth ground acceleration, limited air acceleration, slope/floor snapping, crouch height, running cadence, small camera bob, touch look, and an aim-down-sights FOV transition. Added actual sand footstep playback from the CC0 footstep samples.
- Replaced the previous primitive cube-based gun assembly with imported external weapon mesh scenes/meshes. The import helper normalizes the mesh dimensions and uses the model manifest to locate the authored AK and M4 assets. The first release's active armory is deliberately restricted to these two imported models rather than displaying fake models for PKM/RPG/SVD/DShK/M2, whose correct models have not yet been integrated.
- Player firing now calls recorded firearm samples from the weapon_audio module, rather than the old generated firing tone. The muzzle effect has been reduced to a brief local light instead of the oversized glowing primitive. Tracer thickness and emissive intensity have also been reduced.
- Increased terrain height variation and ridge/gully amplitudes, increased terrain mesh resolution, added UVs, and applied the Poly Haven Rocky Gravel diffuse/normal/roughness maps. Increased rock-outcrop geometry/count and moved the spawn slightly forward onto the terrain. Reduced atmospheric haze so the middle/far terrain remains visible.
- Added tools/prepare_external_assets.sh to download the CC0 models, PBR maps, and recorded audio during CI, unpack/import them into the project and generate the runtime weapon manifest. Updated GitHub Actions to run this step before Godot import/export.
- Expanded the smoke test to require imported weapon geometry, analog joystick/aim controls and a scripted player movement response; it no longer treats simply creating a HUD as proof that movement is implemented.
- Added/updated docs/EXTERNAL_ASSET_SOURCES.md with source, licence, provenance and asset-specific limitations.

### Explicit limits that remain
- The newly imported AK and M4 meshes are static view models in this pass; production reload, firing, sprint and inspect animation clips, rigged first-person hands and animation blending still need to be implemented and playtested.
- Only AK-74-family and M4A1-family meshes/audio proxies are enabled in this asset milestone. The rest of the planned armory is not yet represented by authentic imported models or exact recorded samples.
- AK-47/AR-15 recordings are close family proxies and must not be described as exact AK-74/M4A1 original recordings. There is no verified exact PKM, RPG-7, SVD, DShK or M2 audio sample in this pass.
- Impacts, ricochets and distant combat atmosphere still need recorded/commissioned sound design. This pass replaces the player's main rifle reports and movement footsteps, not every battlefield sound.
- The external model orientation, scale, placement in the camera and tactile HUD positions still require the user's physical-phone playtest. CI can validate importing a mesh and scripted movement logic; it cannot certify that the gun looks correctly held or that movement feels as smooth as Special Forces Group 2 on the user's device.
- Tactical AI, coordinated attacker/defender behavior and LAN/Wi-Fi multiplayer remain unimplemented.

### Validation state
- All new asset downloads, Godot imports, strengthened movement/model smoke tests and Android exports must pass in the next CI run before an APK can be offered.
- No claim of physical-device validation is made until the user installs and tests this exact new artifact.


## CI correction after external asset integration attempt — 2026-11-07

- The external-asset preparation step succeeded in GitHub Actions and downloaded/unpacked the AK glTF model, M4A1 FBX model, firearm recordings, sand footsteps and all three Rocky Gravel PBR maps.
- The first Godot headless import also completed. The log reported a non-fatal attempt to process the included M4A1 Blender source project without Blender available; the Godot-ready FBX was present. The preparation script now deletes the source .blend files from the staged runtime directory after unpacking, keeping only engine-importable assets.
- The strengthened smoke test did not run to completion because Godot 4.7.2 rejected inferred Boolean types for two touch-look-region variables in player_controller.gd. Both variables are now declared explicitly as bool.
- The run stopped before Android export, so no APK was produced by this attempt. The type fix and Blender-source cleanup are committed for the next full import/smoke/export run.


## External asset integration and analog-movement CI validation passed — 2026-11-07

- Final corrected commit: `6a316880f7776fe4195c08b99f2dfbe6d12f8ab7`.
- GitHub Actions run [42411928830](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42411928830) completed with **success**.
- The external asset download/preparation step passed: both AK and M4A1 source archives, firearm field recordings, Fantozzi sand footfalls and the three Poly Haven Rocky Gravel PBR maps were downloaded and staged. Blender-only .blend source files are excluded from the headless Godot import.
- Godot project import passed. The strengthened smoke test passed, including loading a real imported weapon mesh, creating the analog movement joystick and aim controls, finding the pause/armory/settings UI, and confirming that the player position changes when a movement-vector input is applied.
- Android debug APK export passed. APK archive CRC validation, Android signature verification, AndroidManifest.xml, classes.dex and ARM64 native-library checks all passed.
- Verified GitHub Actions artifact: `TacticalStrike-Android-verified`, artifact ID `13589489249`.
- The exact uploaded artifact was downloaded locally. The outer ZIP passed `unzip -t`; the extracted APK also passed a complete ZIP/CRC test and was identified as an Android package.
- APK size: `47,041,431` bytes. SHA-256: `e9d37e3f2eef27569e59a99a5b1ab66bba4dff2d2cdff8de5d5c6ad6d9309cb3`.
- A standalone copy of the APK, checksum sidecar and validation report were prepared for the user to avoid extracting the outer GitHub artifact ZIP.
- **Important boundary:** CI proves asset download, engine import, scripted movement logic, visible imported mesh instantiation, Android packaging and file/signature integrity. It does not prove that the user sees the rifle at the correct scale/orientation, that the touch joystick feels right on the phone, or that the overall visual quality meets the target. This exact APK has not yet been tested on the user's physical phone.
- **Known content limits:** only AK and M4 have imported firearm meshes in this pass; the AK asset is a close AK-family visual model, not an exact AK-74 factory mesh. The firearm recordings are real AK-47 and AR-15 takes, not exact AK-74/M4A1 takes. First-person arm animations, reload/walk/sprint weapon animation, tactical AI and local multiplayer remain unfinished.


## Screenshot-driven technical corrections and stronger acceptance checks — 2026-11-08

- The user's screenshot shows the previous interface layout mirrored: the HUD's right-anchored top buttons appeared at the left while the status panel appeared at the right. The HUD root is forced to Godot's LTR layout direction to keep the FPS movement and combat controls on fixed left/right sides even when the phone locale is Arabic/RTL.
- The earlier terrain pass incorrectly increased the count and size of dark rock objects. This created a dense, nearly continuous dark silhouette in the distance. Reduced outcrops to 520 small, lighter, low-profile rocks and disabled their cast shadows. The ground PBR maps now provide the main gravel texture detail.
- Replaced the contour-like ridged noise with four broad, broken, irregular ridge chains and rolling shoulders, plus shallow gully cuts. The indexed 256-division terrain mesh now generates normals and tangents; tangent generation is required for the terrain normal map to render correctly.
- The user screenshot showed fire and aim controls too close/overlapping. The new touch layout separates them, keeps the floating movement stick left, puts fire/aim/action controls on the right, and uses a specific touch-index capture for left-thumb joystick movement.
- The actual first-person AK-74M asset includes hands, a skinned rig and named authored idle/draw/walk/run/shot/reload animations. The weapon script checks these clips and connects locomotion to movement state and shot/reload to the authored clips. The active weapon list contains only AK-74M because that is the sole model fully integrated in this milestone.
- The smoke test now validates exact imported animation clip names, left/right control placement, non-overlapping fire/aim hit rectangles, six available recorded firearm takes, a real firing action consuming a magazine round, ammo-mode cycling and player displacement. It also checks that player height does not fall through the terrain collision surface.
- The prior procedural noise audio calls were removed from bullet impact, ricochet and near-miss paths. The user's shot plays a randomized one of six recorded field takes, not a generated legacy tone layered under it. The takes are AK-47 recordings used as the closest available proxy for the AK-74M, not falsely labelled exact AK-74M recordings.
- These refinements are not considered complete until the next Godot import, acceptance smoke test, APK export, archive/signature checks and user phone playtest succeed. No current device-side result is claimed. Bots and multiplayer remain deferred.


## Godot strict-typing correction — 2026-11-08

- The first screenshot-driven rebuild run [42417049347](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42417049347) successfully prepared the licensed AK-74M asset, all six firearm sample files, footsteps and PBR terrain maps. Godot imported the authored glTF scene successfully.
- The expanded headless test then caught strict GDScript inference failures on one mobile-touch Vector2 temporary and three UI Rect2 temporaries, with additional explicit typing added for terrain arrays/tangents and test string/audio arrays.
- Corrected the HUD anchor to an explicit Vector2 and the smoke-test screen/control rectangles to explicit Vector2/Rect2 types. No APK was exported from the failed run. The next CI run must pass the new terrain tangents/material/relief checks, animation clip checks, LTR layout/non-overlap checks, player displacement/floor stability checks, recorded audio resource checks and real fire/ammo state checks before export.


## Terrain winding and collision-root-cause correction — 2026-11-08

- The stricter movement/floor-stability test exposed that the player was falling through the generated terrain even though its scene and collision node existed. This is the actual root cause of the "I can look but cannot walk" failure in the latest candidate.
- Godot uses clockwise triangle winding for front faces. The procedural terrain vertices were previously emitted in the opposite order. Reversed both triangles per terrain cell so the heightfield faces upward as a front-facing surface for Godot's one-sided triangle collision.
- Because reversing the triangle order also reverses the generated geometric normals, the mesh now calls SurfaceTool.generate_normals(true) to keep its lighting normals pointing upward, then generates tangents for the PBR normal map. The terrain material uses normal back-face culling again rather than showing its underside.
- Strengthened the smoke test to inspect the terrain's generated normal array and fail if the surface does not point upward, in addition to verifying height relief, diffuse/normal texture bindings, player movement displacement and vertical floor stability.
- This fix was triggered by a failing test, not by guesswork; the next run must demonstrate the player stays above the terrain collision surface before Android export.


## Terrain winding follow-up — collision accepts both face orientations — 2026-11-08

- In run [42417359337](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42417359337), the project and AK model imported successfully, but the smoke test stopped on its first-vertex normal direction check. It had not reached the player movement/floor stability assertions.
- The mesh now verifies its full normal field by mean upward component and percentage of upward-facing normals rather than rejecting a potentially steep first corner vertex.
- The terrain collision shape is explicitly created as ConcavePolygonShape3D with backface collision enabled. This ensures contact from either side while the triangle winding and normal-map lighting are being validated, so a local winding transition in a gully cannot drop the player's capsule through the surface.
- Terrain material is temporarily rendered two-sided for the same visual diagnosis. The next smoke test explicitly checks that the collision shape has backface collision enabled, then proceeds to player movement and vertical stability.
- This is not declared fixed until that subsequent player movement test passes, followed by APK export and signature/file validation.


## Terrain normal direction measurement — 2026-11-08

- The next CI run [42417621963](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42417621963) confirmed that collision backface support was configured, but the full normal-array test measured mean normal Y = -0.905 with 0 of 66,049 normals pointing upward.
- This gave a decisive measurement: the extra `flip=true` argument was inverting the terrain's generated normals. Removed that flip; the mesh now keeps the current triangle order, generates standard normals, then generates tangents. Two-sided terrain rendering and two-sided concave collision remain enabled while the player floor test validates the collision.
- No APK was exported in this failed run. The next run is expected to re-evaluate normal direction first, then continue to player floor stability and the remaining weapon/UI/ammo checks.


## Replace expensive concave collision with a native terrain heightmap — 2026-11-08

- CI confirmed the terrain's full vertex-normal field was inverted by `generate_normals(true)`; the flip flag was removed and the standard generated normals are used.
- The subsequent headless job remained slow while evaluating the full 256×256 concave trimesh during the movement/floor stability test. The terrain is a heightfield with no caves or overhangs, so a HeightMapShape3D is the correct physics representation and much faster than a general concave triangle soup.
- Replaced the collision mesh with a HeightMapShape3D using 257×257 samples from the exact same _terrain_height(x,z) function and the same visual-grid spacing. The heightmap uses a uniform scale on all axes, with stored heights divided by that spacing to preserve the exact world height.
- The new smoke test explicitly checks HeightMapShape3D size/data and then exercises the actual player's movement and floor-stability assertions. This should test the same hilly surface while avoiding a heavyweight concave physics shape.
- The prior run [42417767291](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42417767291) failed before reaching player movement because the generated terrain normals were inverted (mean Y -0.905). No APK was exported. The new heightmap build remains pending full CI validation.


## Player movement and terrain smoke test passed; Android export guard corrected — 2026-11-08

- The first build using HeightMapShape3D collision, run [42418050146](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42418050146), passed the complete headless scene smoke test. This is the first run to pass the full set of new checks after the screenshot:
  - 257×257 native heightmap collision is present and correctly sized.
  - The terrain has upward-facing generated normals, tangents and bound Rocky Gravel diffuse/normal maps.
  - Battlefield topography relief and the less-dense ground stones pass their checks.
  - The actual imported AK-74M model/rig/animation clips are present.
  - The mobile HUD root uses LTR; the movement stick is on the left; fire is on the right; fire/aim hit targets do not overlap.
  - The scripted joystick input moves the player and the player does not fall through/below the terrain.
  - All six recorded firearm takes are available; firing consumes one magazine round; tracer ammo mode cycles correctly.
- Android export then created and signed the APK successfully, but the workflow step returned failure because its final asset-existence assertions still checked obsolete files from the previous AK/AR asset pass (`ak47_27.ogg`, `ar15_24.ogg`) instead of the new six AK sample names and the new AK-74M model paths. This was a stale CI guard, not a Godot project import or GDScript failure.
- Updated the export step's final required-file checks to require the actual pinned AK-74M glTF/bin/base-colour textures, current AK recording sample paths, sand footsteps and terrain PBR diffuse map. The next run must pass Android export, APK integrity, signature and ARM64 checks before an APK can be provided.
- **Validation boundary:** this headless pass proves the player movement logic and collision interaction in Godot CI, but not the touch feel or visual composition on the user's actual phone. The new APK has not yet been produced by a fully green CI run.


## Strengthen mobile touch-to-game acceptance checks — 2026-11-08

- After the latest green smoke test, expanded it again to exercise the HUD's real touch-handling route instead of setting the player's mobile movement vector directly. The test injects a left-side screen-touch press and upward drag into HUD._input, verifies the emitted vector reaches the player controller, holds it over physics frames, releases the same touch index and verifies the player actually changes world position without falling through.
- The test now emits the FIRE button's button_down/button_up signals and verifies a magazine round is consumed; emits AIM and verifies ADS toggles; emits AMMO and verifies the red tracer ammunition mode; and emits RELOAD and verifies that reloading starts with the authored full-reload animation clip.
- This closes a gap between the earlier logic-only movement test and the actual mobile HUD event wiring. It is not yet a phone hardware test, but it gives CI a direct automated check of the touch-event → HUD → player/weapon-controller path.


## Full acceptance workflow passed — animated AK-74M / terrain / mobile input — 2026-11-08

- Latest project source commit validated by GitHub Actions: `71aa6c6784613e47bfbf15c90e5dd54d3a63784f`.
- GitHub Actions run [42418460738](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42418460738) completed with **success**; all 15 build/validation/upload stages passed.
- The complete asset-preparation step passed for the Cransh FPS AK-74M glTF, mesh binary, resized arms/rifle material maps, six distinct recorded rifle takes, four sand footsteps and Poly Haven Rocky Gravel PBR maps.
- Godot project import and strengthened smoke test passed. Unlike the earlier test, this pass injects real left-side touch-down/drag/touch-up events into the HUD input handler, verifies the movement vector reaches the player, confirms the player moves over the actual HeightMapShape3D floor without falling below it, and checks the right-hand fire/aim hit targets do not overlap or get mirrored by RTL layout.
- Terrain acceptance checks passed: 257×257 native heightmap collision; 256×256 indexed visual terrain mesh; upward-facing vertex normals and generated tangents; PBR diffuse/normal maps bound to the material; sufficient vertical relief; controlled ground outcrop count.
- Weapon acceptance checks passed: imported skinned AK-74M/arms scene, AnimationPlayer and idle/draw/walk/run/shot/full-reload clips. The test presses FIRE through the button signal and confirms one round is consumed, toggles AIM and TRACER through their HUD buttons, and starts the authored reload animation through the RELOAD button.
- Android debug APK export succeeded. The archive CRC, APK signature, Android manifest, DEX and ARM64 Godot native library validation all passed. Artifact upload completed.
- Verified GitHub Actions artifact: `TacticalStrike-Android-verified`, artifact ID `13592277208`, generated 2026-11-07 22:01 UTC.
- Exact artifact ZIP downloaded locally and independently checked: outer ZIP passed `unzip -t`; inner APK also passed a complete ZIP/CRC check.
- APK size: `43,118,244` bytes. SHA-256: `b4501b67ab7a481dbecfbe76dbe2fb36916693ab21b88a265305a94882b60778`.
- **Critical boundary:** this is a successful CI/source/input-path acceptance build, not a physical-device or visual screenshot sign-off. It verifies the authored model and clip calls exist and that the mobile input event route moves the player in the engine test, but cannot prove the model's final pose/framing, visual mountain quality, touch feel, audio perception or Android performance on the user's particular phone. The user must try this exact APK before the screenshot-level issues are considered resolved.
- Bots, tactical AI and LAN/hotspot multiplayer remain intentionally deferred until the single-player character, weapon, ammunition, audio and battlefield environment are accepted.


## Engine evaluation — Unity migration discussion — 2026-11-09

- The user questioned whether Godot is the best engine for the intended polished Android tactical FPS and asked whether the project should move to Unity.
- Initial technical assessment: Unity is a strong candidate for this project because of its mature 3D/FPS tooling, animation workflow, mobile profiling, documentation and broad asset ecosystem. It may make it easier to reach a higher production-quality result, but changing engines does not by itself improve graphics or gameplay; authored environment assets, animation, lighting, level design and device optimization remain essential.
- Recommended baseline if migration is approved: Unity 6.3 LTS with the Universal Render Pipeline (URP), configured for Android/mobile scalability. Unity's official release page identifies 6.3 LTS as the current LTS release, supported through December 2027. Avoid HDRP for this mobile-first target.
- Licensing/cost note: Unity Personal is currently free for eligible individuals/organizations below the USD 200,000 revenue-and-funding threshold. The Unity Personal eligibility terms must be checked again before release. GitHub Actions builds also need a working Unity Personal activation/license-secret setup; this CI path must be proven before committing to the migration.
- Reusable project material: original map requirements and design blueprint, SVG battlefield plan, licensed GLB/FBX weapon assets where formats are supported, texture maps, audio recordings, icon/artwork and other independently sourced assets with their licences/provenance.
- Must be rebuilt rather than copied as-is: Godot scenes/resources, GDScript, physics/collision setup, input/HUD wiring, rendering settings and Android export workflow. Gameplay logic will be rewritten in Unity C# and revalidated.
- Migration timing: the project is still a prototype and tactical AI/LAN multiplayer are not implemented, so this is a comparatively early point at which to switch if the user chooses Unity. Keep the current Godot project and its history intact as a fallback/reference until a Unity vertical-slice APK is built and tested on the user's Android phone.
- Proposed proof-of-concept acceptance slice before a full migration: one representative section of the 700 m × 700 m plateau; mobile first-person movement/look/fire/aim/reload; imported animated AK-74M; terrain, trenches/buildings/cover, lighting and touch HUD; Android build through GitHub Actions; device-side visual, touch and performance checks. Target a stable 30 FPS baseline with a 60 FPS goal where the device permits.
- CI constraint: Unity can build Android players in batch mode, and community GameCI workflows support GitHub Actions, but license activation and Android build modules introduce extra setup compared with the existing Godot pipeline. Validate these first.
- Decision status: **discussion only**. No engine migration, Godot source removal, branch change or Unity project creation was performed in this step. Await the user's decision before switching engines.


## Unity migration approved and first vertical-slice implementation — 2026-11-09

### User decision
- The user explicitly approved Unity as the primary engine for Tactical Strike on 2026-11-09 and asked work to begin.
- This approval supersedes the prior section above that described the engine decision as discussion-only. New development should target Unity; do not add new gameplay features to Godot unless a specific reference-build repair is needed.
- The Godot project at the repository root is retained intact as a fallback/reference. Its source, previous assets pipeline, map blueprint, and previous CI history are not deleted.

### Unity project baseline
- Unity Editor version pinned in `UnityProject/ProjectSettings/ProjectVersion.txt`: **6000.3.28f1 (Unity 6.3 LTS)**.
- Render pipeline dependency: **Universal Render Pipeline 17.3.0**; UGUI 2.0.0 for touch UI.
- Target: Android landscape, ARM64, IL2CPP, minimum Android API 25.
- Unity project root: `UnityProject/`.
- Current project is a first vertical-slice scaffold, not a finished production game. Do not describe it as a successful build or physical-device tested until the relevant checks have actually passed.

### Unity implementation added in this migration step
- Runtime bootstrap applies mobile landscape/multitouch/frame-rate defaults, creates the environment and day/night lighting, generates the battlefield, spawns the first-person player and creates the HUD.
- Battlefield generator ports the key map layout into Unity: 700 m × 700 m dusty plateau, procedural broken ridges, shallow gullies and rolling ground, attacker/defender trench lines, two separated defensive houses, firing cover, rocks and distant placeholder defender silhouettes.
- The long sandbag trench lines are combined into batched meshes and mesh colliders per side instead of creating hundreds of independent physics objects. Selected firing blocks remain destructible. This is an initial mobile draw-call/physics optimization, not a substitute for profiling on the phone.
- First-person controller includes movement/look, walk/sprint/crouch/jump, aiming/FOV transition, camera bob, keyboard/mouse Editor controls, mobile joystick/look input and randomized recorded sand footsteps.
- AK-74M combat path includes an imported-model lookup with a procedural fallback, magazine/reserve ammo, FMJ/tracer/AP cycling, firing, reload, muzzle flash, ballistic projectile gravity/drag/raycasting, cover damage and dust impacts. Recorded AK-47 takes remain explicitly a close-family audio proxy, not exact AK-74M recordings.
- Mobile HUD includes a correctly sized left movement joystick, right-side touch-look area, fire/aim/run/crouch/jump/reload/ammo controls, weapon status, a working pause panel, a settings panel, day/night switching and sensitivity controls.

### Asset conversion and provenance
- Added `tools/convert_ak74m_to_fbx.py` to import the existing pinned animated AK-74M glTF into Blender and export an FBX intended to retain the mesh, armature, embedded maps and animation takes.
- Added `tools/prepare_unity_assets.sh`, which reruns the existing pinned/licence-documented asset preparation script, converts the glTF to FBX, and stages the model, terrain PBR maps, firearm takes, footsteps, model licence and attribution under Unity Resources.
- Generated imported Unity runtime binaries are ignored by Git and regenerated during the cloud build. The original GLTF/source files and licence/provenance documentation remain the source of record.
- The Unity Editor still needs to import the generated FBX and confirm its clip naming/orientation/materials. The procedural weapon fallback exists so a failed FBX animation import does not silently leave the player's hands/weapon absent.

### Build workflow
- Added `.github/workflows/unity-android.yml` using `game-ci/unity-builder@v4` and the pinned editor version from the Unity project.
- The workflow is deliberately manual until Unity Personal activation is configured. It expects three GitHub Actions secrets: `UNITY_LICENSE` (the full locally activated Personal `.ulf` contents), `UNITY_EMAIL`, and `UNITY_PASSWORD`. Do not add the license or account credentials to any tracked file or chat.
- The build method creates/ensures URP settings and the main Unity scene, configures Android ARM64/IL2CPP, builds an APK, verifies archive integrity, signature, manifest, DEX, IL2CPP and Unity ARM64 libraries, then uploads the APK and SHA-256 report.
- The previous Godot Android workflow was renamed as a legacy/reference build and its push paths were narrowed to Godot-specific files. Existing Godot source and old workflow definition remain in the repository.
- `.gitignore` now excludes Unity generated Library/Temp/Obj/Builds/Logs/UserSettings files and the Unity imported runtime assets that are re-created by the build pipeline.

### Validation status and blockers
- Source files and GitHub Actions configuration have been added, but **no Unity Editor import, C# compile, Android build, APK signature check or physical-phone test has yet been run for this Unity branch** in this step.
- The first Unity workflow cannot build until the owner activates Unity Personal and sets all three Actions secrets. Once the secrets are present, the workflow must be run manually from Actions.
- First acceptance gates: asset preparation/Blender FBX conversion; Unity import and C# compile; URP scene creation; Android ARM64 IL2CPP APK; APK integrity/signature checks; then install and test on the actual Android phone.
- Visual framing, touch feel, FPS, PBR map import, and all imported AK-74M animation clips remain unverified on a phone. The game is not yet ready to label as an accepted Unity release.
- Tactical bots, full attacker/defender behaviour, the remaining weapon roster and LAN/hotspot multiplayer remain future milestones. Focus first on making the first-person vertical slice visually convincing and responsive on device.

### Files introduced or changed in this migration
- New Unity project files under `UnityProject/`: ProjectVersion, package manifest, bootstrap, battlefield generator, cover integrity, player controller, weapon controller, ballistic projectile, impact effects, mobile UI input helpers and HUD, plus an Editor build/setup script.
- New asset tooling: `tools/convert_ak74m_to_fbx.py`, `tools/prepare_unity_assets.sh`.
- New workflow: `.github/workflows/unity-android.yml`.
- Updated legacy workflow routing: `.github/workflows/android.yml`.
- Updated project overview and setup documentation: `README.md`, `docs/UNITY_MIGRATION.md`, and `.gitignore`.
- This context entry is cumulative and supersedes only the earlier "discussion only" status; previous Godot requirements, test logs, issues and design decisions remain preserved above.


## Unity vertical-slice orientation, terrain and workflow follow-up — 2026-11-09

- Corrected the Unity player's initial yaw: the attacker spawn at the southern edge of the plateau now faces north along negative Z toward the distant defensive sector, rather than spawning with the camera facing away from the opposing position.
- Corrected trench orientation: attacker and defender sandbag rows, rear sandbags, firing walls and cover blocks now mirror correctly so each battlement faces the opposing sector.
- Added actual capsule-height adjustment for crouching while keeping the CharacterController capsule base at ground level.
- Corrected terrain texture tiling: the terrain mesh already stores per-metre UV scaling, so the previous extra 0.065 material tiling multiplication was removed. This avoids stretching the gravel PBR detail across the 700 m map.
- Added Unity texture import rules for terrain PBR maps: normal maps are imported as NormalMap with sRGB disabled; diffuse remains sRGB; data maps are linear; mipmaps/compression and a three-map presence check are enabled.
- Set Android target SDK to API 36 in the build setup, with ARM64 IL2CPP and the configured minimum API 25 retained.
- Improved ballistic tracer line endpoints so the visible streak trails behind the moving projectile.
- The first push after the legacy workflow rename created Actions run [42532278160](https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42532278160), which reported failure but contained zero jobs and no runner logs. The exact failure cause could not be verified. To avoid every Unity commit invoking an unrelated Godot build, restored the previously working Godot workflow definition and restricted it to manual `workflow_dispatch` runs only. The Godot source tree remains untouched.
- These changes are source/configuration edits. They have not yet been compiled by Unity or validated in a Unity Android APK. The Unity Personal license secrets are still required before the manual Unity workflow can proceed.


## Unity Personal activation and GitHub Actions connection instructions — 2026-11-09

- The user asked for practical step-by-step instructions, direct links and account setup to connect Unity with the Tactical Strike GitHub repository.
- The project already contains .github/workflows/unity-android.yml, using GameCI Unity Builder v4. The workflow explicitly checks for three repository Actions secrets before doing asset preparation or building: UNITY_LICENSE, UNITY_EMAIL and UNITY_PASSWORD.
- Current GameCI v4 Personal-license instructions specify: create/sign in to a Unity ID; activate Personal through Unity Hub → Settings/Preferences → Licenses → Add → Get a free Personal license; locate the resulting Unity_lic.ulf; and add its entire text as UNITY_LICENSE alongside the same Unity account email/password as two separate GitHub secrets.
- Important distinction: GameCI v4 warns that simply seeing Personal entitlement in Unity Hub may not mean the .ulf exists; explicitly complete the Add/activation path. Do not substitute the named-user XML entitlement file.
- Windows path: C:\\ProgramData\\Unity\\Unity_lic.ulf (Explorer shortcut: %PROGRAMDATA%\\Unity). macOS: /Library/Application Support/Unity/Unity_lic.ulf. Linux: ~/.local/share/unity3d/Unity/Unity_lic.ulf.
- GitHub secrets page for this project: https://github.com/CybersecurityRahbar/Tactical-Strike-Android/settings/secrets/actions. The owner should create the exact names UNITY_LICENSE, UNITY_EMAIL, and UNITY_PASSWORD; never store their values in repository files, commits, issues or chat.
- Unity Hub activation is the only local task for this cloud-build path. The full Unity Editor and Android Build Support do not need to be installed on the user's constrained local PC; GitHub Actions builds the project on hosted runners.
- Workflow run page: https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions. Once secrets exist, choose Unity Android Build → Run workflow → main. A successful run uploads TacticalStrike-Unity-Android-verified; download the outer artifact ZIP, extract once, then install the contained APK directly.
- As of this instruction, no Unity account details or secret values have been provided, the secrets are not confirmed present, and no Unity build has been triggered using a valid Personal licence. Do not claim activation or build success until GitHub Actions shows it.
- Useful official references: https://game.ci/docs/github/activation/ ; https://game.ci/docs/github/builder/ ; https://docs.unity.com/en-us/hub/manage-license ; https://docs.github.com/en/actions/how-tos/write-workflows/choose-what-workflows-do/use-secrets ; https://unity.com/pages/license-compliance .
- A user-facing step-by-step guide was added at docs/UNITY_LICENSE_SETUP.md.


## User follow-up — activating Unity Personal in Unity Hub — 2026-11-09

- The user confirmed that they have registered a Unity account and asked how to activate the free Unity Personal license.
- Verified the current official Unity Hub instructions: sign in to Unity Hub; open Settings → Licenses; select Add license → Get a free personal license; review the terms and select “Agree and get personal edition license”. If the Personal license was automatically activated on sign-in, no further activation is needed; verify that it appears in the Licenses view.
- This activation must be done in the desktop Unity Hub on a Windows/macOS/Linux computer; it cannot be completed solely from the Android phone browser. The user does not need to install the full Unity Editor for the repository's cloud-build workflow.
- After activation, the next project step remains to locate the generated Unity_lic.ulf and create the three GitHub Actions secrets described in docs/UNITY_LICENSE_SETUP.md. Do not ask the user to send the license file contents or account password in chat.
- Official reference checked: https://docs.unity.com/en-us/hub/manage-license .
- Current status: user says Unity account registration is complete; Personal license activation itself is not yet confirmed.


## Licensing approach review — credentials-only GameCI proposal — 2026-11-09

- The user proposed removing the local Unity Hub/.ulf step and passing only UNITY_EMAIL and UNITY_PASSWORD to GameCI v4, with an additional environment variable UNITY_LICENSING_MODE=machine.
- Checked current GameCI v4 activation and builder documentation and Unity's current licensing documentation. The credentials-only approach is not documented or supported for Unity Personal: GameCI explicitly requires a one-time Personal activation via Unity Hub, the resulting .ulf content in UNITY_LICENSE, and UNITY_EMAIL + UNITY_PASSWORD. The credentials are used alongside the activated license in CI; they do not replace it.
- Unity's command-line activation route is explicitly stated to apply to Unity Pro, not Unity Personal. Unity says Personal activation must be done by signing in to Unity Hub.
- UNITY_LICENSING_MODE=machine is not documented as an input or supported activation setting for GameCI unity-builder@v4. The sample also contains a stray </i> token after machine, which must not be copied into YAML.
- The repository's current .github/workflows/unity-android.yml deliberately checks all three secrets before asset preparation/build. Omitting UNITY_LICENSE will currently fail at the preflight check; removing that check would not establish a Personal license and is not a valid fix.
- Recommended supported route remains: use Unity Hub only (no need to install the full Editor or Android modules locally), activate Unity Personal, copy the .ulf text into GitHub secret UNITY_LICENSE, and add UNITY_EMAIL and UNITY_PASSWORD as separate Actions secrets. Then run the manual workflow.
- No workflow implementation was changed in this review because the suggested credentials-only configuration is unsupported and would reduce reliability. No build was run.
- Current sources checked: GameCI v4 activation https://game.ci/docs/github/activation/ ; GameCI v4 builder https://game.ci/docs/github/builder/ ; Unity license command-line docs https://docs.unity.com/en-us/engine/6000.5/manual/get-started/install-and-upgrade/licenses-and-activation/managing-your-unity-license .


## Unity Hub installer architecture guidance — 2026-11-09

- The user asked whether to download the Windows ARM64 or x64 Unity Hub installer.
- Recommendation: download the Windows x64 (64-bit Intel/AMD) installer. The user's previously stated PC processor is Intel Core i5, which is an x86-64 processor family; ARM64 is intended for Windows devices with ARM processors such as supported Snapdragon systems.
- No code or workflow changes were needed. The user should choose the x64 Windows build of Unity Hub, then sign in and activate Unity Personal.


## Unity Hub x64 installer size guidance — 2026-11-09

- The user asked for the download size of the Windows x64 Unity Hub installer.
- Current publicly reported installer sizes vary by Hub release/package: approximately 176 MB in one current listing and 184.4 MB in another recent listing. Advise budgeting roughly 180–200 MB for the installer download; exact size may change with updates.
- Clarified that this is only Unity Hub, not the multi-gigabyte Unity Editor or Android build modules. For this project's GitHub-hosted build path, the user only needs Hub temporarily to activate Personal and obtain the license file; do not install the full Editor locally.
- Sources: Unity Hub official install guide https://docs.unity.com/hub/install-hub-win-mac ; current reported x64 sizes https://techdows.com/download-unity-hub-for-pc-windows-11-10-mac.html and https://techdows.com/download-unity-game-engine-for-pc-windows-11-10-mac.html .


## User follow-up — Unity Hub installed and signed in; next activation steps — 2026-11-09

- The user confirmed Unity Hub is installed on the Windows x64 computer and they have signed in to their Unity account.
- The exact next action is to open Unity Hub → Settings (gear) → Licenses. Even if a Personal entitlement appears after sign-in, GameCI v4 specifically warns that the entitlement display alone may not have generated the required `.ulf` file; complete the explicit **Add / Add license → Get a free personal license → Agree and get personal edition license** flow.
- After the activation flow, locate the Windows license file at `C:\\ProgramData\\Unity\\Unity_lic.ulf`. To open its parent folder quickly, paste `%PROGRAMDATA%\\Unity` in File Explorer's address bar; the ProgramData folder may be hidden. Confirm the file exists and do not share its contents in chat.
- Next, create the three repository Actions secrets at https://github.com/CybersecurityRahbar/Tactical-Strike-Android/settings/secrets/actions: `UNITY_LICENSE` = entire text of `Unity_lic.ulf`; `UNITY_EMAIL` = the same Unity account email; `UNITY_PASSWORD` = the password for that account. Do not place secret values in Git files, commits, issues, or this context.
- Once all three secrets are saved, start **Actions → Unity Android Build → Run workflow**, select branch `main`, then inspect the run logs. A success should produce artifact `TacticalStrike-Unity-Android-verified`.
- No activation state, .ulf file existence, repository secret, or successful build has been confirmed yet; the user has only confirmed installation and sign-in. The user does not need to install the full Unity Editor or Android Build Support locally for this GitHub-hosted build workflow.
- Current official references rechecked: Unity Hub license management https://docs.unity.com/en-us/hub/manage-license ; GameCI v4 Personal activation https://game.ci/docs/github/activation/ . GameCI explicitly requires the explicit Add activation flow to ensure a .ulf is created even if the license appears in Hub.


## Unity CI activation verification and first compile/build debugging — 2026-11-09

- The user confirmed that the three GitHub Actions repository secrets have been created: `UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD`. Secret values were not read, printed, or recorded.
- The workflow was expanded to run on `push` to `main` when `UnityProject/**`, `tools/**`, or `.github/workflows/unity-android.yml` changes, while retaining manual `workflow_dispatch`. This provides repeatable automatic Android build validation during Unity development.
- Run `42656556063` reached and passed the secrets preflight, but asset preparation failed because Blender 4.0.2's bundled glTF importer tried to import NumPy and failed with `ModuleNotFoundError: No module named 'numpy'`.
- Fixed the CI asset-tools install step to install `python3-numpy` alongside Blender and ffmpeg, and added a direct Blender/NumPy import sanity check before asset conversion. Commit: `9b5f66bb15aac5edd7da97b25deedb5dc70f62a2`.
- Run `42657184217` then passed: runner setup, all three secrets presence checks, asset-tools installation, external source asset downloads, and Blender AK-74M glTF-to-FBX asset preparation. It reached Unity 6.3.28f1 project import and C# compilation, confirming the credentials and Personal license were accepted by GameCI. Unity logs showed successful login, successful license entitlement handling, and a returned ULF license. The build then failed at C# compilation because the project manifest omitted built-in Unity engine modules required by scripts.
- The first compile reported missing `UnityEngine.AnimationModule` and `UnityEngine.AudioModule`. Added built-in packages `com.unity.modules.animation: 1.0.0` and `com.unity.modules.audio: 1.0.0` to `UnityProject/Packages/manifest.json`. Commit: `c398271415088aa76fb4cb4f1fac58c570e74670`.
- The next compile passed the animation/audio references and exposed the remaining missing `UnityEngine.ParticleSystemModule` references in `ImpactFx.cs`. Added `com.unity.modules.particlesystem: 1.0.0` to the manifest. Commit: `e327690e914e9df08f063c2026fdace91101c028`.
- Latest build run at the time of this update: `42660431636`, triggered by commit `e327690e914e9df08f063c2026fdace91101c028`. It has passed runner setup, checkout, all three secret-presence checks, and is currently still in the **Install asset conversion tools** step. No conclusion or APK artifact is available yet. Its step began at `2026-11-09T17:03:09Z`; this unusually long in-progress step needs monitoring and investigation if it does not complete.
- Important verified distinction: all three secret names were present (preflight passed); GameCI authentication and ULF license acceptance were explicitly confirmed in Unity logs; however, no complete Android APK build or on-device test has succeeded yet. Do not tell the user the build is ready until a later run completes all APK verification steps and uploads the artifact.
- Build run links: https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42656556063 ; https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42657184217 ; https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42660431636 .
- Technical reference for manifest modules: Unity 6.3 built-in package listing identifies Animation, Audio, and Particle System as built-in packages; built-in packages must be enabled in the project to include their APIs. https://docs.unity.com/en-us/engine/6000.3/manual/packages-list/packages-all/pack-build .


## CI build investigation — Unity Android build delays and failure causes — 2026-11-09

- The user reported that Unity Android cloud builds were taking a long time and failing, and requested a repository/log review. Inspected GitHub Actions run metadata, job-step summaries, and full logs for runs 42656556063, 42657184217, 42658769638, 42660431636, 42663598361, and 42663660495.
- The three earliest distinct blockers were identified and fixed in the source:
  1. Run 42656556063 failed during external asset preparation because Blender's glTF importer could not import NumPy (`ModuleNotFoundError: No module named 'numpy'`). The workflow now installs Ubuntu package `python3-numpy` alongside Blender and FFmpeg and verifies NumPy from Blender's Python runtime.
  2. Run 42658769638 reached Unity import but C# compilation failed because the manifest did not explicitly enable built-in Animation and Audio modules. Added `com.unity.modules.animation` and `com.unity.modules.audio` to `UnityProject/Packages/manifest.json`.
  3. The subsequent compile found missing Particle System API types in `ImpactFx.cs`. Added built-in module `com.unity.modules.particlesystem` to the same manifest. Unity's official 6000.3 package documentation lists this exact package ID as the Particle System built-in module: https://docs.unity.com/en-us/engine/6000.3/manual/packages-list/packages-all/pack-build .
- After those fixes, Unity itself successfully compiled and produced the Android APK in runs 42660431636, 42663598361 and 42663660495. For example, run 42663598361 logged `TACTICAL STRIKE ANDROID BUILD PASSED` and created `build/Android/TacticalStrike-debug.apk` (about 24 MiB in the job's file listing). Therefore the Unity Personal credentials and Unity licensing were accepted: the preflight secrets check passed, Unity logged in, and the runner retrieved the ULF entitlement. These runs did not complete the workflow and did not publish an artifact because the following Android SDK setup/verification stage failed.
- The later failure was not a Unity/game-code error. `android-actions/setup-android@v3` attempted to create `/usr/local/lib/android` and failed with `EACCES: permission denied, mkdir '/usr/local/lib/android'`. APK verification and artifact upload were consequently skipped.
- Fixed that workflow stage in commit `da91bd94ca3d1b5ab8e7e67c88e7a922aba5201c`: upgraded to `android-actions/setup-android@v4`, set both `ANDROID_HOME` and `ANDROID_SDK_ROOT` to the writable `${{ runner.temp }}/android-sdk`, and removed the obsolete `tools` SDK package from the action inputs. The workflow's subsequent explicit installation of platform-tools, Android API 36 and build-tools 36.0.0 remains responsible for the verification utilities.
- Build duration analysis from run 42663598361 (17:20:16Z to 17:37:38Z, about 17m22s): free-disk preparation took about 3m10s; installing Blender/FFmpeg/NumPy took about 37s; asset preparation about 22s; the Unity Docker import/compile/Android IL2CPP build ran about 13 minutes. The cache step reported a cache miss and Unity logged that it was rebuilding the Library because the asset database was absent. The later SDK setup failure caused the cache post-save step to be skipped, so those failed workflows did not leave a reusable Library cache. Multiple pushes close together also started overlapping full builds (runs 42663598361 and 42663660495 began 22 seconds apart), adding unnecessary resource contention.
- Current validation attempt after the Android SDK setup fix: run 42676736011, commit `da91bd94ca3d1b5ab8e7e67c88e7a922aba5201c`. At the time of this entry it is still in progress at the free-disk preparation step; the corrected Android SDK setup, APK verification and artifact upload have not yet run. Do not report success until this run reaches `success` and the artifact exists.
- Artifact status at the time of investigation: the earlier Unity APKs were created inside failed jobs, but no workflow artifact was published because verification and upload did not run. Use the artifact from the first fully successful run, not the partial APK from a failed run.
- Performance follow-up: if a successful run is not obtained, or if cache post-save is skipped again, switch to explicit `actions/cache/restore@v4` plus `actions/cache/save@v4` immediately after the successful Unity build so Library is saved before any later verification step can fail. Consider adding a per-ref concurrency group with `cancel-in-progress: true` to prevent multiple main-branch pushes from running full Unity builds simultaneously. Preserve the disk-cleanup step unless disk-capacity testing shows it can safely be reduced.
- Sources: current android-actions/setup-android README https://github.com/android-actions/setup-android ; Unity built-in modules 6000.3 reference https://docs.unity.com/en-us/engine/6000.3/manual/packages-list/packages-all/pack-build .


## Validation progress after Android SDK path correction — 2026-11-09

- Rechecked run 42676736011 after the workflow change in commit `da91bd94ca3d1b5ab8e7e67c88e7a922aba5201c`.
- The run has now passed runner setup, free-disk cleanup, checkout, the non-empty Unity secret preflight, Blender/FFmpeg/NumPy installation, and external asset preparation. The cache step completed. The Unity 6.3 Android build step is currently in progress.
- The corrected `android-actions/setup-android@v4` step using the writable runner-temp SDK location has not yet started, so its success is not yet verified. APK integrity/signature/ARM64 checks and artifact upload have also not yet run.
- Latest confirmed status remains `in_progress`; do not announce successful delivery until the run completes with conclusion `success` and the expected artifact is present.


## Definitive Unity APK publication failure and CI timing analysis — 2026-11-09

- Re-opened the full job log for run `42676736011` (job `129360005964`), rather than relying only on the failed job summary. This identified the immediate, definitive reason no downloadable APK was produced:
  `build/unity-apk-zip-validation.txt: Permission denied`.
- Unity did successfully create `build/Android/TacticalStrike-debug.apk` in that run. The editor build log explicitly reported:
  `TACTICAL STRIKE ANDROID BUILD PASSED: /github/workspace/build/Android/TacticalStrike-debug.apk (310679739 bytes)` (about 296 MiB). The runner then failed at the first report redirection because the GameCI Unity Docker container ran with `RUN_AS_HOST_USER=false` and created the mounted `build/` directory as root. The host GitHub Actions runner user could read the APK but could not create validation report files in the root-owned directory. Consequently, the verification step failed, artifact upload was skipped, and GitHub discarded the ephemeral runner workspace. This is why no APK appeared in Artifacts even though the Unity build itself succeeded.
- Exact elapsed-time breakdown for run 42676736011 (job started 18:44:54Z, completed 19:01:59Z; about 17 minutes 5 seconds):
  - Free-disk cleanup: 18:44:58Z to 18:48:10Z, about 3m12s.
  - Asset tools installation: 18:48:10Z to 18:48:43Z, about 33s.
  - External runtime asset preparation: 18:48:43Z to 18:49:09Z, about 26s.
  - Cache restore: cache miss (no previous Unity Library cache was available).
  - Unity Docker image: not present locally; GitHub pulled `unityci/editor:ubuntu-6000.3.28f1-android-3` from 18:49:11Z; it reached the build stage at about 18:52:23Z, taking roughly 3m12s to pull.
  - Unity project import, shader/package processing, C# compilation and Android IL2CPP/APK build: about 9m02s (18:52:23Z to 19:01:25Z).
  - Android SDK setup and package installation: about 15s.
  - APK verification failed about 8 seconds after it began, because it could not write to the root-owned `build/` directory.
- The Unity Library cache was missed because prior runs failed before the `actions/cache@v4` post-job save could persist it; the new key also needed to incorporate generated-asset preparation scripts so changes there invalidate the cache correctly. Thus subsequent runs repeatedly paid the initial import cost.
- Applied a direct workflow fix in commit `0a4782c7f724daeb84211ff7eaa18f36c0412659`:
  - Added a concurrency group that cancels obsolete overlapping main-branch Unity builds.
  - Changed Unity Library caching to explicit `actions/cache/restore@v4` and `actions/cache/save@v4`, saving immediately after a successful Unity build and before later APK verification; the cache key includes `tools/**`. Cache-save failure is non-fatal.
  - Added a host-side ownership repair step that first confirms the APK exists, then runs `sudo chown -R "$(id -u):$(id -g)" build` and checks that the runner can write to the build directories before validation. This specifically fixes the reproduced `Permission denied` failure.
  - Reduced artifact compression from level 9 to level 0 because an APK is already a compressed ZIP container, avoiding expensive double-compression on the large (about 296 MiB) APK.
- A new verification run was automatically started for commit `0a4782c7f724daeb84211ff7eaa18f36c0412659`: run `42680414591`. At the time of this note it was in progress; no success or downloadable artifact has yet been confirmed. The immediate fix is now under CI verification. Do not announce success until APK archive, signature, manifest, DEX, ARM64 libraries validation pass and the artifact appears.
- Root-cause references for implementation details: GameCI's builder docs explain the Linux Docker user's default ownership behavior and the `chownFilesTo` option https://game.ci/docs/github/builder/ ; GitHub cache docs explain automatic post-job cache saving and its dependency on successful completion https://docs.github.com/en/actions/reference/workflows-and-actions/dependency-caching ; the current run log is available at https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42676736011 and the new verification run at https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42680414591 .


## First fully verified Unity Android APK build — 2026-11-09

- Rechecked the complete GitHub Actions run 42680414591 for commit 0a4782c7f724daeb84211ff7eaa18f36c0412659. The run and its only job both completed with conclusion success at 2026-11-09T19:21:50Z (job: 19:08:10Z–19:21:49Z, about 13m39s). Run: https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42680414591 .
- Every required job step passed, including secret-presence preflight, Blender/FFmpeg/NumPy installation, conversion/preparation of the Unity runtime assets, Unity build, Unity Library cache save, output ownership repair, Android SDK setup, APK verification, and artifact upload. GameCI successfully authenticated to Unity and received the configured ULF entitlement. No C# compiler warning CS or error CS entries were found in the job log.
- Asset preparation passed. Blender 4.0.2 ran the AK-74M glTF-to-FBX conversion; the resulting FBX and the required terrain textures and recorded audio were staged under UnityProject/Assets/Resources/TacticalStrike/Imported. Unity's own runtime-asset validation reported that the terrain PBR maps, AK-74M model, and recorded audio are present.
- Unity Editor 6000.3.28f1 completed Android IL2CPP build successfully. The output directory listing in the log showed TacticalStrike-debug.apk at about 24 MiB and the Unity player build ended with Build succeeded.
- Correction to the previous investigation entry: the Unity log message TACTICAL STRIKE ANDROID BUILD PASSED ... (310679811 bytes) prints BuildReport.summary.totalSize, not the APK file's on-disk byte length. The subsequent actual directory listing and the generated validation report establish that the APK itself is 25,086,318 bytes (about 23.9 MiB), not 296 MiB. The earlier statement describing 310,679,811 bytes as the APK's physical size was incorrect; this entry supersedes that interpretation without deleting the historical note.
- The root-owned output issue is fixed in the tested run. The host step executed sudo chown -R "$(id -u):$(id -g)" build and confirmed it could write to both build/ and build/Android/. Android SDK setup then succeeded using the runner-temp SDK path.
- All configured APK checks passed:
  - unzip -t: no corrupt ZIP data.
  - apksigner verify: signature verifies using APK Signature Scheme v2.
  - Required entries exist: AndroidManifest.xml, classes.dex, lib/arm64-v8a/libil2cpp.so, and lib/arm64-v8a/libunity.so.
  - The report marks archive integrity, APK signature, Android manifest, DEX, ARM64 IL2CPP and ARM64 Unity runtime as PASS.
- Independently downloaded the exact workflow artifact and inspected its ZIP contents. It contains five files: the APK, APK SHA-256 file, overall validation report, ZIP-integrity report and signature-validation report. The APK's SHA-256 is 0b3e39db1ecb98fc36e4a7a3457bddf51cc381b13e8914a9cd4b32e1f8e145fc; a separate hash calculated from the downloaded APK matches the report exactly.
- Artifact: TacticalStrike-Unity-Android-verified, ID 13703810664, size 25,111,775 bytes, retention configured to 14 days. Download from the run's Artifacts section: https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42680414591/artifacts/13703810664 .
- The first restore in this run was a cache miss, as expected because this was the first successful run using the corrected cache key. The explicit post-build save then succeeded at 19:21:34Z, saving the Unity Library cache under key unity-library-6000.3-android-cf24f7fa523a8ae4e9ce38b0ea814dfcc26730c37656fb658653211f78e07169. The cache archive sent was 1,351,247,916 bytes. A later build with unchanged inputs should be able to restore this cache and reduce Unity import time; that speedup is not yet measured and must be confirmed on the next run.
- Non-blocking tooling notices appeared: Android's sdkmanager CLI reports deprecation, and GitHub Actions reports Node.js 20 action-runtime deprecation notices. They did not fail the job. Unity also logged SDL leaked 2 allocations during shutdown, but continued to Build succeeded; retain this as a minor diagnostic item if it recurs.
- Important boundary: this is now a successfully built, signed, structurally verified debug APK artifact. No physical Android device installation, launch, touch-control test, rendering/performance test, weapon interaction test, or gameplay/network test has been performed. Passing CI does not prove those runtime behaviors. The next validation is to install this APK on the user's Android phone and test the actual game.

## Unity phone-playtest findings and visual/gameplay correction pass — 2026-11-10

- The user installed the first verified Unity APK and supplied a screenshot from a real phone test. Reported/visible faults: the AK-74M + hands viewmodel is grossly oversized and clipped across most of the view; rifle/hand materials appear nearly white; the tracer is not clearly visible in flight; impact effects appear magenta/pink; the hit response feels weak; the brown rocky terrain is visually unacceptable; cover positions appear poorly arranged; a single fire tap sounds like sustained automatic fire; and the night-mode toggle still looks like dusk rather than night.
- Source inspection found a concrete omission in the asset path: the legacy asset download step produced arms_baseColor.jpg, ak74m_baseColor.jpg, arms_normal.png and ak74m_normal.png under assets/runtime/models/ak74m/textures, but the Unity staging script never copied them into Resources. The runtime material mapper therefore did not have those explicit texture resources. Also, the viewmodel used a fixed scale of 0.72 without measuring the imported rig's camera-space bounds.
- First-person presentation correction committed in 15e03ecef24893b8883d12402f1416df7b04b147: WeaponController now loads arm/rifle diffuse and normal maps explicitly, maps them to imported child materials, converts those materials to URP/Lit, disables viewmodel self-shadowing, fits the rig using its renderer bounds (target largest dimension ~0.84m, clamped scale), and moves it to a lower/right camera-space rest position. It adds a modest ADS offset, light recoil response and reduces the muzzle light. This is a measured code-side fit, not yet visually certified on the user's phone; a further screenshot-driven fine alignment may still be required.
- Projectile/tracer work committed in 790736bac424da224217bc3110195c90cd00f0ab: ballistic movement uses approximately 8 ms substeps, gravity and drag are integrated per substep, the shooter's own collider is excluded from impact tests, and a longer/brighter yellow-orange tracer streak is visible behind tracer-mode projectiles. One shallow-angle ricochet is allowed on selected hard materials, with reduced residual velocity/damage. These are gameplay approximations, not exact real-world ammunition simulation.
- Impact feedback committed in 2e25cf55e2e43244625cf0b35aa0f98350ecd4b0: replaced the previous pink/brown effect with separate directional spark and dust particle systems, a hot white-yellow core fading through yellow/orange to red, and surface-dependent one-shot sound layers for metal, masonry/stone and ground. The sound classifier currently uses collider/material names, so it is still approximate. The three added sounds are generic CC0 game sound effects, not genuine recordings of bullets hitting specific real materials.
- Battlefield work committed in 2b9d666ada8f7e7b9591b5a83da4ba4b9de7ef5c: removed the old rocky-gravel/dry plateau height profile and introduced a rolling green valley with higher west/east mountain shoulders, a central long-range engagement lane, staggered midfield firing barriers, forward shoulder sandbag nests and two additional village/farmhouse shells. The original trench and house generation remains. Added a flank-only vegetation dressing pass so trees/shrubs/grass should not block the main sightline. The map is now intended to read as a green mountain battlefield, not a desert/plateau; it is still procedural stylized terrain rather than a complete high-end photogrammetry map.
- Lighting committed in d3505376754ec7315cba6ca59a12ddc546d1aecc: daytime sky/sun/ambient and haze are shifted to a clearer green-valley daylight palette; night mode now drops direct light intensity to 0.018, ambient intensity to 0.20, and uses a deep blue-black sky/fog instead of the prior dusk tint.
- Asset staging and validation were updated in commits 07438e9fdf99e4e159e58777ce59212831b8cbee, bd422cd427ee59a0049321cfc8ce68ccc1dd721e, b25101db647404f6022708f4f1ebb01d24de569b and e5806461d9ee839723c86d06a337ccb7b5f91b8e: the Unity preparation script now copies all four AK source texture maps into Resources, fetches Poly Haven Sparse Grass CC0 PBR maps, fetches a small pinned CC0 Kenney Nature Kit subset and converts six GLB nature meshes to FBX via tools/convert_nature_assets.py, and stages three CC0 impact audio clips. Firearm OGG takes are shortened to one ~0.42 s transient/decay with a duration check so one tap does not replay the whole long recording. The editor build validator requires the new resources and verifies the six nature models and AK textures actually import. The Android build log now distinguishes on-disk APK bytes from Unity's total player content size.
- The mobile HUD now labels the battlefield GREENRIDGE SECTOR 01 / FIELD OPERATIONS and changes the settings action label to NIGHT MODE; this is included in commit e5806461d9ee839723c86d06a337ccb7b5f91b8e.
- Design references checked during this pass: Special Forces Group 2's official listing describes offline bots, Wi-Fi multiplayer, more than 30 maps and a broad weapon roster (https://play.google.com/store/apps/details?id=com.ForgeGames.SpecialForcesGroup2); Squad 9.0's official notes describe a UE5.5 migration, Chaos Physics, MetaSounds, lighting/global-illumination improvements and foliage rework (https://www.joinsquad.com/updates/squad-9-0-release-notes). Those products are mature, much larger systems; this project's current mobile URP target cannot simply duplicate their graphics stack or proprietary assets. The selected environment and impact assets remain CC0 with source documentation.
- Weapon roster boundary: the current playable weapon is still the animated AK-74M. The user's seven planned weapons are AK-74M, M4A1, PKM, RPG-7, SVD/Dragunov, DShK 12.7 mm and optional M2/.50-calibre HMG. This visual correction pass does NOT claim the other six are integrated or ready. Each needs its own documented model/materials, suitable viewmodel rig/hand alignment, draw/idle/fire/reload animation support (or an intentionally designed equivalent), sound, weapon data, reload/ammo behavior and functional testing. No fake selector entries are to be shown as finished weapons.
- Validation status at the time this context entry is written: the source files and CI asset staging/Unity validation scripts have been committed, but the new GitHub Actions build has not yet been confirmed. The next task is to inspect the latest Unity Android workflow run, repair any asset-download/Blender/import/C# issues from the actual log, and only announce a new APK after APK archive, signature, Manifest, DEX, ARM64 and artifact upload checks all pass. The user's phone test remains necessary for weapon fit, materials, tracer visibility, impact feel/audio, map presentation, night darkness, mobile frame rate and touch controls.

## Viewmodel pivot-centering refinement and latest CI attempt — 2026-11-10

- After reviewing the imported rig fit, a second camera-space correction was added in commit f97bb70593023fac604df99d385bcb36f1d008a7. The fitter now measures the renderer-bounds centre in model space and compensates the root pivot after scaling, so an off-centre FBX origin cannot shift the visible weapon/arms away from the intended lower-right position. The ADS target preserves that calculated root offset.
- The complete source set for this pass is now on main at f97bb70593023fac604df99d385bcb36f1d008a7. The final relevant Unity/asset preparation commits are 07438e9fdf99e4e159e58777ce59212831b8cbee, bd422cd427ee59a0049321cfc8ce68ccc1dd721e, b25101db647404f6022708f4f1ebb01d24de569b, e5806461d9ee839723c86d06a337ccb7b5f91b8e and f97bb70593023fac604df99d385bcb36f1d008a7. Documentation commits are 8930a097a03f76e4a950c1f32c91908d65f56c74 and this context update.
- Latest workflow attempt is run 42701602075, triggered by the final weapon pivot-centering commit. The previous intermediate runs were automatically cancelled by the branch concurrency rule. Run 42701602075 was pending and then entered progress at the free-runner disk cleanup step; no validation result or new APK artifact has been confirmed yet. Track: https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42701602075 .
- The final asset-preparation script downloads Sparse Grass PBR maps and the pinned six-model CC0 Kenney subset, converts the GLBs to FBX using tools/convert_nature_assets.py, copies the AK-74M maps into Unity Resources, stages three CC0 impact one-shots and crops recorded AK reports to <=0.46s, with explicit file/duration checks. Unity's pre-build validation now fails early if any new texture, nature model or impact audio file is absent or not imported. This should prevent publishing another APK that silently lacks the requested art/audio dependencies.

## Flank vegetation placement refinement — 2026-11-10

- Increased the deterministic Greenridge tree scatter from 152 to 280 candidate instances for a visibly greener flank line, retaining the long central firing lane and avoiding both trench corridors. Added explicit no-tree buffers around the two newly-added village farmhouses so the foliage system does not place trees through their masonry shells. Commit: 0827b18448e7b62e013f3756294e33fc5c92bfd5.
- This adjustment changes only procedural placement density/exclusion zones; it does not certify visual quality or performance. The next CI build and subsequent phone test must confirm import, frame rate and that the tree silhouettes read correctly at distance.

## Tracer continuity adjustment for mobile render rates — 2026-11-10

- Final code review identified that a fixed 5.5 m tracer tail is shorter than the distance a 900 m/s projectile travels between rendered frames (about 15 m at 60 FPS), leaving visible gaps. Updated the LineRenderer tail to scale from the frame's travelled distance, clamped to 8–55 m, and narrowed the tracer core/tip widths. This is intended to keep the tracer visibly continuous across 20–60 FPS mobile frame rates. Commit: a5f33c250cbc21b8878424b59978fd8aa2f3678c.
- This correction is still awaiting Android CI and real-phone observation; the dynamic tail can make the tracer visually long at low FPS, so the user's next screenshot/video will determine whether the trail length and brightness need tuning.

## Seven-weapon asset sourcing review — 2026-11-10

- Additional weapon-asset reconnaissance found an explicitly CC0 M4A1 release on OpenGameArt by nisu (3DModelsCC0) and a CC0 RPG-7 Blender model by GuillaumeCote on BlendSwap. A public-domain small-arms pack by Tabasco/OpenGameArt now explicitly lists CC0 alongside CC-BY 3.0 and includes M4/AR-15 and Dragunov/SVD geometry; it has Blender materials but no UV textures, so it needs texture/material authoring and a careful import/scale/attachment pass. Sources: https://opengameart.org/content/m4a1-assault-rifle ; https://blendswap.com/blend/3384 ; https://opengameart.org/content/various-small-arms-assault-rifles-sniper-pistol .
- A separate CC0 M2 .50-calibre Browning model is available from britdawgmasterfunk on Sketchfab and BlendSwap, with split parts intended for modification/animation: https://sketchfab.com/3d-models/m2-50cal-machinegun-cc0-cbd897c1e1554fae9a5302a79ed59640 ; https://blendswap.com/blend/30289 .
- No adequately verified CC0 PKM or DShK asset was confirmed in this reconnaissance. Sketchfab candidates found for PKM/DShK/RPG/SVD include CC-BY, Free Standard, or unclear/ripped-from-game provenance. Do not download or bundle ripped Call of Duty/Squad meshes, NonCommercial material, or a model whose redistribution licence is not clear. The DShK/M2 are crew-served heavy machine guns and should be implemented as mounted positions with traverse/elevation and crew positioning, not treated as shoulder-held rifles.
- The remaining weapon work is a separately testable feature stage: source and document legally reusable geometry; create a shared, correctly fitted hand/weapon attachment pipeline; add per-weapon fire/reload/sight animation and sounds; calibrate individual ammunition, projectile, recoil and damage profiles; add selection/inventory UI; then verify on device. The current corrective APK still has the AK-74M only; no other weapon is represented as complete.

## Corrective build CI progress — run 42702747580 — 2026-11-10

- The earlier candidate build attempts for intermediate code commits 0827b184 and f97bb705 were cancelled by the workflow's intentional main-branch concurrency rule when later source fixes were pushed. The surviving/latest code build is run 42702747580, whose checked-out head is the tracer-continuity commit a5f33c250cbc21b8878424b59978fd8aa2f3678c. The branch's subsequent docs-only commits do not trigger or change this build.
- Confirmed run 42702747580 passed runner setup, free-disk cleanup, checkout, Unity Personal secret preflight, Blender/FFmpeg/NumPy installation, and the full updated runtime asset preparation step. Asset preparation completed successfully in 19 seconds (21:31:05Z–21:31:24Z). The Unity Library cache restore also completed successfully in 28 seconds (21:31:24Z–21:31:52Z).
- At the time of this note the Unity 6.3 Android player build step is in progress. APK generation, archive/signature/ARM64 validation, artifact upload, and device testing are not yet confirmed. Do not announce a new downloadable APK until run 42702747580 is completed successfully and the artifact exists.

## Infantry character asset scouting — 2026-11-10

- Further infantry-character scouting found britdawgmasterfunk's CC0 Military Character Kit 1.1 on BlendSwap (a 99.4 MB Blender kitbash collection of helmets, plate carriers, pouches, boots and other soldier components) and a separate CC0 Military RTS Character 1 model (about 10k triangles, three LODs, 2K diffuse/AO/normal/roughness textures). The latter is described as UE4-mannequin-proportioned and suitable for mobile/RTS but the source does not claim it is already rigged/animated; it would need a humanoid rig and animation retargeting before being used for active soldiers. Sources: https://blendswap.com/blend/27959 ; https://sketchfab.com/3d-models/military-rts-character-1-cc0-908065a7006443b29edca30276750a6d .
- Quaternius also publishes CC0 animated humanoid packs and a Universal Animation Library; their models are intentionally low-poly/stylized and animation sets are not all on one common rig. They may be useful for a mobile test/LOD layer, but should not be presented as realistic Squad/SFG2-grade soldiers. Official license FAQ: https://quaternius.com/faq.html .
- No soldier pack has been integrated in the current corrective APK. The current procedural distant defenders are still placeholders; a proper soldier pass requires a verified textured soldier mesh, rig/retargeted idle-walk-run-crouch-prone, weapon-hold/aim/fire/reload poses, LODs, hit capsules and mobile performance checks.

## First successful corrective phone-feedback APK — build verified and artifact downloaded — 2026-11-10

- The final corrective code commit under test was a5f33c250cbc21b8878424b59978fd8aa2f3678c. GitHub Actions run 42702747580 completed with conclusion success; job 129451525387 also succeeded. Total run duration was about 11m49s (21:28:07Z–21:39:56Z). Run: https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42702747580 .
- All job stages passed: Unity secrets preflight, asset-conversion tool installation, updated runtime asset staging, Unity Library cache restore, Unity 6.3.28f1 Android IL2CPP build, cache save, output ownership repair, Android SDK setup, APK archive/signature/ARM64 verification, and artifact upload.
- Important import confirmation from the Unity log: the AK-74M imported these authored animation clips: Rig|AK_Idle, Rig|AK_Draw, Rig|AK_Walk, Rig|AK_Run, Rig|AK_Shot, Rig|AK_Reload, Rig|AK_Reload_var2 and Rig|AK_Reload_full. All six selected CC0 nature models were converted to FBX by Blender and the Unity pre-build validation explicitly passed the green PBR terrain, AK-74M rig/maps, CC0 nature models and shot/impact audio checks.
- The cached Unity Library was successfully restored using key unity-library-6000.3-android-cf24f7fa523a8ae4e9ce38b0ea814dfcc26730c37656fb658653211f78e07169 in about 28 seconds. After the build, a new cache was saved successfully under key unity-library-6000.3-android-5091822e538fceae88b440be9b6902d1207c8044a3ae9062c5936067b494645c. The run took about 7m27s in the Unity editor build stage, slower than a warm cached no-code-change build may be, because this run invalidated/rebuilt important assets and native IL2CPP code.
- Unity explicitly logged: TACTICAL STRIKE ANDROID BUILD PASSED: build/Android/TacticalStrike-debug.apk (28064409 bytes on disk; 320710678 bytes total player content). This now distinguishes physical APK size from total Player content size.
- Downloaded the actual artifact ZIP and inspected its five contents. Extracted APK physical size is 28,064,409 bytes (about 26.8 MiB). Independently recalculated SHA-256 from the extracted APK and confirmed exact match with the uploaded report: f87435d4306de708cbe970584bcc07efc6d83338d94c3c15f3c3e63db913a5b4 .
- Signature report confirms APK Signature Scheme v2, one signer, Android Debug certificate, RSA 2048. ZIP integrity has no compressed-data errors. Required AndroidManifest.xml, classes.dex, lib/arm64-v8a/libil2cpp.so and lib/arm64-v8a/libunity.so entries passed the workflow checks.
- Artifact: TacticalStrike-Unity-Android-verified, ID 13711834868, size 28,090,965 bytes, retention through 2026-11-23T21:39:54Z. Download page: https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42702747580/artifacts/13711834868 .
- Non-blocking Unity/CI notices: SDK Manager CLI deprecation; Node 20 runtime deprecation warnings for several GitHub Actions; Unity logged a transient licensing access-token update error, a missing package lookup for platforms;android-, and Android asset-pack request rate limit. Despite those notices, Unity reported Build succeeded, all independent APK checks passed, and artifact upload completed. Keep these as observations if later builds exhibit licensing/SDK instability; they are not failures in this run.
- Verification boundary remains important: this is a successfully built and structurally verified debug APK. It has not yet been installed/launched or visually tested on the user's phone. CI cannot confirm that the new viewmodel position/materials, vegetation scale/density, impact particles/sounds, tracer streak, night darkness or mobile frame rate look correct in actual play. The next device retest must check those items. The tracer is currently optional: the weapon starts in FMJ mode; press the AMMO control once to switch to TRACER and observe its flight streak.
- The correction pass still has only the AK-74M as an integrated usable player weapon. M4A1, PKM, RPG-7, SVD/Dragunov, DShK and M2 are not yet complete; no fake weapon-select entries were added. Proper soldier rigs/animations and the final professional sound design also remain future work. The new map is a procedural mobile-friendly green mountain valley with CC0 low-poly foliage and staged cover, not a finished photogrammetry/AAA Squad map.

## Full Unity first-playtest audit and Squad reference study — 2026-11-10

### User's latest review request and reported failures

The user requested a complete review of the current Unity-based project after the first Unity gameplay test, followed by research into Squad's maps, weapons, sound, collisions, movement and aiming. The user reports that:
- The first-person weapon and hands were not acceptably aligned; a previous attempted correction left the weapon/hands absent from the visible screen.
- Tracer ammunition still did not look or read like a bright, burning, visible projectile; a pink square/flat-looking effect was unacceptable. The user expects a visible hot projectile in flight and a distinct, forceful impact effect.
- The agreed seven weapon types were not delivered.
- The map lighting, buildings, trenches and defensive positions did not resemble the dense, authored tactical environments requested from Squad.
- Weapon, impact, movement and ambient sounds were not convincing.
- The user cannot inspect the Unity Editor on their computer and needs a reliable way to preview work without installing or operating the full editor.

### Repository and build snapshot checked for this review

- Repository: https://github.com/CybersecurityRahbar/Tactical-Strike-Android
- Active branch: main.
- Main HEAD at review: 8986b14ef8bb20ae2093a71bf17d751d5d76cf4b (documentation update).
- Latest executable/gameplay code commit at review: a5f33c250cbc21b8878424b59978fd8aa2f3678c.
- Unity: 6000.3.28f1 (Unity 6.3 LTS), URP 17.3.0, Android ARM64/IL2CPP.
- The latest code build run inspected: GitHub Actions run 42702747580, completed successfully. It produced a structurally verified APK with physical size 28,064,409 bytes and SHA-256 f87435d4306de708cbe970584bcc07efc6d83338d94c3c15f3c3e63db913a5b4.
- CI confirmed Unity import/compile, source asset preparation, Android APK archive integrity, v2 signing, manifest, DEX and ARM64 IL2CPP/Unity libraries. It did NOT verify visual framing, gameplay feel, sound quality, scenery quality or Android frame rate.
- Latest docs-only commits do not change the game executable relative to the a5f33c25 build.
- The audit in this entry was a source/repository/research review. No gameplay code or Unity asset was changed during this audit.

### Source-level findings

#### First-person viewmodel

- WeaponController.cs does instantiate the imported AK74M FBX as a child of the player camera; the current source does not intentionally remove the gun/arms.
- It loads arm/rifle albedo and normal textures, uses the imported Legacy Animation clips, computes renderer bounds, scales the combined rig toward a 0.84 m maximum dimension, applies a root-pivot compensation, and sets a lower-right camera-space rest position.
- This is not a reliable final rig-calibration pipeline. Bounds-based automatic fitting cannot guarantee correct left-hand foregrip, right-hand trigger/grip placement, stock-to-shoulder relationship, sight alignment, muzzle axis or per-animation consistency. The bounds are calculated from renderers at initial bind pose, not verified in the actual phone view.
- The procedural fallback in BuildFallbackRifle builds only simple rifle primitives and no arms/hands. Therefore a resource-load failure can directly result in no visible hands.
- The current Unity import/build validation checks the AK FBX and that animation clips exist, but does not check that the actual hand/rifle renderers appear at the intended on-screen coordinates or that the grip poses are correct.
- Conclusion: source contains a viewmodel restoration attempt, but the user's visual complaint remains unresolved until a screenshot/video of the actual current APK proves otherwise. Do not report the weapon alignment as fixed.

#### Tracer and impact VFX

- BallisticProjectile.cs creates the tracer using a single LineRenderer, not a visible projectile head or a spherical hot core.
- The line is extremely narrow (start width 0.012 m; end width 0.0035 m) and relies on a gradient. The line positions are tail then projectile position, while the alpha gradient is opaque at the tail and fades to transparent at the projectile end. This is a poor match for the user's request for a clearly visible hot leading projectile with a luminous tail.
- The most recent source scales the tail length from frame travel (clamped to 8–55 m) to reduce gaps on low frame rates. This is a continuity fix, not a realistic burning tracer effect.
- There is no authored emissive tracer projectile mesh/sprite, no flame-shaped animated sprite/flipbook, no distinct glowing head, and no configured bloom/post-processing volume.
- ImpactFx.cs creates procedural particle systems with a small spark burst (22 normal / 38 heavy) and a separate dust puff. The current particle material is a plain URP particle/unlit material without an authored spark or flame texture atlas. This can read as flat billboard particles rather than convincing sparks/embers.
- The heavy branch is selected only when projectile damage >= 75. AK FMJ/AP values in WeaponController are 38/42, so AK hits always use the small branch, including AP.
- Impact classification is currently approximated from collider and material names. There is no surface-profile data asset, physical material map, or independent effect tuning for dirt, grass, wood, masonry, concrete and metal.
- URP is created with supportsHDR=false and render-scale 0.90; no post-processing volume is configured. A strong emissive look cannot be assumed.
- Conclusion: yellow/orange line and spark code is a procedural placeholder, not the requested visible hot tracer and impact spectacle.

#### Weapon roster and combat data

- WeaponController.cs hardcodes a single AK-74M: 900 m/s initial velocity, fixed drag/range, 30-round magazine, 120 reserve and a 0.10 s fire cooldown.
- It exposes only FMJ, TRACER and AP modes for this one gun. AP changes damage/penetration values slightly, but there is no per-weapon data definition or weapon inventory/selector.
- The agreed roster is AK-74M, M4A1, PKM, RPG-7, SVD/Dragunov, DShK 12.7 mm and M2/.50-calibre HMG. Only AK-74M is integrated. DShK and M2 are to be mounted crew-served positions, not ordinary shoulder weapons.
- No placeholder selector entries should be represented as completed weapons. Each weapon requires a model, materials, rig/hand anchors, animation set, audio, handling/recoil profile, magazine/reload/ammo rules, sight/ADS configuration and test acceptance.

#### Ballistic collision and material damage

- BallisticProjectile uses approximately 8 ms simulation substeps, gravity/drag and Physics.RaycastAll to find the closest surface hit. This is a reasonable prototype approach to avoid tunnelling at high projectile speeds.
- On any collision the projectile applies damage to a BallisticCover component if present and is then destroyed. Penetration currently modifies the damage dealt to cover and ricochet eligibility, but does not allow the projectile to continue through a penetrated object.
- BallisticCover degrades a material colour and destroys the whole cover GameObject when integrity reaches zero; it does not create a convincing break/debris/damage-state system.
- Surface response is string-name-based rather than a proper material/impact profile.
- Conclusion: collision is a basic raycast damage prototype, not a finished penetration, material response or destruction system.

#### Battlefield geometry and art

- BattlefieldGenerator.cs creates a 700 m x 700 m terrain mesh with 256 divisions (257 x 257 vertices; about 2.73 m horizontal vertex spacing).
- The terrain height is generated from Perlin/fBm noise and several Gaussian shoulder/valley shapes. This gives broad procedural slopes, but it is not a hand-sculpted tactical terrain with manually composed ridge chains, gullies, micro-terrain, routes and carefully staged sightlines.
- One sparse-grass diffuse/normal texture is used for the ground. Although a roughness map is staged and validated, the terrain shader does not blend multiple distinct surface layers (earth, grass, rock, mud, road, disturbed trench earth) across the landscape.
- The two trench lines are generated from sinusoidal offsets and rows of capsule-mesh sandbags; height depression is only about 1.8–1.9 m at the trench centre. There are no full branching communication trenches, dugouts, traverse bays, revetments or designed connected trenches.
- Midfield cover is mostly 16 repeated primitive box barriers plus a small number of individual capsule-shaped sandbags.
- Four houses exist in source: two defensive houses and two village farmhouses. They are built from simple box primitives. The four continuous wall panels remain solid; the dark door/window pieces are placed over the walls rather than forming real openings. There is no authentic interior, furniture, rubble, roof detail, village prop dressing or damaged-state variation.
- Rock dressing uses 420 combined flattened sphere meshes. The latest foliage pass attempts 280 tree candidates, with a subset of bushes and 44 grass clumps, mostly on the flanks to keep the centre lane open. These are limited low-poly assets, not a richly authored woodland.
- Distant defenders are five static primitive capsule/head/cube silhouettes. There are no rigged infantry characters, animated soldier poses, squad AI, cover selection, navigation, suppression logic, or tactical combat behavior.
- There are no clearly authored road networks, field tracks, drainage ditches, fences, utility lines, varied farm props, vehicles/wrecks, craters, layered debris fields or authored defensive outposts.
- Conclusion: the existing world is a procedural mobile greybox/vertical-slice foundation. It is not a Squad-like finished battlefield; simply increasing tree count or changing material colours will not solve its underlying design problem.

#### Lighting and presentation

- TacticalStrikeBootstrap configures a procedural skybox, one directional sun, trilight ambient colours and exponential-squared fog. There is no authored lighting scenario, baked GI, environment/reflection probe workflow, light-probe coverage or post-processing volume in the source reviewed.
- URP is intentionally configured with HDR off, renderScale 0.90 and limited shadows for mobile performance. These are valid optimization considerations, but the current art/lighting setup lacks the richer visual hierarchy and controlled high-contrast lighting the user expects.
- MobileHud uses runtime-built legacy Unity UI Text and simple tinted rectangle buttons. It is functional prototype HUD layout rather than a polished tactical FPS interface.

#### Movement, aiming and sound

- FpsPlayerController implements joystick movement/look, sprint, crouch, jump, camera bob and a basic camera-height change. Walk/sprint/crouch speeds are fixed values.
- AIM currently narrows camera FOV from 78 to 57 degrees and shifts the whole weapon model by a fixed offset. It does not align a physical iron sight/optic reticle to the centre of the screen, does not configure per-weapon ADS transforms, and has no proper sight zero/range UI.
- Recoil is a small root pitch rotation with a simple decay. There is no calibrated weapon spring, stance-dependent sway, lateral recoil pattern, aim stabilization, shoulder/stock response, leaning, prone state, breath focus or suppression response.
- Firing audio is loaded as a small set of two AK recordings and is played from an AudioSource with spatialBlend=0 (2D). Footsteps use only two sand-step clips. Impact audio uses three generic CC0 clips (metal, masonry/debris, heavy thud) selected by string matching.
- There is no weapon-specific handling/charging/reload/bolt/magazine audio, indoor/outdoor gunshot variation, distance-layered weapon report, directional bullet crack/whiz, surface-aware footstep library, environmental ambience, wind/weather layer, echo/reverb zone system or tuned audio mixer.
- Conclusion: movement, aim and audio are placeholders, not the more weighty and distinct weapon/stance/sound design the user requested.

### Squad reference study and applicable design conclusions

Official Squad 9.0 release notes (2025-09-04) state that Squad migrated to Unreal Engine 5.5, began using Nanite/frame-generation technology, Chaos physics and MetaSounds, and upgraded map creation/performance tools. The same notes describe the Al Basrah remake as having a larger play area, greater density and fidelity, plus a lighting overhaul using a new Global Illumination system and updates to assets, materials and foliage. These are design/production lessons; Tactical Strike remains a mobile Unity URP project and must not attempt to copy a PC-only UE5 rendering stack directly.
Source: https://www.joinsquad.com/updates/squad-9-0-release-notes

The official Squad Steam announcement on 2026-11-03 described the Oskil River map playtest (2026-11-06 to 2026-11-09) as a war-damaged Eastern European landscape combining damaged settlements/urban areas, open terrain and trenches, with more detailed environmental set dressing. The same announcement said the map was still a work in progress, and included World Partition performance testing. As of this review date (2026-11-10), this was a recent playtest announcement, not evidence that the full 10.7 release had shipped.
Source: https://steamcommunity.com/app/393380/

Squad 9.0 gunplay notes emphasize controlled, less random recoil, keeping the sight picture from being displaced excessively, recoil-camera offset limits, grip alignment/angle limits, sway clamps and a smoother recoil curve. Other notes mention weapon spring behavior responsive to player movement, stationary focus zoom, and more weapon-specific audio/animation fixes. Squad's AI notes describe bots avoiding open ground, moving to cover when reloading, suppressing suspected enemy positions and tracking relevant sounds. These are useful design principles for the project rather than instructions to reproduce proprietary Squad assets.
Source: https://www.joinsquad.com/updates/squad-9-0-release-notes

Recent Squad map research also shows why the request cannot be met by a flat field with random obstacles: terrain/woodland sightlines, urban routes and crossings, settlements, trenches, and varied natural/artificial cover all have distinct gameplay roles. Reference examples include Gorodok's woodland/hill character and the Oskil River combination of damaged settlement, open ground and trench systems.
Map reference: https://squadoutpost.com/maps/
Official playtest announcement: https://steamcommunity.com/app/393380/

### Required direction for the next development stage

1. Treat the current visual vertical slice as a prototype, not as an accepted base for incremental cosmetic tweaks. Preserve the working Unity project/build pipeline, but rebuild the battlefield art/layout and first-person/combat presentation as distinct systems.
2. Produce and approve a top-down battlefield layout before building the scenery: attacker entry/approach, central long-range fire corridor, offset terrain ridges, flanking routes, defensive trench network, two distinct defended compounds/houses, village clusters, approach roads, firing positions and clear sightline-break points.
3. Replace final-map primitives with authored modular assets and a coherent PBR terrain material system. Use multiple blended ground layers and place props deliberately to tell a believable battlefield story, while retaining mobile LOD/instancing and draw-call budgets.
4. Create a per-weapon viewmodel calibration and rig standard with dedicated hand/grip/stock/sight/muzzle anchors and per-weapon ADS transform. Validate using screenshots/video from the actual build, not only model bounds or successful CI compilation.
5. Redesign tracer VFX into a visibly hot leading core, a short continuous emissive tail and distinct airborne/impact behavior; create separate surface/calibre profiles for dust, sparks, debris and sound. Enable any bloom/HDR treatment only through measured mobile quality tiers.
6. Replace hard-coded single-weapon code with data-driven weapon definitions and a real inventory/selection flow. Build the seven agreed weapon types individually; no model or selector may be labeled complete until rig, animation, sound, ammo, recoil, ADS and test criteria pass.
7. Rework the first-person movement and aim system around responsive weapon spring, controlled recoil, stance-dependent sway, physical sight alignment, weapon-specific ADS and meaningful walk/sprint/crouch/aim behavior.
8. Build a layered audio system with firearm-specific close and distant reports, handling/reload sounds, material-specific impact layers, directional projectile pass-by sounds, surface-dependent footsteps and environmental ambience. Avoid playing every gunshot as a flat 2D sample.
9. Make visual inspection possible without requiring the user to install the Unity Editor. A desktop-browser WebGL preview is a possible auxiliary review build, with desktop controls added where needed; it is NOT a substitute for the Android APK or Android device performance testing, and must be evaluated for its platform limitations before adoption.
10. Add visual acceptance evidence to future CI where feasible (terrain/map overview, first-person frame, weapon/hand frame, tracer/impact frame) and require a real Android device test for final framing, controls, frame rate and sound. A green CI badge proves build integrity, not artistic quality.

### Audit completion and verification boundary

- Reviewed the repository tree, README, cumulative context, Unity player/weapon/projectile/impact/world/UI/bootstrap/build scripts, asset preparation workflow, Unity package/editor versions, and latest main-branch GitHub Actions runs.
- Verified the last published Unity APK is structurally valid and its build succeeded; no current source or CI evidence proves the user-facing visual defects are solved.
- No gameplay implementation was changed in this audit. This entry records the user feedback, source-level findings, Squad research and next-stage acceptance rules so the next session can continue from the same evidence rather than repeating exploratory work.


## Preview workflow discussion — Unity Hub versus Editor and browser preview — 2026-11-10

- The user asked how they can preview the game and help with design, and whether Unity Hub (installed previously to activate Unity Personal) can preview the project.
- Clarification: Unity Hub is a management/launcher application. It can install/manage Unity Editor versions, projects and licences, and open a project in the Editor; Hub itself is not the 3D game editor/preview runtime. The actual in-editor preview is through Unity Editor's Game view and Play mode, with Device Simulator available for mobile layout simulation. Official references: https://docs.unity.com/en-us/hub ; https://docs.unity.com/en-us/hub/add-editor ; https://docs.unity.com/en-us/engine/6000.0/manual/unity-editor/editor-windows-views-reference/game-view .
- The project uses Unity Editor 6000.3.28f1. To open locally, user must have this Editor version installed and add Android/Web modules only if needed. In Hub -> Installs, check whether this exact version is listed; installing Hub or signing in/activating the Personal licence does not by itself prove the Editor is installed. Hub -> Projects -> Add/Open from disk can open the extracted/cloned repository's UnityProject folder. Actual Play-mode use requires project source/assets to be prepared and imported. Current external runtime assets are generated/staged by tools/prepare_unity_assets.sh in CI, and some generated imported binaries are intentionally not committed; thus a plain clone may not match the full CI-prepared build without asset preparation.
- User's local PC is previously described as Core i5, 4 GB RAM and 4 GB GPU. Unity Editor may be heavy on this system, so do not make local Editor usage mandatory or promise smooth operation. Keep the Editor as an optional direct inspection path if 6000.3.28f1 is already installed and usable.
- Recommended primary collaborative preview path: a dedicated desktop-browser Web build (Unity 6.3 Web platform/WebGL) produced by GitHub Actions and published to a review URL, potentially GitHub Pages. Unity official Web documentation confirms WebGL delivers interactive 3D to desktop browsers; a Web build requires the Web module, a hosted server or Build and Run, and supported 64-bit WebAssembly/WebGL2 browser. References: https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/webgl ; https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/webgl/intro/gettingstarted ; https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/webgl/intro/browsercompatibility .
- The Web preview must be treated as a design-review build, not a substitute for Android acceptance testing. Browser platform constraints/performance differ; Unity's Web documentation describes WebAssembly threading limitations and other Web-specific restrictions: https://docs.unity.cn/6000.3/Documentation/Manual/webgl-technical-overview.html . Android APK remains required to validate phone touch controls, native performance and audio behavior.
- Proposed review controls for the browser build (to be implemented after the Web build path is validated): WASD movement, mouse look, left-click firing, right-click ADS, reload, sprint/crouch/jump, plus explicit camera/view toggles for first-person, overhead map overview, and weapon/tracer/impact inspection. Add a light-weight quality option for browser preview and a repeatable link/version label so user feedback is tied to a particular commit/build.
- User collaboration loop proposed: user opens the review URL in Chrome, tests a specific scene/weapon/map area, shares a screenshot or short screen recording and describes what is visually wrong; subsequent revisions should be judged against that evidence. Include automated screenshots where practical, but do not treat CI compilation as visual approval.
- No Editor installation state was confirmed in this turn. Ask user to open Unity Hub -> Installs and report whether 6000.3.28f1 is listed before recommending any local installation/download. Do not ask them to share Unity licence credentials/secrets.


## First implementation of the browser-review path and new rebuild baseline — 2026-11-10

### User's decision

- User confirmed they will not install additional software and wants to use the browser preview workflow so they can help review the game's visual design.
- The project direction is now a **full rebuild of the previous gameplay presentation and battlefield**, using Squad as a reference for principles and quality. Preserve Unity 6.3 LTS, the working cloud asset-preparation process, and verified Android pipeline; do not treat the current procedural Greenridge scene or monolithic AK script as the final architecture.
- No proprietary Squad code, maps, models, sounds, or branding may be copied. Use original design and redistributable assets with documented licences.

### Changes prepared in this implementation commit

1. Added `TacticalStrikeBuild.BuildWebGL()` to the Unity Editor build entry point. It:
   - builds the same enabled runtime scene for `BuildTarget.WebGL`;
   - keeps Unity 6.3.28f1 / URP 17.3.0;
   - enables Gzip compression, decompression fallback and data caching for static hosting;
   - disables WebGL threads so GitHub Pages does not require cross-origin-isolation headers;
   - sets a 2048 MB maximum WebAssembly heap;
   - validates the generated `index.html` and writes a `.nojekyll` marker.
2. Updated desktop input compilation to include `UNITY_WEBGL`. Keyboard/mouse support now includes mouse capture on an actual background click, ignores HUD clicks while capturing, prevents the capture click from also firing the weapon, and releases firing/ADS when Escape unlocks the pointer.
3. Updated the HUD to retain touch controls on mobile while showing desktop keyboard/mouse instructions instead of a large touch joystick/button layout in a desktop browser.
4. Added `.github/workflows/unity-web-preview.yml`. It checks the already configured Unity Personal secrets, installs Blender/FFmpeg/NumPy, prepares the exact same pinned runtime assets, restores/saves a separate WebGL Unity Library cache, invokes GameCI for WebGL, validates the output, and deploys it with GitHub Pages actions.
5. Added `tools/validate_web_preview.py`. It requires non-empty `index.html`, loader, compressed .data, .framework.js and .wasm payloads; opens the compressed payloads to verify their gzip headers/streams; and writes `preview-build.json`, a file inventory and `.nojekyll`.
6. Added `docs/WEB_PREVIEW_GUIDE.md` with browser controls and the manual first-time GitHub Pages setting.
7. Added `docs/TACTICAL_REBUILD_PLAN.md` and `docs/northwind-valley-layout.svg`. The first map concept is a 700m x 700m temperate rural/highland combat space with muted natural olive/earth palette, broken ridges, central long-range lane, covered western/eastern flanks, a branched defender trench/communication network, two distinct enterable compounds, farm ruins, service tracks and crew-served heavy MG positions.

### Browser controls intended for the first review build

- Click the game surface to capture pointer; Escape releases pointer.
- WASD: move; mouse: look; left click: fire; right click: ADS; Left Shift: sprint; C: crouch; Space: jump; R: reload; T: cycle ammunition.
- The initial browser build is for desktop Chrome review. It does not replace Android touch/performance/audio acceptance.

### Required one-time repository setting

- In repository Settings -> Pages -> Build and deployment -> Source, select **GitHub Actions**. The expected review URL is `https://cybersecurityrahbar.github.io/Tactical-Strike-Android/`.
- This Settings selection is a manual prerequisite; the workflow file alone cannot guarantee that the repository's Pages site is enabled or configured.
- Do not ask the user to install Unity Editor or share Unity secrets.

### Verification status and honest boundary

- The GameCI documentation confirms the `targetPlatform: WebGL` build path with an existing Unity Personal license; Unity 6.3.28f1 documents the Gzip compression/decompressionFallback/dataCaching settings used here.
- At the time this commit is authored, the new WebGL workflow has **not yet run**. The WebGL module in the GameCI editor image, Unity C# compilation for WebGL, generated filenames, Pages deployment permissions/settings, and the final site URL remain to be validated by Actions.
- The desktop controller/HUD code has not yet passed the Unity WebGL compiler or an interactive browser test. The pointer-lock path and mouse button behavior must be verified in Chrome.
- No browser preview URL is being claimed as live until the Pages deployment step succeeds and a real HTTP open confirms the page.
- The map diagram and rebuild plan are design baselines only; the scene geometry has not yet been replaced. The old procedural world remains temporarily visible to establish the preview path first.
- Android behavior should remain unchanged by the WebGL conditional/HUD selection, but an Android rebuild is required to confirm no regression.

### Sources checked for this change

- GameCI Builder v4 WebGL target: https://game.ci/docs/github/builder/
- Unity 6.3 Web build workflow and hosting: https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/webgl/intro/gettingstarted
- Unity 6.3 WebGL PlayerSettings: https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/playersettings/webgl
- Unity compression fallback: https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/playersettings/webgl/decompressionfallback
- GitHub Pages custom Action workflow: https://docs.github.com/en/pages/getting-started-with-github-pages/using-custom-workflows-with-github-pages


### Deployment packaging safeguard — 2026-11-10

- Reviewed the current `actions/upload-pages-artifact@v4` input contract after the first workflow was queued. Hidden files are excluded by default, which would silently omit Unity's `.nojekyll` marker.
- Added `include-hidden-files: true` to the Pages artifact upload step so the marker created by the post-build validator is actually published. The upload path contains only the generated Unity WebGL build and its generated preview manifest/inventory; no repository secrets or source-control directories are inside that path.
- This is a preventive packaging correction. The new workflow attempt must still prove the action accepts the input and the final site deploys.


### Desktop pointer-capture correction — 2026-11-10

- A second HUD review found that the transparent right-side touch-look surface was still being created on desktop WebGL even though the joystick/combat buttons were hidden. That raycast target would have intercepted mouse clicks over most of the right half of the viewport, preventing the browser build from requesting pointer lock.
- Moved creation of the touch-look surface inside the mobile-platform condition. Desktop Web builds now have no transparent touch surface; only visible HUD controls intercept clicks. Android continues to create the look surface, joystick and touch combat buttons.
- This is a source-level input correction. Unity WebGL/Android builds and actual cursor/touch behavior still require CI and runtime verification.


### First WebGL and Android verification runs started — 2026-11-10

- Final source commit under current browser-preview test: `48209d190668eded6fd809ef4f8dfc092bbaaca1` (desktop touch-look surface correction). Latest main docs-only commit at this note: `61df6976dfec5a08055b866d4a39845677055567`.
- WebGL workflow run `42830985681`: https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42830985681 . GitHub registered the workflow, started the job, passed runner setup, free-disk preparation, checkout and all three Unity Personal secret-presence checks. At the latest status inspection it was still running the Blender/FFmpeg/NumPy installation step. Asset preparation, WebGL Unity compilation, output validation, Pages setup/deployment and live URL check have not yet occurred.
- Android regression workflow run `42830985872`: https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42830985872 . It was still in the free-disk preparation stage at the latest status inspection. No conclusion is available.
- Earlier attempts were cancelled only because subsequent commits intentionally superseded them under the branch concurrency rule; the latest source test above is the surviving attempt for the current code.
- The browser URL `https://cybersecurityrahbar.github.io/Tactical-Strike-Android/` remains an expected URL only, not a confirmed live deployment. Do not tell the user it is ready until GitHub Pages deployment succeeds and the exact URL is verified.
- One repository-owner action may be needed: Settings -> Pages -> Build and deployment -> Source = GitHub Actions. The Pages workflow will expose an explicit failure if the repository is not enabled/configured.
- Android's prior verified APK (run 42702747580) does not contain these WebGL input/HUD changes; this current Android CI run is required to establish no regression. A successful compile/build does not replace phone visual testing.


### User enabled GitHub Pages with GitHub Actions — 2026-11-10

- User completed repository Settings -> Pages -> Build and deployment -> Source -> GitHub Actions.
- GitHub displayed the confirmation banner: "GitHub Pages source saved." This confirms the Pages source selection has been saved successfully.
- The user sees GitHub's generic suggested workflow cards (GitHub Pages Jekyll and Static HTML). Do NOT ask them to select either card: the repository already contains the custom Unity workflow `.github/workflows/unity-web-preview.yml`, which builds Unity WebGL and deploys the generated output. These suggested cards are generic starter workflows and are not part of this project's pipeline.
- Current WebGL run inspected after the user's confirmation: run `42830985681`, still in progress at step "Install asset conversion tools"; Unity WebGL compilation, file validation, Pages artifact upload and deploy have not yet been reached. Run URL: https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42830985681 .
- Android run `42830985872` has completed all reported build and APK verification steps successfully; its final run/job state was still finishing the final GitHub post-job step at last inspection. Run URL: https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42830985872 .
- Therefore, no live WebGL URL is confirmed yet. The expected URL remains `https://cybersecurityrahbar.github.io/Tactical-Strike-Android/`, and must only be called ready after the WebGL run completes its Pages deployment and the exact URL is opened/verified.
- Next user instruction: open repository Actions tab and select the latest **Unity Browser Preview** run to inspect its status; no more changes in Pages settings are needed. If the WebGL workflow later fails specifically because Pages is not configured/available, inspect that failure and re-run after confirming the already-saved setting. Do not ask the user to repeat the Pages-source setup.

## WebGL preview failure diagnosis and repeat-build optimization — 2026-11-10

### Failed run root cause (verified from GitHub Actions logs)

- Inspected Unity Browser Preview run `42830985681`:
  https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42830985681
- The run completed in 24 minutes 6 seconds (`14:07:48Z` to `14:31:54Z`) with conclusion `failure`.
- This was **not** a Unity compiler/build failure and **not** a GitHub Pages configuration failure:
  - Unity license checks passed.
  - Runtime asset preparation passed.
  - Unity WebGL Player build completed successfully (`Build succeeded`; `TACTICAL STRIKE WEB PREVIEW BUILD PASSED`) with reported player content of 17,534,539 bytes.
  - Pages configuration/upload/deployment were skipped only because the validator failed first.
- Exact validator exception: `RuntimeError: Missing required WebGL compressed data file; tried ('*.data.gz',)`.
- Unity's build log explicitly produced GzipMarker files with names:
  - `build/WebGL/Build/WebGL.data.unityweb`
  - `build/WebGL/Build/WebGL.framework.js.unityweb`
  - `build/WebGL/Build/WebGL.wasm.unityweb`
  The validator incorrectly assumed `.gz` suffixes. Unity's Gzip output uses `.unityweb` suffix in this build.
- The Android build run for the same source commit `48209d190668eded6fd809ef4f8dfc092bbaaca1` completed successfully: run `42830985872`.
  https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42830985872

### Why the first preview build was slow

- The first WebGL run was a cold build. Its Unity Library restore explicitly reported: `Cache not found for input keys`.
- Unity therefore imported the project from scratch and created a new Library cache. The first run then saved a cache of approximately 911,707,445 bytes (about 912 MB).
- The workflow also installed Blender, FFmpeg and NumPy, then re-downloaded and converted the pinned AK-74M, nature, terrain and audio runtime assets on the first run.
- Unity's actual WebGL Player build succeeded; the failure happened during post-build file validation, so the long wait did not represent a failed Unity compilation.
- A WebGL build and deployment still run for every relevant gameplay/source change. The user's Pages source setting is one-time; the site URL stays the same and is replaced on each successful deployment. Documentation-only commits do not trigger the build.
- Build timing is inherently variable on hosted runners. Do not promise a fixed duration.

### Corrective changes committed

1. `cf0f3bc8ed80c798e94ac621a6a19c6e74e095a2` — fixed `tools/validate_web_preview.py`:
   - Accepts Unity's `*.data.unityweb`, `*.framework.js.unityweb`, and `*.wasm.unityweb` output as well as `.gz` variants.
   - Reads each gzip stream to completion in chunks, so truncation/CRC errors are detected instead of checking only the first 64 bytes.
   - Records actual filenames and compressed/decompressed sizes in `preview-build.json`.
2. `5741911f76d304a33db8ba7d405f798b77027116` — adds a distinct prepared-runtime-assets cache in `.github/workflows/unity-web-preview.yml`:
   - Caches `assets/runtime` and `UnityProject/Assets/Resources/TacticalStrike/Imported`.
   - Cache key is tied to the pinned asset preparation and conversion scripts.
   - On a valid hit, skips installing Blender/FFmpeg/NumPy and skips repeated asset downloads/conversion.
   - On a miss or invalid cache, prepares the assets and saves them for subsequent runs.
3. `ec062e4b79db0ac2b69e9cf11c0e164408b1c228` — corrected the runtime-cache marker write command.
4. `f7de6f4b450687744a145d1ec904bff3bc9e7eaf` — documented build cadence and cache behavior in `docs/WEB_PREVIEW_GUIDE.md`.

### Expected behavior after optimization

- The first run using the new runtime-asset cache still needs to create that cache, so it can still perform the asset-tool installation and asset conversion once.
- Subsequent runs with unchanged preparation scripts should reuse both the prepared-runtime-assets cache and Unity Library cache. Gameplay code changes still cause a fresh WebGL build/deployment, but should no longer repeat the full external-asset installation/download/conversion stage.
- Changes to the pinned asset preparation/conversion scripts intentionally invalidate the prepared runtime asset cache. Changes to Unity project assets may cause Unity to reimport the changed assets, while cached unchanged Library data can still be reused.
- Current validator fix is source-level only until a new Actions run passes validation, uploads the Pages artifact, deploys, and the expected URL is opened successfully.
- Expected URL (not yet declared live until verified): https://cybersecurityrahbar.github.io/Tactical-Strike-Android/

### Corrected WebGL pipeline validation run started — 2026-11-10

- Latest corrective WebGL workflow run: `42836781579` (commit `ec062e4b79db0ac2b69e9cf11c0e164408b1c228`): https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42836781579 .
- GitHub accepted the updated workflow and created the job. At the latest inspection it was still running the `Free runner disk space for Unity Web build` step; the runtime-assets cache, validator, Pages artifact and deployment steps had not yet run.
- Earlier runs `42836663512` and `42836702284` were cancelled by the next commits under the intended `cancel-in-progress` concurrency rule; they are superseded attempts, not new build defects.
- The latest source code/validator fixes are committed to `main`; the current Pages deployment is still unverified until run `42836781579` succeeds through `Deploy browser preview` and the URL is opened.

### First successful Unity WebGL browser preview deployment — 2026-11-10

- Corrected WebGL run `42836781579` completed with `conclusion: success` on commit `ec062e4b79db0ac2b69e9cf11c0e164408b1c228`:
  https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42836781579
- Verified every job step succeeded, including prepared runtime asset checks/cache, Unity WebGL build, Unity payload validation, GitHub Pages configuration, Pages artifact upload, and `Deploy browser preview`.
- GitHub Actions job final environment URL was evaluated as `https://cybersecurityrahbar.github.io/Tactical-Strike-Android/`.
- This confirms the first WebGL build was published successfully. It remains a desktop browser review build; visual/gameplay correctness and browser runtime loading still need the user's manual review.
- Recommended review sequence: open the URL in desktop Chrome, wait for Unity to load, click the game canvas to capture the pointer, use WASD/mouse, Escape to release cursor, then inspect player hands/AK-74M, weapon scale/alignment, movement/aim, projectile tracer visibility, terrain, trenches, and building layout. Record screenshots/video and report defects against the build commit.
- Successful run URL remains the same for subsequent preview updates; relevant commits trigger another build/deploy and replace the published browser version.

## User visual review: browser preview still showed old gameplay — 2026-11-10

- User opened the first successful WebGL deployment and reported that it looked exactly like the prior prototype, not the full rebuild that had been discussed.
- Source audit confirms the report is correct. `UnityProject/Assets/TacticalStrike/Scripts/Core/TacticalStrikeBootstrap.cs` still constructs the playable world by calling `BattlefieldGenerator.Build(worldRoot)`.
- `UnityProject/Assets/TacticalStrike/Scripts/World/BattlefieldGenerator.cs` remains the old procedural Greenridge implementation (700m map with the prior two trench/cover bands, old midfield cover, old buildings and scattered props). It has not been replaced by a playable Northwind Valley scene.
- `docs/northwind-valley-layout.svg` is only a static top-down concept diagram; it is not imported into Unity and does not represent real playable terrain. The plan itself labels it as concept layout / not final terrain geometry.
- `docs/TACTICAL_REBUILD_PLAN.md` describes the intended new design; most of those features remain planned, not implemented. The seven-weapon roster is also a specification; the runtime `WeaponController.cs` still loads and builds the existing AK-74M viewmodel only.
- The first WebGL run `42836781579` successfully built and deployed the **old active game scene**. It verified the WebGL pipeline only. It did not apply the full gameplay, weapon or map redesign. Previous wording suggesting the map had been added to the playable game was misleading; correct this explicitly in future replies.
- The commits after source build `48209d190668eded6fd809ef4f8dfc092bbaaca1` up to `ec062e4b79db0ac2b69e9cf11c0e164408b1c228` changed the WebGL workflow, validator and context only; the successful preview source did not change the active gameplay/world implementation.
- Updated `docs/TACTICAL_REBUILD_PLAN.md` in commit `49ee57747e3db0ab54eaab77878e9d8dcc2797b0` to clearly distinguish design specification from implemented playable scene and to state the next required map implementation milestone.
- Communication correction: call the existing browser build a **technical preview of the old prototype**, not the rebuilt game. The Northwind Valley map must be implemented as real playable Unity geometry and integrated through the bootstrap before claiming it exists in the browser preview.

## First playable Northwind Valley implementation started — 2026-11-10

### Actual Unity source changes committed to main

1. `7118fd19b56187e5cf95089813075bb6f08af651` — added `UnityProject/Assets/TacticalStrike/Scripts/World/NorthwindValleyGenerator.cs` and switched `TacticalStrikeBootstrap.cs` from `BattlefieldGenerator.Build` to `NorthwindValleyGenerator.Build`. The player spawn is moved to the southern deployment side at z=304, facing north.
2. `b93a850fce8bbeb71eca347fe8b10670dd4c0027` — first movement/weapon/HUD integration pass; `3a322ad42cf4ea6c7d0dec86997c33fff4653d01` replaces an incomplete intermediate player-controller snapshot with the fully wired stamina/prone/recoil implementation.
3. `5f812c2416bf7ee69d5404e73395df3b8aa88271` — corrected trench parapet placement relative to exterior ground and made the tracer tip share its emissive material with the line to avoid per-shot material leakage.
4. `f751b25c6d42fff2787cbfa3585d7b3816761cc9` — added combined disturbed-earth trench floors and corrected sight-anchor offset calculation to include the imported weapon root scale.

### Current map implementation details

- The active bootstrap calls the new `NorthwindValleyGenerator`; the previous `BattlefieldGenerator` is no longer the active world builder.
- The new generator creates a 700 m x 700 m collidable sculpted terrain mesh (384 subdivisions per side), central valley/saddles, eastern and western ridge shoulders, drainage gullies, and a northern ridge line.
- Defender and attacker trench networks are explicit irregular connected polylines with communication branches, depression carved into terrain, combined traverses/revetments, sandbag parapets, firing bays, and dark disturbed-earth floor meshes. The map also contains dugouts/covered assembly shelter, a southern staging side, road/track segments, two seasonal drainage channels, two defensive compounds, two additional rural/farm buildings, courtyards, open doorways, window apertures, interior partitions, village/farm sheds, static DShK/M2 emplacement geometry, woodland props, ridge rocks, fences and debris.
- The two main houses are enterable in the collision geometry: door openings and side window apertures are actual gaps rather than decorative panels. The mounted DShK/M2 placements are currently visual static positions, not yet player-operable emplacements.

### Current movement / weapon handling implementation details

- `FpsPlayerController.cs` now has smoothed acceleration/deceleration, walking/sprinting/crouching/prone stances, stamina drain/recovery, slower ADS movement, stationary focus zoom (right-mouse ADS plus Shift), stance-dependent camera bob and weapon accuracy modifiers, and camera recoil callbacks.
- `WeaponController.cs` remains AK-74M-only. It now has movement/stamina/stance-sensitive viewmodel sway, recoil spring and camera kick, shot-to-shot recoil progression, stance/movement-dependent spread, and a search for actual rear-sight/optic and muzzle transforms in the imported rig, with safe fallback positions.
- `BallisticProjectile.cs` tracer rounds now use a thicker visible emissive tail and a visible incandescent leading point. Current ballistic damage and surface response are still the earlier single-AK implementation and need a broader pass.
- `MobileHud.cs` displays Northwind Valley / Sector 01 and stamina; mobile includes prone control; desktop controls document Z for prone.

### Deliberate remaining scope — not yet complete

- This is the first true playable-map and infantry-handling implementation pass, **not** a claim of full Squad parity or visual polish. The map still needs high-quality PBR texture blending, building/prop art refinement, decals, more natural lighting/post-processing and device performance profiling.
- Only AK-74M has an integrated first-person rig. M4A1, PKM, RPG-7, SVD, DShK and M2 need individual assets, hands/rig anchors, animations, weapon definitions, sound and gameplay integration. DShK/M2 meshes are currently static emplacement geometry.
- AI infantry, suppression, objectives/round flow, functional emplacements, lean/vaulting, weapon-specific reload/switch animations, layered distance audio, rich impact material profiles, progressive cover damage and reliable penetration are not yet implemented as complete systems.
- The latest source commit under CI is `f751b25c6d42fff2787cbfa3585d7b3816761cc9`. WebGL run `42850646475` and Android run `42850646495` were still in progress at the latest inspection; the WebGL job had restored both prepared-runtime-assets and Unity Library caches, then entered the Unity WebGL build step. Do not claim compile success or send the old preview URL as the new map until the latest WebGL run passes validation, deploys, and the actual page is checked.
- Earlier build runs at intermediate commits were cancelled by newer commits under `cancel-in-progress`; those cancellations are superseded attempts, not compiler failures.

### Squad reference study and asset policy

- Squad 9.0 official notes were reviewed for principles to implement in an original Unity codebase: standardize camera recoil limits and weapon-grip alignment, clamp sway, smooth recoil curves, stationary vision focus, weapon spring response and MG handling/suppression. Source: https://www.joinsquad.com/updates/squad-9-0-release-notes
- The Squad Oskil River description was reviewed for a mix of damaged settlement, open terrain, trench lines and environmental dressing. Source: https://store.steampowered.com/oldnews/?appgroupname=squad&appids=393380&feed=steam_community_announcements&l=english
- A CC0 environment collection was checked for selected possible next-pass building/ground assets: https://github.com/petroulacl/fps-buildings-env-kit . The Kenney Modular Buildings pack's `License.txt` explicitly identifies CC0. The collection README also lists ambientCG/Quaternius/Kenney/Poly Haven assets as CC0. Poly Haven license: https://polyhaven.com/license . No new models from this collection have been integrated yet as of this note.
- The user must not be given the old browser build as the redesigned build. The first live WebGL deployment at `ec062e4...` still contained the old Greenridge gameplay scene. New design is in source at `f751b25...` and awaits new CI.

## Compile failure diagnosis, HUD repair and first PBR surface pass — 2026-11-10

### CI failure root cause

- WebGL run `42850646475` and Android run `42850646495`, both from source commit `f751b25c6d42fff2787cbfa3585d7b3816761cc9`, failed during Unity C# script compilation, before player packaging or deployment.
- Exact compiler error in both logs: `Assets/TacticalStrike/Scripts/Core/TacticalStrikeBootstrap.cs(38,20): error CS1061: 'FpsPlayerController' does not contain a definition for 'AttachHud'`.
- The new `FpsPlayerController` implementation had removed the `AttachHud(MobileHud)` method that the bootstrap still calls immediately after creating the HUD. This was an integration omission in the new player-controller rewrite, not a Unity/WebGL platform issue.

### HUD repair and PBR asset pass

- Commit `e4475302dc9c198938d916f0822939f21ce8a96b` restored the `MobileHud` field and `AttachHud(MobileHud)` method in `FpsPlayerController`. Stamina and sensitivity messages now use the attached HUD reference. The same commit adds a prepared CC0 PBR surface asset pass:
  - Downloads pinned ambientCG Grass004, Ground037, Bricks066, Concrete012 and Road001 source sets from the CC0 collection revision `1dd8e932402deb82422920df2e7fd079391e55d3`.
  - Downloads diffuse Color and OpenGL Normal maps and resizes them to 1K via FFmpeg for mobile/WebGL memory control.
  - Copies the maps into Unity Resources and validates their presence before building.
  - Records source/rights information in `NORTHWIND_PBR_SOURCES.txt`.
- Commit `8c4e6516abd2585a0084788359e2e3bc11382f73` connects the new surface maps to the active `NorthwindValleyGenerator` materials: grass terrain, dirt/earth, disturbed trench floors, weathered brick walls, concrete and roads. Base-colour and normal-map tiling are matched; roughness has not been incorrectly assigned to the metallic channel.

### Current verification state

- Latest source head is `8c4e6516abd2585a0084788359e2e3bc11382f73`.
- Latest WebGL workflow run `42852093866`: in_progress at last inspection; job steps: Set up job:completed, Free runner disk space for Unity Web build:in_progress, Checkout repository:pending, Check Unity Personal license secrets:pending, Restore prepared runtime assets cache:pending, Verify restored runtime asset cache:pending, Install asset conversion tools:pending, Prepare pinned runtime assets:pending, Verify prepared runtime assets:pending, Save prepared runtime assets cache:pending, Restore Unity WebGL Library cache:pending, Build WebGL preview with Unity 6.3 LTS:pending, Save Unity WebGL Library cache:pending, Fix ownership of WebGL output:pending, Validate WebGL files and add preview manifest:pending, Configure GitHub Pages:pending, Upload WebGL preview as Pages artifact:pending, Deploy browser preview:pending. URL: https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42852093866
- Paired Android workflow run `42852093702` was also started for the same source commit. Do not declare compile success, publish a new-preview link as ready, or assume the external texture download paths have passed until both builds and especially WebGL validation/deployment complete.
- Superseded run IDs `42851997309` and `42851997450` were cancelled when the newer PBR commit was pushed; these were interrupted by `cancel-in-progress`, not additional C# errors.
- The old deployed GitHub Pages preview still corresponds to the old Greenridge scene until run `42852093866` successfully deploys the new `main` source.

## First Northwind Valley browser deployment and crew-served weapon integration — 2026-11-10

### First playable-map build successfully deployed

- WebGL run `42852093866` completed with `conclusion: success` for source commit `8c4e6516abd2585a0084788359e2e3bc11382f73`:
  https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42852093866
- Verified success through `Build WebGL preview`, `Validate WebGL files and add preview manifest`, `Configure GitHub Pages`, `Upload WebGL preview as Pages artifact`, and `Deploy browser preview`.
- This is the first successful browser deployment whose Unity bootstrap invokes `NorthwindValleyGenerator.Build(worldRoot)` and whose new valley PBR surfaces are loaded by the active map generator. It is the first preview build of the actual new Northwind source, not the previous Greenridge version.
- The deployed page is a Unity WebGL runtime. Deployment success confirms publish; it does not replace a visual/browser interaction test. The user must still review actual scene appearance, scale, collision, building interiors, movement, sight picture, tracer and browser performance.

### Android build succeeded for the new Northwind map source

- Android workflow run `42852093702` completed with `conclusion: success` on the same commit `8c4e6516abd2585a0084788359e2e3bc11382f73`:
  https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42852093702
- Verified `Build Android APK`, APK archive integrity, APK signature, manifest/dex and `lib/arm64-v8a/libil2cpp.so` + `libunity.so`. Artifact `TacticalStrike-Unity-Android-verified` was uploaded (about 33.7 MB artifact size).
- Android build success is not a physical-phone gameplay/performance test; still needs device review.

### New crew-served weapon implementation on main

- Commit `30cc3a967330c362a5cde873cbc59beeded47a8d` adds `UnityProject/Assets/TacticalStrike/Scripts/Combat/MountedWeaponEmplacement.cs` and wires the HMG nests in `NorthwindValleyGenerator` to this component.
- Both named positions (`DShK 12.7mm` and `M2 Browning .50`) now spawn an active mounted-emplacement component in the runtime world. One operator can approach within 3.8m and mount with `E` on desktop/WebGL or the contextual `MOUNT` touch button on Android. `E`/`DISMOUNT` returns the operator to a safe dismount point.
- The weapon assembly is attached to a traverse/elevation pivot. Traverse is clamped to ±66°, elevation +24°, depression -18°. The camera aligns to the pivot while mounted, the personal AK viewmodel is hidden, and player movement is disabled while operating the mount.
- The HMG fires original 12.7mm ballistic projectiles at 890m/s with 128 damage, 0.94 penetration factor, 1250m max range, 9.2 rounds/s cyclic rate, and a tracer presentation every third shot. Each mount has a 250-round belt plus 750 reserve, and a 3.35-second belt-change reload using `R` or the touch reload button.
- `FpsPlayerController` routes look/fire/reload and mounting to the crew-served station while occupied. `MobileHud` shows the contextual mount/dismount button and mounted belt/reserve ammunition, and keeps AK ammunition display separate after dismount. `WeaponController` exposes `SetViewModelVisible` for the operator transition.
- The current DShK/M2 receiver/barrel/sight geometry is still assembled from runtime primitive meshes. The firing/traverse/reload loop is functional in source; it is **not yet a high-detail authentic DShK/M2 model**, and the mounts still need Unity CI and actual gameplay validation. Do not describe them as art-complete.

### Ground detail and current validation

- `NorthwindValleyGenerator` now creates conforming PBR earth patches at the south staging area, northern reserve, defensive compounds, farm/village yards and HMG positions, using the imported Ground037 surface map. These sit on the actual heightfield and are separate from the trench-floor geometry.
- Source commit `30cc3a967330c362a5cde873cbc59beeded47a8d` triggers the follow-up validation runs: WebGL `42855335693` and Android `42855335569`. Both were in progress at the last check; don't claim the mounted-weapon changes compile until these runs complete.
- The previously successful WebGL preview run `42852093866` is the currently deployed Northwind baseline before the just-added HMG controls. The newer HMG commit is not deployed until run `42855335693` passes WebGL build/validation/deployment.

## HMG compile repair, layered Unity Terrain and near-miss suppression — 2026-11-10

### Verified compile failure and repair

- Both the WebGL run `42855335693` and Android run `42855335569` from source commit `30cc3a967330c362a5cde873cbc59beeded47a8d` failed during Unity script compilation.
- The only compiler error reported in both logs was `NorthwindValleyGenerator.cs(693,17): error CS0103: The name 'darkMetal' does not exist in the current context`. The actual material field created by the map is `metalMaterial`.
- The current map generator corrects the HMG pedestal material reference to `metalMaterial`. The warning about obsolete `Object.FindObjectsOfType<T>()` in the mount component is a warning, not the compile failure.

### Native layered terrain and PBR blend

- Commit `1adc16e0b9ec84a62fb560dbab44595e4fec092f` replaces the old single-material 384-division terrain mesh with a native Unity `TerrainData` heightfield and `TerrainCollider`.
- Heightmap resolution is 257, alphamap resolution 256, world area 700m × 700m and vertical range 120m. The height field retains the central valley, flank ridges, saddles and drainage/trench cuts.
- Three `TerrainLayer`s are blended using slope, elevation and broad Perlin variation: Grass004 on gentler grassland, Ground037 in exposed/lowland earth, and Poly Haven rocky gravel on steep ridge faces. Splat weights are normalized per alphamap texel.
- The runtime asset preparation now copies and verifies `rocky_gravel_diff_1k.jpg`, `rocky_gravel_nor_gl_1k.jpg`, and its roughness map. Unity Editor build validation explicitly requires the diffuse and normal textures used by the terrain layers.
- Roads, trench floors/parapets, drainage, houses, woodland, dugouts, firing bays, ground patches and HMG nests have been adjusted to sit above the new terrain heightfield. Channel water follows the carved drainage network.
- This is actual terrain splat blending, not only separately coloured boxes. Shader appearance, transition sharpness, vegetation and device performance still require runtime inspection/tuning.

### Near-miss suppression system added

- `BallisticProjectile` now checks a small capsule around each simulated trajectory segment with `Physics.OverlapCapsuleNonAlloc`, then checks line of sight so terrain/buildings block suppression.
- Nearby non-shooter infantry receive a suppression factor based on distance and projectile damage. The factor decays over time and uses a short repeat-hit cooldown.
- `FpsPlayerController` passes suppression to `WeaponController` and the HUD. Higher suppression increases weapon sway and shot spread, and blocks stationary focus/breath-hold temporarily.
- The HUD uses a low-alpha red edge vignette; it does not place a blur over the centre of the sight picture.
- This is a first-pass infantry suppression mechanism, not a full Squad equivalent: no AI morale/suppression state, squad voice effects, suppression audio layer or multiplayer authority synchronization is claimed.

### Current validation runs for this source

- New WebGL run `42857326872` (commit `1adc16e0b9ec84a62fb560dbab44595e4fec092f`): https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42857326872 .
- New Android run `42857326877` (same commit): https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions/runs/42857326877 .
- Both were accepted by GitHub and were `in_progress` at documentation time. Do not claim layered Terrain or suppression has compiled, deployed or been visually tested until both relevant Unity builds complete; WebGL must also validate, upload the Pages artifact and deploy.
- The currently deployed site still reflects the last successful Northwind baseline `8c4e6516abd2585a0084788359e2e3bc11382f73`; it does not yet include the mount controls, layered Terrain or suppression from this latest commit. The older preview must not be represented as containing these additions.

## Build review and TerrainLayer constructor correction — 2026-11-10

- User asked for a direct review of the build after repeated failures. Reviewed latest GitHub Actions runs `42857326877` / `42857326872` (source `1adc16e...`) and `42859537616` / `42859537696` (source `e8f6424...`) for Android and WebGL.
- In `1adc16e...`, Unity reported a large cascade of C# syntax diagnostics starting at `NorthwindValleyGenerator.cs(389,9)`. Root cause: after replacing the terrain mesh method with native `UnityEngine.Terrain`, the full orphaned old mesh method body remained outside a method. There was also a wrong `darkMetal` material identifier.
- Commit `e8f64242717a3c65a97e43590af28e5027f7364b` removed that orphaned legacy mesh block, removed remaining `TerrainDivisions` / `terrainMaterial` references, and changed the HMG pedestal material reference to the declared `metalMaterial`. A source check confirmed balanced braces and removal of those old identifiers.
- The next builds on `e8f6424...` exposed one remaining API misuse: `ScriptableObject.CreateInstance<TerrainLayer>()` is invalid because Unity 6 `TerrainLayer` inherits from `UnityEngine.Object`, not `ScriptableObject`. The official Unity 6.0 reference and current Unity C# reference confirm the inheritance/constructor (`new TerrainLayer()`): https://docs.unity3d.com/kr/6000.0/ScriptReference/TerrainLayer.html and https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/TerrainEditor/Utilities/TerrainLayerUtility.cs
- Commit `86d5729731a837a9385b7928bcade35a61d4d07a` replaced the invalid generic factory with `new TerrainLayer()`.
- New WebGL run `42863168322` and Android run `42863168319` target exactly commit `86d5729...`; both were queued at the time this context note was prepared. Do not say build is fully fixed or successful until the two workflows finish with success. The earlier successful Northwind WebGL/Android build `8c4e651...` did not include the later HMG or native splat-terrain additions.



## Repository context and CI error audit — 2026-11-10

- User reported a new build error and asked for the project context to be reviewed. This audit used the actual GitHub Actions job summaries and decoded job logs, not assumptions based on the UI or a previous message.
- Confirmed error progression:
  1. Commit `30cc3a9...`: `NorthwindValleyGenerator.cs(693,17)` referenced `darkMetal`, which was not declared. The active material field is `metalMaterial`.
  2. Commit `1adc16e...`: the old mesh-terrain method body had been left orphaned after the native Terrain rewrite. Unity emitted a cascade beginning near line 389 (`CS1519`, `CS8803`, `CS0106`, `CS1022`). These were syntax fallout from the orphaned block, not dozens of independent defects.
  3. Commit `e8f6424...`: the orphaned block and wrong material identifier were removed, but Unity then reported `CS0311` because `TerrainLayer` cannot be instantiated using `ScriptableObject.CreateInstance<T>()`.
  4. Commit `86d5729...`: changed the constructor to `new TerrainLayer()`, which is the Unity API form used by the active native terrain layer creation method.
- The obsolete `Object.FindObjectsOfType<T>()` notice in `MountedWeaponEmplacement.cs` is a deprecation warning; it was not the cause of those failed builds.
- Current source head under test: `86d5729731a837a9385b7928bcade35a61d4d07a`. WebGL run `42863168322` and Android run `42863168319` both restored their required caches and progressed into the Unity build step at the time of this audit. Neither was yet complete, so this entry does **not** claim the latest fix passed compilation.
- Until WebGL run `42863168322` completes Unity build, payload validation and Pages deployment, the public preview remains on the last successful published build, commit `8c4e6516abd2585a0084788359e2e3bc11382f73`. The HMG interaction and the newer native layered Terrain/suppression changes are not assumed to be present in that deployed preview.
- The context header was corrected in commit `75c7873a4dc0476e47abc053b29765e035b905d6`: the active project is Unity 6000.3.28f1 / URP 17.3.0. The dated Godot 4.7.2 notes remain as historical records only.
- Next verification: confirm both latest CI workflows finish successfully; for WebGL also confirm manifest/payload checks and GitHub Pages deployment. Then verify the newly deployed page and inspect actual in-browser visuals and controls. A successful Unity build alone does not complete visual/gameplay acceptance.


## Successful deployment and actual feature audit — 2026-11-10

### Verified CI results for source commit `86d5729731a837a9385b7928bcade35a61d4d07a`

- Unity Browser Preview run `42863168322` (run #17) completed with `conclusion: success`.
  - Unity WebGL build passed.
  - WebGL payload/manifest validation passed.
  - GitHub Pages artifact upload passed (artifact ID `13787885242`, 28,751,620 bytes).
  - GitHub Pages deployment passed; the deploy log reports success for Pages build version `86d5729731a837a9385b7928bcade35a61d4d07a`.
  - Browser preview URL: https://cybersecurityrahbar.github.io/Tactical-Strike-Android/
- Unity Android Build run `42863168319` (run #34) completed with `conclusion: success`.
  - Unity Android/ARM64 IL2CPP build passed.
  - APK archive integrity, signature, manifest/dex and ARM64 Unity/IL2CPP libraries all passed validation.
  - Verified artifact `TacticalStrike-Unity-Android-verified` uploaded (artifact ID `13786947346`, 35,437,635 bytes).
- These results prove the two target builds and WebGL deployment for the specified source commit. They do not prove physical Android device performance, browser-side visual quality or feature parity with Squad.
- Documentation-only context commits after `86d5729...` do not change the deployed game source because the Unity workflows trigger on UnityProject, tools and their workflow files.

### What the browser preview actually contains

- `TacticalStrikeBootstrap` calls `NorthwindValleyGenerator.Build(worldRoot)`; it no longer calls the old Greenridge `BattlefieldGenerator`.
- The active map is a runtime-generated, collidable 700m × 700m Northwind Valley world. It uses a native Unity Terrain heightfield/Collider (heightmap resolution 257) and three terrain layers (grass, exposed earth and rocky gravel) blended over a 256 alphamap.
- Runtime world composition includes the southern attacker staging sector; central uneven valley and flank relief; separate branching attacker and defender trench paths with floors/parapets/traverse cover; four house/compound placements (including the two northern defensive compounds plus farm/village houses); open door and window apertures with basic interior floors/partitions; reserve/assembly dugouts; service roads; drainage/stream beds; village/farm sheds; two protected heavy-machine-gun positions; woodland belts, ridge rocks and field props.
- This is the new playable map foundation and is materially different from the Greenridge prototype. However, much of the architecture/fortification/prop geometry is still assembled procedurally from primitive meshes and is not a final hand-authored, asset-rich Squad-quality environment. Terrain/material blending is a first pass, not a final art pass.

### Actual weapon and player feature boundary

- Fully integrated first-person handheld weapon: **AK-74M only**, with its current imported model/arms/viewmodel implementation. The current `WeaponController` loads AK-74M resources; there is no completed interchangeable inventory or `WeaponDefinition` data architecture for the roster.
- **DShK 12.7mm and M2 Browning .50** are present as separate mounted emplacement positions with first-pass mounting/dismounting, traverse/elevation, firing, ballistic projectile, belt ammunition and reload logic. Their current geometry is procedural, not final authentic high-detail meshes/rigs/sounds/animations. Do not count them as completed portable player weapons.
- **M4A1, PKM, RPG-7 and SVD/Dragunov are not yet integrated playable weapons.** They remain requirements.
- Current player controller includes walk, sprint, crouch, prone, jump, stamina, aiming/focus, camera movement and first-pass weapon sway/recoil/spread. Lean, vaulting and complete Squad-style movement/animation layering are not finished.
- Projectile logic includes ballistic travel, emissive tracer presentation and a first-pass near-miss suppression effect. It is not yet a complete per-calibre/per-surface Squad-equivalent ballistics, destruction and audio suite.
- Tactical enemy squads/AI behaviour, objective/match logic, vehicles, command/communications, full multiplayer and Wi-Fi/hotspot networking, complete layered positional battle audio and full mobile controls/performance acceptance are not implemented/verified.

### User-facing truth statement and acceptance boundary

- **Yes:** the browser preview now deploys the actual Northwind Valley map source, not the previous Greenridge map. It includes the first-pass map geometry and the above AK-74M / mounted-heavy-gun features.
- **No:** this preview is not a complete game with the full seven-weapon roster or the full movement, squad AI, audio, vehicles, networking and overall quality/scope of Squad. It must not be advertised as “like Squad in everything” or as finished.
- The source and CI deployment have been verified, but no automated screenshot/visual browser acceptance was produced. The user must inspect the rendered map, weapon alignment, mounted gun use, sight picture, tracers, collision, interiors and performance in the browser; record screenshots/video findings and fix them before calling this milestone visually accepted.
- For a clean check after deployment, open the preview and perform a hard refresh (Ctrl+F5) to avoid a stale cached WebGL payload.


## User runtime failure review and first corrective pass — 2026-11-10

The user tested the deployed Northwind preview and reported four blocking runtime defects: (1) the player continuously falls through the map, (2) the AK-74M is not visible/held in the first-person view, (3) the shot sound is unacceptable, and (4) tracer rounds are pink, flat/rectangular and unlike a glowing projectile. These are gameplay blockers, not cosmetic feedback.

### Confirmed source-level causes and corrections

- Grounding: the player spawned only 0.12m above sampled terrain and depended solely on CharacterController contact with a runtime TerrainCollider; there was no recovery path if contact was missed. The current pass explicitly assigns/enables the TerrainCollider, adds colliders to conforming ground patches, synchronizes transforms before spawning, raises spawn clearance, enables CharacterController overlap recovery and adds a Terrain.SampleHeight emergency ground correction.
- Viewmodel: the imported model received an additional 180-degree yaw after the glTF-to-FBX conversion had already used Unity's forward-axis convention. This turned the rifle away from the first-person sight line. That extra yaw is removed; the viewmodel also has corrected fit bounds, enabled renderers/layer inheritance and an updated camera near clip.
- Firearm sound: the asset pipeline downloaded AK-47 long-burst recordings C_27P/C_36P, cropped them to 0.42 seconds and converted stereo to mono before treating them as individual shots. This was incorrect. The new pipeline selects actual single-shot near/mid recordings C_28P/C_31P, preserves stereo and natural report decay, and prepares AR-15/M4 single-shot samples D_32P/D_24P for later. The AK-47 7.62x39 recordings are documented as the closest available CC0 reference, not exact AK-74M 5.45 recordings.
- Tracer: the prior presentation was a 3.8cm flat LineRenderer plus a 6.5cm sphere. It is replaced by a rounded incandescent head and narrow circular 3D tapered mesh trail. The material uses yellow/amber emission, camera HDR and restrained global Bloom. Tracer diameter is read from each projectile profile, so 5.45mm and 12.7mm rounds have different visual scales.

### Calibre-specific profiles added in source

- AK-74M: 5.45×39 7N6 PS nominal 3.42g, 5.45mm, 890m/s; 7T3 tracer nominal 3.20g/883m/s; separate 7N22-style AP profile.
- DShK: 12.7×108 nominal 48.3g/850m/s reference profile.
- M2: 12.7×99 M33 nominal 42.87g/887m/s reference profile.
- The quadratic drag factor is computed from relative frontal area and projectile mass against a 5.45mm baseline. It is explicitly a game-tuned approximation, not a published G1/G7 ballistic coefficient. Damage, penetration and ranges remain gameplay tuning values.

### Verification boundary

- These source changes and asset-preparation changes are committed together so WebGL and Android consume the same runtime content. Prepared-asset cache keys must invalidate because the preparation scripts changed.
- Compilation/build success is not confirmed until the next Unity Android and WebGL runs complete. WebGL must also finish payload validation and Pages deployment.
- No visual success is claimed until a new preview is tested: the player must remain on the ground, AK/arms must be visible and aligned, the shot must sound natural, and tracers must be yellow/orange and round rather than pink/rectangular.
- The sound pipeline now fixes the incorrect source recordings/truncation/channel conversion, but only the AK first-person report is wired in this pass. Complete weapon-specific audio for all seven guns and authentic 12.7mm DShK/M2 recordings remain outstanding.
- Full seven-weapon inventory, final weapon rig/animation alignment, tactical AI, objectives, vehicles, networking, advanced audio and physical Android performance acceptance remain outstanding.


## Follow-up integration correction — 2026-11-10

- A post-commit diff review found that the initial corrective tree had a partial `WeaponController` version. A follow-up source update now wires the AK single-shot near/mid audio clips, the 7N6 / 7T3 tracer / 7N22 AP profiles, and the corrected first-person yaw in both the initial pose and the per-frame sway target. The mounted DShK/M2 components use their separate 12.7×108 and 12.7×99 profiles.
- Ground recovery sampling was tightened to avoid ambiguity from a non-zero Terrain transform: it now samples `TerrainData.GetInterpolatedHeight` in Terrain-local coordinates and transforms that point back into world space. This matters because the Northwind Terrain origin has a vertical offset of -25m.
- The current AK sound uses a genuine single-shot near recording with a restrained mid-distance recording layer. The source recordings are AK-47 7.62×39 CC0 reference audio, not exact AK-74M 5.45×39 recordings; this limitation remains explicit. DShK/M2 still need their own licensed 12.7mm recordings.
- The initial source/asset corrective change and this follow-up integration/ground-sampling change are separate main-branch commits. Rebuild validation must be read from the latest run for the current main commit; a prior completed run must not be used to claim this final source is built.
- Final acceptance remains pending new WebGL/Android CI completion plus user browser testing of ground contact, visible hand/weapon pose, sound character, tracer geometry and tracer colour.


## Build failure audit and tactical control-system replacement — 2026-11-10

### Confirmed root cause of latest failed Unity builds

- Android run #37 (`42878716029`) and WebGL run #20 (`42878716026`), both for source commit `bb79f3b8d2a36143ea67ef5eb75d3cafcafd55a8`, failed at C# compilation in `NorthwindValleyGenerator.cs` lines 381-383.
- The compiler emitted `CS1069`: `TerrainCollider` was forwarded to `UnityEngine.TerrainPhysicsModule`, but this built-in module was not enabled in `UnityProject/Packages/manifest.json`.
- The missing built-in requirement is `com.unity.modules.terrainphysics`; the terrain runtime is also explicitly included as `com.unity.modules.terrain`. Unity documentation identifies Terrain Physics as the module implementing `TerrainCollider`. This is a project package-manifest defect, not a Unity licence or runner-storage issue.
- Since Unity compilation failed, Android APK verification and WebGL payload/Pages deployment were skipped. The public preview remains on the previous successful deployment and does not include the latest grounding, AK viewmodel/audio/tracer corrections.

### Player controls — user-requested redesign

- User explicitly rejected the primitive touch-control layout and requested a tactical-FPS scheme comparable in control feel to Squad.
- Replaced the mobile layout with a dual-thumb arrangement:
  - Left thumb: dedicated movement joystick.
  - Right thumb: broad look surface.
  - Independent hold actions: fire, ADS, left lean and right lean.
  - Tap actions: jump, crouch toggle, prone toggle, reload, ammunition mode and contextual mount/dismount.
  - Sprint is a toggle, so steering can remain on the left thumb.
- Touch joystick and look surface now track active pointer IDs. This allows movement, camera look and additional combat/stance actions on multiple fingers without one finger-up event cancelling another finger's input.
- Desktop/browser controls are aligned to the tactical layout: WASD movement, mouse look, left mouse held fire, right mouse held ADS, Shift sprint, C crouch toggle, Z prone toggle, Space jump, Q/E lean, R reload, T ammunition and F interact/mount/dismount.
- Lean is smoothed and shifts the first-person camera laterally with a restrained roll. This is a first-person input/presentation pass, not yet a complete collision-aware body lean or final character animation rig.
- These changes do not claim finished vaulting, an interchangeable weapon inventory, controller remapping, full-body stance/lean animations or final Android ergonomics.

### Source edits and validation boundary

- Package manifest now includes Terrain and Terrain Physics built-in modules.
- `FpsPlayerController.cs` has toggle crouch, smoothed lean, hold-to-ADS and the revised keyboard bindings.
- `MobileHud.cs` now builds the dual-thumb control scheme; `TouchJoystick.cs` and `TouchLookArea.cs` isolate multitouch pointer ownership.
- The fix is committed directly to `main`. It is not considered successful until new Unity Android and WebGL runs compile; WebGL must also pass manifest validation and GitHub Pages deployment. No physical-device acceptance or browser screenshot is claimed yet.
- QA gate after CI: verify actual deployed SHA; player-ground collision; simultaneous left-stick movement/right-look/fire/ADS; sprint, crouch, prone and lean; mounted DShK/M2; AK viewmodel; shot sound; tracer appearance and frame rate.


### CI tracking for tactical-controls commit — 2026-11-10

- Source commit under validation: `bde1ff61427d22294f1f904b6b95fe7eea5cd005`.
- Unity Browser Preview run #21: `42886394093` — still `in_progress` at the latest poll; Unity WebGL build step active. No validation manifest, Pages artifact or deployment completion was yet visible.
- Unity Android Build run #38: `42886393910` — still `in_progress` at the latest poll; cache restoration active before Unity Android build. No APK validation or artifact upload completion was yet visible.
- No current-source CI failure had been reported in those two runs at the time of this entry, but neither run was complete. Do not interpret the active status as a successful build.
- The working preview URL remains the previous deployment until WebGL run #21 completes its build, output validation and Pages deployment. The Android APK artifact is not available until run #38 completes APK verification and uploads the artifact.


## WebGL runtime asset validation failure and repair — 2026-11-10

### Exact failure from browser preview run #21

- WebGL run `42886394093` for source `bde1ff61427d22294f1f904b6b95fe7eea5cd005` successfully restored/prepared runtime assets and compiled the Unity C# assemblies. There were no C# compilation errors in the log; the obsolete FindObjectsOfType notice was only a warning.
- The Unity build method then aborted in `TacticalStrikeBuild.ValidateRuntimeAssets()` at line 283 with:
  `BuildFailedException: Required prepared runtime asset is missing: Assets/Resources/TacticalStrike/Imported/Audio/Firearms/ak_C_27P.ogg`
- Root cause: the asset-preparation pipeline and `WeaponController` had already moved from legacy `ak_C_27P.ogg` / `ak_C_36P.ogg` burst clips to current files `ak_report_near.ogg` and `ak_report_mid.ogg` (plus prepared M4 near/mid samples). However, the Unity editor build gate still checked for the two obsolete filenames. This was a stale validator contract, not a missing cache, download, compiler or terrain-physics error.
- The current asset preparation script was already copying and checking the new file names correctly. The actual AK runtime Resources.Load paths also matched `ak_report_near` and `ak_report_mid`.

### Repair committed to main

- Commit `30306af3b80817e65c279b35ff3b6c144d4b1450` updates `UnityProject/Assets/Editor/TacticalStrikeBuild.cs`:
  - Removes the obsolete C_27P/C_36P validation entries.
  - Requires `ak_report_near.ogg`, `ak_report_mid.ogg`, `m4_report_near.ogg`, `m4_report_mid.ogg`, and `FIREARM_AUDIO_SOURCES.txt`.
  - Performs Unity `AssetDatabase.LoadAssetAtPath<AudioClip>` checks for all four report clips, in addition to checking that the files exist. This catches both missing files and importer failures before player build.
  - Updates the validation success message to report the actual current audio pipeline.
- The Android workflow #38 for the previous source was cancelled by the new push before it could independently finish; its result must not be treated as a passing APK.
- New validation runs for commit `30306af...`:
  - WebGL run #22: `42888188822` (queued at documentation time).
  - Android run #39: `42888188720` (pending at documentation time).
- Do not claim success or ask the user to test the browser preview until WebGL run #22 completes Unity build, output validation, Pages artifact upload and deployment. Android run #39 must also pass APK validation before its artifact can be called ready.


## User-reported browser preview visual failure and battlefield rebuild correction — 2026-11-10

### Visual evidence and acceptance failure

- User supplied a browser screenshot of the deployed WebGL preview. It shows a broken-looking first-person scene: detached/floating black geometry in the distance, a very large upright dark slab near the centre, broad flat green ground, a separate oversized dark road slab on the right, disconnected/uneven path and trench surfaces, and no acceptable first-person weapon/arms presentation in the visible view.
- User also reports the AK/hands look wrong, tracers appear as long laser beams, impacts lack convincing sparks, dust and ground scars, and the map is not playable/finished. This evidence overrides the earlier assumption that a successful CI deployment made the preview ready for testing.
- The top-down SVG is only a layout diagram. It is not the rendered 3D battlefield, does not encode surface geometry or material quality and must never be described as proof that the actual game already matches the diagram. The previous communication did not distinguish this clearly enough; that was misleading.

### Confirmed source-level causes

- URP was configured with render scale 0.90, MSAA 2x, HDR disabled, only 85m shadow distance, one pixel light and LOD bias 1.25. The runtime environment also used a bright olive/yellow-green procedural sky/ambient/fog tint. This was a low preview-quality preset, not an appropriate visual baseline for the 700m tactical map.
- The native Terrain heightmap used 257×257 samples over 700m (about 2.73m between samples). Roads were assembled from independent flat cube segments with roughly 7m spacing and only horizontal rotation; this allowed seams, flat slabs, terrain intersections and large gaps on slopes.
- Trench floors were separate thin cube plates at roughly 2.8m intervals. Parapets and traverses were also isolated cube segments rotated horizontally, not aligned to the local terrain slope. This construction could not produce a continuous, terrain-conforming trench system.
- Nature models were scaled by assumed factors without normalizing their imported bounds or grounding their lowest renderer bounds to the heightfield. Unexpected FBX scale/pivot could therefore cause oversized or floating tree/shrub instances.
- The tracer tail formula was `velocity.magnitude * frameDelta * 0.42`, clamped up to 8.5m. At rifle muzzle speeds this creates a long laser-like ribbon. The previous size did not read as a small moving incandescent projectile.
- Impact logic created sparks even for dirt/grass, used a slightly upward dust gravity modifier, and had no visible temporary mark/soil scar on terrain. It therefore could not produce differentiated soil vs metal/masonry strike behaviour.
- The AK-74M first-person rig was fit to a large 0.82m dimension and placed very close to the camera. The imported model/rig still needs actual visual validation and may require replacement; a build passing does not establish correct hand grip, animation, weapon proportions or sight alignment.

### Corrective source pass prepared for validation

- URP profile is raised to full render scale, HDR enabled, 4x MSAA, four shadow cascades, a 2048 main-light shadow map, 220m sun-shadow range, depth texture, and a higher LOD/aniso baseline. Runtime QualitySettings now match the intended higher-quality battlefield profile.
- Runtime lighting/fog/sky colours are neutralized toward subdued daylight instead of the previous strong olive/yellow-green cast. ACES tonemapping and a restrained bloom/contrast grade are configured to preserve highlights and tracer readability.
- Terrain heightmap and alphamap are raised to 513×513 / 512. The heightmap sample spacing is now about 1.37m. Terrain LOD/basemap/tree distances are increased and heightmap pixel error lowered.
- Roads are now intended to be Catmull-Rom smoothed mesh ribbons with both edges sampled from `HeightAt`, not independent 7m cube slabs. Each road has a mesh collider. Trench floors use continuous terrain-conforming ribbons; parapet/rear-revetment blocks sample both ends and rotate to the local 3D slope.
- Imported vegetation instances normalize their renderer-bounds height to a controlled target range and shift their lowest renderer bound down to the terrain surface. This only corrects imported scale/pivot placement; it does not replace poor source models with professional military environment assets.
- Tracer core remains calibre-scaled but the luminous wake is shortened to 0.09–0.22m. This is intended to remove the long laser appearance; visual acceptance is still pending.
- Impact sparks are now restricted to metal/hard masonry. Dust falls under positive gravity on non-metal impacts. Dirt/grass hits receive a temporary dark compacted mark with an irregular raised soil lip for 32 seconds. This is a surface-mark mesh, not real destructive TerrainData sculpting.
- AK-74M ready pose and fit scale are reduced/repositioned. This is an alignment correction, not a completed full-body first-person rig replacement or proof that the weapon now looks like Squad.

### Scope and truth boundary

- These are corrective technical changes to terrain, roads, trenches, lighting, tracer shape and first-impact presentation. They do not turn the current greybox/procedural map into Squad-quality art assets by themselves.
- Many compounds, walls, parapets, firing bays and props are still generated from primitive geometry/materials. The current environment still needs a separate genuine art-production phase: detailed terrain/road materials, authored trench profiles/revetment assets, realistic foliage/rocks, architectural material sets, vegetation dressing and proper composition.
- The AK first-person rig/hands still require visual inspection against the screenshot/device; full weapon inventory, advanced animations and weapon-specific sounds for all guns remain outside this corrective pass.
- CI success only proves Unity compilation, packaging and WebGL deployment. It does not prove that the resulting scene renders correctly. Do not ask the user to test until new WebGL and Android jobs pass, the exact WebGL SHA is deployed, and a new runtime screenshot is inspected.
- Acceptance criteria for the next preview: no floating/detached geometry; continuous terrain-matched roads and trench floors; parapets seated on slopes; grounded foliage at reasonable scale; visible correctly sized AK/arms at lower right; short visible incandescent tracer, not a beam; surface-appropriate dust/sparks and visible ground marks; stable browser frame rate. A screenshot/video is still needed to verify these points.


### Spawn sightline correction — 2026-11-10

- Follow-up inspection of the active spawn/map coordinates found the attacker covered assembly dugout was placed at `(-8, z=266)`, only about 38m in front of the player spawn at `(0, z=304)` and nearly on the forward viewing axis. This could dominate the opening view and resemble the central upright obstruction in the submitted screenshot.
- Moved this covered assembly shelter laterally to `(-132, z=266)`, leaving the central forward approach clearer. The location still uses the generated terrain height.
- Source commit: `d87549ef25c3b972cde1bea96b0b3b1763538c32`.
- New Unity Browser Preview run #24 `42897243040` and Android Build run #41 `42897243202` were queued for that exact source commit. Do not claim visual correctness until WebGL is deployed and inspected.


### URP cache reapplication guard — 2026-11-10

- A follow-up build-script audit found that the newly raised URP properties were set only inside the `pipeline == null` asset-creation branch. If Unity restored a previously generated URP asset from its Library cache, the existing asset could retain the old HDR-off / 0.90 render scale / 85m shadow profile.
- The URP quality assignments are now applied unconditionally after loading or creating the pipeline asset. This makes the configured profile deterministic across a fresh import and a cache restore.
- Source commit `e937e17fb43bc72431083416940fa1f8d01060f1`; this is a build-configuration correctness fix. Current build validation must still complete for the exact latest main commit.


### HUD bounds, tracer envelope and impact-mesh follow-up — 2026-11-10

- Screenshot review found the top-left `Mission status` panel used inverted vertical RectTransform offsets (`offsetMin.y=-22`, `offsetMax.y=-148`) for a top-left fixed anchor, yielding a negative-height rectangle. The offsets are now ordered as `-148` to `-22`, restoring a positive 126px panel height.
- The initial shortened tracer wake was paired with a 5.45mm-scale luminous head that would subtend substantially less than a pixel at normal engagement distances. The emissive envelope is now scaled to a visible 2.8–6cm profile by calibre, with a rounded core and shorter tapered wake. This is a deliberate visual-light envelope larger than the physical bullet diameter, not a claim that the projectile itself physically grows.
- Ground impact scars now sit closer to the contacted surface and schedule their transient runtime meshes for destruction together with the 32-second mark, avoiding unbounded native mesh accumulation during sustained fire.
- These are source changes awaiting CI; no visual acceptance is claimed until the deployed scene is inspected.


### Nature foliage material/alpha import correction — 2026-11-10

- A further screenshot-focused audit found `NatureModelPostprocessor` changed imported tree/shrub/grass materials to URP/Lit but preserved only base color. It did not transfer the imported albedo or normal texture and did not retain the alpha-test render state.
- For leaf/grass card meshes, this can render the complete rectangular card as an opaque dark polygon instead of cutting out transparent pixels. This is a credible source-level explanation for parts of the floating black geometry along the horizon; the screenshot alone cannot prove every polygon comes from this path.
- The postprocessor now captures and restores albedo/normal maps when switching to URP/Lit, restores metallic/smoothness, enables AlphaClip with cutoff 0.42, uses two-sided culling for foliage cards, sets the alpha-test render queue and instancing.
- A dedicated Nature texture importer now keeps source alpha, mipmaps, compression and a 2K texture ceiling for imported nature textures.
- Source commit: `b56212a5a26ad48a5532cbbc33a7bc2efaa7b193`. WebGL/Android validation for this exact source remains pending. If the polygons persist after the new build, the next investigation is the imported model hierarchy/FBX axis and per-renderer bounds, then remove/replace any asset that fails that inspection.


## CC0 authored battlefield prop integration — 2026-11-10

- Added a reproducible Poly Haven 1K glTF asset path:
  - `tools/download_polyhaven_models.py` downloads each selected 1K GLTF and all referenced include files, retries requests, calculates a SHA-256 for every downloaded file and writes `POLYHAVEN_PROPS_SOURCES.txt`.
  - `tools/convert_battlefield_props.py` imports the downloaded GLTF files into Blender and exports Unity-ready FBX models while logging mesh count, dimensions and file size.
  - `tools/prepare_external_assets.sh` now downloads the four selected CC0 prop assets, and `tools/prepare_unity_assets.sh` converts them into `Resources/TacticalStrike/Imported/Props`.
- Selected authored assets from official Poly Haven sources:
  - `concrete_road_barrier` for the two outer shoulders of the southern attacker staging lane.
  - `old_military_crate` for defensive supply positions and the HMG nests.
  - `ammo_box` for staging, HMG resupply and interior supply positions.
  - `metal_jerrycan_green` for secondary supply dressing.
- Source material import is handled by `BattlefieldPropModelPostprocessor` and `BattlefieldPropTexturePostprocessor`, which transfer albedo/normal/metallic-gloss maps to URP Lit and configure linear data maps/alpha import.
- The Unity build gate requires all four prop FBX files, the provenance manifest and successful GameObject/Renderer imports before WebGL or Android build can proceed.
- The runtime map now places these props at defined coordinates, scales them from imported renderer bounds, grounds the lowest renderer bound to `HeightAt`, adds collider bounds and ballistic cover integrity. The spawn's central approach remains free of barriers; resupply props are kept beside HMG mounts and building courtyards.
- Official asset sources/licence: Poly Haven asset pages for Concrete Road Barrier, Old Military Crate, Ammo Box and Metal Jerrycan Green; Poly Haven assets are CC0 1.0. These are original external CC0 assets, not copied Squad assets or code.
- This is a controlled first prop replacement, not completion of the entire environment. The wider trench/sandbag/house/terrain system still includes procedural primitive geometry; final art acceptance requires new WebGL/Android CI, a deployed screenshot, correct prop scale/materials, and then further environment replacement based on visible defects.
- Build/cache note: both asset preparation scripts changed and are included in runtime asset cache key hashing; a new run must rebuild the prepared asset cache rather than assuming earlier cached props exist.


### URP metallic map keyword correction — 2026-11-10

- Inspection against the URP Lit shader reference showed the metallic/roughness map shader variant is named `_METALLICSPECGLOSSMAP`, not `_METALLICGLOSSMAP`.
- Updated the Poly Haven prop material importer to enable the actual URP keyword when restoring the metallic-gloss texture. This prevents imported weathered metal assets from silently falling back to a plain scalar metallic value.
- Source commit: `1dddd0be498b02d147ed309c6d0e45393b79bb0b`. Build run triggered for the updated source; this correction has not yet passed Unity import/build verification.


## User screenshot review: old WebGL deployment, floating foliage, map-art gap — 2026-11-11

### Screenshot and deployment verification

- User uploaded a 1366×768 browser screenshot showing a first-person view with dark detached foliage/large slab-like geometry across the horizon, a tall dark upright slab near the centre, broad flat olive ground, a separated dark road slab, uneven/discontinuous trench/road edges, and no visible acceptable AK-74M/arms viewmodel.
- The latest successful GitHub Pages deployment at this checkpoint is still WebGL run #17, source commit `86d5729731a837a9385b7928bcade35a61d4d07a`. All later preview runs up to #33 were cancelled or failed; current WebGL run #34 (`42900456819`) was still building source `1dddd0be498b02d147ed309c6d0e45393b79bb0b`. Android run #51 (`42900456803`) was also still building that source. Therefore the screenshot URL still served the prior deployed build; the map/foliage/quality corrections on newer `main` commits had not yet been published when the screenshot was captured.
- Do not ask the user to retest the same link until run #34 passes Unity WebGL build, output validation, Pages artifact upload, and Pages deployment. Confirm the published source SHA before asking for any retest.

### Confirmed source issues already corrected on current main (not yet visually accepted)

- Commit `84122e3e08bbcdafaeb94acaaae9eb3bf0c615c7` documents and corrects the old screenshot causes: old URP was render scale 0.90 / MSAA 2x / HDR off / 85m shadow distance / LOD bias 1.25; old roads used separated flat cube sections; trench floors used spaced thin plates and horizontal-only block rotations; foliage scale/pivots were not normalized or grounded; tracer tail could reach 6–8.5m; surface impacts lacked suitable dirt marks and differentiated effects.
- Current URP build configuration is applied unconditionally, including after Unity Library cache restoration: render scale 1.0, 4x MSAA, HDR enabled, 220m shadow distance, four cascades, 2048 main-light shadow map, two additional lights, depth texture, LOD bias 1.75 and anisotropic filtering. Runtime QualitySettings are aligned.
- Terrain/road/trench correction raises the terrain heightmap to 513×513 and alphamap to 512 over 700m, creates Catmull-Rom-smoothed road ribbons following both terrain edges, makes trench floors continuous conforming ribbons, and aligns parapet blocks to local 3D slope. This fixes the former spaced slab construction at source level, pending deployed visual verification.
- Nature model transforms are normalized from renderer bounds and placed on the terrain surface. Commit `b56212a5a26ad48a5532cbbc33a7bc2efaa7b193` also fixes the URP foliage material postprocessor: it preserves albedo/normal/alpha inputs and enables alpha-tested, two-sided foliage rendering. This directly addresses the previous dark opaque foliage-card/slab appearance.
- Tracer source now uses a short 0.09–0.22m tapered wake with calibre-scaled 2.8–6cm luminous envelope instead of the prior multi-metre laser-like tail. Impact logic distinguishes hard surfaces from dirt/grass, spawns sparks for metal/masonry, dust for non-metal strikes, and a temporary 32-second soil mark/lip on suitable ground hits. These are first-pass effects; the ground scar is a temporary mesh, not destructive TerrainData deformation.
- HUD mission panel negative-height RectTransform was corrected in commit `706c2d4e2fd1b32dec45cd6e6b7964f58e8487cb`.

### Remaining art and gameplay gap — do not claim Squad-quality parity

- The current 700m world still uses substantial procedural greybox geometry: houses, sheds, dugouts, firing bays, parts of parapets and field debris are made from primitive cubes; ridge rocks are combined scaled spheres; several props are four CC0 Poly Haven models (road barrier, military crate, ammo box and green jerrycan).
- The map currently places about 210 tree instances and some shrubs along the flanks. The imported `grass-large` model is prepared but is not yet painted/scattered as a ground-detail layer; the central valley still lacks dense natural ground cover, litter, small rocks, varied vegetation clusters, realistic terrain transitions and environmental dressing.
- The top-down `docs/northwind-valley-layout.svg` is explicitly a conceptual layout plan, not a rendered 3D map, terrain sculpt or visual-quality proof. It should not be presented as if it were the in-game map screenshot.
- Current weapon viewmodel is still imported from an FBX/legacy animation path and has no confirmed visual acceptance: the new deployed build must show both arms and AK in the intended lower-right first-person pose, correctly gripped, scaled, oriented and animated. If it does not, the rig needs a separate replacement/re-rig pass; build success alone does not prove viewmodel visibility.
- Professional target therefore requires a true art-production pass after current corrections deploy: authored/sourced military sandbag and revetment assets, detailed terrain/road shoulder materials, ground-cover/vegetation scatter with correct alpha/LOD, authored buildings and interiors, dressing/decals, plus a validated first-person rig. No claim that the current procedural geometry is already visually equivalent to Squad.
- Acceptance remains pending exact current-source WebGL deployment and a fresh screenshot. User testing must not be requested against the old published page.


## Unity build CS0104 fix and first-person AK-74M camera calibration — 2026-11-11

### Latest failed build — exact root cause

- WebGL run #34 `42900456819` and Android run #51 `42900456803`, both for source `1dddd0be498b02d147ed309c6d0e45393b79bb0b`, failed at C# compilation before packaging.
- Both logs report `CS0104` at `Assets/TacticalStrike/Scripts/Core/TacticalStrikeBootstrap.cs(30,48)`: `ShadowResolution` is ambiguous between `UnityEngine.ShadowResolution` and `UnityEngine.Rendering.Universal.ShadowResolution`. The same unqualified enum use was also present in the editor build setup at `TacticalStrikeBuild.cs`.
- The cause is importing both UnityEngine and URP namespaces while referencing `ShadowResolution.High` without qualifying the owning type. Fix: use `UnityEngine.ShadowResolution.High` in both runtime and build setup. This is unrelated to assets, Unity licensing or runner resources.

### Root-cause investigation of rifle / hands orientation

- The pinned original AK glTF source is the Sketchfab asset “FPS AK-74m animations” by Cransh, with CC-BY-4.0 attribution. Its glTF scene hierarchy contains an authored node named `Camera` under the arms rig and 8 animation takes: `Rig|AK_Idle`, `Rig|AK_Draw`, `Rig|AK_Walk`, `Rig|AK_Run`, `Rig|AK_Shot`, `Rig|AK_Reload`, `Rig|AK_Reload_var2`, and `Rig|AK_Reload_full`.
- Runtime `WeaponController.BuildViewModel()` previously ignored that authored camera reference, repositioned the whole arm/rifle rig using the combined renderer bounds centre, and used a generic fixed offset. Runtime `Update()` then replaced the root local rotation every frame with only the procedural sway/recoil Euler rotation. This discarded the rig's authored camera-space orientation and grip reference. Therefore changing a guessed 180-degree yaw or a hand-made offset could not reliably align the rifle and both hands.
- The converter already exports Blender EMPTY objects; the source Camera is a transform-only node in the glTF hierarchy. The revised converter preserves/renames that source node as `Viewmodel_Camera_Anchor` and logs it as part of FBX conversion.
- Runtime now computes the marker rotation relative to the imported rig root, sets the inverse base orientation so the marker faces the player's camera, and translates the marker to a calibrated camera-space reference position. Per-frame sway/recoil is applied on top of this base rotation, rather than overwriting the base rig orientation.
- The editor build validator now requires the camera anchor and checks for idle, draw, walk, run, shot and reload animation clips. This prevents building a rifle asset that silently lacks its first-person camera reference or core motions.
- The original asset's existence and hierarchy have been inspected from the pinned glTF JSON. The new FBX marker preservation and actual rendered pose remain pending Unity CI and visual screenshot acceptance; do not claim the weapon is now visually correct until checked in the built WebGL/Android runtime.

### Squad mapping research — design references, not proprietary implementation claims

- Official Squad / Offworld Fallujah development articles describe restarting the layout design based on real-world city layout and imagery, collecting extensive photo references, building gameplay blockouts, validating spaces, progressing from basic-material Alpha to secondary/tertiary forms, Beta material development and final decals/prop dressing. Their follow-up describes roughly six months of map optimization and broad playtesting. Sources:
  - `https://store.steampowered.com/news/posts/?appids=393380&enddate=1597860357&feed=steam_community_announcements`
  - `https://store.steampowered.com/news/posts/?appids=393380&enddate=1598644081`
- Official Squad's Harju interview explains how foliage and road networks create gameplay constraints and how much of the map was refined/rebuilt before official release; the designer reported 950 hours before the last Workshop release and another 1,400 hours of official integration work. Source: `https://store.steampowered.com/news/posts/?appids=393380&enddate=1670353163&feed=steam_community_announcements`
- Squad's official modding help points users to the UE4 Squad SDK and official map-design/modding learning communities: `https://offworldindustries.zendesk.com/hc/en-us/articles/221394408-How-do-I-start-modding-in-Squad`.
- Community Squad SDK guides identify World Machine and USGS-to-Unreal heightmaps as common terrain source workflows; this is community guidance, not a claim that every official Squad map uses one tool: `https://squad.fandom.com/wiki/Creating_a_Custom_Map`.
- Unreal's official docs demonstrate height/weight Landscape material layer blends, including height blending between rock/soil/grass; Landscape foliage tooling supports controlled scatter over landscape and surfaces. Sources:
  - `https://dev.epicgames.com/documentation/unreal-engine/landscape-materials-in-unreal-engine`
  - `https://dev.epicgames.com/documentation/unreal-engine/landscape-quick-start-guide-in-unreal-engine`
  - `https://dev.epicgames.com/documentation/unreal-engine/unreal-engines-systems-and-workflows-overview-for-unity-developers`
- Practical design principles to adopt for Northwind: real-world reference board and terrain source; broad macro terrain sculpt with erosion-derived drainage/ridges; a gameplay greybox and route/POI playtesting pass before art; a consistent layered landscape material system; dense but gameplay-aware foliage and cover placement; a vertical-slice quality gate; secondary/tertiary forms; decals and environmental storytelling; and performance profiling after art dressing. Avoid copying proprietary Squad assets or source.

### Source changes now being committed

- Qualify the shadow enum explicitly in both runtime bootstrap and the editor setup.
- Preserve the source AK camera reference in the FBX conversion, align the runtime rig through that marker, retain its base rotation during sway/recoil, and validate marker plus required animation takes before either build.
- These changes are not considered complete until fresh WebGL and Android CI runs pass and the rendered first-person rig is inspected. The map research has been recorded; a separate art-production pass for terrain detail, vegetation density, trench/road material transitions and authored military assets remains necessary.


### Follow-up compile repair for viewmodel camera alignment — 2026-11-11

- Android run #52 (`42903750667`) reached Unity and verified the source assets. The Blender log explicitly confirmed:
  - `Viewmodel_Camera_Anchor` was preserved in the AK-74M FBX at world position `(0.0, 0.1577, 0.4715)`.
  - AK authored animations were imported: Idle, Draw, Walk, Run, Shot, Reload, Reload_var2 and Reload_full.
- Unity C# compilation then failed in `WeaponController.cs(129,47)` with `CS0023: Operator '-' cannot be applied to operand of type 'Quaternion'`.
- Cause: the camera-alignment offset expression applied unary minus to the Quaternion instead of to the rotated Vector3 result.
- Repair: parenthesize the rotation-times-scaled-anchor vector before negation:
  `viewModelRootOffset = -(model.transform.localRotation * Vector3.Scale(anchorLocal, model.transform.localScale));`
- The camera-marker mathematics and authored alignment approach are unchanged; this is a syntax/operator correction only. A fresh build must confirm compilation and continue through the camera-anchor/clip validation and packaging stages.


### CI state after AK-74M viewmodel compile repair — 2026-11-11

- Current source head before this documentation-only update: `122f7181a8ec88466a5a3fa1a6fdfa26ee918458`.
- WebGL run #36 (`42905370661`) and Android run #53 (`42905370668`) are both executing `Build ... with Unity 6.3 LTS` on that exact source SHA. No new failed step or compile error had been reported at the latest poll.
- Both must complete before further `UnityProject/**` / `tools/**` edits are pushed, because the workflows use cancel-in-progress concurrency. WebGL still must pass output validation and GitHub Pages deployment; Android still must pass APK archive/signature/ARM64 verification.
- Next planned terrain-art source pass, not yet committed: convert the existing `grass-large` CC0 mesh to a single instanced detail prototype, paint terrain detail-density maps from grass alphamap weight and slope, exclude roads/trenches/drainage/buildings/emplacements/spawn corridors, and build continuous soil road shoulders under the compacted road ribbons. This must be performance-tested for both Android and WebGL before accepting higher detail density.
- Current source does not yet have a detail-prototype ground-cover layer or road shoulders. The map is still a procedural sculpt with three PBR terrain layers, limited manually placed trees, and procedural/primitive structures. This is a verified remaining art gap, not a finished Squad-equivalent battlefield.
