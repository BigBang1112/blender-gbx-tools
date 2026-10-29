using BlenderGbxTools.Models;
using BlenderGbxTools.Tests.Helpers;
using GBX.NET;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Models;

internal sealed class SurfaceTests
{
    [Test]
    public async Task Constructor_SphereSurf_SetsSphereSizeAndSurfaceIndex()
    {
        // Arrange
        var surface = new CPlugSurface { Surf = new CPlugSurface.Sphere { Size = 2.5f, SurfaceIndex = 3 } };

        // Act
        var result = new Surface("Ball.Shape.Gbx", surface);

        // Assert
        await Assert.That(result.Sphere).IsEqualTo(2.5f);
        await Assert.That(result.SurfaceIndex).IsEqualTo(3);
        await Assert.That(result.Ellipsoid).IsNull();
        await Assert.That(result.Positions).IsNull();
    }

    [Test]
    public async Task Constructor_EllipsoidSurf_SetsEllipsoidSizeAndSurfaceIndex()
    {
        // Arrange
        var surface = new CPlugSurface { Surf = new CPlugSurface.Ellipsoid { Size = new Vec3(1, 2, 3), SurfaceIndex = 4 } };

        // Act
        var result = new Surface("Egg.Shape.Gbx", surface);

        // Assert
        await Assert.That(result.Ellipsoid).IsEqualTo(new Vec3(1, 2, 3));
        await Assert.That(result.SurfaceIndex).IsEqualTo(4);
        await Assert.That(result.Sphere).IsNull();
    }

    [Test]
    public async Task Constructor_GeomSurfAndSurf_PrefersGeomSurf()
    {
        // Arrange
        var surface = new CPlugSurface
        {
            Geom = new CPlugSurfaceGeom { Surf = new CPlugSurface.Sphere { Size = 1 } },
            Surf = new CPlugSurface.Ellipsoid { Size = new Vec3(5, 5, 5) },
        };

        // Act
        var result = new Surface(null, surface);

        // Assert
        await Assert.That(result.Sphere).IsEqualTo(1);
        await Assert.That(result.Ellipsoid).IsNull();
    }

    [Test]
    public async Task Constructor_MeshSurf_WritesVertexPositionsAsFloats()
    {
        // Arrange
        var surface = new CPlugSurface
        {
            Surf = new CPlugSurface.Mesh { Vertices = [new Vec3(1, 2, 3), new Vec3(4, 5, 6)] },
        };

        // Act
        var result = new Surface(null, surface);

        // Assert
        await Assert.That(Buffers.ToFloats(result.Positions)).IsEquivalentTo([1f, 2f, 3f, 4f, 5f, 6f]);
        await Assert.That(result.Indices).IsNull();
    }

    [Test]
    public async Task Constructor_MeshWithCookedTriangles_WritesSurfaceIndexThenVertexIndices()
    {
        // Arrange
        var mesh = new CPlugSurface.Mesh
        {
            Vertices = [default, default, default],
            CookedTriangles =
            [
                new(U01: default, Indices: new Int3(0, 1, 2), SurfaceIndex: 7, U04: 0, U05: 0),
                new(U01: default, Indices: new Int3(2, 1, 0), SurfaceIndex: 8, U04: 0, U05: 0),
            ],
        };

        // Act
        var result = new Surface(null, new CPlugSurface { Surf = mesh });

        // Assert
        await Assert.That(result.Indices).IsEquivalentTo([7, 0, 1, 2, 8, 2, 1, 0]);
    }

    [Test]
    public async Task Constructor_MeshWithOnlyTriangles_WritesSurfaceIndexThenVertexIndices()
    {
        // Arrange
        var mesh = new CPlugSurface.Mesh
        {
            Vertices = [default, default, default],
            Triangles = [new(Indices: new Int3(0, 1, 2), U02: 0, U03: 0, SurfaceIndex: 5)],
        };

        // Act
        var result = new Surface(null, new CPlugSurface { Surf = mesh });

        // Assert
        await Assert.That(result.Indices).IsEquivalentTo([5, 0, 1, 2]);
    }

    [Test]
    public async Task Constructor_MeshWithCookedTrianglesAndTriangles_PrefersCookedTriangles()
    {
        // Arrange
        var mesh = new CPlugSurface.Mesh
        {
            Vertices = [default, default, default],
            CookedTriangles = [new(U01: default, Indices: new Int3(0, 1, 2), SurfaceIndex: 1, U04: 0, U05: 0)],
            Triangles = [new(Indices: new Int3(2, 1, 0), U02: 0, U03: 0, SurfaceIndex: 2)],
        };

        // Act
        var result = new Surface(null, new CPlugSurface { Surf = mesh });

        // Assert
        await Assert.That(result.Indices).IsEquivalentTo([1, 0, 1, 2]);
    }

    [Test]
    public async Task Constructor_MeshWithMoreTrianglesThanUShortMax_Throws()
    {
        // Arrange
        var mesh = new CPlugSurface.Mesh
        {
            Vertices = [],
            CookedTriangles = new CPlugSurface.Mesh.CookedTriangle[ushort.MaxValue + 1],
        };
        var surface = new CPlugSurface { Surf = mesh };

        // Act
        var act = () => new Surface(null, surface);

        // Assert
        await Assert.That(act).Throws<Exception>().WithMessageContaining("too many triangles");
    }

    [Test]
    public async Task Constructor_Standalone_MapsSurfaceMaterials()
    {
        // Arrange
        var surface = new CPlugSurface
        {
            Materials = [SurfMaterials.Create(CPlugSurface.MaterialId.Grass, @"Media\Material\Grass.Material.Gbx")],
        };

        // Act
        var result = new Surface(null, surface);

        // Assert
        await Assert.That(result.SurfaceMaterials).HasSingleItem();
        await Assert.That(result.SurfaceMaterials![0].SurfaceId).IsEqualTo(CPlugSurface.MaterialId.Grass);
        await Assert.That(result.SurfaceMaterials[0].Name).IsEqualTo("Grass");
    }

    [Test]
    public async Task Constructor_NotStandalone_LeavesSurfaceMaterialsToParent()
    {
        // Arrange
        var surface = new CPlugSurface { Materials = [SurfMaterials.Create(CPlugSurface.MaterialId.Grass)] };

        // Act
        var result = new Surface(null, surface, standalone: false);

        // Assert
        await Assert.That(result.SurfaceMaterials).IsNull();
    }
}
