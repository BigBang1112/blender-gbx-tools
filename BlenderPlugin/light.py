import bpy
import math
    
def create(light_dict, name):
    type = light_dict["Type"]
    light_data = bpy.data.lights.new(name, type)

    light_data.energy = light_dict["Intensity"] * 250
    light_data.color = (light_dict["R"], light_dict["G"], light_dict["B"])
    light_data["NightOnly"] = light_dict.get("NightOnly", False)

    if "Radius" in light_dict:
        light_data.use_custom_distance = True
        light_data.cutoff_distance = light_dict["Radius"]
            
    if type == "SPOT":
        angle_inner = light_dict["AngleInner"]
        angle_outer = light_dict["AngleOuter"]

        light_data["AngleInner"] = angle_inner
        light_data["AngleOuter"] = angle_outer
        
        # Convert outer angle from degrees to radians for spot_size
        light_data.spot_size = math.radians(angle_outer)
        
        # Calculate spot_blend: represents the falloff between inner and outer cone
        # spot_blend = 0 means hard edge (inner = outer)
        # spot_blend = 1 means smooth falloff (inner = 0)
        if angle_outer > 0:
            light_data.spot_blend = 1.0 - (angle_inner / angle_outer)
        else:
            light_data.spot_blend = 0.0

    return light_data