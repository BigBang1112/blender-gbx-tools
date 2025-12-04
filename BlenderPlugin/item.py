import bpy

from . import material
from . import prefab

def create(dict, material_set_dict=None, settings=None):
    if material_set_dict is None:
        material_set_dict = material.create_multiple(dict.get("Materials"), settings)
    
    root_object = bpy.data.objects.new(dict["Name"], None)
    bpy.context.collection.objects.link(root_object)

    prefab_dict = dict.get("Prefab")
    if prefab_dict is not None:
        object = prefab.create(prefab_dict, material_set_dict, None, settings)
        if object is not None:
            object.parent = root_object