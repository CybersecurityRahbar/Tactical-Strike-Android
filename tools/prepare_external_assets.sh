#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

BASE="https://raw.githubusercontent.com/Ayush-Mohanty/FPS-Arms-3D/c80a452e680c5a27c9e936176eb17e446869529a"
AUDIO_BASE="https://raw.githubusercontent.com/petroulacl/fps-asset-kit/a19b7458a593598211c95ec46ef4eb4b6d1f94d7"
MODEL_DIR="assets/runtime/models/ak74m"
TEXTURE_DIR="$MODEL_DIR/textures"
AUDIO_DIR="assets/runtime/audio"
TERRAIN_DIR="assets/runtime/terrain"
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT
mkdir -p "$TEXTURE_DIR" "$AUDIO_DIR" "$TERRAIN_DIR"

download() {
  local url="$1"
  local output="$2"
  echo "Downloading: $url"
  curl --fail --location --retry 3 --retry-delay 2 --user-agent "TacticalStrikeAssetBuilder/2.0" "$url" -o "$output"
  test -s "$output"
}

# Real first-person AK-74M + hands + skeleton + eight authored animation clips.
download "$BASE/Models/fps_ak-74m_animations/scene.gltf" "$MODEL_DIR/scene.gltf"
download "$BASE/Models/fps_ak-74m_animations/scene.bin" "$MODEL_DIR/scene.bin"
download "$BASE/Models/fps_ak-74m_animations/license.txt" "$MODEL_DIR/MODEL_LICENSE.txt"

# Original maps, resized for Android; base-colour maps are JPEG, data maps remain lossless PNG.
download "$BASE/Models/fps_ak-74m_animations/textures/arms_baseColor.png" "$TMP/arms_baseColor.png"
download "$BASE/Models/fps_ak-74m_animations/textures/arms_metallicRoughness.png" "$TMP/arms_metallicRoughness.png"
download "$BASE/Models/fps_ak-74m_animations/textures/arms_normal.png" "$TMP/arms_normal.png"
download "$BASE/Models/fps_ak-74m_animations/textures/ak74m_baseColor.png" "$TMP/ak74m_baseColor.png"
download "$BASE/Models/fps_ak-74m_animations/textures/ak74m_metallicRoughness.png" "$TMP/ak74m_metallicRoughness.png"
download "$BASE/Models/fps_ak-74m_animations/textures/ak74m_normal.png" "$TMP/ak74m_normal.png"
download "$BASE/Models/fps_ak-74m_animations/textures/ak74m_specularf0.png" "$TMP/ak74m_specularf0.png"

command -v ffmpeg >/dev/null 2>&1 || { echo "ffmpeg is required to prepare the game assets"; exit 1; }
ffmpeg -hide_banner -loglevel error -y -i "$TMP/arms_baseColor.png" -vf "scale='min(2048,iw)':'min(2048,ih)':force_original_aspect_ratio=decrease" -q:v 2 "$TEXTURE_DIR/arms_baseColor.jpg"
ffmpeg -hide_banner -loglevel error -y -i "$TMP/ak74m_baseColor.png" -vf "scale='min(2048,iw)':'min(2048,ih)':force_original_aspect_ratio=decrease" -q:v 2 "$TEXTURE_DIR/ak74m_baseColor.jpg"
ffmpeg -hide_banner -loglevel error -y -i "$TMP/arms_metallicRoughness.png" -vf "scale='min(1024,iw)':'min(1024,ih)':force_original_aspect_ratio=decrease" -compression_level 9 "$TEXTURE_DIR/arms_metallicRoughness.png"
ffmpeg -hide_banner -loglevel error -y -i "$TMP/arms_normal.png" -vf "scale='min(1024,iw)':'min(1024,ih)':force_original_aspect_ratio=decrease" -compression_level 9 "$TEXTURE_DIR/arms_normal.png"
ffmpeg -hide_banner -loglevel error -y -i "$TMP/ak74m_metallicRoughness.png" -vf "scale='min(1024,iw)':'min(1024,ih)':force_original_aspect_ratio=decrease" -compression_level 9 "$TEXTURE_DIR/ak74m_metallicRoughness.png"
ffmpeg -hide_banner -loglevel error -y -i "$TMP/ak74m_normal.png" -vf "scale='min(1024,iw)':'min(1024,ih)':force_original_aspect_ratio=decrease" -compression_level 9 "$TEXTURE_DIR/ak74m_normal.png"
ffmpeg -hide_banner -loglevel error -y -i "$TMP/ak74m_specularf0.png" -vf "scale='min(512,iw)':'min(512,ih)':force_original_aspect_ratio=decrease" -compression_level 9 "$TEXTURE_DIR/ak74m_specularf0.png"

