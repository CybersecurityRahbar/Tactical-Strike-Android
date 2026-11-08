#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

# Reuse the pinned, licence-documented source preparation used by the legacy project.
bash tools/prepare_external_assets.sh

command -v blender >/dev/null 2>&1 || {
  echo "Blender is required to convert the animated AK-74M glTF into FBX." >&2
  exit 1
}
command -v ffmpeg >/dev/null 2>&1 || {
  echo "ffmpeg is required by the source asset preparation pipeline." >&2
  exit 1
}

UNITY_RES="UnityProject/Assets/Resources/TacticalStrike/Imported"
mkdir -p "$UNITY_RES/Terrain" "$UNITY_RES/Audio/Firearms" "$UNITY_RES/Audio/Footsteps"

blender --background --factory-startup --python tools/convert_ak74m_to_fbx.py -- \
  "assets/runtime/models/ak74m/scene.gltf" \
  "$UNITY_RES/AK74M.fbx"

cp assets/runtime/terrain/rocky_gravel_diff_1k.jpg "$UNITY_RES/Terrain/"
cp assets/runtime/terrain/rocky_gravel_nor_gl_1k.jpg "$UNITY_RES/Terrain/"
cp assets/runtime/terrain/rocky_gravel_rough_1k.jpg "$UNITY_RES/Terrain/"
cp assets/runtime/audio/ak_*.ogg "$UNITY_RES/Audio/Firearms/"
cp assets/runtime/audio/sand_*.ogg "$UNITY_RES/Audio/Footsteps/"
cp assets/runtime/models/ak74m/MODEL_LICENSE.txt "$UNITY_RES/AK74M_SOURCE_LICENSE.txt"

cat > "$UNITY_RES/AK74M_ATTRIBUTION.txt" <<'EOF'
First-person AK-74M, hands and authored animation asset:
"FPS AK-74m animations" by Cransh
Source: https://sketchfab.com/3d-models/fps-ak-74m-animations-94be8385c402474cacd39bc096c6ca14
Author: https://sketchfab.com/ccransh
Licence: Creative Commons Attribution 4.0 International (CC-BY-4.0)
https://creativecommons.org/licenses/by/4.0/
The FBX used by Unity is converted from the pinned glTF source in Ayush-Mohanty/FPS-Arms-3D.
EOF

test -s "$UNITY_RES/AK74M.fbx"
test -s "$UNITY_RES/Terrain/rocky_gravel_diff_1k.jpg"
test -s "$UNITY_RES/Terrain/rocky_gravel_nor_gl_1k.jpg"
test -s "$UNITY_RES/Audio/Firearms/ak_C_27P.ogg"
test -s "$UNITY_RES/Audio/Firearms/ak_C_36P.ogg"
test -s "$UNITY_RES/Audio/Footsteps/sand_l.ogg"
test -s "$UNITY_RES/Audio/Footsteps/sand_r.ogg"
test -s "$UNITY_RES/AK74M_ATTRIBUTION.txt"

echo "UNITY ASSET PREPARATION PASSED"
find "$UNITY_RES" -maxdepth 3 -type f -printf '%P (%s bytes)\n' | sort
