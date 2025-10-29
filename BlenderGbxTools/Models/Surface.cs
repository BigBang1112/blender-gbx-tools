using GBX.NET;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed class Surface
{
    public Vec3? Ellipsoid { get; }
    public float? Sphere { get; }
    public byte[]? Positions { get; }
    public int[]? Indices { get; }
    public int? SurfaceIndex { get; }

    public Surface()
    {
        
    }

    public Surface(CPlugSurface surface)
    {
        var surf = surface.Geom?.Surf ?? surface.Surf;

        if (surf is CPlugSurface.Ellipsoid ellipsoid)
        {
            Ellipsoid = ellipsoid.Size;
            SurfaceIndex = ellipsoid.U02;
        }

        if (surf is CPlugSurface.Sphere sphere)
        {
            Sphere = sphere.Size;
            SurfaceIndex = sphere.U02;
        }

        if (surf is CPlugSurface.Mesh mesh)
        {
            using var posStream = new MemoryStream();
            using var posWriter = new BinaryWriter(posStream);

            foreach (var vertex in mesh.Vertices)
            {
                posWriter.Write(vertex.X);
                posWriter.Write(vertex.Y);
                posWriter.Write(vertex.Z);
            }

            Positions = posStream.ToArray();

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
                    inds[i * 4 + 0] = tri.U03;
                    inds[i * 4 + 1] = tri.U02.X;
                    inds[i * 4 + 2] = tri.U02.Y;
                    inds[i * 4 + 3] = tri.U02.Z;
                }

                Indices = inds;
            }
            else if (mesh.Triangles is not null)
            {
                if (mesh.Triangles.Length > ushort.MaxValue)
                {
                    throw new Exception($"Mesh has too many triangles for ushort indices ({mesh.Triangles.Length} > {ushort.MaxValue})");
                }

                var inds = new int[mesh.Triangles.Length * 4];

                for (var i = 0; i < mesh.Triangles.Length; i++)
                {
                    var tri = mesh.Triangles[i];
                    inds[i * 4 + 0] = tri.U04;
                    inds[i * 4 + 1] = tri.U01.X;
                    inds[i * 4 + 2] = tri.U01.Y;
                    inds[i * 4 + 3] = tri.U01.Z;
                }

                Indices = inds;
            }
        }
    }
}