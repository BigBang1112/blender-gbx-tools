import bpy
from bpy.types import Operator


class MergeForCustomItem(Operator):
    """Merge objects for custom item creation"""
    bl_idname = "object.merge_for_custom_item"
    bl_label = "Merge for custom item"
    bl_options = {'REGISTER', 'UNDO'}

    @classmethod
    def poll(cls, context):
        return context.active_object is not None

    def execute(self, context):
        selected_object = context.active_object
        
        if selected_object is None:
            self.report({'ERROR'}, "No active object selected")
            return {'CANCELLED'}

        try:
            # Apply transforms to selected object and children
            self.apply_transforms_recursive(selected_object)
            
            # Find all LOD distances in the hierarchy
            lod_distances = self.find_lod_distances(selected_object)
            
            if lod_distances:
                # Extract lights and Surface objects before LOD processing
                lights_and_surfaces = self.extract_lights_and_surfaces(selected_object)
                
                # Create separate objects for each LOD level
                for distance in lod_distances:
                    lod_name = f"{selected_object.name}_LOD{distance}"
                    
                    # Duplicate the entire hierarchy
                    lod_root = self.duplicate_hierarchy(selected_object, lod_name)
                    
                    # Remove objects that don't belong to this LOD level
                    self.remove_non_lod_objects(lod_root, distance)
                    
                    # Remove lights and Surface objects from LOD hierarchy
                    self.remove_lights_and_surfaces_from_hierarchy(lod_root)
                    
                    # Collect, merge, and clean up for this LOD
                    mesh_objects = self.collect_mesh_objects(lod_root)
                    if mesh_objects:
                        self.merge_mesh_objects(mesh_objects, lod_name)
                    self.remove_empty_objects(lod_root)
                
                # Place extracted lights and Surface objects at root level
                self.place_lights_and_surfaces_at_root(lights_and_surfaces)
            else:
                # No LODs found, do standard merge
                mesh_objects = self.collect_mesh_objects(selected_object)
                if not mesh_objects:
                    self.report({'WARNING'}, "No mesh objects found to merge")
                    return {'CANCELLED'}
                
                self.merge_mesh_objects(mesh_objects, selected_object.name)
                self.remove_empty_objects(selected_object)
            
            # Remove the original tree hierarchy TODO make this optional
            self.remove_object_hierarchy(selected_object)
            
            self.report({'INFO'}, "Successfully merged objects for custom item")
            return {'FINISHED'}
            
        except Exception as e:
            self.report({'ERROR'}, f"Error during merge operation: {str(e)}")
            return {'CANCELLED'}

    def apply_transforms_recursive(self, obj):
        """Apply all transforms to object and its children recursively"""
        # Make sure object is visible for transform application
        was_hidden = obj.hide_get()
        if was_hidden:
            obj.hide_set(False)
        
        # Select and make active
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.select_all(action='DESELECT')
        obj.select_set(True)
        
        # Apply transforms to all object types (including Empty objects)
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
        
        # Restore visibility
        if was_hidden:
            obj.hide_set(True)
        
        # Apply to children
        for child in obj.children:
            self.apply_transforms_recursive(child)

    def find_lod_distances(self, root_obj):
        """Find all unique LOD distances in the hierarchy"""
        distances = set()
        
        def find_recursive(obj):
            if "Distance" in obj:
                distances.add(obj["Distance"])
            for child in obj.children:
                find_recursive(child)
        
        find_recursive(root_obj)
        return sorted(distances) if distances else []
    
    def duplicate_hierarchy(self, root_obj, new_name):
        """Duplicate the entire object hierarchy"""
        def duplicate_recursive(obj, parent_copy=None):
            # Create copy of object
            obj_copy = obj.copy()
            if obj.data:
                obj_copy.data = obj.data.copy()
            
            # Link to scene
            bpy.context.collection.objects.link(obj_copy)
            
            # Set parent relationship
            if parent_copy:
                obj_copy.parent = parent_copy
            
            # Duplicate children
            for child in obj.children:
                duplicate_recursive(child, obj_copy)
            
            return obj_copy
        
        root_copy = duplicate_recursive(root_obj)
        root_copy.name = new_name
        return root_copy
    
    def remove_non_lod_objects(self, root_obj, target_distance):
        """Remove objects that don't belong to the specified LOD level"""
        def should_keep_object(obj):
            # Keep objects without Distance property (shared objects)
            if "Distance" not in obj:
                return True
            # Keep objects with matching Distance
            return obj["Distance"] == target_distance
        
        def remove_recursive(obj):
            children_to_remove = []
            
            # Process children first
            for child in list(obj.children):
                if should_keep_object(child):
                    # Keep this child, but check its children
                    remove_recursive(child)
                else:
                    # Remove this child and all its descendants
                    children_to_remove.append(child)
            
            # Remove unwanted children
            for child in children_to_remove:
                self.remove_object_hierarchy(child)
        
        remove_recursive(root_obj)
    
    def remove_object_hierarchy(self, obj):
        """Remove object and all its children"""
        for child in list(obj.children):
            self.remove_object_hierarchy(child)
        if obj.name in bpy.data.objects:
            bpy.data.objects.remove(obj, do_unlink=True)
    
    def extract_lights_and_surfaces(self, root_obj):
        """Extract lights and Surface objects from hierarchy"""
        extracted_objects = []
        
        def extract_recursive(obj):
            # Check if this is a light or Surface object
            if obj.type == 'LIGHT' or obj.get("Surface", False):
                # Create a copy with applied transforms
                obj_copy = obj.copy()
                if obj.data:
                    obj_copy.data = obj.data.copy()
                
                # Apply the world transform to the copy
                obj_copy.matrix_world = obj.matrix_world
                
                extracted_objects.append(obj_copy)
            
            # Continue checking children
            for child in obj.children:
                extract_recursive(child)
        
        extract_recursive(root_obj)
        return extracted_objects
    
    def remove_lights_and_surfaces_from_hierarchy(self, root_obj):
        """Remove lights and Surface objects from a hierarchy"""
        def remove_recursive(obj):
            children_to_remove = []
            
            # Process children first
            for child in list(obj.children):
                if child.type == 'LIGHT' or child.get("Surface", False):
                    children_to_remove.append(child)
                else:
                    remove_recursive(child)
            
            # Remove lights and Surface objects
            for child in children_to_remove:
                self.remove_object_hierarchy(child)
        
        remove_recursive(root_obj)
    
    def place_lights_and_surfaces_at_root(self, objects):
        """Place lights and Surface objects at scene root level"""
        for obj in objects:
            # Clear parent relationship
            obj.parent = None
            
            # Link to current collection
            bpy.context.collection.objects.link(obj)

    def collect_mesh_objects(self, root_obj):
        """Collect all mesh objects from root and children, including hidden ones, excluding Surface objects"""
        mesh_objects = []
        
        def collect_recursive(obj):
            if obj.type == 'MESH':
                # Skip objects with Surface custom property set to True
                if not obj.get("Surface", False):
                    mesh_objects.append(obj)
            for child in obj.children:
                collect_recursive(child)
        
        collect_recursive(root_obj)
        return mesh_objects



    def merge_mesh_objects(self, mesh_objects, target_name):
        """Merge multiple mesh objects into one"""
        if not mesh_objects:
            return None
        
        # Clear any existing selection first
        bpy.ops.object.select_all(action='DESELECT')
        
        # Ensure all objects are visible and selectable
        for obj in mesh_objects:
            obj.hide_set(False)
            obj.select_set(True)
        
        # Set the first object as active
        bpy.context.view_layer.objects.active = mesh_objects[0]
        
        # Join all selected mesh objects
        bpy.ops.object.join()
        
        # Rename the result
        merged_obj = bpy.context.active_object
        merged_obj.name = target_name
        
        # Clear selection after merge
        bpy.ops.object.select_all(action='DESELECT')
        
        return merged_obj

    def remove_empty_objects(self, root_obj):
        """Remove all empty objects from hierarchy after mesh merge"""
        def collect_all_objects(obj, objects_list):
            """Recursively collect all objects in hierarchy"""
            objects_list.append(obj)
            for child in obj.children:
                collect_all_objects(child, objects_list)
        
        # Collect all objects in the hierarchy
        all_objects = []
        collect_all_objects(root_obj, all_objects)
        
        # Remove all empty objects (since meshes are merged, empties are no longer needed)
        for obj in all_objects:
            if obj.type == 'EMPTY' and obj.name in bpy.data.objects:
                bpy.data.objects.remove(obj, do_unlink=True)


def register():
    bpy.utils.register_class(MergeForCustomItem)


def unregister():
    bpy.utils.unregister_class(MergeForCustomItem)