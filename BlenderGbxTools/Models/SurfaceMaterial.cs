using GBX.NET;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed record SurfaceMaterial
{
    public CPlugSurface.MaterialId? SurfaceId { get; init; }
    public string? Name { get; init; }
    public Material? Material { get; init; }

    public static SurfaceMaterial From(CPlugSurface.SurfMaterial material)
    {
        return new SurfaceMaterial
        {
            SurfaceId = material.SurfaceId,
            Name = GbxPath.GetFileNameWithoutExtension(material.References.Material.File?.FilePath),
            Material = material.Material is CPlugMaterial plugMaterial ? Material.From(plugMaterial) : null,
        };
    }
}
