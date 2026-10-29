using BlenderGbxTools.Enums;
using BlenderGbxTools.Models;
using GBX.NET;
using GBX.NET.Engines.Graphic;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Models;

internal sealed class LightTests
{
    [Test]
    public async Task From_NoPlugLight_LeavesLightDisabled()
    {
        // Arrange
        var treeLight = new CPlugTreeLight();

        // Act
        var light = Light.From(treeLight);

        // Assert
        await Assert.That(light.Intensity).IsEqualTo(0);
        await Assert.That(light.NightOnly).IsFalse();
    }

    [Test]
    public async Task From_PlugLightWithoutGxLight_KeepsNightOnly()
    {
        // Arrange
        var treeLight = new CPlugTreeLight { PlugLight = new CPlugLight { NightOnly = true } };

        // Act
        var light = Light.From(treeLight);

        // Assert
        await Assert.That(light.NightOnly).IsTrue();
        await Assert.That(light.Intensity).IsEqualTo(0);
    }

    [Test]
    public async Task From_GxLight_CopiesColorAndIntensity()
    {
        // Arrange
        var treeLight = CreateTreeLight(new GxLightPoint { Color = new Vec3(0.25f, 0.5f, 0.75f), Intensity = 3 });

        // Act
        var light = Light.From(treeLight);

        // Assert
        await Assert.That(light.R).IsEqualTo(0.25f);
        await Assert.That(light.G).IsEqualTo(0.5f);
        await Assert.That(light.B).IsEqualTo(0.75f);
        await Assert.That(light.Intensity).IsEqualTo(3);
    }

    [Test]
    [MethodDataSource(nameof(GxLightTypes))]
    public async Task From_GxLightType_MapsToBlenderLightType(GxLight gxLight, LightType expectedType)
    {
        // Arrange
        var treeLight = CreateTreeLight(gxLight);

        // Act
        var light = Light.From(treeLight);

        // Assert
        await Assert.That(light.Type).IsEqualTo(expectedType);
    }

    public static IEnumerable<Func<(GxLight, LightType)>> GxLightTypes()
    {
        yield return () => (new GxLightPoint(), LightType.POINT);
        yield return () => (new GxLightBall(), LightType.POINT);
        yield return () => (new GxLightSpot(), LightType.SPOT);
    }

    [Test]
    public async Task From_PointLight_HasNoRadiusOrAngles()
    {
        // Arrange
        var treeLight = CreateTreeLight(new GxLightPoint());

        // Act
        var light = Light.From(treeLight);

        // Assert
        await Assert.That(light.Radius).IsNull();
        await Assert.That(light.AngleInner).IsNull();
        await Assert.That(light.AngleOuter).IsNull();
    }

    [Test]
    public async Task From_BallLight_SetsRadius()
    {
        // Arrange
        var treeLight = CreateTreeLight(new GxLightBall { Radius = 12 });

        // Act
        var light = Light.From(treeLight);

        // Assert
        await Assert.That(light.Radius).IsEqualTo(12);
        await Assert.That(light.AngleInner).IsNull();
    }

    [Test]
    public async Task From_SpotLight_SetsRadiusAndAngles()
    {
        // Arrange
        var treeLight = CreateTreeLight(new GxLightSpot { Radius = 20, AngleInner = 30, AngleOuter = 45 });

        // Act
        var light = Light.From(treeLight);

        // Assert
        await Assert.That(light.Radius).IsEqualTo(20);
        await Assert.That(light.AngleInner).IsEqualTo(30);
        await Assert.That(light.AngleOuter).IsEqualTo(45);
    }

    private static CPlugTreeLight CreateTreeLight(GxLight gxLight)
    {
        return new CPlugTreeLight { PlugLight = new CPlugLight { Light = gxLight } };
    }
}
