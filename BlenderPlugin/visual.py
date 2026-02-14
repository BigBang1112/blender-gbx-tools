import bpy

import array
import binascii

def create(visual_dict, name):    
    vert_base64 = visual_dict["Vertices"]
    vert_bytes = binascii.a2b_base64(vert_base64)
    
    vert_floats = array.array('f', vert_bytes)
    
    # make mesh
    vertices = [
        (vert_floats[i], -vert_floats[i+2], vert_floats[i+1])
        for i in range(0, len(vert_floats), 3)
    ]
    
    indices = visual_dict["Indices"]
    
    edges = [] # Edges are not part of Solid.Gbx, Blender doesn't need them luckily
    
    # (0, 1, 2), (3, 4, 5), ...
    faces = [
        (indices[i], indices[i+2], indices[i+1])
        for i in range(0, len(indices), 3)
    ]
    
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces)
    mesh.update(calc_edges=True)
    mesh.flip_normals()

    normals_base64 = visual_dict.get("Normals")
    
    if normals_base64:
        normals_bytes = binascii.a2b_base64(normals_base64)
        normals_floats = array.array('f', normals_bytes)

        # Transform normals with same coordinate system as vertices
        vertex_normals = [
            (normals_floats[i], -normals_floats[i+2], normals_floats[i+1])
            for i in range(0, len(normals_floats), 3)
        ]
    
        mesh.normals_split_custom_set_from_vertices(vertex_normals)

    colors_base64 = visual_dict.get("Colors")

    if colors_base64:
        colors_bytes = binascii.a2b_base64(colors_base64)
        colors_ints = array.array('i', colors_bytes)

        colors = mesh.color_attributes.new(name="Color",type='FLOAT_COLOR', domain='POINT')
        for i in range(0, len(mesh.vertices)):
            color_int = colors_ints[i] & 0xFFFFFFFF
            r = (color_int >> 16) & 0xFF
            g = (color_int >> 8) & 0xFF
            b = color_int & 0xFF
            a = (color_int >> 24) & 0xFF
            colors.data[i].color = (r/255.0, g/255.0, b/255.0, a/255.0);
        
    counter = 0
    # make uv layers TODO rework for proper shader support
    for uv_base64 in visual_dict["TexCoords"]:
        uv_bytes = binascii.a2b_base64(uv_base64)
        uv_floats = array.array('f', uv_bytes)
        uvs = [
            (uv_floats[i], uv_floats[i+1])
            for i in range(0, len(uv_floats), 2)
        ]

        layer_name = "UVMap"
        if counter == 0:
            layer_name = "BaseMaterial"
        elif counter == 1:
            layer_name = "Lightmap"
        counter += 1
            
        uv_layer = mesh.uv_layers.new(name=layer_name)
        
        # apply UV on each vertex through loops
        for loop in mesh.loops:
            uv_layer.data[loop.index].uv = uvs[loop.vertex_index]
    
    return mesh