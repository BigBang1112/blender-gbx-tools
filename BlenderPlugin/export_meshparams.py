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
    
    filename_ext = ".MeshParams.xml"
    filter_glob: StringProperty(
        default="*.xml",
        options={'HIDDEN'},
        maxlen=255,
    ) # type: ignore
    
    def check(self, context):
        """Override to handle filename extension properly"""
        changed = False
        filepath = self.filepath
        
        # Remove duplicate extensions if they exist
        if filepath.endswith(".MeshParams.xml.MeshParams.xml"):
            self.filepath = filepath.replace(".MeshParams.xml.MeshParams.xml", ".MeshParams.xml")
            changed = True
        elif not filepath.endswith(".MeshParams.xml"):
            # Only add extension if it's not already there
            if not filepath.endswith(".xml"):
                self.filepath = filepath + self.filename_ext
                changed = True
        
        return changed
    
    # MeshParams properties
    scale: FloatProperty(
        name="Scale",
        description="Scale value for the mesh",
        default=1,
        min=0.001,
        max=100.0
    ) # type: ignore
    
    collection: EnumProperty(
        name="Collection",
        description="Collection name",
        items=[
            ('Canyon', 'Canyon', 'Canyon collection'),
            ('Stadium', 'Stadium', 'Stadium collection'),
            ('Valley', 'Valley', 'Valley collection'),
            ('Lagoon', 'Lagoon', 'Lagoon collection'),
        ],
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
    
    texture_base_path: StringProperty(
        name="Texture Base Path",
        description="Base path for texture references",
        default="../../../Textures"
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
        
        # Collect materials from selected objects, or all mesh objects if none selected
        selected_materials = set()
        selected_objects = bpy.context.selected_objects
        mesh_objects_to_check = selected_objects if selected_objects else bpy.data.objects
        
        for obj in mesh_objects_to_check:
            if obj.type == 'MESH' and obj.data.materials:
                for material in obj.data.materials:
                    if material is not None:  # Material slot might be empty
                        selected_materials.add(material)
        
        for material in selected_materials:
                
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
        """Determine the shader model based on Shader property"""
        # Dictionary mapping shader names to model names
        shader_to_model = {
            "Tech3 Block TDiff_Spec_Norm": "TDSN",
            "Tech3 Block TDiffA_Spec_Norm": "TDOSN", 
            #"TDSNE": "TDSNE",
            "Tech3 Block TSelfIllum": "TDSNI",
            "Tech3_Block_TSelfI_TxDiffA": "TDSNI",
            "Tech3 Block TSelfIllumNightOnly": "TDSNI_Night"
        }
        
        # Check for Shader property in material
        if "Shader" not in material:
            raise ValueError(f"Material '{material.name}' is missing required 'Shader' custom property")
        
        shader_name = str(material["Shader"])
        
        if shader_name not in shader_to_model:
            raise ValueError(f"Unknown shader '{shader_name}' in material '{material.name}'")
        
        return shader_to_model[shader_name]
    
    def get_base_texture_path(self, material):
        """Get the base texture path using material name and configurable base path"""
        # Use material name as texture name with configurable base path
        return f"{self.texture_base_path}/{material.name}"
    
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
        """Add light objects to XML - selected lights if any are selected, otherwise all lights"""
        light_count = 1
        
        # Check if any lights are selected
        selected_lights = [obj for obj in bpy.context.selected_objects if obj.type == 'LIGHT']
        lights_to_process = selected_lights if selected_lights else [obj for obj in bpy.data.objects if obj.type == 'LIGHT']
        
        for obj in lights_to_process:
            light_elem = ET.SubElement(lights_elem, "Light")
            light_elem.set("Name", obj.name if obj.name else f"Light{light_count}")
            
            # Determine light type - only Point and Spot are supported
            light_type = "Point"  # Default
            if obj.data.type == 'SPOT':
                light_type = "Spot"
            
            light_elem.set("Type", light_type)
            
            # sRGB color (convert from linear to hex)
            color = obj.data.color
            r = int(min(255, max(0, color[0] * 255)))
            g = int(min(255, max(0, color[1] * 255)))
            b = int(min(255, max(0, color[2] * 255)))
            srgb_hex = f"{r:02x}{g:02x}{b:02x}"
            light_elem.set("sRGB", srgb_hex)
            
            # Intensity (convert from Blender energy)
            intensity = obj.data.energy / 250.0  # Reverse the scaling from import
            light_elem.set("Intensity", str(intensity))
            
            # Distance (if custom distance is enabled)
            if hasattr(obj.data, 'use_custom_distance') and obj.data.use_custom_distance:
                light_elem.set("Distance", str(obj.data.cutoff_distance))
            
            # Point light specific attributes
            if light_type == "Point":
                # PointEmissionRadius - check custom property or use default
                if "PointEmissionRadius" in obj.data:
                    light_elem.set("PointEmissionRadius", str(obj.data["PointEmissionRadius"]))
                
                # PointEmissionLength - check custom property
                if "PointEmissionLength" in obj.data:
                    light_elem.set("PointEmissionLength", str(obj.data["PointEmissionLength"]))
            
            # Spot light specific attributes
            elif light_type == "Spot":
                # Inner and outer angles from custom properties
                if "AngleInner" in obj.data:
                    light_elem.set("SpotInnerAngle", str(obj.data["AngleInner"]))
                if "AngleOuter" in obj.data:
                    light_elem.set("SpotOuterAngle", str(obj.data["AngleOuter"]))
                else:
                    # Fallback: convert from Blender's spot_size
                    import math
                    outer_angle = math.degrees(obj.data.spot_size)
                    light_elem.set("SpotOuterAngle", str(outer_angle))
                
                # Spot emission size - check custom properties
                if "SpotEmissionSizeX" in obj.data:
                    light_elem.set("SpotEmissionSizeX", str(obj.data["SpotEmissionSizeX"]))
                if "SpotEmissionSizeY" in obj.data:
                    light_elem.set("SpotEmissionSizeY", str(obj.data["SpotEmissionSizeY"]))
            
            # NightOnly attribute
            night_only = False
            if "NightOnly" in obj.data:
                night_only = obj.data["NightOnly"]
            
            if night_only:
                light_elem.set("NightOnly", "true")
            
            light_count += 1
    
    def save_xml_to_file(self, xml_root, filepath):
        """Save XML to file with pretty formatting"""
        # Create a rough string of the XML
        rough_string = ET.tostring(xml_root, encoding='unicode')
        
        # Parse it with minidom for pretty printing
        dom = minidom.parseString(rough_string)
        
        # Write to file with proper formatting
        with open(filepath, 'w', encoding='utf-8') as f:
            # Remove the XML declaration from minidom output and write the rest
            pretty_xml = dom.documentElement.toprettyxml(indent="\t")
            f.write(pretty_xml)


def export_meshparams_to_xml(filepath=None, scale=0.01, collection="Canyon", mesh_type="Static", texture_base_path="../../../Textures"):
    """Utility function to export MeshParams to XML programmatically"""
    if filepath is None:
        filepath = "meshparams_export.xml"
    
    exporter = ExportMeshParamsXML()
    exporter.scale = scale
    exporter.collection = collection  
    exporter.mesh_type = mesh_type
    exporter.texture_base_path = texture_base_path
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