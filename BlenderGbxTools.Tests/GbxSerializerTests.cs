using BlenderGbxTools.Tests.Builders;
using BlenderGbxTools.Tests.Helpers;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Graphic;
using GBX.NET.Engines.MwFoundations;
using GBX.NET.Engines.Plug;
using System.Text.Json;

namespace BlenderGbxTools.Tests;

internal sealed class GbxSerializerTests
{
    [Test]
    [MethodDataSource(nameof(UnsupportedNodes))]
    public async Task Serialize_UnsupportedNode_ThrowsNotSupportedException(CMwNod? node)
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        var act = () => GbxSerializer.Serialize(stream, "Unsupported.Gbx", node);

        // Assert
        await Assert.That(act).ThrowsExactly<NotSupportedException>();
    }

    public static IEnumerable<Func<CMwNod?>> UnsupportedNodes()
    {
        yield return () => null;
        yield return () => new CPlugTree();
    }

    [Test]
    public async Task Serialize_Solid_WritesFileNameAndTree()
    {
        // Arrange
        var solid = new CPlugSolid { Tree = new CPlugTree { PlugId = "Root" } };

        // Act
        using var json = Serialize("Block.Solid.Gbx", solid);

        // Assert
        await Assert.That(json.RootElement.GetProperty("Name").GetString()).IsEqualTo("Block.Solid.Gbx");
        await Assert.That(json.RootElement.GetProperty("Tree").GetProperty("Name").GetString()).IsEqualTo("Root");
    }

    [Test]
    public async Task Serialize_NullProperties_AreOmitted()
    {
        // Arrange
        var surface = new CPlugSurface { Surf = new CPlugSurface.Sphere { Size = 2 } };

        // Act
        using var json = Serialize("Ball.Shape.Gbx", surface);

        // Assert
        await Assert.That(json.RootElement.GetProperty("Sphere").GetSingle()).IsEqualTo(2);
        await Assert.That(json.RootElement.TryGetProperty("Ellipsoid", out _)).IsFalse();
        await Assert.That(json.RootElement.TryGetProperty("Positions", out _)).IsFalse();
    }

    [Test]
    public async Task Serialize_LightType_IsWrittenAsString()
    {
        // Arrange
        var solid = new CPlugSolid
        {
            Tree = new CPlugTreeLight { PlugLight = new CPlugLight { Light = new GxLightSpot() } },
        };

        // Act
        using var json = Serialize("Lamp.Solid.Gbx", solid);

        // Assert
        var light = json.RootElement.GetProperty("Tree").GetProperty("Light");
        await Assert.That(light.GetProperty("Type").GetString()).IsEqualTo("SPOT");
    }

    [Test]
    public async Task Serialize_Prefab_WritesEntsWithMeshes()
    {
        // Arrange
        var prefab = new CPlugPrefabBuilder()
            .WithStaticObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialId("Dirt").Build())
            .Build();

        // Act
        using var json = Serialize("Tree.Prefab.Gbx", prefab);

        // Assert
        var ents = json.RootElement.GetProperty("Ents");
        await Assert.That(ents.GetArrayLength()).IsEqualTo(1);
        await Assert.That(ents[0].TryGetProperty("Mesh", out _)).IsTrue();
        await Assert.That(json.RootElement.GetProperty("Materials").TryGetProperty("Dirt", out _)).IsTrue();
    }

    [Test]
    public async Task Serialize_TreeWithEmbeddedMaterial_MaterialNameIsKeyOfMaterials()
    {
        // Arrange
        var solid = new CPlugSolid
        {
            Tree = new CPlugTree { Children = new([new CPlugTree { Shader = new CPlugMaterial() }]) },
        };

        // Act
        using var json = Serialize("Block.Solid.Gbx", solid);

        // Assert
        var materialName = json.RootElement.GetProperty("Tree").GetProperty("Children")[0].GetProperty("Material").GetString();
        await Assert.That(json.RootElement.GetProperty("Materials").TryGetProperty(materialName!, out _)).IsTrue();
    }

    [Test]
    public async Task Serialize_ShadedGeomWithEmbeddedMaterial_MaterialNameIsKeyOfMaterials()
    {
        // Arrange
        var solid2 = new CPlugSolid2Model
        {
            Visuals = new([new CPlugVisualIndexed()]),
            Materials = new([new CPlugMaterial()]),
            ShadedGeoms = [new CPlugSolid2Model.ShadedGeom { VisualIndex = 0, MaterialIndex = 0 }],
        };

        // Act
        using var json = Serialize("Mesh.Mesh.Gbx", solid2);

        // Assert
        var materialName = json.RootElement.GetProperty("ShadedGeoms")[0].GetProperty("Material").GetString();
        await Assert.That(json.RootElement.GetProperty("Materials").TryGetProperty(materialName!, out _)).IsTrue();
    }

    [Test]
    public async Task Serialize_Surface_WritesEverySurfaceMaterialInOrder()
    {
        // Arrange
        var surface = new CPlugSurface
        {
            Surf = new CPlugSurface.Sphere(),
            Materials =
            [
                SurfMaterials.Create(CPlugSurface.MaterialId.Grass),
                SurfMaterials.Create(CPlugSurface.MaterialId.Grass),
                SurfMaterials.Create(CPlugSurface.MaterialId.Ice),
            ],
        };

        // Act
        using var json = Serialize("Ball.Shape.Gbx", surface);

        // Assert
        // Triangles refer to surface materials by index, so duplicates must be kept
        var surfaceIds = json.RootElement.GetProperty("SurfaceMaterials").EnumerateArray()
            .Select(x => x.GetProperty("SurfaceId").GetString())
            .ToList();
        await Assert.That(surfaceIds).IsEquivalentTo(["Grass", "Grass", "Ice"]);
        await Assert.That(json.RootElement.TryGetProperty("Materials", out _)).IsFalse();
    }

    [Test]
    public async Task Serialize_NestedModels_WriteExecutionTimeOnlyOnRoot()
    {
        // Arrange
        var solid = new CPlugSolid
        {
            Tree = new CPlugTree { Surface = new CPlugSurface { Surf = new CPlugSurface.Sphere() } },
        };

        // Act
        using var json = Serialize("Block.Solid.Gbx", solid);

        // Assert
        var nestedSurface = json.RootElement.GetProperty("Tree").GetProperty("Surface");
        await Assert.That(json.RootElement.TryGetProperty("ExecutionTimeInSeconds", out _)).IsTrue();
        await Assert.That(nestedSurface.TryGetProperty("ExecutionTimeInSeconds", out _)).IsFalse();
        await Assert.That(nestedSurface.TryGetProperty("SurfaceMaterials", out _)).IsFalse();
    }

    [Test]
    public async Task Serialize_BlockInfoVariantMobils_WritesMaterialsOfSolidsAndPrefabs()
    {
        // Arrange
        var solid = new CPlugSolid { Tree = new CPlugTree { Shader = new CPlugMaterial() } };
        var prefab = new CPlugPrefabBuilder()
            .WithStaticObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialId("Dirt").Build())
            .Build();
        var blockInfo = new CGameCtnBlockInfoClassic
        {
            Ident = new("RoadStraight"),
            VariantBaseAir = new CGameCtnBlockInfoVariantAir
            {
                Mobils = [new([new CGameCtnBlockInfoMobil { SolidFid = solid, PrefabFid = prefab }])],
            },
        };

        // Act
        using var json = Serialize("RoadStraight.EDClassic.Gbx", blockInfo);

        // Assert
        var mobil = json.RootElement.GetProperty("VariantAir").GetProperty("Mobils")[0][0];
        var solidMaterial = mobil.GetProperty("Solid").GetProperty("Tree").GetProperty("Material").GetString();
        var materials = json.RootElement.GetProperty("Materials");
        await Assert.That(materials.TryGetProperty(solidMaterial!, out _)).IsTrue();
        await Assert.That(materials.TryGetProperty("Dirt", out _)).IsTrue();
    }

    private static JsonDocument Serialize(string fileName, CMwNod node)
    {
        using var stream = new MemoryStream();
        GbxSerializer.Serialize(stream, fileName, node);
        return JsonDocument.Parse(stream.ToArray());
    }
}