python3 - <<'PY'
import json
from pathlib import Path

path = Path("assets/runtime/models/ak74m/scene.gltf")
data = json.loads(path.read_text())
texture_replacements = {
    "textures/arms_baseColor.png": "textures/arms_baseColor.jpg",
    "textures/ak74m_baseColor.png": "textures/ak74m_baseColor.jpg",
}
for image in data.get("images", []):
    image["uri"] = texture_replacements.get(image["uri"], image["uri"])
path.write_text(json.dumps(data, separators=(",", ":")) + "\n")

names = [item.get("name", "") for item in data.get("animations", [])]
required = [
    "Rig|AK_Idle", "Rig|AK_Draw", "Rig|AK_Walk", "Rig|AK_Run",
    "Rig|AK_Shot", "Rig|AK_Reload_full"
]
missing = [name for name in required if name not in names]
if missing:
    raise SystemExit("AK74M model is missing required animation clips: " + ", ".join(missing))
print("AK74M authored animation clips:", ", ".join(names))
PY

# Correct single-shot field recordings: C_28P is AK-47 near/shooter perspective,
# C_31P is its mid-distance perspective. Preserve stereo and the natural decay;
# do not crop long-burst recordings into artificial one-shot sounds.
download "$AUDIO_BASE/sfx/firearm_sfx/Prepared%20SFX%20Library/AK-47/C_28P.wav" "$TMP/ak_report_near.wav"
download "$AUDIO_BASE/sfx/firearm_sfx/Prepared%20SFX%20Library/AK-47/C_31P.wav" "$TMP/ak_report_mid.wav"
ffmpeg -hide_banner -loglevel error -y -i "$TMP/ak_report_near.wav" \
  -ac 2 -ar 48000 -c:a libvorbis -q:a 6 "$AUDIO_DIR/ak_report_near.ogg"
ffmpeg -hide_banner -loglevel error -y -i "$TMP/ak_report_mid.wav" \
  -ac 2 -ar 48000 -c:a libvorbis -q:a 6 "$AUDIO_DIR/ak_report_mid.ogg"

# Prepare the later M4 profile from single-shot AR-15 / 5.56x45 reference recordings.
download "$AUDIO_BASE/sfx/firearm_sfx/Prepared%20SFX%20Library/AR-15/D_32P.wav" "$TMP/m4_report_near.wav"
download "$AUDIO_BASE/sfx/firearm_sfx/Prepared%20SFX%20Library/AR-15/D_24P.wav" "$TMP/m4_report_mid.wav"
ffmpeg -hide_banner -loglevel error -y -i "$TMP/m4_report_near.wav" \
  -ac 2 -ar 48000 -c:a libvorbis -q:a 6 "$AUDIO_DIR/m4_report_near.ogg"
ffmpeg -hide_banner -loglevel error -y -i "$TMP/m4_report_mid.wav" \
  -ac 2 -ar 48000 -c:a libvorbis -q:a 6 "$AUDIO_DIR/m4_report_mid.ogg"

cat > "$AUDIO_DIR/FIREARM_AUDIO_SOURCES.txt" <<'EOF'
Firearm recording provenance
Collection: The Free Firearm Sound Library
Repository: https://github.com/petroulacl/fps-asset-kit
Original page: https://opengameart.org/content/the-free-firearm-sound-library
Recordists: Ben Jaszczak, Brian Nelson, Kevin Heras, Matthew Nanney
Licence: CC0 1.0

ak_report_near.ogg: AK-47 7.62x39 single shot, near/shooter perspective (C_28P).
ak_report_mid.ogg: AK-47 7.62x39 single shot, mid-distance perspective (C_31P).
m4_report_near.ogg: AR-15 / 5.56x45 single shot, near/shooter perspective (D_32P).
m4_report_mid.ogg: AR-15 / 5.56x45 single shot, mid-distance perspective (D_24P).

The AK-47 7.62x39 recordings are a closest-available CC0 reference for the AK-74M;
they are not represented as exact AK-74M / 5.45x39 recordings. 12.7mm DShK and
M2 recordings still need their own licensed field recordings.
EOF

