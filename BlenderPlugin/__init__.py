# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful, but
# WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTIBILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
# General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program. If not, see <http://www.gnu.org/licenses/>.

bl_info = {
    "name": "Blender Gbx Tools",
    "author": "BigBang1112",
    "description": "Working with Gbx files inside of Blender.",
    "blender": (4, 0, 0),
    "version": (0, 0, 1),
    "location": "File > Import",
    "support": "TESTING",
    "warning": "",
    "category": "Add Mesh",
}

from . import import_gbx

def register():
    import_gbx.register()

def unregister():
    import_gbx.unregister()
