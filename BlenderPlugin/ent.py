import bpy
import mathutils

from . import solid2
from . import surface
from . import prefab

def create(dict, name, material_set_dict=None, surface_material_set_list=None, settings=None):
    root_object = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(root_object)

    pos = dict.get("Position")
    if pos is not None:
        root_object.location = (pos["X"], -pos["Z"], pos["Y"])

    rot = dict.get("Rotation")
    if rot is not None:
        # Apply quaternion rotation
        root_object.rotation_mode = 'QUATERNION'
        root_object.rotation_quaternion = mathutils.Quaternion((rot["W"], rot["X"], -rot["Z"], rot["Y"]))
    
    surface_dict = dict.get("Surface")
    if surface_dict is not None:
        surface_name = name + ".Surface"
        surface_object = surface.create(surface_dict, surface_name, surface_material_set_list, settings)  
        if surface_object is not None:
            surface_object.parent = root_object
            surface_object.hide_set(True)
    
    mesh_dict = dict.get("Mesh")
    if mesh_dict is not None:
        object = solid2.create(mesh_dict, material_set_dict, settings)
        if object is not None:
            object.parent = root_object

    prefab_dict = dict.get("Prefab")
    if prefab_dict is not None:
        prefab_object = prefab.create(prefab_dict, material_set_dict, surface_material_set_list, settings)
        if prefab_object is not None:
            prefab_object.parent = root_object

    return root_object