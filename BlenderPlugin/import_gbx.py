import bpy
from bpy.types import Operator
from bpy.props import StringProperty, BoolProperty, EnumProperty
from bpy_extras.io_utils import ImportHelper

from . import exec

from . import solid
from . import solid2
from . import item
from . import blockinfo
from . import prefab
from . import surface

# Base class for all Gbx importers
class BaseGbxImporter(Operator, ImportHelper):
    """Base class for Gbx file importers"""
    
    execpath: StringProperty(
        name="Executable path",
        description="Executable to retrieve Gbx data as JSON",
        default="./BlenderGbxTools",
    ) # type: ignore

    hide_lod: BoolProperty(
        name="Hide LOD objects",
        description="Hide Level of Detail (LOD) objects",
        default=True,
    ) # type: ignore

    reuse_existing_materials: BoolProperty(
        name="Reuse existing materials",
        description="Reuse existing materials if they exist",
        default=True,
    ) # type: ignore

    def execute(self, context):
        response = exec.gbx_to_json(self.filepath, self.execpath, context, self.report)
        
        if response is None:
            return {'CANCELLED'}
        
        self.import_object(response)
        return {'FINISHED'}

    def get_import_settings(self):
        """Get import settings as a dictionary"""
        return {
            'hide_lod': self.hide_lod,
            'report_func': self.report,
            'reuse_existing_materials': True,
            # Add other settings here as needed
        }

    def import_object(self, response):
        """Override this method in subclasses"""
        raise NotImplementedError("Subclasses must implement import_object method")


class ImportSolidGbx(BaseGbxImporter):
    """Import Solid.Gbx files"""
    bl_idname = "import_gbx.solid"
    bl_label = "Import Solid.Gbx"
    
    filter_glob: StringProperty(
        default="*.Solid.Gbx",
        options={'HIDDEN'},
        maxlen=255,
    ) # type: ignore
    
    def import_object(self, response):
        solid.create(response, None, None, self.get_import_settings())


class ImportMeshGbx(BaseGbxImporter):
    """Import Mesh.Gbx files"""
    bl_idname = "import_gbx.mesh"
    bl_label = "Import Mesh.Gbx"
    
    filter_glob: StringProperty(
        default="*.Mesh.Gbx;*.Solid2.Gbx",
        options={'HIDDEN'},
        maxlen=255,
    ) # type: ignore
    
    def import_object(self, response):
        solid2.create(response, None, self.get_import_settings())


class ImportPrefabGbx(BaseGbxImporter):
    """Import Mesh.Gbx files"""
    bl_idname = "import_gbx.prefab"
    bl_label = "Import Prefab.Gbx"
    
    filter_glob: StringProperty(
        default="*.Prefab.Gbx",
        options={'HIDDEN'},
        maxlen=255,
    ) # type: ignore
    
    def import_object(self, response):
        prefab.create(response, None, None, self.get_import_settings())


class ImportShapeGbx(BaseGbxImporter):
    """Import Shape.Gbx files"""
    bl_idname = "import_gbx.shape"
    bl_label = "Import Shape.Gbx"
    
    filter_glob: StringProperty(
        default="*.Shape.Gbx",
        options={'HIDDEN'},
        maxlen=255,
    ) # type: ignore
    
    def import_object(self, response):
        surface.create(response, None, None, self.get_import_settings())


class ImportItemGbx(BaseGbxImporter):
    """Import Item.Gbx files"""
    bl_idname = "import_gbx.item"
    bl_label = "Import Item.Gbx"
    
    filter_glob: StringProperty(
        default="*.Item.Gbx",
        options={'HIDDEN'},
        maxlen=255,
    ) # type: ignore

    def import_object(self, response):
        item.create(response, None, self.get_import_settings())


class ImportBlockInfoGbx(BaseGbxImporter):
    """Import block files (TM/EDClassic.Gbx, TM/EDRoad.Gbx, TM/EDClip.Gbx, ...)"""
    bl_idname = "import_gbx.blockinfo"
    bl_label = "Import block"
    
    filter_glob: StringProperty(
        default="*.ED*.Gbx;*.TMED*.Gbx;",
        options={'HIDDEN'},
        maxlen=255,
    ) # type: ignore

    variant_extract: EnumProperty(
        name="Variant",
        description="Select the variant to import",
        items=[
            ('AIR_AND_GROUND', "Air and Ground", "Import both Air and Ground variants"),
            ('AIR', "Air", "Import Air variant only"),
            ('GROUND', "Ground", "Import Ground variant only"),
        ],
        default='AIR_AND_GROUND',
    ) # type: ignore

    include_spawn_point: BoolProperty(
        name="Include spawn point",
        description="Include spawn point object in the import",
        default=True,
    ) # type: ignore

    include_editor_helpers: BoolProperty(
        name="Include editor helpers",
        description="Include editor helpers objects in the import",
        default=False,
    ) # type: ignore

    # override get_import_settings to include variant
    def get_import_settings(self):
        settings = super().get_import_settings()
        settings["variant"] = self.variant_extract
        settings["include_spawn_point"] = self.include_spawn_point
        settings["include_editor_helpers"] = self.include_editor_helpers
        return settings

    def import_object(self, response):
        blockinfo.create(response, self.get_import_settings())


# Menu functions
def menu_func_import_solid(self, context):
    self.layout.operator(ImportSolidGbx.bl_idname, text="Solid (.Solid.Gbx)")

def menu_func_import_mesh(self, context):
    self.layout.operator(ImportMeshGbx.bl_idname, text="Mesh (.Mesh.Gbx)")

def menu_func_import_prefab(self, context):
    self.layout.operator(ImportPrefabGbx.bl_idname, text="Prefab (.Prefab.Gbx)")

def menu_func_import_shape(self, context):
    self.layout.operator(ImportShapeGbx.bl_idname, text="Shape (.Shape.Gbx)")

def menu_func_import_item(self, context):
    self.layout.operator(ImportItemGbx.bl_idname, text="Item (.Item.Gbx)")

def menu_func_import_blockinfo(self, context):
    self.layout.operator(ImportBlockInfoGbx.bl_idname, text="Block (.*ED*.Gbx)")


# Registration
IMPORTERS = [
    (ImportSolidGbx, menu_func_import_solid),
    (ImportMeshGbx, menu_func_import_mesh),
    (ImportPrefabGbx, menu_func_import_prefab),
    (ImportShapeGbx, menu_func_import_shape),
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
