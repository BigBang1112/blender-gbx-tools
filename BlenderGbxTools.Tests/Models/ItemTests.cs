using BlenderGbxTools.Mapping;
using BlenderGbxTools.Models;
using BlenderGbxTools.Tests.Builders;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Models;

internal sealed class ItemTests
{
    [Test]
    public async Task From_CrystalItem_AddsCrystalMaterials()
    {
        // Arrange
        var item = new CGameItemModel
        {
            EntityModelEdition = new CGameCommonItemEntityModelEdition { MeshCrystal = CreateCrystal("Wood") },
        };
        var context = new MappingContext();

        // Act
        var result = Item.From("Ramp.Item.Gbx", item, context);

        // Assert
        await Assert.That(result.Name).IsEqualTo("Ramp.Item.Gbx");
        await Assert.That(result.Crystal).IsNotNull();
        await Assert.That(result.Prefab).IsNull();
        await Assert.That(context.GetMaterials().Keys).IsEquivalentTo(["Wood"]);
    }

    [Test]
    public async Task From_PrefabItem_MapsPrefabAndAddsItsMaterials()
    {
        // Arrange
        var item = new CGameItemModel
        {
            EntityModel = new CPlugPrefabBuilder()
                .WithStaticObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialId("Dirt").Build())
                .Build(),
        };
        var context = new MappingContext();

        // Act
        var result = Item.From("Tree.Item.Gbx", item, context);

        // Assert
        await Assert.That(result.Prefab!.Ents).HasSingleItem();
        await Assert.That(result.Prefab.Materials).IsNull();
        await Assert.That(context.GetMaterials().Keys).IsEquivalentTo(["Dirt"]);
    }

    [Test]
    public async Task From_CrystalAndPrefab_AddsMaterialsOfBoth()
    {
        // Arrange
        var item = new CGameItemModel
        {
            EntityModelEdition = new CGameCommonItemEntityModelEdition { MeshCrystal = CreateCrystal("Wood") },
            EntityModel = new CPlugPrefabBuilder()
                .WithStaticObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialId("Dirt").Build())
                .Build(),
        };
        var context = new MappingContext();

        // Act
        Item.From("Mixed.Item.Gbx", item, context);

        // Assert
        await Assert.That(context.GetMaterials().Keys).IsEquivalentTo(["Wood", "Dirt"]);
    }

    private static CPlugCrystal CreateCrystal(string materialName)
    {
        return new CPlugCrystal
        {
            Materials = [new CPlugCrystal.Material { MaterialUserInst = new CPlugMaterialUserInst { MaterialName = materialName } }],
        };
    }
}
