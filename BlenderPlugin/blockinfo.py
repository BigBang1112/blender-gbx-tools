import bpy

from . import material
from . import solid

def create(dict):
    material_set_dict = material.create_multiple(dict.get("Materials"))
    surface_material_set_list = material.create_surface_multiple(dict.get("SurfaceMaterials"))

    air_object = create_variant("Air", dict.get("VariantAir"), material_set_dict, surface_material_set_list)
    ground_object = create_variant("Ground", dict.get("VariantGround"), material_set_dict, surface_material_set_list)

    root_object = bpy.data.objects.new(dict["Name"], None)

    if air_object is not None:
        air_object.parent = root_object
    if ground_object is not None:
        ground_object.parent = root_object

    bpy.context.collection.objects.link(root_object)

def create_variant(name, variant_dict, material_set_dict, surface_material_set_list):
    if variant_dict is None:
        return None

    variant_object = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(variant_object)

    for i, mobil_list in enumerate(variant_dict["Mobils"]):

        mobil_object = bpy.data.objects.new(f"MobilSet_{i}", None)
        bpy.context.collection.objects.link(mobil_object)
        mobil_object.parent = variant_object

        for j, mobil_dict in enumerate(mobil_list):

            submobil_object = bpy.data.objects.new(f"Mobil_{i}_{j}", None)
            bpy.context.collection.objects.link(submobil_object)
            submobil_object.parent = mobil_object

            solid_dict = mobil_dict.get("Solid")
            if solid_dict is not None:
                solid_object = solid.create(solid_dict, material_set_dict, surface_material_set_list)
                solid_object.parent = submobil_object

    return variant_object
