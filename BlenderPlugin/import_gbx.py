import bpy
from bpy.types import Operator
from bpy.props import StringProperty
from bpy_extras.io_utils import ImportHelper

from . import exec

from . import solid
from . import solid2
from . import item
from . import blockinfo

is_dev = True

# Base class for all Gbx importers
class BaseGbxImporter(Operator, ImportHelper):
    """Base class for Gbx file importers"""

    exec_defaultpath = "./BlenderGbxTools" if not is_dev else "../BlenderGbxTools/bin/Debug/net10.0/BlenderGbxTools"
    
    execpath: StringProperty(
        name="Executable path",
        description="Executable to retrieve Gbx data as JSON",
        default=exec_defaultpath,
    ) # type: ignore

    def execute(self, context):
        response = exec.gbx_to_json(self.filepath, self.execpath, context, self.report)
        
        if response is None:
            return {'CANCELLED'}
        
        self.import_object(response)
        return {'FINISHED'}

    def import_object(self, response):
        """Override this method in subclasses"""
        raise NotImplementedError("Subclasses must implement import_object method")


class ImportSolidGbx(BaseGbxImporter):
    """Import Solid.Gbx files"""
    bl_idname = "import_gbx.solid"
    bl_label = "Import Solid.Gbx"
    
    filename_ext = ".Solid.Gbx"
    filter_glob: StringProperty(
        default="*.Solid.Gbx",
        options={'HIDDEN'},
        maxlen=255,
    ) # type: ignore
    
    def import_object(self, response):
        solid.create(response)


class ImportMeshGbx(BaseGbxImporter):
    """Import Mesh.Gbx files"""
    bl_idname = "import_gbx.mesh"
    bl_label = "Import Mesh.Gbx"
    
    filename_ext = ".Mesh.Gbx"
    filter_glob: StringProperty(
        default="*.Mesh.Gbx;*.Solid2.Gbx",
        options={'HIDDEN'},
        maxlen=255,
    ) # type: ignore
    
    def import_object(self, response):
        solid2.create(response)


class ImportItemGbx(BaseGbxImporter):
    """Import Item.Gbx files"""
    bl_idname = "import_gbx.item"
    bl_label = "Import Item.Gbx"
    
    filename_ext = ".Item.Gbx"
    filter_glob: StringProperty(
        default="*.Item.Gbx",
        options={'HIDDEN'},
        maxlen=255,
    ) # type: ignore

    def import_object(self, response):
        item.create(response)


class ImportBlockInfoGbx(BaseGbxImporter):
    """Import block files (TM/EDClassic.Gbx, TM/EDRoad.Gbx, TM/EDClip.Gbx, ...)"""
    bl_idname = "import_gbx.blockinfo"
    bl_label = "Import block"
    
    filename_ext = ".EDClassic.Gbx"
    filter_glob: StringProperty(
        default="*.EDClassic.Gbx;*.EDClip.Gbx;*.EDFlat.Gbx;*.EDFrontier.Gbx;*.EDPylon.Gbx;*.RectAsym.Gbx;*.EDRoad.Gbx;*.TMEDClassic.Gbx;*.TMEDClip.Gbx;*.TMEDFlat.Gbx;*.TMEDFrontier.Gbx;*.TMEDPylon.Gbx;*.TMRectAsym.Gbx;*.TMEDRoad.Gbx",
        options={'HIDDEN'},
        maxlen=255,
    ) # type: ignore
    
    def import_object(self, response):
        blockinfo.create(response)


# Menu functions
def menu_func_import_solid(self, context):
    self.layout.operator(ImportSolidGbx.bl_idname, text="Solid (.Solid.Gbx)")

def menu_func_import_mesh(self, context):
    self.layout.operator(ImportMeshGbx.bl_idname, text="Mesh (.Mesh.Gbx)")

def menu_func_import_item(self, context):
    self.layout.operator(ImportItemGbx.bl_idname, text="Item (.Item.Gbx)")

def menu_func_import_blockinfo(self, context):
    self.layout.operator(ImportBlockInfoGbx.bl_idname, text="Block (.*ED*.Gbx)")


# Registration
IMPORTERS = [
    (ImportSolidGbx, menu_func_import_solid),
    (ImportMeshGbx, menu_func_import_mesh),
    (ImportItemGbx, menu_func_import_item),
    (ImportBlockInfoGbx, menu_func_import_blockinfo),
]

def register():
    for importer_class, menu_func in IMPORTERS:
        bpy.utils.register_class(importer_class)
        bpy.types.TOPBAR_MT_file_import.append(menu_func)

def unregister():
    for importer_class, menu_func in IMPORTERS:
        bpy.utils.unregister_class(importer_class)
        bpy.types.TOPBAR_MT_file_import.remove(menu_func)