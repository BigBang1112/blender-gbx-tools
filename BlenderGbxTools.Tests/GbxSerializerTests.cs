using BlenderGbxTools.Tests.Builders;
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

    private static JsonDocument Serialize(string fileName, CMwNod node)
    {
        using var stream = new MemoryStream();
        GbxSerializer.Serialize(stream, fileName, node);
        return JsonDocument.Parse(stream.ToArray());
    }
}
