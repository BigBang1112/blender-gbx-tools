using GBX.NET.Engines.Plug;
using GBX.NET.Engines.Scene;
using System.Runtime.InteropServices;

namespace BlenderGbxTools.Models;

internal sealed class ObjectLink
{
    public Solid? Solid { get; }
    public byte[]? Location { get; }

    public ObjectLink(CSceneObjectLink objectLink)
    {
        if (objectLink.Mobil?.Item?.Solid?.Tree is CPlugSolid solid)
        {
            var fileName = objectLink.Mobil.Item.Solid.TreeFile is null ? Guid.NewGuid().ToString() : Path.GetFileName(objectLink.Mobil.Item.Solid.TreeFile.GetFullPath());
            Solid = new Solid(fileName, solid, standalone: false);
        }

        Location = MemoryMarshal.AsBytes([objectLink.RelativeLocation]).ToArray();
    }
}
