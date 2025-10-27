using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Models;

internal sealed class Material
{
    public ImmutableDictionary<string, Texture>? Textures { get; private init; }

    public Material()
    {
        
    }

    public Material(CPlugMaterial material)
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

                // bitmap.TextureFile
                textures[textureName] = new Texture(bitmap.TextureFile?.GetFullPath(), texture);
            }
        }
        else if (material.DeviceMaterials?.Length > 0)
        {
            // shader-material!
        }
        else
        {
            throw new Exception("Material has no custom material or device materials");
        }

        Textures = textures.ToImmutable();
    }
}