using BlenderGbxTools.Models;
using BlenderGbxTools.Tests.Builders;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Models;

internal sealed class ItemTests
{
    [Test]
    public async Task Constructor_CrystalItem_TakesMaterialsFromCrystal()
    {
        // Arrange
        var item = new CGameItemModel
        {
            EntityModelEdition = new CGameCommonItemEntityModelEdition { MeshCrystal = CreateCrystal("Wood") },
        };

        // Act
        var result = new Item("Ramp.Item.Gbx", item);

        // Assert
        await Assert.That(result.Name).IsEqualTo("Ramp.Item.Gbx");
        await Assert.That(result.Crystal).IsNotNull();
        await Assert.That(result.Prefab).IsNull();
        await Assert.That(result.Materials!.Keys).IsEquivalentTo(["Wood"]);
    }

    [Test]
    public async Task Constructor_PrefabItem_MapsPrefabWithoutOwnMaterials()
    {
        // Arrange
        var item = new CGameItemModel
        {
            EntityModel = new CPlugPrefabBuilder()
                .WithStaticObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialId("Dirt").Build())
                .Build(),
        };

        // Act
        var result = new Item("Tree.Item.Gbx", item);

        // Assert
        await Assert.That(result.Prefab!.Ents).HasSingleItem();
        await Assert.That(result.Prefab.Materials).IsNull();
        await Assert.That(result.Materials!.Keys).IsEquivalentTo(["Dirt"]);
    }

    [Test]
    public async Task Constructor_CrystalAndPrefab_TakesMaterialsFromPrefab()
    {
        // Arrange
        var item = new CGameItemModel
        {
            EntityModelEdition = new CGameCommonItemEntityModelEdition { MeshCrystal = CreateCrystal("Wood") },
            EntityModel = new CPlugPrefabBuilder()
                .WithStaticObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialId("Dirt").Build())
                .Build(),
        };

        // Act
        var result = new Item("Mixed.Item.Gbx", item);

        // Assert
        await Assert.That(result.Materials!.Keys).IsEquivalentTo(["Dirt"]);
    }

    [Test]
    public async Task Constructor_NotStandalone_LeavesMaterialsToParent()
    {
        // Arrange
        var item = new CGameItemModel
        {
            EntityModelEdition = new CGameCommonItemEntityModelEdition { MeshCrystal = CreateCrystal("Wood") },
        };

        // Act
        var result = new Item("Ramp.Item.Gbx", item, standalone: false);

        // Assert
        await Assert.That(result.Materials).IsNull();
    }

    private static CPlugCrystal CreateCrystal(string materialName)
    {
        return new CPlugCrystal
        {
            Materials = [new CPlugCrystal.Material { MaterialUserInst = new CPlugMaterialUserInst { MaterialName = materialName } }],
        };
    }
}
