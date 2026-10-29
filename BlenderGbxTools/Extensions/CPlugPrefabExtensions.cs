using BlenderGbxTools.Models;
using GBX.NET;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Extensions;

internal static class CPlugPrefabExtensions
{
    public static IEnumerable<CPlugSurface.SurfMaterial> GetDistinctSurfaceMaterials(this CPlugPrefab prefab)
    {
        var materials = new List<CPlugSurface.SurfMaterial>();

        foreach (var ent in prefab.Ents)
        {
            var surface = default(CPlugSurface);

            if (ent.Model is CPlugStaticObjectModel solid)
            {
                surface = solid.Shape;
            }
            else if (ent.Model is CPlugDynaObjectModel dynaSolid)
            {
                surface = dynaSolid.StaticShape;
            }
            else if (ent.Model is CPlugPrefab innerPrefab)
            {
                var innerMaterials = innerPrefab.GetDistinctSurfaceMaterials();
                materials.AddRange(innerMaterials);
            }
            else
            {
                continue;
            }
            if (surface is null)
            {
                continue;
            }
            materials.AddRange(surface.Materials);
        }

        return materials
            .DistinctBy(surfMat => new { surfMat.SurfaceId, MaterialName = GbxPath.GetFileNameWithoutExtension(surfMat.References.Material.File?.FilePath) });
    }

    public static ImmutableList<SurfaceMaterial> GetAllSurfaceMaterials(this CPlugPrefab prefab)
    {
        return prefab.GetDistinctSurfaceMaterials()
            .Select(SurfaceMaterial.From)
            .ToImmutableList();
    }
}
