using BlenderGbxTools.Extensions;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Models;

internal sealed record Material
{
    public ImmutableDictionary<string, Texture>? Textures { get; init; }
    public CPlugSurface.MaterialId SurfaceId { get; init; }
    public string? Shader { get; init; }

    public static Material From(CPlugMaterial material)
    {
        var textures = ImmutableDictionary.CreateBuilder<string, Texture>();

        if (material.CustomMaterial is not null)
        {
            // typical material
            foreach (var bitmap in material.CustomMaterial.Textures ?? [])
            {
                var textureName = bitmap.Name ?? throw new Exception("Texture has no name");

                if (bitmap.Texture is not CPlugBitmap texture)
                {
                    continue;
                }

                textures[textureName] = Texture.From(bitmap.References.Texture.File?.GetFullPath(), texture);
            }
        }
        else if (material.DeviceMaterials?.Length > 0)
        {
            // shader-material!
        }
        else
        {
            // often case with special effects?
        }

        return new Material
        {
            Textures = textures.ToImmutable(),
            SurfaceId = material.SurfaceId,
            Shader = material.GetShaderName(),
        };
    }

    public static Material From(CPlugMaterialUserInst mat)
    {
        return new Material { SurfaceId = mat.SurfacePhysicId };
    }
}
