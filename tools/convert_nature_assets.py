import bpy
import os
import sys

NATURE_ASSETS = (
    "tree-pinerounda",
    "tree-pineroundb",
    "tree-oak",
    "tree-default",
    "plant-bushdetailed",
    "grass-large",
)


def parse_arguments():
    if "--" not in sys.argv:
        raise RuntimeError("Expected: -- <input-nature-dir> <output-fbx-dir>")
    args = sys.argv[sys.argv.index("--") + 1:]
    if len(args) != 2:
        raise RuntimeError("Expected exactly an input and output directory")
    return os.path.abspath(args[0]), os.path.abspath(args[1])


def convert_one(source_dir, output_dir, name):
    source = os.path.join(source_dir, name + ".glb")
    destination = os.path.join(output_dir, name + ".fbx")
    if not os.path.isfile(source):
        raise FileNotFoundError(source)

    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=source)
    export_objects = [
        obj for obj in bpy.context.scene.objects
        if obj.type in {"MESH", "EMPTY"}
    ]
    if not any(obj.type == "MESH" for obj in export_objects):
        raise RuntimeError("Nature model contains no mesh: " + source)

    bpy.ops.object.select_all(action="DESELECT")
    for obj in export_objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = next(
        obj for obj in export_objects if obj.type == "MESH"
    )

    bpy.ops.export_scene.fbx(
        filepath=destination,
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

    if not os.path.isfile(destination) or os.path.getsize(destination) < 1000:
        raise RuntimeError("Blender did not create a valid FBX: " + destination)
    print("NATURE FBX PASSED:", name, os.path.getsize(destination), "bytes")


def main():
    source_dir, output_dir = parse_arguments()
    os.makedirs(output_dir, exist_ok=True)
    for asset in NATURE_ASSETS:
        convert_one(source_dir, output_dir, asset)


if __name__ == "__main__":
    main()
