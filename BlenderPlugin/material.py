import bpy

import random

from . import texture

def create_multiple(material_set_dict, settings=None):
    if material_set_dict is None:
        return None
    
    materials = {}
    
    for material_name, material_dict in material_set_dict.items():
        # Check if we should reuse existing material
        if settings and settings.get("reuse_existing_materials", False):
            existing_mat = bpy.data.materials.get(material_name)
            if existing_mat:
                materials[material_name] = existing_mat
                continue
        
        mat = bpy.data.materials.new(material_name)
        mat.use_nodes = True
        materials[material_name] = mat

        if material_dict is None:
            continue

        mat["SurfaceId"] = material_dict.get("SurfaceId")
        mat["Shader"] = shader_name = material_dict.get("Shader")

        textures_dict = material_dict.get("Textures")

        tex_index = 0
        tex_length = len(textures_dict) if textures_dict else 0
        tex_height_dist = 50

        diffuse_texture_node = None
        blend_mix_node = None
        overlay_mix_node = None
        global_uv_node = None

        principled_bsdf_node = mat.node_tree.nodes.get("Principled BSDF")

        # Add global UV nodes
        if shader_name in [
            "PDiff PDiff PA PX2", # typical blend
            "PDiff PDiff PA TDiffA PX2", # blend with Blend3
            "SoilGen21", # extra global dirt in Stadium
            "Tech3 Block PDiff_Spec_Norm", # tm2 blend
            "Tech3 Block PDiff_Spec_Norm GrassX2", # tm2 grass
            "Tech3 Block PTDiff_Spec_Norm PGrassX2", # tm2 grass with DiffuseBlendA
            "PDiff Fresnel TDiffA PX2", # TMUF ice
            "SoilFixToGen21",
            "PDisp PDiff PX2",
            "PDisp PDiff TDiffA PX2"
            ]:
            global_uv_node = add_global_uv_nodes(mat)

        if textures_dict is not None:
            for texture_name, texture_dict in textures_dict.items():
                texture_node = create_texture(texture_name, texture_dict, mat, settings)
                texture_node.location = (principled_bsdf_node.location[0] - 1000, tex_index*tex_height_dist-tex_length/2*tex_height_dist)
                
                scale_u = texture_dict.get("ScaleU", 1.0)
                scale_v = texture_dict.get("ScaleV", 1.0)

                if scale_u != 1.0 or scale_v != 1.0:
                    tex_coord_node = get_or_create_tex_coord_node(mat)
                    
                    # Add Vector Math Multiply node to scale the texture
                    vector_math_node = mat.node_tree.nodes.new("ShaderNodeVectorMath")
                    vector_math_node.operation = "MULTIPLY"
                    vector_math_node.location = (texture_node.location[0] - 200, texture_node.location[1])
                    vector_math_node.inputs[1].default_value = (scale_u, scale_v, 1.0)
                    vector_math_node.hide = True
                    
                    # Connect global UV or regular UV to the scaling node
                    if global_uv_node and texture_name != "Blend3":
                        mat.node_tree.links.new(vector_math_node.inputs[0], global_uv_node.outputs["Vector"])
                    else:
                        mat.node_tree.links.new(vector_math_node.inputs[0], tex_coord_node.outputs["UV"])
                    
                    mat.node_tree.links.new(texture_node.inputs["Vector"], vector_math_node.outputs["Vector"])
                elif global_uv_node and texture_name not in ["Blend3", "Borders", "DiffuseBlendA"]:
                    # Connect global UV directly if no scaling is needed
                    mat.node_tree.links.new(texture_node.inputs["Vector"], global_uv_node.outputs["Vector"])
                
                tex_index += 1

                if texture_name in ["Blend1", "Blend2", "BlendI"]:
                    if blend_mix_node is None:
                        blend_mix_node = mat.node_tree.nodes.new("ShaderNodeMix")
                        blend_mix_node.label = "Blend Mix"
                        blend_mix_node.data_type = "RGBA"
                        blend_mix_node.location = (principled_bsdf_node.location[0] - 400, 200)

                    if texture_name == "Blend1":
                        mat.node_tree.links.new(blend_mix_node.inputs[6], texture_node.outputs["Color"])
                    elif texture_name == "Blend2":
                        mat.node_tree.links.new(blend_mix_node.inputs[7], texture_node.outputs["Color"])
                    elif texture_name == "BlendI":
                        mat.node_tree.links.new(blend_mix_node.inputs["Factor"], texture_node.outputs["Color"])

                if overlay_mix_node is None and texture_name in ["Blend3", "SoilFix", "Borders", "DiffuseBlendA"]:
                    overlay_mix_node = mat.node_tree.nodes.new("ShaderNodeMix")
                    overlay_mix_node.label = "Overlay Mix"
                    overlay_mix_node.data_type = "RGBA"
                    overlay_mix_node.location = (principled_bsdf_node.location[0] - 200, -100)
                    mat.node_tree.links.new(overlay_mix_node.inputs[7], texture_node.outputs["Color"])
                    mat.node_tree.links.new(overlay_mix_node.inputs["Factor"], texture_node.outputs["Alpha"])

                if texture_name in ["Diffuse", "PxzDiffuse", "Soil", "Advert", "BaseColor", "BaseColorOp", "BaseColorSurface", "BaseColor"]:
                    mat.node_tree.links.new(principled_bsdf_node.inputs["Base Color"], texture_node.outputs["Color"])
                    
                    # For CSpecL shaders, connect Alpha to Specular
                    if "CSpecL" in shader_name or shader_name in ["TDiff PX2"]:
                        mat.node_tree.links.new(principled_bsdf_node.inputs[13], texture_node.outputs["Alpha"])
                    
                    # For transparent shaders, connect Alpha to Alpha
                    if "Trans" in shader_name or "Alpha" in shader_name or shader_name in ["TDiff_Spec_Nrm TOcc CSpecSoft", "Tech3 Block TDiffA_Spec_Norm"]:
                        mat.node_tree.links.new(principled_bsdf_node.inputs["Alpha"], texture_node.outputs["Alpha"])
                    
                    # For TAdd Night ZBias shader, add RGB to BW conversion for alpha
                    if shader_name in ["TAdd Night", "TAdd Night ZBias"]:
                        rgb_to_bw_node = mat.node_tree.nodes.new("ShaderNodeRGBToBW")
                        rgb_to_bw_node.location = (texture_node.location[0] + 100, texture_node.location[1] - 100)
                        mat.node_tree.links.new(rgb_to_bw_node.inputs["Color"], texture_node.outputs["Color"])
                        mat.node_tree.links.new(principled_bsdf_node.inputs["Alpha"], rgb_to_bw_node.outputs["Val"])
                    
                    # Connect global UV for SoilGen21 shader
                    if global_uv_node and shader_name in ["SoilGen21"]:
                        mat.node_tree.links.new(texture_node.inputs["Vector"], global_uv_node.outputs["Vector"])

                    diffuse_texture_node = texture_node

                elif texture_name == "Normal" and shader_name not in [
                    # TM2 shader that need complex normal handling
                    "Tech3 Block PDiff_Spec_Norm GrassX2",
                    "Tech3 Block TDiff_Spec_Norm"
                    ] and "TSDN" not in shader_name:
                    normal_map_node = apply_normal_map_tmuf(mat, texture_node)
                    mat.node_tree.links.new(principled_bsdf_node.inputs["Normal"], normal_map_node.outputs["Normal"])

                elif texture_name == "BlendI" and texture_node.image is not None:
                    texture_node.image.colorspace_settings.name = "Non-Color"

        # TODO this part needs to be rewritten
        if overlay_mix_node is not None:
            mat.node_tree.links.new(principled_bsdf_node.inputs["Base Color"], overlay_mix_node.outputs[2])
            if shader_name == "Tech3 Block PTDiff_Spec_Norm PGrassX2" or shader_name == "PDisp PDiff TDiffA PX2":
                mat.node_tree.links.new(overlay_mix_node.inputs[6], diffuse_texture_node.outputs["Color"])
            elif blend_mix_node is not None:
                mat.node_tree.links.new(overlay_mix_node.inputs[6], blend_mix_node.outputs[2])
        elif blend_mix_node is not None:
            mat.node_tree.links.new(principled_bsdf_node.inputs["Base Color"], blend_mix_node.outputs[2])

    return materials

