import bpy

from . import texture

def create_multiple(material_set_dict):
    if material_set_dict is None:
        return None
    
    materials = {}
    
    for material_name, material_dict in material_set_dict.items():
        mat = bpy.data.materials.new(material_name)
        mat.use_nodes = True
        mat["SurfaceId"] = material_dict.get("SurfaceId")
        mat["Shader"] = material_dict.get("Shader")

        textures_dict = material_dict.get("Textures")

        tex_index = 0
        tex_length = len(textures_dict)
        tex_height_dist = 50

        if textures_dict is not None:
            for texture_name, texture_dict in textures_dict.items():
                texture_node = create_texture(texture_name, texture_dict, mat)
                texture_node.location = (-1000, tex_index*tex_height_dist-tex_length/2*tex_height_dist)
                tex_index += 1

        materials[material_name] = mat

    return materials

def create_texture(texture_name, texture_dict, mat):
    texture_node = texture.create(texture_dict, mat)
    texture_node.label = texture_name
    
    if texture_name == "Diffuse":
        apply_diffuse_map(mat, texture_node)
    #elif texture_name == "Normal":
    #    apply_normal_map(mat, texture_node)

    return texture_node

def apply_diffuse_map(mat, img_tex_node):
    mat.node_tree.links.new(mat.node_tree.nodes["Principled BSDF"].inputs["Base Color"], img_tex_node.outputs["Color"])

def apply_normal_map(mat, img_tex_node):
    bsdf_node = mat.node_tree.nodes["Principled BSDF"]

    img_tex_node.image.colorspace_settings.name = "Non-Color"

    separate_color_node = mat.node_tree.nodes.new("ShaderNodeSeparateColor")
    separate_color_node.location = (img_tex_node.location[0] + 300, img_tex_node.location[1] - 100)
    mat.node_tree.links.new(separate_color_node.inputs["Color"], img_tex_node.outputs["Color"])

    combine_color_node = mat.node_tree.nodes.new("ShaderNodeCombineColor")
    combine_color_node.location = (separate_color_node.location[0] + 200, separate_color_node.location[1] + 100)
    mat.node_tree.links.new(combine_color_node.inputs["Red"], img_tex_node.outputs["Alpha"])
    mat.node_tree.links.new(combine_color_node.inputs["Green"], separate_color_node.outputs["Green"])
    mat.node_tree.links.new(combine_color_node.inputs["Blue"], separate_color_node.outputs["Blue"])

    normal_map_node = mat.node_tree.nodes.new("ShaderNodeNormalMap")
    normal_map_node.location = (combine_color_node.location[0] + 200, combine_color_node.location[1])
    mat.node_tree.links.new(normal_map_node.inputs["Color"], combine_color_node.outputs["Color"])
    mat.node_tree.links.new(bsdf_node.inputs["Normal"], normal_map_node.outputs["Normal"])