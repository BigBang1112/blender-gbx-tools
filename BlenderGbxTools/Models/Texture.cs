using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed record Texture
{
    public string? GbxPath { get; init; }
    public string? ImagePath { get; init; }
    public float ScaleU { get; init; }
    public float ScaleV { get; init; }

    public static Texture From(string? filePath, CPlugBitmap texture)
    {
        return new Texture
        {
            GbxPath = filePath,
            ImagePath = texture.References.Image.File?.GetFullPath(),
            ScaleU = texture.DefaultTexCoordScale.X,
            ScaleV = texture.DefaultTexCoordScale.Y,
        };
    }
}
