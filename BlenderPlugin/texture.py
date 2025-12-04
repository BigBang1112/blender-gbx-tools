import os
import bpy

def create(texture_dict, mat, settings=None):
    image_path = texture_dict["ImagePath"]

    texture_node = mat.node_tree.nodes.new("ShaderNodeTexImage")

    if os.path.exists(image_path):
        texture_node.image = bpy.data.images.load(image_path, check_existing=True)
    else:
        settings['report_func']({'WARNING'}, f"Missing texture file: {image_path}")

    texture_node.hide = True
    
    return texture_node, None
