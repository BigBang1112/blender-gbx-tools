import bpy

import array
import binascii
import math
import mathutils

from . import visual
from . import light
from . import surface

def create(dict, material_set_dict, surface_material_set_list, hide, settings):
    name = dict.get("Name")
    children = dict.get("Children")
    visual_dict = dict.get("Visual")
    location = dict.get("Location")
    visual_mip = dict.get("VisualMip")
    flags = dict.get("Flags")
    light_dict = dict.get("Light")
    material = dict.get("Material")
    surface_dict = dict.get("Surface")
    
    object_data = None

    # CPlugTree.Visual
    if visual_dict is not None:
        object_data = visual.create(visual_dict, name)
    
    # CPlugTreeLight
    if light_dict is not None:
        object_data = light.create(light_dict, name)
    
    # make object from mesh or light data
    object = bpy.data.objects.new(name, object_data)
    object["Name"] = name; # Custom property
    object["Flags"] = flags; # Custom property

    # CPlugTree.Shader - only apply materials to visual objects (meshes)
    if material is not None and visual_dict is not None:
        mat = material_set_dict.get(material)
        if object.data.materials: # assign to 1st material slot
            object.data.materials[0] = mat
        else: # no slots
            object.data.materials.append(mat)

    # CPlugTree.Surface
    if surface_dict is not None:
        surface_name = name + ".Surface"
        surface_object = surface.create(surface_dict, surface_name, surface_material_set_list)  
        if surface_object is not None:
            surface_object.parent = object
            surface_object.hide_set(True)

    # LINK OBJECT TO SCENE
    bpy.context.collection.objects.link(object)

    # CPlugTree.Location
    if location is not None:
        apply_location(object, location)
    
    # CPlugTree children
    if children is not None:
        for child in children:
            create(child, material_set_dict, surface_material_set_list, hide, settings).parent = object
    
    object.select_set(True)
    object.hide_set(hide)
    
    # CPlugTreeVisualMip
    if visual_mip is not None:
        first = True
        for distance, level in visual_mip.items():
            if first:
                first = False
            elif settings.get("hide_lod", False):
                hide = True
            mip_object = create(level, material_set_dict, surface_material_set_list, hide, settings)
            mip_object["Distance"] = float(distance)
            mip_object.name = f"{name} (LOD {distance})"
            mip_object.parent = object
    
    return object

def apply_location(object, location):
    trans_bytes = binascii.a2b_base64(location)
    iso4 = array.array('f', trans_bytes)
    object.location = (iso4[9], -iso4[11], iso4[10])
    object.scale = (
        math.sqrt(iso4[0]**2+iso4[3]**2+iso4[6]**2),
        math.sqrt(iso4[2]**2+iso4[5]**2+iso4[8]**2),
        math.sqrt(iso4[1]**2+iso4[4]**2+iso4[7]**2)
    )
    
    rot_mat = mathutils.Matrix([
        [iso4[0], iso4[1], iso4[2]],
        [iso4[3], iso4[4], iso4[5]],
        [iso4[6], iso4[7], iso4[8]],
    ]).to_3x3()
    
    euler = rot_mat.to_euler()
    object.rotation_euler = (-euler.x, euler.z, euler.y)