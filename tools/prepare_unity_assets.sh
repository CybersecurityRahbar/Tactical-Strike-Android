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
NATURE_DIR="assets/runtime/models/nature"
PROPS_DIR="assets/runtime/models/battlefield_props"
NATURE_BASE="https://raw.githubusercontent.com/Hidencod/tge-assets/ea2f2aa1ca8da0898330c97df8a5489976b051d0/packs/nature-kit"
mkdir -p "$UNITY_RES/Terrain" "$UNITY_RES/Textures" "$UNITY_RES/Nature" \
  "$UNITY_RES/Audio/Firearms" "$UNITY_RES/Audio/Footsteps" "$UNITY_RES/Audio/Impacts" \
  "$UNITY_RES/Props" "$NATURE_DIR" "$PROPS_DIR" assets/runtime/audio/impacts

download_asset() {
  local url="$1"
  local output="$2"
  echo "Downloading runtime asset: $url"
  curl --fail --location --retry 3 --retry-delay 2 \
    --user-agent "TacticalStrikeAssetBuilder/2.1" "$url" -o "$output"
  test -s "$output"
}

# Authored AK-74M model with its source arm/rifle albedo and normal maps.
blender --background --factory-startup --python tools/convert_ak74m_to_fbx.py -- \
  "assets/runtime/models/ak74m/scene.gltf" \
  "$UNITY_RES/AK74M.fbx"
cp assets/runtime/models/ak74m/textures/arms_baseColor.jpg "$UNITY_RES/Textures/"
cp assets/runtime/models/ak74m/textures/ak74m_baseColor.jpg "$UNITY_RES/Textures/"
cp assets/runtime/models/ak74m/textures/arms_normal.png "$UNITY_RES/Textures/"
cp assets/runtime/models/ak74m/textures/ak74m_normal.png "$UNITY_RES/Textures/"

# Sparse Grass from Poly Haven (CC0); 1K maps are tuned for mobile memory use.
download_asset "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/sparse_grass/sparse_grass_diff_1k.jpg" \
  "assets/runtime/terrain/sparse_grass_diff_1k.jpg"
download_asset "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/sparse_grass/sparse_grass_nor_gl_1k.jpg" \
  "assets/runtime/terrain/sparse_grass_nor_gl_1k.jpg"
download_asset "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/sparse_grass/sparse_grass_rough_1k.jpg" \
  "assets/runtime/terrain/sparse_grass_rough_1k.jpg"
cp assets/runtime/terrain/sparse_grass_* "$UNITY_RES/Terrain/"
cp assets/runtime/terrain/rocky_gravel_* "$UNITY_RES/Terrain/"
cp assets/runtime/terrain/northwind_* "$UNITY_RES/Terrain/"
cp assets/runtime/terrain/NORTHWIND_PBR_SOURCES.txt "$UNITY_RES/Terrain/"

# Compact CC0 Kenney Nature Kit subset for forest edges, shrubs and ground grass.
for name in tree-pinerounda tree-pineroundb tree-oak tree-default plant-bushdetailed grass-large; do
  download_asset "$NATURE_BASE/$name.glb" "$NATURE_DIR/$name.glb"
done
blender --background --factory-startup --python tools/convert_nature_assets.py -- \
  "$NATURE_DIR" "$UNITY_RES/Nature"

# Convert CC0 Poly Haven cover and resupply models to Unity FBX.
blender --background --factory-startup --python tools/convert_battlefield_props.py -- \
  "$PROPS_DIR" "$UNITY_RES/Props"
cp "$PROPS_DIR/POLYHAVEN_PROPS_SOURCES.txt" "$UNITY_RES/Props/"

# Preserve the stereo near/mid report recordings and their complete natural decay.
cp assets/runtime/audio/ak_report_near.ogg "$UNITY_RES/Audio/Firearms/"
cp assets/runtime/audio/ak_report_mid.ogg "$UNITY_RES/Audio/Firearms/"
cp assets/runtime/audio/m4_report_near.ogg "$UNITY_RES/Audio/Firearms/"
cp assets/runtime/audio/m4_report_mid.ogg "$UNITY_RES/Audio/Firearms/"
cp assets/runtime/audio/FIREARM_AUDIO_SOURCES.txt "$UNITY_RES/Audio/Firearms/"

