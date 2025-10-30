import bpy

import binascii
import array
import math
import mathutils

from . import material
from . import solid

def create(dict, settings=None):
    material_set_dict = material.create_multiple(dict.get("Materials"))
    surface_material_set_list = material.create_surface_multiple(dict.get("SurfaceMaterials"))

    air_object = create_variant("Air", dict.get("VariantAir"), material_set_dict, surface_material_set_list, settings)
    ground_object = create_variant("Ground", dict.get("VariantGround"), material_set_dict, surface_material_set_list, settings)

    root_object = bpy.data.objects.new(dict["Name"], None)

    if air_object is not None:
        air_object.parent = root_object
    if ground_object is not None:
        ground_object.parent = root_object

    bpy.context.collection.objects.link(root_object)

def create_variant(name, variant_dict, material_set_dict, surface_material_set_list, settings):
    if variant_dict is None:
        return None
    
    mobils = variant_dict.get("Mobils")
    if mobils is None or len(mobils) == 0:
        return None

    variant_object = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(variant_object)

    for i, mobil_list in enumerate(mobils):
        mobil_object = bpy.data.objects.new(f"MobilSet_{i}", None)
        bpy.context.collection.objects.link(mobil_object)
        mobil_object.parent = variant_object

        for j, mobil_dict in enumerate(mobil_list):

            submobil_object = bpy.data.objects.new(f"Mobil_{i}_{j}", None)
            bpy.context.collection.objects.link(submobil_object)
            submobil_object.parent = mobil_object

            solid_dict = mobil_dict.get("Solid")
            if solid_dict is not None:
                solid_object = solid.create(solid_dict, material_set_dict, surface_material_set_list, settings)
                solid_object.parent = submobil_object

            objectlink_list = mobil_dict.get("ObjectLinks")
            if objectlink_list is not None:
                for k, objectlink_dict in enumerate(objectlink_list):
                    if objectlink_dict.get("Solid") is None:
                        continue
                    objectlink_object = solid.create(objectlink_dict["Solid"], material_set_dict, surface_material_set_list, settings)
                    objectlink_object.parent = submobil_object
                    apply_location(objectlink_object, objectlink_dict.get("Location"))

    waypoint_solid_dict = variant_dict.get("Waypoint")
    if waypoint_solid_dict is not None:
        waypoint_object = solid.create(waypoint_solid_dict, material_set_dict, surface_material_set_list, settings)
        waypoint_object.parent = variant_object

    return variant_object

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
    object.rotation_euler = (euler.x, euler.z, euler.y)