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

    air_object = None
    ground_object = None

    if settings is None or settings.get("variant") == "AIR_AND_GROUND" or settings.get("variant") == "AIR":
        air_object = create_variant("Air", dict.get("VariantAir"), material_set_dict, surface_material_set_list, settings)
    if settings is None or settings.get("variant") == "AIR_AND_GROUND" or settings.get("variant") == "GROUND":
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
        mobil_object = bpy.data.objects.new(f"{name}_Mobils_{i}", None)
        bpy.context.collection.objects.link(mobil_object)
        mobil_object.parent = variant_object

        for j, mobil_dict in enumerate(mobil_list):

            submobil_object = bpy.data.objects.new(f"{name}_Mobil_{i}_{j}", None)
            bpy.context.collection.objects.link(submobil_object)
            submobil_object.parent = mobil_object

            geom_trans = mobil_dict.get("GeomTranslation")
            if geom_trans is not None:
                submobil_object.location = (geom_trans["X"], -geom_trans["Z"], geom_trans["Y"])

            geom_rot = mobil_dict.get("GeomRotation")
            if geom_rot is not None:
                submobil_object.rotation_euler = (
                    math.radians(geom_rot["X"]),
                    math.radians(geom_rot["Z"]),
                    math.radians(geom_rot["Y"])
                )

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

    if settings is None or settings.get("include_editor_helpers", False):
        helper_solid_dict = variant_dict.get("Helper")
        if helper_solid_dict is not None:
            helper_object = solid.create(helper_solid_dict, material_set_dict, surface_material_set_list, settings)
            helper_object.parent = variant_object

    if settings is None or settings.get("include_spawn_point", True):
        spawn_loc = variant_dict.get("SpawnLoc")
        spawn_trans = variant_dict.get("SpawnTrans")
        spawn_pitch = variant_dict.get("SpawnPitch")
        spawn_yaw = variant_dict.get("SpawnYaw")

        if spawn_loc is not None or spawn_trans is not None or spawn_pitch is not None or spawn_yaw is not None:
            spawn_object = bpy.data.objects.new(f"{name}_Spawn", None)
            spawn_object.empty_display_type = 'SINGLE_ARROW'
            spawn_object.empty_display_size = 10.0
            bpy.context.collection.objects.link(spawn_object)
            spawn_object.parent = variant_object
            
            if spawn_loc is not None:
                apply_location(spawn_object, spawn_loc)
                spawn_object.rotation_euler.x += math.radians(90)  # Adjust pitch by 90 degrees
            
            if spawn_trans is not None:
                spawn_object.location = (spawn_trans["X"], -spawn_trans["Z"], spawn_trans["Y"])

            if spawn_pitch is not None:
                spawn_object.rotation_euler.x = math.radians(spawn_pitch + 90)
            
            if spawn_yaw is not None:
                spawn_object.rotation_euler.z = math.radians(spawn_yaw)

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