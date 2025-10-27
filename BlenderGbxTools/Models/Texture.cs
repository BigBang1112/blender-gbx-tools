using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed class Texture
{
    public string? GbxPath { get; }
    public string? ImagePath { get; }

    public Texture()
    {
        
    }

    public Texture(string? filePath, CPlugBitmap texture)
    {
        GbxPath = filePath;
        ImagePath = texture.ImageFile?.GetFullPath();
    }
}
