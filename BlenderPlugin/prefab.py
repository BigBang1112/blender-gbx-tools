import bpy

from . import material
from . import ent

def create(dict, material_set_dict=None, surface_material_set_list=None, settings=None):
    if material_set_dict is None:
        material_set_dict = material.create_multiple(dict.get("Materials"), settings)
    if surface_material_set_list is None:
        surface_material_set_list = material.create_surface_multiple(dict.get("SurfaceMaterials"))
    
    root_object = bpy.data.objects.new(dict["Name"], None)
    bpy.context.collection.objects.link(root_object)

    ents = dict.get("Ents")
    if ents is None:
        return root_object
    
    for i, ent_dict in enumerate(ents):
        object = ent.create(ent_dict, str(i), material_set_dict, surface_material_set_list, settings)
        if object is not None:
            object.parent = root_object

    return root_object