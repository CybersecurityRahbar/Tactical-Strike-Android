# Unity project

This directory contains the Unity implementation of Tactical Strike.

## Editor baseline

- Unity Editor: **6000.3.28f1 (Unity 6.3 LTS)**
- Render pipeline: **Universal Render Pipeline 17.3.0**
- Target: **Android ARM64**, IL2CPP, landscape orientation
- Android minimum API: **25**
- Android target API: **36**

## Project packages

The project explicitly enables Unity's built-in Animation, Audio, and Particle System modules because the gameplay scripts use those APIs. The remaining scene, terrain, player, weapon, and HUD systems are generated from the source assets and editor build entry point.

## Cloud build

The repository workflow .github/workflows/unity-android.yml validates the Personal license secrets, prepares the pinned source assets, imports/compiles the Unity project, builds an Android APK, and verifies the APK archive, signature, manifest, DEX payload, and ARM64 IL2CPP libraries.

Routine Android builds run on GitHub Actions; the local computer does not need the full Unity Editor or Android Build Support. Install the Unity Editor only when you need to open and edit the project interactively.
