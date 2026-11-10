#!/usr/bin/env python3
"""Validate Unity WebGL output before it is published to GitHub Pages."""

from __future__ import annotations

import gzip
import json
import os
import sys
import zlib
from datetime import datetime, timezone
from pathlib import Path


def require_file(root: Path, patterns: tuple[str, ...], label: str) -> Path:
    matches: list[Path] = []
    for pattern in patterns:
        matches.extend(root.rglob(pattern))
    matches = [path for path in matches if path.is_file() and path.stat().st_size > 0]
    if not matches:
        raise RuntimeError(
            f"Missing required WebGL {label} file; tried {patterns}. "
            "Unity Gzip builds commonly use the .unityweb suffix, not .gz."
        )
    return sorted(matches, key=lambda path: path.stat().st_size, reverse=True)[0]


def verify_gzip(path: Path, label: str) -> int:
    """Read the entire gzip stream so truncated payloads and CRC errors are detected."""
    decompressed_bytes = 0
    try:
        with gzip.open(path, "rb") as stream:
            while chunk := stream.read(1024 * 1024):
                decompressed_bytes += len(chunk)
    except (OSError, EOFError, zlib.error) as exc:
        raise RuntimeError(f"Invalid or truncated gzip data in WebGL {label}: {path}") from exc
    if decompressed_bytes == 0:
        raise RuntimeError(f"WebGL {label} decompressed to an empty file: {path}")
    return decompressed_bytes


def main() -> int:
    if len(sys.argv) != 2:
        raise SystemExit("Usage: validate_web_preview.py <Unity WebGL output directory>")

    root = Path(sys.argv[1]).resolve()
    if not root.is_dir():
        raise SystemExit(f"WebGL output directory does not exist: {root}")

    index = require_file(root, ("index.html",), "entry page")
    data = require_file(root, ("*.data.unityweb", "*.data.gz"), "compressed data")
    framework = require_file(
        root, ("*.framework.js.unityweb", "*.framework.js.gz"), "compressed framework"
    )
    wasm = require_file(root, ("*.wasm.unityweb", "*.wasm.gz"), "compressed WebAssembly")
    loader = require_file(root, ("*.loader.js",), "loader")

    compressed_payloads = {}
    for path, label in ((data, "data"), (framework, "framework"), (wasm, "WebAssembly")):
        compressed_payloads[label] = {
            "file": str(path.relative_to(root)),
            "compressed_bytes": path.stat().st_size,
            "decompressed_bytes": verify_gzip(path, label),
        }

    build_info = {
        "name": "Tactical Strike - Browser Review Build",
        "platform": "Unity WebGL",
        "unity": "6000.3.28f1",
        "render_pipeline": "URP 17.3.0",
        "git_commit": os.environ.get("GITHUB_SHA", "local"),
        "built_at_utc": datetime.now(timezone.utc).isoformat(),
        "compression": "Gzip; Unity uses the .unityweb filename suffix for compressed payloads",
        "files": {
            "index": str(index.relative_to(root)),
            "data": compressed_payloads["data"]["file"],
            "framework": compressed_payloads["framework"]["file"],
            "wasm": compressed_payloads["WebAssembly"]["file"],
            "loader": str(loader.relative_to(root)),
        },
        "payload_sizes": compressed_payloads,
        "controls": {
            "move": "WASD",
            "look": "Mouse",
            "fire": "Left mouse button",
            "aim": "Right mouse button",
            "sprint": "Left Shift",
            "crouch": "C",
            "jump": "Space",
            "reload": "R",
            "ammo_mode": "T",
            "release_cursor": "Escape",
        },
        "verification": (
            "Static file checks and complete gzip decompression/CRC checks passed; "
            "browser rendering and game feel still require manual review."
        ),
    }

    (root / ".nojekyll").write_text("", encoding="utf-8")
    (root / "preview-build.json").write_text(
        json.dumps(build_info, indent=2) + "\n", encoding="utf-8"
    )

    inventory = []
    for path in sorted(p for p in root.rglob("*") if p.is_file()):
        if path.name == "web-preview-inventory.txt":
            continue
        inventory.append(f"{path.stat().st_size:>12}  {path.relative_to(root)}")
    (root / "web-preview-inventory.txt").write_text(
        "\n".join(inventory) + "\n", encoding="utf-8"
    )

    print("WEB PREVIEW VALIDATION PASSED")
    print(json.dumps(build_info, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
