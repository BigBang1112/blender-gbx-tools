import bpy

from . import tree
from . import material

def create(dict, material_set_dict=None):
    if material_set_dict is None:
        material_set_dict = material.create_multiple(dict.get("Materials"))

    root_object = bpy.data.objects.new(dict["Name"], None)

    object = tree.create(dict["Tree"], material_set_dict)
    object.parent = root_object

    bpy.context.collection.objects.link(root_object)

    return root_object