def create_surface_multiple(surface_material_set_list):
    if surface_material_set_list is None:
        return None

    materials = []

    for material in surface_material_set_list:
        material_name = material.get("Name", material.get("SurfaceId")) + ".Surface"
        surface_id = material.get("SurfaceId")
        if surface_id is None:
            surface_mat = material.get("Material")
            if surface_mat:
                surface_id = surface_mat.get("SurfaceId")

        mat = bpy.data.materials.new(material_name)
        mat.use_nodes = True
        mat["SurfaceId"] = surface_id

        # Enable wireframe display
        mat.use_backface_culling = False
        
        # Set up wireframe material with random color
        random_color = (random.random(), random.random(), random.random(), 1.0)
        
        # Get the principled BSDF node
        bsdf = mat.node_tree.nodes["Principled BSDF"]
        bsdf.inputs["Base Color"].default_value = random_color
        bsdf.inputs["Metallic"].default_value = 0.0
        bsdf.inputs["Roughness"].default_value = 0.8
        
        # Add wireframe node
        wireframe_node = mat.node_tree.nodes.new("ShaderNodeWireframe")
        wireframe_node.location = (-600, 0)
        wireframe_node.inputs["Size"].default_value = 0.1  # Wireframe thickness
        
        # Add math node to brighten wireframe
        math_add_node = mat.node_tree.nodes.new("ShaderNodeMath")
        math_add_node.operation = 'ADD'
        math_add_node.location = (-300, 0)
        math_add_node.inputs[1].default_value = 0.5
        
        # Connect wireframe to math add, then to alpha
        mat.node_tree.links.new(math_add_node.inputs[0], wireframe_node.outputs["Fac"])
        mat.node_tree.links.new(bsdf.inputs["Alpha"], math_add_node.outputs["Value"])

        materials.append(mat)

    return materials

