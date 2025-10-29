using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed class Texture
{
    public string? GbxPath { get; }
    public string? ImagePath { get; }
    public float ScaleU { get; }
    public float ScaleV { get; }

    public Texture()
    {
        
    }

    public Texture(string? filePath, CPlugBitmap texture)
    {
        GbxPath = filePath;
        ImagePath = texture.ImageFile?.GetFullPath();
        ScaleU = texture.DefaultTexCoordScale.X;
        ScaleV = texture.DefaultTexCoordScale.Y;
    }
}