for pair in "sand_l:Fantozzi-SandL1" "sand_r:Fantozzi-SandR1" "sand_l2:Fantozzi-SandL2" "sand_r2:Fantozzi-SandR2"; do
  name="${pair%%:*}"
  source="${pair#*:}"
  download "$AUDIO_BASE/sfx/footsteps/Fantozzi-footsteps/flac/$source.flac" "$TMP/$name.flac"
  ffmpeg -hide_banner -loglevel error -y -i "$TMP/$name.flac" -ac 1 -ar 44100 -c:a libvorbis -q:a 4 "$AUDIO_DIR/$name.ogg"
done

# Rocky Gravel PBR ground material: diffuse, tangent-space normal, and roughness.
download "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/rocky_gravel/rocky_gravel_diff_1k.jpg" "$TERRAIN_DIR/rocky_gravel_diff_1k.jpg"
download "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/rocky_gravel/rocky_gravel_nor_gl_1k.jpg" "$TERRAIN_DIR/rocky_gravel_nor_gl_1k.jpg"
download "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/rocky_gravel/rocky_gravel_rough_1k.jpg" "$TERRAIN_DIR/rocky_gravel_rough_1k.jpg"

# Photorealistic CC0 ambientCG surface sets, pinned to a known repository revision.
# These five 2K source packs are converted to 1K for predictable mobile/WebGL memory use.
AMBIENTCG_BASE="https://raw.githubusercontent.com/petroulacl/fps-buildings-env-kit/1dd8e932402deb82422920df2e7fd079391e55d3/environment/ground-textures/ambientcg"
prepare_ambientcg_surface() {
  local pack="$1"
  local target="$2"
  local source_folder="${pack}_2K-JPG"
  download "$AMBIENTCG_BASE/$source_folder/${source_folder}_Color.jpg" "$TMP/$target-color.jpg"
  download "$AMBIENTCG_BASE/$source_folder/${source_folder}_NormalGL.jpg" "$TMP/$target-normal.jpg"
  ffmpeg -hide_banner -loglevel error -y -i "$TMP/$target-color.jpg" \
    -vf "scale=1024:1024:force_original_aspect_ratio=decrease" -q:v 2 "$TERRAIN_DIR/northwind_${target}_diff_1k.jpg"
  ffmpeg -hide_banner -loglevel error -y -i "$TMP/$target-normal.jpg" \
    -vf "scale=1024:1024:force_original_aspect_ratio=decrease" -q:v 2 "$TERRAIN_DIR/northwind_${target}_nor_gl_1k.jpg"
}

prepare_ambientcg_surface "Grass004" "grass"
prepare_ambientcg_surface "Ground037" "ground"
prepare_ambientcg_surface "Bricks066" "brick"
prepare_ambientcg_surface "Concrete012" "concrete"
prepare_ambientcg_surface "Road001" "road"

cat > "$TERRAIN_DIR/NORTHWIND_PBR_SOURCES.txt" <<'EOF'
Northwind Valley surface texture sources
AmbientCG CC0: https://ambientcg.com/
Source collection: https://github.com/petroulacl/fps-buildings-env-kit
Pinned source commit: 1dd8e932402deb82422920df2e7fd079391e55d3
Grass004, Ground037, Bricks066, Concrete012, Road001
Diffuse Color and OpenGL Normal maps are resized to 1K for mobile.
The source collection README identifies ambientCG assets as CC0 / Public Domain.
EOF

python3 - <<'PY'
import json
from pathlib import Path

root = Path("assets/runtime/models/ak74m")
data = json.loads((root / "scene.gltf").read_text())
if not (root / "scene.bin").is_file():
    raise SystemExit("AK74M mesh buffer missing")
for image in data.get("images", []):
    if not (root / image["uri"]).is_file():
        raise SystemExit("AK74M texture missing: " + image["uri"])
for path in Path("assets/runtime/audio").glob("*.ogg"):
    if path.stat().st_size < 100:
        raise SystemExit("Invalid audio sample: " + str(path))
for path in Path("assets/runtime/terrain").glob("*"):
    if path.stat().st_size < 100:
        raise SystemExit("Invalid terrain texture: " + str(path))
manifest = {"ak74m": "res://assets/runtime/models/ak74m/scene.gltf"}
Path("assets/runtime/weapon_manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
print("EXTERNAL ASSET PREPARATION PASSED")
print(json.dumps(manifest, indent=2))
PY
