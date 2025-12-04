using BlenderGbxTools.Models;
using GBX.NET;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Extensions;

internal static class CPlugPrefabExtensions
{
    public static ImmutableDictionary<string, Material?> GetAllMaterials(this CPlugPrefab prefab)
    {
        var materials = ImmutableDictionary.CreateBuilder<string, Material?>();

        foreach (var ent in prefab.Ents)
        {
            var mesh = default(CPlugSolid2Model);

            if (ent.Model is CPlugStaticObjectModel solid)
            {
                mesh = solid.Mesh;
            }
            else if (ent.Model is CPlugDynaObjectModel dynaSolid)
            {
                mesh = dynaSolid.Mesh;
            }
            else if (ent.Model is CPlugPrefab innerPrefab)
            {
                var innerMaterials = innerPrefab.GetAllMaterials();
                foreach (var kvp in innerMaterials)
                {
                    if (!materials.ContainsKey(kvp.Key))
                    {
                        materials.Add(kvp.Key, kvp.Value);
                    }
                }
            }
            else
            {
                continue;
            }
            if (mesh is null)
            {
                continue;
            }

            var meshMaterials = mesh.GetAllMaterials();

            foreach (var kvp in meshMaterials)
            {
                if (!materials.ContainsKey(kvp.Key))
                {
                    materials.Add(kvp.Key, kvp.Value);
                }
            }
        }

        return materials.ToImmutable();
    }

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
            .DistinctBy(surfMat => new { surfMat.SurfaceId, MaterialName = GbxPath.GetFileNameWithoutExtension(surfMat.MaterialFile?.FilePath) });
    }

    public static ImmutableList<SurfaceMaterial> GetAllSurfaceMaterials(this CPlugPrefab prefab)
    {
        return prefab.GetDistinctSurfaceMaterials()
            .Select(x => new SurfaceMaterial(x))
            .ToImmutableList();
    }
}
