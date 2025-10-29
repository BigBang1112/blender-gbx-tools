import bpy
import os
import subprocess
import json

def gbx_to_json(filepath, execpath, context, report_func):
    full_execpath = find_executable(execpath, report_func)
    if full_execpath is None:
        return None

    # Third parameter would be the GameData folder, but it's not needed for now
    process_result = subprocess.run([full_execpath, filepath], capture_output=True, text=True)

    print("Return code: " + str(process_result.returncode))

    if process_result.returncode != 0:
        print(process_result.stdout)
        return None
    
    return json.loads(process_result.stdout)

def find_executable(execpath, report_func):
    if os.name == 'nt' and not execpath.lower().endswith('.exe'):
        execpath += '.exe'

    execfilename = os.path.basename(execpath)
    
    # Try local executable relative to blend file
    local_execpath = bpy.path.abspath("//" + execfilename)
    if os.path.exists(local_execpath):
        print(f"Local {execfilename} executable found ({local_execpath})")
        return local_execpath
    
    # Try external executable relative to this file
    normalized_execpath = os.path.normpath(execpath)
    external_execpath = os.path.join(os.path.dirname(os.path.realpath(__file__)), normalized_execpath)
    if os.path.exists(external_execpath):
        print(f"External {execfilename} executable found ({external_execpath})")
        return external_execpath
    
    # Executable not found
    report_func({'WARNING'}, f"{execfilename} executable was not found")
    return None