import bpy
import os
import sys

def get_paths():
    if "--" not in sys.argv:
        raise RuntimeError("Expected: -- <input.gltf> <output.fbx>")
    values = sys.argv[sys.argv.index("--") + 1:]
    if len(values) != 2:
        raise RuntimeError("Expected exactly an input glTF and output FBX path")
    return os.path.abspath(values[0]), os.path.abspath(values[1])

source, destination = get_paths()
if not os.path.isfile(source):
    raise FileNotFoundError(source)
os.makedirs(os.path.dirname(destination), exist_ok=True)

bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.gltf(filepath=source)

# The source glTF contains an authored first-person camera transform. Preserve it
# as a named Empty in the FBX so Unity can align the hand/rifle rig to the player
# camera instead of guessing from the combined renderer bounds.
camera_candidates = [
    obj for obj in bpy.context.scene.objects
    if "camera" in obj.name.lower()
]
if not camera_candidates:
    raise RuntimeError("AK-74M source glTF is missing its authored Camera reference node")

source_camera = camera_candidates[0]
if source_camera.type == "EMPTY":
    camera_anchor = source_camera
    camera_anchor.name = "Viewmodel_Camera_Anchor"
else:
    camera_anchor = bpy.data.objects.new("Viewmodel_Camera_Anchor", None)
    bpy.context.scene.collection.objects.link(camera_anchor)
    camera_anchor.matrix_world = source_camera.matrix_world.copy()
    camera_anchor.empty_display_type = "ARROWS"

export_objects = [
    obj for obj in bpy.context.scene.objects
    if obj.type in {"MESH", "ARMATURE", "EMPTY"}
]
if not export_objects:
    raise RuntimeError("AK-74M glTF import returned no exportable objects")
if camera_anchor not in export_objects:
    raise RuntimeError("Authored first-person camera anchor is not included in FBX export objects")

bpy.ops.object.select_all(action="DESELECT")
for obj in export_objects:
    obj.select_set(True)
bpy.context.view_layer.objects.active = next(
    (obj for obj in export_objects if obj.type == "ARMATURE"),
    export_objects[0]
)

# Keep imported actions referenced so the FBX exporter can emit all animation takes.
for action in bpy.data.actions:
    action.use_fake_user = True

bpy.ops.export_scene.fbx(
    filepath=destination,
    use_selection=True,
    object_types={"EMPTY", "MESH", "ARMATURE"},
    apply_scale_options="FBX_SCALE_ALL",
    axis_forward="-Z",
    axis_up="Y",
    add_leaf_bones=False,
    use_mesh_modifiers=True,
    mesh_smooth_type="FACE",
    use_tspace=True,
    path_mode="COPY",
    embed_textures=True,
    bake_anim=True,
    bake_anim_use_all_actions=True,
    bake_anim_force_startend_keying=True,
    bake_anim_step=1.0,
    bake_anim_simplify_factor=0.0,
)

if not os.path.isfile(destination) or os.path.getsize(destination) < 10000:
    raise RuntimeError("Blender did not create a valid AK-74M FBX")
print("AK-74M authored camera anchor preserved:",
      camera_anchor.name, "type=", camera_anchor.type,
      "world_position=", tuple(round(v, 4) for v in camera_anchor.matrix_world.translation))
print("AK-74M FBX conversion passed:", destination, os.path.getsize(destination), "bytes")
