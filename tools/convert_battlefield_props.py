#!/usr/bin/env python3
"""Convert downloaded 1K Poly Haven glTF props into Unity-ready FBX assets."""
from __future__ import annotations

import os
import sys
from pathlib import Path

import bpy
from mathutils import Vector

PROPS = (
    "concrete_road_barrier",
    "old_military_crate",
    "ammo_box",
    "metal_jerrycan_green",
)


def parse_arguments() -> tuple[Path, Path]:
    if "--" not in sys.argv:
        raise RuntimeError("Expected: -- <input-props-dir> <output-fbx-dir>")
    args = sys.argv[sys.argv.index("--") + 1:]
    if len(args) != 2:
        raise RuntimeError("Expected exactly an input and output directory")
    return Path(args[0]).resolve(), Path(args[1]).resolve()


def scene_bounds(objects: list) -> tuple[Vector, Vector]:
    points: list[Vector] = []
    bpy.context.view_layer.update()
    for obj in objects:
        if obj.type != "MESH":
            continue
        points.extend(obj.matrix_world @ Vector(corner) for corner in obj.bound_box)
    if not points:
        raise RuntimeError("Imported prop contains no renderable mesh bounds")
    minimum = Vector((min(p.x for p in points), min(p.y for p in points), min(p.z for p in points)))
    maximum = Vector((max(p.x for p in points), max(p.y for p in points), max(p.z for p in points)))
    return minimum, maximum


def convert_one(input_root: Path, output_root: Path, asset: str) -> None:
    asset_dir = input_root / asset
    candidates = sorted(asset_dir.glob("*.gltf")) + sorted(asset_dir.glob("*.glb"))
    if len(candidates) != 1:
        raise RuntimeError(f"Expected one glTF/GLB file for {asset}; found {len(candidates)}")
    source = candidates[0]
    destination = output_root / (asset + ".fbx")
    destination.parent.mkdir(parents=True, exist_ok=True)

    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(source))
    objects = [obj for obj in bpy.context.scene.objects if obj.type in {"MESH", "EMPTY"}]
    meshes = [obj for obj in objects if obj.type == "MESH"]
    if not meshes:
        raise RuntimeError("Poly Haven prop contains no mesh: " + str(source))

    minimum, maximum = scene_bounds(meshes)
    dimensions = maximum - minimum

    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]

    bpy.ops.export_scene.fbx(
        filepath=str(destination),
        use_selection=True,
        object_types={"EMPTY", "MESH"},
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        add_leaf_bones=False,
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        path_mode="COPY",
        embed_textures=True,
        bake_anim=False,
    )

    if not destination.is_file() or destination.stat().st_size < 1000:
        raise RuntimeError("Blender did not create a valid FBX: " + str(destination))

    print(
        "BATTLEFIELD PROP FBX PASSED:",
        asset,
        "dimensions(m)=",
        tuple(round(v, 3) for v in dimensions),
        "meshes=",
        len(meshes),
        "bytes=",
        destination.stat().st_size,
    )


def main() -> None:
    input_root, output_root = parse_arguments()
    if not input_root.is_dir():
        raise FileNotFoundError(str(input_root))
    output_root.mkdir(parents=True, exist_ok=True)
    for asset in PROPS:
        convert_one(input_root, output_root, asset)
    print("BATTLEFIELD PROP CONVERSION PASSED:", len(PROPS), "props")


if __name__ == "__main__":
    main()
