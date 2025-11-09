using GBX.NET.Engines.Plug;
using System.Runtime.InteropServices;

namespace BlenderGbxTools.Models;

internal sealed class Visual
{
    public byte[]? Vertices { get; }
    public int[]? Indices { get; }
    public byte[]? Normals { get; }
    public byte[][]? TexCoords { get; }

    public Visual()
    {
        
    }

    public Visual(CPlugVisualIndexed visual)
    {
        using var vertexStream = new MemoryStream();
        using var vertexWriter = new BinaryWriter(vertexStream);

        using var normalStream = new MemoryStream();
        using var normalWriter = new BinaryWriter(normalStream);

        if (visual.VertexStreams.Count == 0)
        {
            for (var i = 0; i < visual.Vertices.Length; i++)
            {
                var vertex = visual.Vertices[i];
                vertexWriter.Write(vertex.Position.X);
                vertexWriter.Write(vertex.Position.Y);
                vertexWriter.Write(vertex.Position.Z);

                if (vertex.Normal.HasValue)
                {
                    normalWriter.Write(vertex.Normal.Value.X);
                    normalWriter.Write(vertex.Normal.Value.Y);
                    normalWriter.Write(vertex.Normal.Value.Z);
                }
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

                foreach (var normal in stream.Normals ?? [])
                {
                    normalWriter.Write(normal.X);
                    normalWriter.Write(normal.Y);
                    normalWriter.Write(normal.Z);
                }
            }
        }

        var indices = visual.IndexBuffer?.Indices ?? [];

        Vertices = vertexStream.Length == 0 ? null : vertexStream.ToArray();
        Indices = indices.Length == 0 ? null : indices;
        Normals = normalStream.Length == 0 ? null : normalStream.ToArray();

        if (visual.VertexStreams.Count == 0)
        {
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
        else
        {
            TexCoords = visual.VertexStreams[0].UVs.Select(uvSet =>
            {
                return MemoryMarshal.AsBytes(uvSet.Value).ToArray();
            }).ToArray();
        }
    }
}