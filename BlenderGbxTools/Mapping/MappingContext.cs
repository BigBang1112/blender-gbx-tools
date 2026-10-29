using BlenderGbxTools.Models;
using GBX.NET;
using GBX.NET.Components;
using GBX.NET.Engines.MwFoundations;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Mapping;

internal sealed class MappingContext
{
    private readonly Dictionary<string, Material?> materials = [];
    private readonly Dictionary<CMwNod, string> uniqueNames = new(ReferenceEqualityComparer.Instance);
    private readonly List<SurfaceMaterial> surfaceMaterials = [];
    private readonly Dictionary<(CPlugSurface.MaterialId? SurfaceId, string? Name), int> surfaceMaterialIndices = [];

    public string? GetName(GbxRefTableFile? file, CMwNod? node)
    {
        var fileName = GbxPath.GetFileNameWithoutExtension(file?.FilePath);

        if (fileName is not null)
        {
            return fileName;
        }

        if (node is null)
        {
            return null;
        }

        if (!uniqueNames.TryGetValue(node, out var name))
        {
            // Random rather than sequential, so Blender doesn't reuse a material with the same name from another import
            name = Guid.NewGuid().ToString();
            uniqueNames.Add(node, name);
        }

        return name;
    }

    public void AddMaterial(string name, Func<Material?> createMaterial)
    {
        if (!materials.ContainsKey(name))
        {
            materials.Add(name, createMaterial());
        }
    }

    public ImmutableDictionary<string, Material?> GetMaterials()
    {
        return materials.ToImmutableDictionary();
    }

    // Returns the index of the material in GetSurfaceMaterials, which all surfaces of the file share
    public int AddSurfaceMaterial(CPlugSurface.SurfMaterial surfMaterial)
    {
        var key = (surfMaterial.SurfaceId, GbxPath.GetFileNameWithoutExtension(surfMaterial.References.Material.File?.FilePath));

        if (!surfaceMaterialIndices.TryGetValue(key, out var index))
        {
            index = surfaceMaterials.Count;
            surfaceMaterials.Add(SurfaceMaterial.From(surfMaterial));
            surfaceMaterialIndices.Add(key, index);
        }

        return index;
    }

    public ImmutableList<SurfaceMaterial> GetSurfaceMaterials()
    {
        return [.. surfaceMaterials];
    }
}