cp assets/runtime/audio/sand_*.ogg "$UNITY_RES/Audio/Footsteps/"

# One-shot impact beds from the SpritePilot CC0 game-audio pack.
download_asset "https://spritepilot.com/free-assets/sound-effects/explosions/heavy-thud.ogg" \
  "assets/runtime/audio/impacts/heavy-thud.ogg"
download_asset "https://spritepilot.com/free-assets/sound-effects/explosions/debris-crash.ogg" \
  "assets/runtime/audio/impacts/debris-crash.ogg"
download_asset "https://spritepilot.com/free-assets/sound-effects/explosions/metal-impact.ogg" \
  "assets/runtime/audio/impacts/metal-impact.ogg"
cp assets/runtime/audio/impacts/*.ogg "$UNITY_RES/Audio/Impacts/"

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
test -s "$UNITY_RES/Terrain/sparse_grass_diff_1k.jpg"
test -s "$UNITY_RES/Terrain/sparse_grass_nor_gl_1k.jpg"
test -s "$UNITY_RES/Terrain/sparse_grass_rough_1k.jpg"
test -s "$UNITY_RES/Terrain/rocky_gravel_diff_1k.jpg"
test -s "$UNITY_RES/Terrain/rocky_gravel_nor_gl_1k.jpg"
test -s "$UNITY_RES/Terrain/rocky_gravel_rough_1k.jpg"
for surface in grass ground brick concrete road; do
  test -s "$UNITY_RES/Terrain/northwind_${surface}_diff_1k.jpg"
  test -s "$UNITY_RES/Terrain/northwind_${surface}_nor_gl_1k.jpg"
done
test -s "$UNITY_RES/Terrain/NORTHWIND_PBR_SOURCES.txt"
test -s "$UNITY_RES/Textures/arms_baseColor.jpg"
test -s "$UNITY_RES/Textures/ak74m_baseColor.jpg"
test -s "$UNITY_RES/Textures/arms_normal.png"
test -s "$UNITY_RES/Textures/ak74m_normal.png"
test -s "$UNITY_RES/Audio/Impacts/heavy-thud.ogg"
test -s "$UNITY_RES/Audio/Impacts/debris-crash.ogg"
test -s "$UNITY_RES/Audio/Impacts/metal-impact.ogg"
test -s "$UNITY_RES/Nature/tree-pinerounda.fbx"
test -s "$UNITY_RES/Nature/tree-pineroundb.fbx"
test -s "$UNITY_RES/Nature/tree-oak.fbx"
test -s "$UNITY_RES/Nature/tree-default.fbx"
test -s "$UNITY_RES/Nature/plant-bushdetailed.fbx"
test -s "$UNITY_RES/Nature/grass-large.fbx"
for prop in concrete_road_barrier old_military_crate ammo_box metal_jerrycan_green; do
  test -s "$UNITY_RES/Props/$prop.fbx"
done
test -s "$UNITY_RES/Props/POLYHAVEN_PROPS_SOURCES.txt"
test -s "$UNITY_RES/Audio/Firearms/ak_report_near.ogg"
test -s "$UNITY_RES/Audio/Firearms/ak_report_mid.ogg"
test -s "$UNITY_RES/Audio/Firearms/m4_report_near.ogg"
test -s "$UNITY_RES/Audio/Firearms/m4_report_mid.ogg"
test -s "$UNITY_RES/Audio/Firearms/FIREARM_AUDIO_SOURCES.txt"
test -s "$UNITY_RES/Audio/Footsteps/sand_l.ogg"
test -s "$UNITY_RES/Audio/Footsteps/sand_r.ogg"
test -s "$UNITY_RES/AK74M_ATTRIBUTION.txt"

echo "UNITY ASSET PREPARATION PASSED"
find "$UNITY_RES" -maxdepth 3 -type f -printf '%P (%s bytes)\n' | sort
