using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed class Visual
{
    public byte[]? Vertices { get; }
    public int[]? Indices { get; }
    public byte[][]? TexCoords { get; }

    public Visual()
    {
        
    }

    public Visual(CPlugVisualIndexed visual)
    {
        using var vertexStream = new MemoryStream();
        using var vertexWriter = new BinaryWriter(vertexStream);

        if (visual.VertexStreams.Count == 0)
        {
            for (var i = 0; i < visual.Vertices.Length; i++)
            {
                var vertex = visual.Vertices[i];
                vertexWriter.Write(vertex.Position.X);
                vertexWriter.Write(vertex.Position.Y);
                vertexWriter.Write(vertex.Position.Z);
            }
        }
        else
        {
            foreach (var stream in visual.VertexStreams)
            {
                foreach (var vertex in stream.Positions ?? [])
                {
                    vertexWriter.Write(vertex.X);
                    vertexWriter.Write(vertex.Y);
                    vertexWriter.Write(vertex.Z);
                }
            }
        }

        var indices = visual.IndexBuffer?.Indices ?? [];

        Vertices = vertexStream.Length == 0 ? null : vertexStream.ToArray();
        Indices = indices.Length == 0 ? null : indices;
        TexCoords = visual.TexCoords.Select(set =>
        {
            using var uvStream = new MemoryStream();
            using var uvWriter = new BinaryWriter(uvStream);

            foreach (var uv in set.TexCoords)
            {
                uvWriter.Write(uv.UV.X);
                uvWriter.Write(uv.UV.Y);
            }

            return uvStream.ToArray();
        }).ToArray();
    }
}