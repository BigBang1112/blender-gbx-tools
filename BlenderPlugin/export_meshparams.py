import bpy
import xml.etree.ElementTree as ET
import xml.dom.minidom as minidom
from bpy.types import Operator
from bpy.props import StringProperty, FloatProperty, EnumProperty
from bpy_extras.io_utils import ExportHelper
import os


class ExportMeshParamsXML(Operator, ExportHelper):
    """Export MeshParams XML format"""
    bl_idname = "export_meshparams.xml"
    bl_label = "Export MeshParams XML"
    
    filename_ext = ".xml"
    filter_glob: StringProperty(
        default="*.xml",
        options={'HIDDEN'},
        maxlen=255,
    ) # type: ignore
    
    # MeshParams properties
    scale: FloatProperty(
        name="Scale",
        description="Scale value for the mesh",
        default=0.01,
        min=0.001,
        max=100.0
    ) # type: ignore
    
    collection: StringProperty(
        name="Collection",
        description="Collection name (e.g., Canyon, Stadium, Valley)",
        default="Canyon"
    ) # type: ignore
    
    mesh_type: EnumProperty(
        name="Mesh Type",
        description="Type of mesh",
        items=[
            ('Static', 'Static', 'Static mesh'),
            ('Dynamic', 'Dynamic', 'Dynamic mesh'),
            ('DynaLink', 'DynaLink', 'Dynamic link mesh')
        ],
        default='Static'
    ) # type: ignore
    
    def execute(self, context):
        try:
            xml_data = self.generate_meshparams_xml()
            self.save_xml_to_file(xml_data, self.filepath)
            self.report({'INFO'}, f"MeshParams exported to {self.filepath}")
            return {'FINISHED'}
        except Exception as e:
            self.report({'ERROR'}, f"Failed to export MeshParams: {str(e)}")
            return {'CANCELLED'}
    
    def generate_meshparams_xml(self):
        """Generate MeshParams XML from Blender materials and lights"""
        root = ET.Element("MeshParams")
        root.set("Scale", str(self.scale))
        root.set("Collection", self.collection)
        root.set("MeshType", self.mesh_type)
        
        # Add Materials section
        materials_elem = ET.SubElement(root, "Materials")
        
        for material in bpy.data.materials:
            if material.users == 0:  # Skip unused materials
                continue
                
            material_elem = ET.SubElement(materials_elem, "Material")
            material_elem.set("Name", material.name)
            
            # Set Model (shader model) - try to determine from material properties
            model = self.get_material_model(material)
            material_elem.set("Model", model)
            
            # Set BaseTexture path - get from primary texture
            base_texture = self.get_base_texture_path(material)
            if base_texture:
                material_elem.set("BaseTexture", base_texture)
            
            # Set PhysicsId - use SurfaceId if available, otherwise default
            surface_id = "Metal"  # Default
            if "SurfaceId" in material:
                surface_id = material["SurfaceId"]
            elif "PhysicsId" in material:
                surface_id = str(material["PhysicsId"])

            material_elem.set("PhysicsId", surface_id)

        # Add Lights section
        lights_elem = ET.SubElement(root, "Lights")
        self.add_lights_to_xml(lights_elem)
        
        return root
    
    def get_material_model(self, material):
        """Determine the shader model based on material properties"""
        # Default model
        model = "TDSN"
        
        # Check for transparency/alpha
        if material.use_nodes:
            principled = self.get_principled_bsdf_node(material)
            if principled and principled.inputs["Alpha"].default_value < 1.0:
                model = "TDOSN"  # Transparent model
        
        # Check material name for specific models
        name_lower = material.name.lower()
        if "night" in name_lower or "light" in name_lower:
            model = "TDSNI_Night" if "night" in name_lower else "TDSNI"
        elif "alpha" in name_lower:
            model = "TDOSN"
        
        # Check for custom model in material properties
        if "Model" in material:
            model = str(material["Model"])
        
        return model
    
    def get_base_texture_path(self, material):
        """Get the base texture path from material"""
        if not material.use_nodes:
            return None
        
        # Look for the main diffuse/base color texture
        for node in material.node_tree.nodes:
            if node.type == 'TEX_IMAGE' and node.image:
                # Check if connected to Base Color or if it's labeled as Diffuse
                if (node.label and node.label.lower() in ["diffuse", "basecolor", "base"]) or \
                   self.is_connected_to_base_color(node, material):
                    if node.image.filepath:
                        # Convert to relative path format used in MeshParams
                        path = node.image.filepath
                        # Remove file extension and convert to relative format
                        if path.endswith('.dds') or path.endswith('.tga') or path.endswith('.png'):
                            path = path.rsplit('.', 1)[0]
                        # Convert absolute path to relative format like "../../../Textures/TextureName"
                        filename = os.path.basename(path)
                        return f"../../../Textures/{filename}"
                    else:
                        # For packed images, use the image name
                        name = node.image.name
                        if '.' in name:
                            name = name.rsplit('.', 1)[0]
                        return f"../../../Textures/{name}"
        
        return None
    
    def is_connected_to_base_color(self, texture_node, material):
        """Check if texture node is connected to base color"""
        if not material.node_tree:
            return False
        
        for link in material.node_tree.links:
            if (link.from_node == texture_node and 
                link.to_node.type == 'BSDF_PRINCIPLED' and
                link.to_socket.name == "Base Color"):
                return True
        return False
    
    def get_principled_bsdf_node(self, material):
        """Get the Principled BSDF node from material"""
        if not material.use_nodes:
            return None
        
        for node in material.node_tree.nodes:
            if node.type == 'BSDF_PRINCIPLED':
                return node
        return None
    
    def add_lights_to_xml(self, lights_elem):
        """Add light objects to XML"""
        light_count = 1
        
        for obj in bpy.data.objects:
            if obj.type == 'LIGHT':
                light_elem = ET.SubElement(lights_elem, "Light")
                light_elem.set("Name", obj.name if obj.name else f"Light{light_count}")
                
                # Determine light type
                light_type = "Point"  # Default
                if obj.data.type == 'SUN':
                    light_type = "Directional"
                elif obj.data.type == 'SPOT':
                    light_type = "Spot"
                elif obj.data.type == 'AREA':
                    light_type = "Area"
                
                light_elem.set("Type", light_type)
                
                # Check if it's a night-only light (based on name or custom property)
                night_only = False
                if "night" in obj.name.lower() or "NightOnly" in obj:
                    night_only = True
                
                light_elem.set("NightOnly", str(night_only).lower())
                
                light_count += 1
    
    def save_xml_to_file(self, xml_root, filepath):
        """Save XML to file with pretty formatting"""
        # Create a rough string of the XML
        rough_string = ET.tostring(xml_root, encoding='unicode')
        
        # Parse it with minidom for pretty printing
        dom = minidom.parseString(rough_string)
        
        # Write to file with proper formatting
        with open(filepath, 'w', encoding='utf-8') as f:
            # Write the XML declaration and pretty printed XML
            f.write('<?xml version="1.0" encoding="UTF-8"?>\n')
            # Remove the XML declaration from minidom output and write the rest
            pretty_xml = dom.documentElement.toprettyxml(indent="  ")
            f.write(pretty_xml)


def export_meshparams_to_xml(filepath=None, scale=0.01, collection="Canyon", mesh_type="Static"):
    """Utility function to export MeshParams to XML programmatically"""
    if filepath is None:
        filepath = "meshparams_export.xml"
    
    exporter = ExportMeshParamsXML()
    exporter.scale = scale
    exporter.collection = collection  
    exporter.mesh_type = mesh_type
    xml_data = exporter.generate_meshparams_xml()
    exporter.save_xml_to_file(xml_data, filepath)
    return filepath


def menu_func_export_meshparams(self, context):
    """Add export option to File > Export menu"""
    self.layout.operator(ExportMeshParamsXML.bl_idname, text="MeshParams XML (.xml)")


def register():
    """Register the export operator"""
    bpy.utils.register_class(ExportMeshParamsXML)
    bpy.types.TOPBAR_MT_file_export.append(menu_func_export_meshparams)


def unregister():
    """Unregister the export operator"""
    bpy.utils.unregister_class(ExportMeshParamsXML)
    bpy.types.TOPBAR_MT_file_export.remove(menu_func_export_meshparams)


if __name__ == "__main__":
    register()