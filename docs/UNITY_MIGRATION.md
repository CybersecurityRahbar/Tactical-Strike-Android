# Unity migration and vertical-slice notes

## Decision

On 2026-11-09 the user approved migrating the active Tactical Strike development path from Godot to Unity. Unity is now the primary engine. The Godot project is intentionally preserved at the repository root as a legacy/reference implementation until the Unity version has passed Android build and physical-device acceptance.

## Pinned engine and render pipeline

- Unity Editor: **6000.3.28f1 (Unity 6.3 LTS)**.
- Render pipeline: **Universal Render Pipeline 17.3.0**.
- Android architecture: ARM64, IL2CPP.
- Minimum Android API: 25.
- Orientation: landscape.
- Project root: `UnityProject/`.

Unity 6.3 LTS is supported through December 2027. The selected 6000.3.28f1 patch is a released LTS patch, and GameCI has a matching Linux Android editor image. Sources:
- https://unity.com/releases/unity-6/support
- https://unity.com/releases/editor/whats-new/6000.3.28f1
- https://hub.docker.com/layers/unityci/editor/6000.3.28f1-android-3/images/sha256-60c6a9901ec12534a1698f2fc44e79c94c82860c89363306615107f0c51e5548

## First Unity vertical slice

The Unity code builds the initial battlefield at runtime so a scene does not need to be hand-authored before the first cloud import:

- `TacticalStrikeBootstrap` creates the environment, player and HUD, applies mobile frame-rate/orientation defaults, and handles day/night lighting.
- `BattlefieldGenerator` builds the 700 m plateau mesh, irregular ridges and gullies, both trench/cover sectors, two separated defensive houses, low-profile rocks and distant silhouettes.
- Long sandbag rows are combined into a small number of render meshes and mesh colliders rather than thousands of separate GameObjects, reducing mobile draw-call and physics overhead. Firing walls and selected cover blocks retain individual destructible integrity.
- `FpsPlayerController` provides first-person movement, look, sprint, crouch, jump, aim, camera bob and recorded sand footsteps.
- `WeaponController` provides the first-person AK-74M path, a procedural model fallback if model conversion fails, firing/reload/ammunition logic, recorded firearm takes, a muzzle flash and ballistic projectile creation.
- `BallisticProjectile` applies gravity and drag, raycasts the travelled segment, damages `BallisticCover` and creates `ImpactFx` dust.
- `MobileHud` creates a landscape touch interface with a correctly sized movement joystick, right-side look area, combat controls, ammo/status, pause and settings panels.

This is a working code scaffold for a first vertical slice; it is not being represented as a finished game or as a build already verified on a phone.

## Asset pipeline and provenance

`tools/prepare_unity_assets.sh` reruns the existing pinned asset preparation pipeline, then:

1. Uses Blender in background mode to convert the pinned animated AK-74M glTF source to Unity FBX.
2. Copies the Rocky Gravel PBR maps into Unity Resources.
3. Separates firearm recordings and sand footsteps into distinct Resources folders.
4. Copies the model licence and writes an attribution note beside the staged FBX.

The staged runtime binaries are ignored by Git and regenerated during the build. Their source pins and licence details remain documented in `docs/EXTERNAL_ASSET_SOURCES.md`. The recorded AK-47 takes remain labelled as close-family audio proxies, not exact AK-74M factory recordings.

## Cloud build and licensing

- `.github/workflows/unity-android.yml` is a manual workflow using `game-ci/unity-builder@v4`.
- The build method is `TacticalStrike.Editor.TacticalStrikeBuild.BuildAndroid`, located under Unity's required `Assets/Editor` path.
- The workflow verifies APK archive integrity, Android signature, manifest, DEX, ARM64 IL2CPP library and Unity runtime library, then uploads the APK and SHA-256 report.
- Unity Personal activation is not automated by the deprecated GameCI activation-file action. The current GameCI v4 instructions require a local Unity Hub Personal activation and Actions secrets named `UNITY_LICENSE`, `UNITY_EMAIL` and `UNITY_PASSWORD`. See https://game.ci/docs/github/activation/ and https://game.ci/docs/github/builder/.
- No secret, password, serial or license file is stored in Git.

## Legacy workflow

The previous `.github/workflows/android.yml` remains available for explicit Godot reference builds through `workflow_dispatch` only. New Unity commits do not automatically start the legacy Godot build.

## Acceptance gates and known limits

- [ ] Unity 6000.3.28f1 imports the complete project without C# or package errors.
- [ ] Blender conversion emits an FBX containing the AK-74M mesh, armature and the authored idle/draw/walk/run/shot/reload takes.
- [ ] Unity imports the FBX, PBR maps and recorded audio into the correct Resources paths.
- [ ] Unity Android ARM64 IL2CPP build passes and the APK archive/signature checks succeed.
- [ ] Install this exact Unity APK on the user's physical Android phone.
- [ ] Inspect the battlefield from the player spawn, weapon pose/scale, touch controls, movement, aim, fire, reload, audio and actual frame rate.
- [ ] Tune visuals and performance from the phone test before implementing the larger AI and LAN systems.

Not implemented yet: completed tactical bot behaviour trees, multiple fully modelled weapons, coordinated attacks, local Wi-Fi/hotspot multiplayer, production map art and final sound design. The Godot project has not been deleted, rewritten or moved as part of this initial migration.
