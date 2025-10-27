using GBX.NET;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed class Surface
{
    public Vec3? Ellipsoid { get; }
    public float? Sphere { get; }
    public byte[]? Positions { get; }
    public byte[]? Indices { get; }

    public Surface()
    {
        
    }

    public Surface(CPlugSurface surface)
    {
        var surf = surface.Geom?.Surf ?? surface.Surf;

        var ell = default(Vec3?);
        if (surf is CPlugSurface.Ellipsoid ellipsoid)
        {
            ell = ellipsoid.Size;
        }

        var sph = default(float?);
        if (surf is CPlugSurface.Sphere sphere)
        {
            sph = sphere.Size;
        }

        var pos = default(byte[]);
        var ind = default(byte[]);
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

            pos = posStream.ToArray();

            using var indStream = new MemoryStream();
            using var indWriter = new BinaryWriter(indStream);

            foreach (var tri in mesh.CookedTriangles ?? [])
            {
                indWriter.Write((ushort)tri.U02.X);
                indWriter.Write((ushort)tri.U02.Y);
                indWriter.Write((ushort)tri.U02.Z);
            }

            ind = indStream.ToArray();
        }

        Ellipsoid = ell;
        Sphere = sph;
        Positions = pos;
        Indices = ind;
    }
}