import bpy

from . import visual

def create(dict, material_set_dict, hide=False):
    name = dict.get("Name")
    children = dict.get("Children")
    visual_dict = dict.get("Visual")
    translation = dict.get("Translation")
    visual_mip = dict.get("VisualMip")
    flags = dict.get("Flags")
    light_dict = dict.get("Light")
    material = dict.get("Material")
    surface_dict = dict.get("Surface")
    
    object_data = None

    # CPlugTree.Visual
    if visual_dict is not None:
        object_data = visual.create(visual_dict, name)
    
    # make object from mesh
    object = bpy.data.objects.new(name, object_data)
    object["Name"] = name; # Custom property

    # CPlugTree.Shader
    if material is not None and object_data is not None:
        mat = material_set_dict.get(material)
        if object.data.materials: # assign to 1st material slot
            object.data.materials[0] = mat
        else: # no slots
            object.data.materials.append(mat)
    
    # LINK OBJECT TO SCENE
    bpy.context.collection.objects.link(object)
    
    # CPlugTree children
    if children is not None:
        for child in children:
            create(child, material_set_dict, hide).parent = object
    
    object.select_set(True)
    object.hide_set(hide)
    
    # CPlugTreeVisualMip
    if visual_mip is not None:
        first = True
        for distance, level in visual_mip.items():
            if first:
                first = False
            else:
                hide = True
            mip_object = create(level, material_set_dict, hide)
            mip_object["Distance"] = float(distance)
            mip_object.name = f"{name} (LOD {distance})"
            mip_object.parent = object
    
    return object