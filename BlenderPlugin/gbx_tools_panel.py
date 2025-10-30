import bpy


class VIEW3D_PT_gbx_tools(bpy.types.Panel):
    """Panel for Gbx Tools functionality"""
    bl_label = "Gbx Tools"
    bl_idname = "VIEW3D_PT_gbx_tools"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "Gbx Tools"

    def draw(self, context):
        layout = self.layout
        
        # Custom Item section
        box = layout.box()
        box.label(text="Custom Item", icon='MESH_CUBE')
        
        row = box.row()
        row.operator("object.merge_for_custom_item", text="Merge for custom item", icon='AUTOMERGE_ON')
        
        row = box.row()
        row.operator("export_meshparams.xml", text="Export MeshParams", icon='EXPORT')


def register():
    bpy.utils.register_class(VIEW3D_PT_gbx_tools)


def unregister():
    bpy.utils.unregister_class(VIEW3D_PT_gbx_tools)