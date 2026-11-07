#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
BASE="https://raw.githubusercontent.com/petroulacl/fps-asset-kit/a19b7458a593598211c95ec46ef4eb4b6d1f94d7"
mkdir -p assets/runtime/models/ak74 assets/runtime/models/m4a1 assets/runtime/audio assets/runtime/terrain
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

download() {
  local url="$1"
  local output="$2"
  echo "Downloading: $url"
  curl --fail --location --retry 3 --retry-delay 2 --user-agent "TacticalStrikeAssetBuilder/1.0" "$url" -o "$output"
  test -s "$output"
}

download "https://opengameart.org/sites/default/files/ak_godotfiles_0.zip" "$TMP/ak_godotfiles.zip"
download "https://opengameart.org/sites/default/files/m4a1.zip" "$TMP/m4a1.zip"
unzip -q -o "$TMP/ak_godotfiles.zip" -d assets/runtime/models/ak74
unzip -q -o "$TMP/m4a1.zip" -d assets/runtime/models/m4a1

download "$BASE/sfx/firearm_sfx/Prepared%20SFX%20Library/AK-47/C_27P.wav" "$TMP/ak47_27.wav"
download "$BASE/sfx/firearm_sfx/Prepared%20SFX%20Library/AK-47/C_31P.wav" "$TMP/ak47_31.wav"
download "$BASE/sfx/firearm_sfx/Prepared%20SFX%20Library/AK-47/C_36P.wav" "$TMP/ak47_36.wav"
download "$BASE/sfx/firearm_sfx/Prepared%20SFX%20Library/AR-15/D_24P.wav" "$TMP/ar15_24.wav"
download "$BASE/sfx/firearm_sfx/Prepared%20SFX%20Library/AR-15/D_32P.wav" "$TMP/ar15_32.wav"
download "$BASE/sfx/firearm_sfx/Prepared%20SFX%20Library/Tikka/W_24P.wav" "$TMP/tikka_24.wav"
download "$BASE/sfx/footsteps/Fantozzi-footsteps/flac/Fantozzi-SandL1.flac" "$TMP/sand_l.flac"
download "$BASE/sfx/footsteps/Fantozzi-footsteps/flac/Fantozzi-SandR1.flac" "$TMP/sand_r.flac"
download "$BASE/sfx/footsteps/Fantozzi-footsteps/flac/Fantozzi-SandL2.flac" "$TMP/sand_l2.flac"
download "$BASE/sfx/footsteps/Fantozzi-footsteps/flac/Fantozzi-SandR2.flac" "$TMP/sand_r2.flac"

command -v ffmpeg >/dev/null 2>&1 || { echo "ffmpeg is required to prepare audio assets"; exit 1; }
for pair in "ak47_27:$TMP/ak47_27.wav" "ak47_31:$TMP/ak47_31.wav" "ak47_36:$TMP/ak47_36.wav" "ar15_24:$TMP/ar15_24.wav" "ar15_32:$TMP/ar15_32.wav" "tikka_24:$TMP/tikka_24.wav" "sand_l:$TMP/sand_l.flac" "sand_r:$TMP/sand_r.flac" "sand_l2:$TMP/sand_l2.flac" "sand_r2:$TMP/sand_r2.flac"; do
  name="${pair%%:*}"
  source="${pair#*:}"
  ffmpeg -hide_banner -loglevel error -y -i "$source" -ac 1 -ar 44100 -c:a libvorbis -q:a 4 "assets/runtime/audio/${name}.ogg"
done

download "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/rocky_gravel/rocky_gravel_diff_1k.jpg" "assets/runtime/terrain/rocky_gravel_diff_1k.jpg"
download "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/rocky_gravel/rocky_gravel_nor_gl_1k.jpg" "assets/runtime/terrain/rocky_gravel_nor_gl_1k.jpg"
download "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/rocky_gravel/rocky_gravel_rough_1k.jpg" "assets/runtime/terrain/rocky_gravel_rough_1k.jpg"

python3 - <<'PY'
import json
from pathlib import Path

root = Path("assets/runtime/models")
def choose(folder: Path, tokens: tuple[str, ...]) -> str:
    files = [p for p in folder.rglob("*") if p.is_file() and p.suffix.lower() in {".glb", ".gltf", ".fbx", ".obj"}]
    if not files:
        raise SystemExit(f"No supported 3D model found in {folder}")
    def score(p: Path):
        name = p.name.lower()
        ext_rank = {".glb": 4, ".gltf": 3, ".fbx": 2, ".obj": 1}.get(p.suffix.lower(), 0)
        token_rank = 1 if any(t in name for t in tokens) else 0
        return (token_rank, ext_rank, p.stat().st_size)
    selected = max(files, key=score)
    return "res://" + selected.as_posix()

manifest = {
    "ak74": choose(root / "ak74", ("ak", "kalash")),
    "m4a1": choose(root / "m4a1", ("m4", "carbine")),
}
Path("assets/runtime/weapon_manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
print("Prepared weapon model manifest:")
print(json.dumps(manifest, indent=2))
PY

python3 - <<'PY'
from pathlib import Path
required = [
    "assets/runtime/audio/ak47_27.ogg", "assets/runtime/audio/ak47_31.ogg",
    "assets/runtime/audio/ak47_36.ogg", "assets/runtime/audio/ar15_24.ogg",
    "assets/runtime/audio/ar15_32.ogg", "assets/runtime/audio/tikka_24.ogg",
    "assets/runtime/audio/sand_l.ogg", "assets/runtime/audio/sand_r.ogg",
    "assets/runtime/audio/sand_l2.ogg", "assets/runtime/audio/sand_r2.ogg",
    "assets/runtime/terrain/rocky_gravel_diff_1k.jpg",
    "assets/runtime/terrain/rocky_gravel_nor_gl_1k.jpg",
    "assets/runtime/terrain/rocky_gravel_rough_1k.jpg",
]
for path in required:
    p = Path(path)
    if not p.is_file() or p.stat().st_size < 100:
        raise SystemExit(f"Missing or invalid external asset: {path}")
print("EXTERNAL ASSET PREPARATION PASSED")
PY
