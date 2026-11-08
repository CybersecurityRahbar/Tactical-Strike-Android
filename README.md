# Tactical Strike

**An original Android-first tactical infantry FPS.**

The active game project is now being rebuilt in **Unity 6.3 LTS (`6000.3.28f1`) with URP 17.3.0**. Unity is the primary engine for new work. The earlier Godot project remains in the repository as a preserved reference; its source and history have not been deleted.

## Current Unity implementation

The Unity project lives in `UnityProject/`. Its first vertical slice is being built around the original battlefield and combat requirements:

- A 700 m × 700 m dry plateau with rolling, broken ridges, shallow gullies and long-range sightlines.
- Attacker and defender trench lines, batched sandbag battlements, masonry firing cover and two separated defensive houses.
- A first-person player with mobile landscape controls, movement/look, sprint, crouch, jump and aim.
- An AK-74M first-person weapon path with a procedural fallback, magazine/reserve handling, FMJ/tracer/AP modes, firing, reload, ballistic projectiles and impact dust.
- Recorded AK-family firing takes and sand footsteps, staged from the pinned, attributed sources.
- Day/night lighting controls and a touch HUD with pause and settings panels.

The environment is currently a **procedural vertical-slice foundation**, not a finished authored commercial map. The AK-74M is the first integrated weapon; the remaining armory, tactical AI and LAN/hotspot multiplayer are not yet implemented. A successful Unity editor import/build and physical-phone acceptance test have not yet been recorded.

## Unity source layout

- `UnityProject/Assets/TacticalStrike/Scripts/World/`: battlefield and destructible cover.
- `UnityProject/Assets/TacticalStrike/Scripts/Player/`: first-person controller.
- `UnityProject/Assets/TacticalStrike/Scripts/Combat/`: weapon, projectile and impact effects.
- `UnityProject/Assets/TacticalStrike/Scripts/UI/`: mobile HUD, joystick and touch controls.
- `UnityProject/Assets/Editor/`: reproducible URP/scene setup and Android build validation.
- `tools/prepare_unity_assets.sh`: prepares the pinned licensed assets for Unity.
- `tools/convert_ak74m_to_fbx.py`: converts the original animated AK-74M glTF to FBX with Blender.
- `.github/workflows/unity-android.yml`: manual Unity Android build workflow.

Generated imported binaries are not committed under `UnityProject/Assets/Resources/TacticalStrike/Imported/`; the workflow regenerates them from the pinned, licence-documented sources before Unity imports the project.

## Build the Android APK in GitHub Actions

A Unity Personal license must be activated once and stored in repository Actions secrets. GameCI's current v4 guidance uses a locally activated `.ulf` file and these secrets:

1. Install Unity Hub on a computer and sign in to the Unity account.
2. In Unity Hub, open **Preferences → Licenses → Add → Get a free Personal license**.
3. Copy the generated license file:
   - Windows: `C:\\ProgramData\\Unity\\Unity_lic.ulf`
   - Linux: `~/.local/share/unity3d/Unity/Unity_lic.ulf`
   - macOS: `/Library/Application Support/Unity/Unity_lic.ulf`
4. In GitHub, open **Settings → Secrets and variables → Actions** and add:
   - `UNITY_LICENSE`: the entire contents of `Unity_lic.ulf`.
   - `UNITY_EMAIL`: the Unity account email.
   - `UNITY_PASSWORD`: the Unity account password.

Do not commit the license or account credentials to the repository. After adding the secrets, run **Actions → Unity Android Build → Run workflow**. The job prepares the assets, imports the Unity project, builds an ARM64 Android APK, checks the APK signature and contents, and uploads the APK plus its SHA-256 report.

The Unity workflow is manual until the license secrets are present. The current Godot workflow is retained as a legacy/reference build and only responds to changes in its Godot-specific files.

## Preserved Godot reference

The root-level Godot 4.7.2 project and its previous CI history are retained for comparison and recovery. The last documented successful Godot build was a CI acceptance build, not a physical-device visual sign-off. See [the cumulative project context](PROJECT_CONTEXT.md), [battlefield design](docs/BATTLEFIELD_DESIGN.md), [battlefield layout SVG](docs/battlefield-layout.svg), [asset provenance](docs/EXTERNAL_ASSET_SOURCES.md), and [Unity migration notes](docs/UNITY_MIGRATION.md).

## Roadmap after the vertical slice

1. Pass Unity asset import, C# compilation and Android APK validation in GitHub Actions.
2. Validate landscape layout, viewmodel framing, input feel and frame time on a physical Android phone.
3. Polish the environment art, weapon animation/audio and mobile performance.
4. Add the tactical defender/attacker AI behaviours.
5. Add local Wi-Fi/hotspot multiplayer after single-player combat is stable.
