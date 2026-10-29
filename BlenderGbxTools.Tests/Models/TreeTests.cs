using BlenderGbxTools.Models;
using BlenderGbxTools.Tests.Helpers;
using GBX.NET;
using GBX.NET.Engines.Graphic;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Models;

internal sealed class TreeTests
{
    [Test]
    public async Task Constructor_IdentityLocation_LeavesLocationNull()
    {
        // Arrange
        var plugTree = new CPlugTree { Location = Iso4.Identity };

        // Act
        var tree = new Tree(plugTree);

        // Assert
        await Assert.That(tree.Location).IsNull();
    }

    [Test]
    public async Task Constructor_TranslatedLocation_WritesIso4AsTwelveFloats()
    {
        // Arrange
        var plugTree = new CPlugTree { Location = Iso4.Identity with { TX = 1, TY = 2, TZ = 3 } };

        // Act
        var tree = new Tree(plugTree);

        // Assert
        var location = Buffers.ToFloats(tree.Location);
        await Assert.That(location).IsEquivalentTo([1f, 0, 0, 0, 1, 0, 0, 0, 1, 1, 2, 3]);
    }

    [Test]
    public async Task Constructor_NoChildren_LeavesChildrenNull()
    {
        // Arrange
        var plugTree = new CPlugTree();

        // Act
        var tree = new Tree(plugTree);

        // Assert
        await Assert.That(tree.Children).IsNull();
    }

    [Test]
    public async Task Constructor_NestedChildren_MapsWholeHierarchy()
    {
        // Arrange
        var grandChild = new CPlugTree { PlugId = "GrandChild" };
        var child = new CPlugTree { PlugId = "Child", Children = new([grandChild]) };
        var plugTree = new CPlugTree { PlugId = "Root", Children = new([child]) };

        // Act
        var tree = new Tree(plugTree);

        // Assert
        await Assert.That(tree.Children).HasSingleItem();
        await Assert.That(tree.Children![0].Name).IsEqualTo("Child");
        await Assert.That(tree.Children[0].Children![0].Name).IsEqualTo("GrandChild");
    }

    [Test]
    public async Task Constructor_VisualMipWithDuplicateFarZ_KeepsFirstLevel()
    {
        // Arrange
        var plugTree = new CPlugTreeVisualMip
        {
            Levels =
            [
                new(100, new CPlugTree { PlugId = "First" }),
                new(100, new CPlugTree { PlugId = "Duplicate" }),
                new(200, new CPlugTree { PlugId = "Far" }),
            ],
        };

        // Act
        var tree = new Tree(plugTree);

        // Assert
        await Assert.That(tree.VisualMip!.Keys).IsEquivalentTo([100f, 200f]);
        await Assert.That(tree.VisualMip[100].Name).IsEqualTo("First");
    }

    [Test]
    public async Task Constructor_VisualMipLevelWithoutTree_SkipsLevel()
    {
        // Arrange
        var plugTree = new CPlugTreeVisualMip
        {
            Levels = [new(100, null!), new(200, new CPlugTree())],
        };

        // Act
        var tree = new Tree(plugTree);

        // Assert
        await Assert.That(tree.VisualMip!.Keys).IsEquivalentTo([200f]);
    }

    [Test]
    public async Task Constructor_ShaderFile_UsesShaderFileNameAsMaterial()
    {
        // Arrange
        var plugTree = new CPlugTree();
        plugTree.References.Shader.Descriptor = GbxFiles.Create(@"Media\Material\RoadTech.Material.Gbx");

        // Act
        var tree = new Tree(plugTree);

        // Assert
        await Assert.That(tree.Material).IsEqualTo("RoadTech");
    }

    [Test]
    public async Task Constructor_TreeLight_MapsLight()
    {
        // Arrange
        var plugTree = new CPlugTreeLight
        {
            PlugLight = new CPlugLight { Light = new GxLightBall { Intensity = 2 } },
        };

        // Act
        var tree = new Tree(plugTree);

        // Assert
        await Assert.That(tree.Light).IsNotNull();
        await Assert.That(tree.Light!.Intensity).IsEqualTo(2);
    }

    [Test]
    public async Task Constructor_PlainTree_HasNoLight()
    {
        // Arrange
        var plugTree = new CPlugTree();

        // Act
        var tree = new Tree(plugTree);

        // Assert
        await Assert.That(tree.Light).IsNull();
    }

    [Test]
    public async Task Constructor_TreeSurface_MapsSurfaceWithoutMaterials()
    {
        // Arrange
        var plugTree = new CPlugTree
        {
            Surface = new CPlugSurface
            {
                Surf = new CPlugSurface.Sphere { Size = 4 },
                Materials = [SurfMaterials.Create(CPlugSurface.MaterialId.Metal)],
            },
        };

        // Act
        var tree = new Tree(plugTree);

        // Assert
        await Assert.That(tree.Surface!.Sphere).IsEqualTo(4);
        await Assert.That(tree.Surface.SurfaceMaterials).IsNull();
    }
}
