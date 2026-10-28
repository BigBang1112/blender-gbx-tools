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
        if (objectLink.Mobil?.Model?.Item?.Solid?.Model is CPlugSolid solid)
        {
            var modelFile = objectLink.Mobil.Model.Item.Solid.References.Model.File;
            var fileName = modelFile is null ? Guid.NewGuid().ToString() : Path.GetFileName(modelFile.GetFullPath());
            Solid = new Solid(fileName, solid, standalone: false);
        }

        Location = MemoryMarshal.AsBytes([objectLink.RelativeLocation]).ToArray();
    }
}
