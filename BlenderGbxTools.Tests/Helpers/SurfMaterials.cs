using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Helpers;

internal static class SurfMaterials
{
    public static CPlugSurface.SurfMaterial Create(CPlugSurface.MaterialId surfaceId, string? materialFilePath = null)
    {
        var surfMaterial = new CPlugSurface.SurfMaterial { SurfaceId = surfaceId };

        if (materialFilePath is not null)
        {
            surfMaterial.References.Material.Descriptor = GbxFiles.Create(materialFilePath);
        }

        return surfMaterial;
    }
}
