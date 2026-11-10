#!/usr/bin/env python3
"""Download pinned-size Poly Haven 1K glTF models and their referenced files.

The model IDs are stable Poly Haven asset slugs. The API response supplies the
current 1K glTF URL and exact relative include map; every referenced file is
stored under its original relative path for deterministic Blender import.
"""
from __future__ import annotations

import hashlib
import json
import sys
import time
from pathlib import Path
from urllib.error import HTTPError, URLError
from urllib.parse import urlparse
from urllib.request import Request, urlopen

ASSETS = (
    "concrete_road_barrier",
    "old_military_crate",
    "ammo_box",
    "metal_jerrycan_green",
)

USER_AGENT = "TacticalStrikeAssetBuilder/1.0 (Unity 6.3; PolyHaven CC0 assets)"


def fetch(url: str, retries: int = 4) -> bytes:
    last_error: Exception | None = None
    for attempt in range(retries):
        try:
            request = Request(url, headers={"User-Agent": USER_AGENT})
            with urlopen(request, timeout=90) as response:
                data = response.read()
            if not data:
                raise RuntimeError("Empty response from " + url)
            return data
        except (HTTPError, URLError, TimeoutError, RuntimeError) as exc:
            last_error = exc
            if attempt + 1 < retries:
                time.sleep(2 ** attempt)
    raise RuntimeError(f"Download failed after {retries} attempts: {url}: {last_error}")


def safe_relative_path(value: str) -> Path:
    path = Path(value)
    if path.is_absolute() or ".." in path.parts:
        raise RuntimeError("Unsafe relative path in Poly Haven manifest: " + value)
    return path


def download_asset(asset_id: str, output_root: Path, records: list[dict]) -> None:
    metadata_url = f"https://api.polyhaven.com/files/{asset_id}"
    metadata = json.loads(fetch(metadata_url).decode("utf-8"))
    try:
        model = metadata["gltf"]["1k"]["gltf"]
        model_url = model["url"]
    except (KeyError, TypeError) as exc:
        raise RuntimeError(f"Poly Haven 1K glTF variant missing for {asset_id}") from exc

    asset_dir = output_root / asset_id
    asset_dir.mkdir(parents=True, exist_ok=True)

    def save(url: str, relative: Path) -> None:
        data = fetch(url)
        destination = asset_dir / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        destination.write_bytes(data)
        digest = hashlib.sha256(data).hexdigest()
        records.append({
            "asset": asset_id,
            "path": str(destination.relative_to(output_root)),
            "url": url,
            "bytes": len(data),
            "sha256": digest,
        })
        print(f"POLYHAVEN FILE PASSED: {asset_id}/{relative} ({len(data)} bytes)")

    model_name = Path(urlparse(model_url).path).name
    save(model_url, Path(model_name))

    include = model.get("include", {})
    for relative, entry in sorted(include.items()):
        if not isinstance(entry, dict) or not entry.get("url"):
            raise RuntimeError(f"Invalid include entry for {asset_id}: {relative}")
        save(entry["url"], safe_relative_path(relative))


def main() -> None:
    if len(sys.argv) != 2:
        raise SystemExit("Usage: download_polyhaven_models.py <output-directory>")
    output_root = Path(sys.argv[1]).resolve()
    output_root.mkdir(parents=True, exist_ok=True)

    records: list[dict] = []
    for asset_id in ASSETS:
        download_asset(asset_id, output_root, records)

    report = [
        "Northwind Valley battlefield prop sources",
        "Source library: https://polyhaven.com/models",
        "Asset license: CC0 1.0 / public domain equivalent",
        "Official license: https://polyhaven.com/license",
        "",
        "Assets downloaded in the 1K glTF variant through the Poly Haven files API:",
        *[
            f"- {asset}: https://polyhaven.com/a/{asset}"
            for asset in ASSETS
        ],
        "",
        "Per-file SHA-256 manifest:",
        *[
            f"{r['sha256']}  {r['path']}  ({r['bytes']} bytes)"
            for r in records
        ],
    ]
    (output_root / "POLYHAVEN_PROPS_SOURCES.txt").write_text(
        "\n".join(report) + "\n", encoding="utf-8"
    )
    print("POLYHAVEN BATTLEFIELD PROP DOWNLOAD PASSED:", len(records), "files")


if __name__ == "__main__":
    main()
