# Browser preview workflow

## What this is

The browser preview is a desktop review build of the Unity project. GitHub Actions prepares runtime assets, builds the project for Unity WebGL, validates the generated loader/data/framework/WebAssembly files, and deploys the result to GitHub Pages.

Expected project URL after the first successful deployment:

`https://cybersecurityrahbar.github.io/Tactical-Strike-Android/`

The first deployment requires GitHub Pages to be enabled for the repository:

1. Open the repository on GitHub.
2. Go to **Settings → Pages**.
3. Under **Build and deployment → Source**, select **GitHub Actions**.
4. Save. The workflow will perform the actual deployment on the next successful run.

No Unity Editor installation is required on the user's PC for this workflow. The existing Unity Personal secrets are reused by the cloud builder; do not expose or commit their values.

## Desktop controls

| Action | Input |
|---|---|
| Capture mouse / enter look mode | Click inside the game view |
| Release mouse / leave look mode | Escape |
| Move | WASD |
| Look | Mouse |
| Fire | Left mouse button while captured |
| Aim down sights | Right mouse button while captured |
| Sprint | Left Shift |
| Crouch | C |
| Jump | Space |
| Reload | R |
| Cycle FMJ / tracer / AP | T |

Clicking a HUD element while the cursor is free must not recapture the pointer. Escape releases the pointer so the user can interact with the in-game pause/settings buttons.

## Review procedure

1. Open the latest preview URL in desktop Chrome.
2. Click the game view to capture the mouse.
3. Check the first-person weapon and both hands before judging the map.
4. Test the walk, sprint, crouch, jump and aim transitions.
5. Use T to select tracer ammunition, then inspect the visible projectile path and impact.
6. Inspect the central approach, both flanks, the trench network and all building entrances.
7. Share a screenshot or short screen recording with the commit/build ID shown in `preview-build.json`.

The browser build is for visual/design iteration. It does not certify Android performance, touch controls, native audio behavior, or physical-device compatibility. Every approved milestone still needs an Android APK and a phone test.

## Known platform boundaries

- This is a desktop-first WebGL review target, not the Android release.
- Browser memory, WebAssembly, graphics drivers, audio restrictions and pointer-lock behavior differ from Android.
- The workflow disables WebGL threads to avoid cross-origin-isolation requirements on GitHub Pages.
- Gzip output plus Unity decompression fallback is used so Pages does not need custom server-side compression headers.
- A successful deployment proves that the site was published; it does not prove that the game looks or feels correct. Manual browser review remains mandatory.
