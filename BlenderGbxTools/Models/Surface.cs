using GBX.NET;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Models;

internal sealed record Surface
{
    public string? Name { get; init; }
    public Vec3? Ellipsoid { get; init; }
    public float? Sphere { get; init; }
    public byte[]? Positions { get; init; }
    public int[]? Indices { get; init; }
    public int? SurfaceIndex { get; init; }

    // Only set on the root model
    public ImmutableList<SurfaceMaterial>? SurfaceMaterials { get; init; }
    public double? ExecutionTimeInSeconds { get; init; }

    public static Surface From(string? fileName, CPlugSurface surface)
    {
        var surf = surface.Geom?.Surf ?? surface.Surf;

        return surf switch
        {
            CPlugSurface.Ellipsoid ellipsoid => new Surface
            {
                Name = fileName,
                Ellipsoid = ellipsoid.Size,
                SurfaceIndex = ellipsoid.SurfaceIndex,
            },
            CPlugSurface.Sphere sphere => new Surface
            {
                Name = fileName,
                Sphere = sphere.Size,
                SurfaceIndex = sphere.SurfaceIndex,
            },
            CPlugSurface.Mesh mesh => new Surface
            {
                Name = fileName,
                Positions = GetPositions(mesh),
                Indices = GetIndices(mesh),
            },
            _ => new Surface { Name = fileName },
        };
    }

    private static byte[] GetPositions(CPlugSurface.Mesh mesh)
    {
        using var posStream = new MemoryStream();
        using var posWriter = new BinaryWriter(posStream);

        foreach (var vertex in mesh.Vertices)
        {
            posWriter.Write(vertex.X);
            posWriter.Write(vertex.Y);
            posWriter.Write(vertex.Z);
        }

        return posStream.ToArray();
    }

    private static int[]? GetIndices(CPlugSurface.Mesh mesh)
    {
        if (mesh.CookedTriangles is not null)
        {
            if (mesh.CookedTriangles.Length > ushort.MaxValue)
            {
                throw new Exception($"Mesh has too many triangles for ushort indices ({mesh.CookedTriangles.Length} > {ushort.MaxValue})");
            }

            var inds = new int[mesh.CookedTriangles.Length * 4];

            for (var i = 0; i < mesh.CookedTriangles.Length; i++)
            {
                var tri = mesh.CookedTriangles[i];
                inds[i * 4 + 0] = tri.SurfaceIndex;
                inds[i * 4 + 1] = tri.Indices.X;
                inds[i * 4 + 2] = tri.Indices.Y;
                inds[i * 4 + 3] = tri.Indices.Z;
            }

            return inds;
        }

        if (mesh.Triangles is not null)
        {
            if (mesh.Triangles.Length > ushort.MaxValue)
            {
                throw new Exception($"Mesh has too many triangles for ushort indices ({mesh.Triangles.Length} > {ushort.MaxValue})");
            }

            var inds = new int[mesh.Triangles.Length * 4];

            for (var i = 0; i < mesh.Triangles.Length; i++)
            {
                var tri = mesh.Triangles[i];
                inds[i * 4 + 0] = tri.SurfaceIndex;
                inds[i * 4 + 1] = tri.Indices.X;
                inds[i * 4 + 2] = tri.Indices.Y;
                inds[i * 4 + 3] = tri.Indices.Z;
            }

            return inds;
        }

        return null;
    }
}
