using BlenderGbxTools.Extensions;
using BlenderGbxTools.Mapping;
using GBX.NET.Engines.Plug;
using GBX.NET.Engines.Scene;
using System.Runtime.InteropServices;

namespace BlenderGbxTools.Models;

internal sealed record ObjectLink
{
    public Solid? Solid { get; init; }
    public byte[]? Location { get; init; }

    public static ObjectLink From(CSceneObjectLink objectLink, MappingContext context)
    {
        var solid = default(Solid);

        if (objectLink.Mobil?.Model?.Item?.Solid?.Model is CPlugSolid plugSolid)
        {
            var fileName = objectLink.Mobil.Model.Item.Solid.References.Model.File.GetFileNameOrUniqueName();
            solid = Solid.From(fileName, plugSolid, context);
        }

        return new ObjectLink
        {
            Solid = solid,
            Location = MemoryMarshal.AsBytes([objectLink.RelativeLocation]).ToArray(),
        };
    }
}
