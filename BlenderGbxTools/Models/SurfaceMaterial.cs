using GBX.NET;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed class SurfaceMaterial
{
    public CPlugSurface.MaterialId? SurfaceId { get; }
    public string? Name { get; }
    public Material? Material { get; }

    public SurfaceMaterial()
    {
        
    }

    public SurfaceMaterial(CPlugSurface.SurfMaterial material)
    {
        SurfaceId = material.SurfaceId;
        Name = GbxPath.GetFileNameWithoutExtension(material.MaterialFile?.FilePath);
        Material = material.Material is CPlugMaterial plugMaterial ? new Material(plugMaterial) : null;
    }
}
