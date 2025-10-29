import bpy

from . import tree
from . import material

def create(dict, material_set_dict=None, surface_material_set_list=None):
    if material_set_dict is None:
        material_set_dict = material.create_multiple(dict.get("Materials"))
    if surface_material_set_list is None:
        surface_material_set_list = material.create_surface_multiple(dict.get("SurfaceMaterials"))

    root_object = bpy.data.objects.new(dict["Name"], None)

    object = tree.create(dict["Tree"], material_set_dict, surface_material_set_list)
    object.parent = root_object

    bpy.context.collection.objects.link(root_object)

    return root_object