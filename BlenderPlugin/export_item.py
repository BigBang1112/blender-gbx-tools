import bpy
import xml.etree.ElementTree as ET
import xml.dom.minidom as minidom
from bpy.types import Operator, Panel
from bpy.props import StringProperty, FloatVectorProperty, EnumProperty, FloatProperty, BoolProperty
from bpy_extras.io_utils import ExportHelper
import os


class ExportItemXMLOperator(Operator):
    """Export Item XML format"""
    bl_idname = "export_item.modal"
    bl_label = "Export Item XML"
    bl_options = {'REGISTER', 'UNDO'}
    
    # Item properties
    item_type: EnumProperty(
        name="Type",
        description="Item type",
        items=[
            ('StaticObject', 'StaticObject', 'Static object item'),
            ('DynaObject', 'DynaObject', 'Dynamic object item'),
            ('Checkpoint', 'Checkpoint', 'Checkpoint item'),
            ('Start', 'Start', 'Start item'),
            ('Finish', 'Finish', 'Finish item')
        ],
        default='StaticObject'
    ) # type: ignore
    
    collection: StringProperty(
        name="Collection",
        description="Collection name (e.g., Canyon, Stadium, Valley)",
        default="Canyon"
    ) # type: ignore
    
    author_name: StringProperty(
        name="Author Name",
        description="Author of the item",
        default="BigBang1112"
    ) # type: ignore
    
    # Physics properties
    shape_file: StringProperty(
        name="Shape File",
        description="Path to the physics shape file (.Shape.gbx)",
        default="Meshes/ItemName.Shape.gbx"
    ) # type: ignore
    
    # Visual properties
    mesh_file: StringProperty(
        name="Mesh File",
        description="Path to the visual mesh file (.Mesh.gbx)",
        default="Meshes/ItemName.Mesh.gbx"
    ) # type: ignore
    
    # Pivot properties
    pivot_pos: FloatVectorProperty(
        name="Pivot Position",
        description="Pivot position (X Y Z)",
        default=(32.0, 0.0, 32.0),
        size=3
    ) # type: ignore
    
    # Grid snap properties
    grid_h_step: FloatProperty(
        name="H Step",
        description="Horizontal grid step",
        default=64.0,
        min=1.0,
        max=512.0
    ) # type: ignore
    
    grid_h_offset: FloatProperty(
        name="H Offset",
        description="Horizontal grid offset",
        default=32.0,
        min=0.0,
        max=512.0
    ) # type: ignore
    
    grid_v_step: FloatProperty(
        name="V Step",
        description="Vertical grid step",
        default=16.0,
        min=1.0,
        max=128.0
    ) # type: ignore
    
    # Levitation properties
    levitation_v_step: FloatProperty(
        name="Levitation V Step",
        description="Levitation vertical step",
        default=16.0,
        min=1.0,
        max=128.0
    ) # type: ignore
    
    ghost_mode: BoolProperty(
        name="Ghost Mode",
        description="Enable ghost mode for levitation",
        default=True
    ) # type: ignore
    
    # Output file
    filepath: StringProperty(
        name="File Path",
        description="Output XML file path",
        default="Item.xml",
        maxlen=1024,
        subtype='FILE_PATH'
    ) # type: ignore
    
    def execute(self, context):
        try:
            xml_data = self.generate_item_xml()
            self.save_xml_to_file(xml_data, self.filepath)
            self.report({'INFO'}, f"Item XML exported to {self.filepath}")
            return {'FINISHED'}
        except Exception as e:
            self.report({'ERROR'}, f"Failed to export Item XML: {str(e)}")
            return {'CANCELLED'}
    
    def invoke(self, context, event):
        # Auto-generate file paths based on active object or collection
        if context.active_object:
            base_name = context.active_object.name
            self.mesh_file = f"Meshes/{base_name}.Mesh.gbx"
            self.shape_file = f"Meshes/{base_name}.Shape.gbx"
            self.filepath = f"{base_name}.Item.xml"
        
        # Show the modal dialog
        return context.window_manager.invoke_props_dialog(self, width=400)
    
    def draw(self, context):
        layout = self.layout
        
        # Item properties section
        box = layout.box()
        box.label(text="Item Properties", icon='OBJECT_DATA')
        box.prop(self, "item_type")
        box.prop(self, "collection")
        box.prop(self, "author_name")
        
        # File paths section
        box = layout.box()
        box.label(text="File Paths", icon='FILE')
        box.prop(self, "shape_file")
        box.prop(self, "mesh_file")
        box.prop(self, "filepath")
        
        # Pivot section
        box = layout.box()
        box.label(text="Pivot", icon='EMPTY_ARROWS')
        box.prop(self, "pivot_pos")
        
        # Grid snap section
        box = layout.box()
        box.label(text="Grid Snap", icon='SNAP_GRID')
        row = box.row()
        row.prop(self, "grid_h_step")
        row.prop(self, "grid_h_offset")
        box.prop(self, "grid_v_step")
        
        # Levitation section
        box = layout.box()
        box.label(text="Levitation", icon='MOD_PHYSICS')
        box.prop(self, "levitation_v_step")
        box.prop(self, "ghost_mode")
    
    def generate_item_xml(self):
        """Generate Item XML structure"""
        root = ET.Element("Item")
        root.set("Type", self.item_type)
        root.set("Collection", self.collection)
        root.set("AuthorName", self.author_name)
        
        # Physics section
        phy_elem = ET.SubElement(root, "Phy")
        move_shape_elem = ET.SubElement(phy_elem, "MoveShape")
        move_shape_elem.set("Type", "mesh")
        move_shape_elem.set("File", self.shape_file)
        
        # Visual section
        vis_elem = ET.SubElement(root, "Vis")
        mesh_elem = ET.SubElement(vis_elem, "Mesh")
        mesh_elem.set("File", self.mesh_file)
        
        # Pivots section
        pivots_elem = ET.SubElement(root, "Pivots")
        pivot_elem = ET.SubElement(pivots_elem, "Pivot")
        pivot_pos_str = f"{self.pivot_pos[0]} {self.pivot_pos[1]} {self.pivot_pos[2]}"
        pivot_elem.set("Pos", pivot_pos_str)
        
        # Grid snap section
        grid_snap_elem = ET.SubElement(root, "GridSnap")
        grid_snap_elem.set("HStep", str(int(self.grid_h_step)))
        grid_snap_elem.set("HOffset", str(int(self.grid_h_offset)))
        grid_snap_elem.set("VStep", str(int(self.grid_v_step)))
        
        # Levitation section
        levitation_elem = ET.SubElement(root, "Levitation")
        levitation_elem.set("VStep", str(self.levitation_v_step))
        levitation_elem.set("GhostMode", "true" if self.ghost_mode else "false")
        
        return root
    
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
            pretty_xml = dom.documentElement.toprettyxml(indent="\t")
            f.write(pretty_xml)


