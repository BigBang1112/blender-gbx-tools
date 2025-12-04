import bpy
import bmesh

import array
import binascii

from . import material

def create(surface_dict, name, surface_material_set_list, settings=None):
    if surface_material_set_list is None:
        surface_material_set_list = material.create_surface_multiple(surface_dict.get("SurfaceMaterials"))
    
    ellipsoid = surface_dict.get("Ellipsoid")
    sphere = surface_dict.get("Sphere")
    compound = surface_dict.get("Compound")
    positions = surface_dict.get("Positions")
    indices = surface_dict.get("Indices")
    surfaceIndex = surface_dict.get("SurfaceIndex")
    if name is None:
        name = surface_dict.get("Name")

    mesh = None
    scale = (1, 1, 1)

    if ellipsoid is not None:
        mesh = bpy.data.meshes.new(name)
        scale = (ellipsoid["X"], ellipsoid["Z"], ellipsoid["Y"])
        
        bm = bmesh.new()
        bmesh.ops.create_uvsphere(bm, u_segments=32, v_segments=16, radius=1)
        bm.to_mesh(mesh)
        bm.free()
        # Enable smooth shading
        for poly in mesh.polygons:
            poly.use_smooth = True
    
    if sphere is not None:
        mesh = bpy.data.meshes.new(name)
        scale = (sphere, sphere, sphere)
        
        bm = bmesh.new()
        bmesh.ops.create_uvsphere(bm, u_segments=32, v_segments=16, radius=1)
        bm.to_mesh(mesh)
        bm.free()
        # Enable smooth shading
        for poly in mesh.polygons:
            poly.use_smooth = True

    if positions is not None and indices is not None:
        vert_bytes = binascii.a2b_base64(positions)
        vert_floats = array.array('f', vert_bytes)
        # make mesh
        vertices = [
            (vert_floats[i], -vert_floats[i+2], vert_floats[i+1])
            for i in range(0, len(vert_floats), 3)
        ]

        faces = [
            (indices[i+1], indices[i+3], indices[i+2])
            for i in range(0, len(indices), 4)
        ]

        surfaceIndices = [
            indices[i] for i in range(0, len(indices), 4)
        ]

        mesh = bpy.data.meshes.new(name)
        mesh.from_pydata(vertices, [], faces)
        
        mesh.update(calc_edges=True)
        
        # Apply materials based on surface indices
        if surface_material_set_list:
            # Apply materials per face using surfaceIndices
            applied_materials = {}
            
            for face_index, surface_idx in enumerate(surfaceIndices):
                if surface_idx < len(surface_material_set_list):
                    surf_mat = surface_material_set_list[surface_idx]
                    
                    # Track which materials we've added
                    if surf_mat not in applied_materials:
                        mesh.materials.append(surf_mat)
                        applied_materials[surf_mat] = len(mesh.materials) - 1
                    
                    # Assign material to this face
                    if face_index < len(mesh.polygons):
                        mesh.polygons[face_index].material_index = applied_materials[surf_mat]
    # Apply global surface material if no per-face indices
    elif surfaceIndex is not None and surface_material_set_list:
        if surfaceIndex < len(surface_material_set_list):
            surf_mat = surface_material_set_list[surfaceIndex]
            mesh.materials.append(surf_mat)
            # All faces will use material index 0 by default

    surface_object = bpy.data.objects.new(name, mesh)
    surface_object.scale = scale
    surface_object["Surface"] = True  # Custom property to identify surface objects
    bpy.context.collection.objects.link(surface_object)

    return surface_object