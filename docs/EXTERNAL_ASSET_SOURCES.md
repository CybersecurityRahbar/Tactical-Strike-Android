# External runtime assets

The Android build prepares a small, pinned set of external game assets. They are bundled in the APK; the user's phone does not need network access at runtime.

## 3D weapons

- AK model: “AK” by loafbrr_1, OpenGameArt, CC0 1.0. The author lists 2,670 triangles and 1K textures, with a Godot-ready archive. Source: https://opengameart.org/content/ak
- M4A1 model: “M4A1 Assault Rifle” by nisu / 3DModelsCC0, OpenGameArt, CC0 1.0. Source: https://opengameart.org/content/m4a1-assault-rifle

The AK model is a close visual stand-in for the AK-74 family, not a claim that the mesh is a factory-exact AK-74. The M4 mesh is the M4A1 family asset.

## Terrain material

- Rocky Gravel by Poly Haven, CC0. 1K diffuse, OpenGL normal and roughness maps create a visible dry rocky surface. Source: https://polyhaven.com/a/rocky_gravel

## Firearm recordings and footsteps

- The Free Firearm Sound Library by Ben Jaszczak, Brian Nelson, Kevin Heras and Matthew Nanney, CC0. The build uses recorded AK-47, AR-15 and Tikka takes; these are real recorded firearm takes but are not exact AK-74, M4A1, PKM or DShK recordings. Source: https://opengameart.org/content/the-free-firearm-sound-library
- Fantozzi's Footsteps by Fantozzi, OpenGameArt, CC0. Four sand footstep takes are used for the player's movement. Source: https://opengameart.org/content/fantozzis-footsteps-grasssand-stone

Audio sources are pinned to commit a19b7458a593598211c95ec46ef4eb4b6d1f94d7 in petroulacl/fps-asset-kit. They are converted to OGG Vorbis during the build to reduce APK size.

## Notes

These assets improve the prototype but do not constitute a complete animated weapon system. Reload, aim, sway, and recoil presentation still require playtesting and iteration. Separate exact weapon models and recorded samples are still needed for PKM, RPG-7, SVD, DShK and M2 if the project is to represent every weapon accurately.
