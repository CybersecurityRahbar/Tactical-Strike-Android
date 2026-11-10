#!/usr/bin/env python3
"""Validate Unity WebGL output before it is published to GitHub Pages."""

from __future__ import annotations

import gzip
import json
import os
import sys
from datetime import datetime, timezone
from pathlib import Path


def require_file(root: Path, patterns: tuple[str, ...], label: str) -> Path:
    matches: list[Path] = []
    for pattern in patterns:
        matches.extend(root.rglob(pattern))
    matches = [path for path in matches if path.is_file() and path.stat().st_size > 0]
    if not matches:
        raise RuntimeError(f"Missing required WebGL {label} file; tried {patterns}")
    return sorted(matches, key=lambda path: path.stat().st_size, reverse=True)[0]


def verify_gzip(path: Path, label: str) -> None:
    try:
        with gzip.open(path, "rb") as stream:
            stream.read(64)
    except (OSError, EOFError) as exc:
        raise RuntimeError(f"Invalid gzip data in WebGL {label}: {path}") from exc


def main() -> int:
    if len(sys.argv) != 2:
        raise SystemExit("Usage: validate_web_preview.py <Unity WebGL output directory>")

    root = Path(sys.argv[1]).resolve()
    if not root.is_dir():
        raise SystemExit(f"WebGL output directory does not exist: {root}")

    index = require_file(root, ("index.html",), "entry page")
    data = require_file(root, ("*.data.gz",), "compressed data")
    framework = require_file(root, ("*.framework.js.gz",), "compressed framework")
    wasm = require_file(root, ("*.wasm.gz",), "compressed WebAssembly")
    loader = require_file(root, ("*.loader.js",), "loader")

    for path, label in ((data, "data"), (framework, "framework"), (wasm, "WebAssembly")):
        verify_gzip(path, label)

    build_info = {
        "name": "Tactical Strike - Browser Review Build",
        "platform": "Unity WebGL",
        "unity": "6000.3.28f1",
        "render_pipeline": "URP 17.3.0",
        "git_commit": os.environ.get("GITHUB_SHA", "local"),
        "built_at_utc": datetime.now(timezone.utc).isoformat(),
        "files": {
            "index": str(index.relative_to(root)),
            "data": str(data.relative_to(root)),
            "framework": str(framework.relative_to(root)),
            "wasm": str(wasm.relative_to(root)),
            "loader": str(loader.relative_to(root)),
        },
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
        "verification": "Static file and gzip integrity checks passed; browser rendering and game feel still require manual review.",
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
