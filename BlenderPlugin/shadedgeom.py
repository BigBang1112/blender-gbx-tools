import bpy

from . import visual

def create(dict, name, material_set_dict=None, settings=None):
    visual_dict = dict.get("Visual")
    material = dict.get("Material")
    lod = dict.get("Lod")

    hide = lod > 1
    
    if visual_dict is None:
        return None
    
    object_data = visual.create(visual_dict, name)
    
    # make object from mesh
    object = bpy.data.objects.new(name, object_data)
    object["Lod"] = lod; # Custom property
    
    if material is not None and object_data is not None:
        mat = material_set_dict.get(material)
        if object.data.materials: # assign to 1st material slot
            object.data.materials[0] = mat
        else: # no slots
            object.data.materials.append(mat)
    
    # LINK OBJECT TO SCENE
    bpy.context.collection.objects.link(object)
    
    object.select_set(True)
    object.hide_set(hide)
    
    return object