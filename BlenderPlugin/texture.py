import bpy

def create(texture_dict, mat):
    image_path = texture_dict["ImagePath"]

    texture_node = mat.node_tree.nodes.new("ShaderNodeTexImage")
    texture_node.image = bpy.data.images.load(image_path)
    texture_node.hide = True

    return texture_node