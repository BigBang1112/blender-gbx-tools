using BlenderGbxTools.Models;
using BlenderGbxTools.Tests.Helpers;
using GBX.NET;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Models;

internal sealed class VisualTests
{
    [Test]
    public async Task Constructor_EmptyVisual_LeavesBuffersNull()
    {
        // Arrange
        var plugVisual = new CPlugVisualIndexed();

        // Act
        var visual = new Visual(plugVisual);

        // Assert
        await Assert.That(visual.Vertices).IsNull();
        await Assert.That(visual.Indices).IsNull();
        await Assert.That(visual.Normals).IsNull();
        await Assert.That(visual.Colors).IsNull();
    }

    [Test]
    public async Task Constructor_LegacyVertices_WritesPositionsAndNormalsAsFloats()
    {
        // Arrange
        var plugVisual = new CPlugVisualIndexed
        {
            Vertices =
            [
                new(Position: new Vec3(1, 2, 3), Normal: new Vec3(0, 1, 0), Color: null),
                new(Position: new Vec3(4, 5, 6), Normal: new Vec3(0, 0, 1), Color: null),
            ],
        };

        // Act
        var visual = new Visual(plugVisual);

        // Assert
        await Assert.That(Buffers.ToFloats(visual.Vertices)).IsEquivalentTo([1f, 2f, 3f, 4f, 5f, 6f]);
        await Assert.That(Buffers.ToFloats(visual.Normals)).IsEquivalentTo([0f, 1f, 0f, 0f, 0f, 1f]);
    }

    [Test]
    public async Task Constructor_LegacyVerticesWithoutNormals_LeavesNormalsNull()
    {
        // Arrange
        var plugVisual = new CPlugVisualIndexed
        {
            Vertices = [new(Position: new Vec3(1, 2, 3), Normal: null, Color: null)],
        };

        // Act
        var visual = new Visual(plugVisual);

        // Assert
        await Assert.That(visual.Vertices).IsNotNull();
        await Assert.That(visual.Normals).IsNull();
    }

    [Test]
    public async Task Constructor_IndexBuffer_CopiesIndices()
    {
        // Arrange
        var plugVisual = new CPlugVisualIndexed { IndexBuffer = new CPlugIndexBuffer { Indices = [0, 1, 2, 2, 1, 3] } };

        // Act
        var visual = new Visual(plugVisual);

        // Assert
        await Assert.That(visual.Indices).IsEquivalentTo([0, 1, 2, 2, 1, 3]);
    }

    [Test]
    public async Task Constructor_LegacyTexCoordSets_WritesOneUvBufferPerSet()
    {
        // Arrange
        var plugVisual = new CPlugVisualIndexed
        {
            TexCoords =
            [
                new CPlugVisual.TexCoordSet { TexCoords = [new(new Vec2(0.25f, 0.5f), null, null)] },
                new CPlugVisual.TexCoordSet { TexCoords = [new(new Vec2(1, 0), null, null)] },
            ],
        };

        // Act
        var visual = new Visual(plugVisual);

        // Assert
        await Assert.That(visual.TexCoords!).Count().IsEqualTo(2);
        await Assert.That(Buffers.ToFloats(visual.TexCoords![0])).IsEquivalentTo([0.25f, 0.5f]);
        await Assert.That(Buffers.ToFloats(visual.TexCoords[1])).IsEquivalentTo([1f, 0f]);
    }
}
