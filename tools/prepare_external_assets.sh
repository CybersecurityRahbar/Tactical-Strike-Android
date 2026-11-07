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

# Real firearm field recordings. Multiple takes reduce the machine-gun-like repeated sample.
for take in C_27P C_28P C_29P C_31P C_34P C_36P; do
  download "$AUDIO_BASE/sfx/firearm_sfx/Prepared%20SFX%20Library/AK-47/$take.wav" "$TMP/$take.wav"
  ffmpeg -hide_banner -loglevel error -y -i "$TMP/$take.wav" -ac 1 -ar 44100 -c:a libvorbis -q:a 4 "$AUDIO_DIR/ak_$take.ogg"
done

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
