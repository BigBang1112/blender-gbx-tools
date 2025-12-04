import bpy

from . import material
from . import shadedgeom

def create(dict, material_set_dict=None, settings=None):
    if material_set_dict is None:
        material_set_dict = material.create_multiple(dict.get("Materials"), settings)
    
    root_object = bpy.data.objects.new(dict["Name"], None)
    bpy.context.collection.objects.link(root_object)

    shaded_geoms = dict.get("ShadedGeoms")
    if shaded_geoms is None:
        return root_object
    
    for i, visual in enumerate(shaded_geoms):
        object = shadedgeom.create(visual, str(i), material_set_dict, settings)
        if object is not None:
            object.parent = root_object

    return root_object