def create_texture(texture_name, texture_dict, mat, settings):
    texture_node, mapping_node = texture.create(texture_dict, mat, settings)
    texture_node.label = texture_name
    return texture_node

def get_or_create_tex_coord_node(mat):
    tex_coord_node = mat.node_tree.nodes.get("Texture Coordinate")
    if tex_coord_node is None:
        tex_coord_node = mat.node_tree.nodes.new("ShaderNodeTexCoord")
        tex_coord_node.location = (-2000, 0)
    return tex_coord_node

def add_global_uv_nodes(mat):
    tex_coord_node = get_or_create_tex_coord_node(mat)

    vector_transform_node = mat.node_tree.nodes.new("ShaderNodeVectorTransform")
    vector_transform_node.convert_from = "OBJECT"
    vector_transform_node.convert_to = "WORLD"
    vector_transform_node.location = (tex_coord_node.location[0] + 200, tex_coord_node.location[1])
    mat.node_tree.links.new(vector_transform_node.inputs["Vector"], tex_coord_node.outputs["Object"])

    object_info_node = mat.node_tree.nodes.new("ShaderNodeObjectInfo")
    object_info_node.location = (vector_transform_node.location[0], vector_transform_node.location[1] - 200)

    vector_math_add_node = mat.node_tree.nodes.new("ShaderNodeVectorMath")
    vector_math_add_node.location = (vector_transform_node.location[0] + 200, vector_transform_node.location[1])
    mat.node_tree.links.new(vector_math_add_node.inputs[0], vector_transform_node.outputs["Vector"])
    mat.node_tree.links.new(vector_math_add_node.inputs[1], object_info_node.outputs["Location"])

    return vector_math_add_node

def apply_normal_map_tmuf(mat, img_tex_node):
    #img_tex_node.image.colorspace_settings.name = "Non-Color"

    separate_color_node = mat.node_tree.nodes.new("ShaderNodeSeparateColor")
    separate_color_node.location = (img_tex_node.location[0] + 300, img_tex_node.location[1] - 100)
    mat.node_tree.links.new(separate_color_node.inputs["Color"], img_tex_node.outputs["Color"])

    combine_color_node = mat.node_tree.nodes.new("ShaderNodeCombineColor")
    combine_color_node.location = (separate_color_node.location[0] + 200, separate_color_node.location[1] + 100)
    mat.node_tree.links.new(combine_color_node.inputs["Red"], img_tex_node.outputs["Alpha"])
    mat.node_tree.links.new(combine_color_node.inputs["Green"], separate_color_node.outputs["Green"])
    mat.node_tree.links.new(combine_color_node.inputs["Blue"], separate_color_node.outputs["Blue"])

    normal_map_node = mat.node_tree.nodes.new("ShaderNodeNormalMap")
    normal_map_node.inputs[0].default_value = 0.5  # Strength
    normal_map_node.location = (combine_color_node.location[0] + 200, combine_color_node.location[1])
    mat.node_tree.links.new(normal_map_node.inputs["Color"], combine_color_node.outputs["Color"])

    return normal_map_node