class ExportItemXMLFileOperator(Operator, ExportHelper):
    """Export Item XML to file"""
    bl_idname = "export_item.xml"
    bl_label = "Export Item XML"
    
    filename_ext = ".xml"
    filter_glob: StringProperty(
        default="*.xml",
        options={'HIDDEN'},
        maxlen=255,
    ) # type: ignore
    
    def execute(self, context):
        # Use the modal operator's properties if available
        modal_op = ExportItemXMLOperator()
        
        try:
            xml_data = modal_op.generate_item_xml()
            modal_op.save_xml_to_file(xml_data, self.filepath)
            self.report({'INFO'}, f"Item XML exported to {self.filepath}")
            return {'FINISHED'}
        except Exception as e:
            self.report({'ERROR'}, f"Failed to export Item XML: {str(e)}")
            return {'CANCELLED'}


class ITEM_PT_export_panel(Panel):
    """Panel in the 3D viewport for Item XML export"""
    bl_label = "Item XML Export"
    bl_idname = "ITEM_PT_export_panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "Gbx Tools"
    
    def draw(self, context):
        layout = self.layout
        
        layout.label(text="Export Item XML", icon='EXPORT')
        layout.separator()
        
        col = layout.column()
        col.operator("export_item.modal", text="Export Item XML (Modal)", icon='SETTINGS')
        col.operator("export_item.xml", text="Export Item XML (File)", icon='FILE')


def menu_func_export_item_modal(self, context):
    """Add modal export option to File > Export menu"""
    self.layout.operator(ExportItemXMLOperator.bl_idname, text="Item XML (Modal)")


def menu_func_export_item_file(self, context):
    """Add file export option to File > Export menu"""
    self.layout.operator(ExportItemXMLFileOperator.bl_idname, text="Item XML (.xml)")


def register():
    """Register the export operators and panel"""
    bpy.utils.register_class(ExportItemXMLOperator)
    bpy.utils.register_class(ExportItemXMLFileOperator)
    bpy.utils.register_class(ITEM_PT_export_panel)
    bpy.types.TOPBAR_MT_file_export.append(menu_func_export_item_modal)
    bpy.types.TOPBAR_MT_file_export.append(menu_func_export_item_file)


def unregister():
    """Unregister the export operators and panel"""
    bpy.utils.unregister_class(ExportItemXMLOperator)
    bpy.utils.unregister_class(ExportItemXMLFileOperator)
    bpy.utils.unregister_class(ITEM_PT_export_panel)
    bpy.types.TOPBAR_MT_file_export.remove(menu_func_export_item_modal)
    bpy.types.TOPBAR_MT_file_export.remove(menu_func_export_item_file)


if __name__ == "__main__":
    register()