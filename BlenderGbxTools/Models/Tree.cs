using BlenderGbxTools.Extensions;
using GBX.NET;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace BlenderGbxTools.Models;

internal sealed class Tree
{
    public string? Name { get; }
    public Visual? Visual { get; }
    public byte[]? Location { get; }
    public ImmutableList<Tree>? Children { get; }
    public ulong? Flags { get; }
    public ImmutableDictionary<float, Tree>? VisualMip { get; }
    public Light? Light { get; }
    public Surface? Surface { get; }
    public string? Material { get; }
    public bool Visible { get; }

    public Tree()
    {
        
    }

    public Tree(CPlugTree tree)
    {
        var visual = default(Visual);

        if (tree.Visual is CPlugVisualIndexed visualIndexed)
        {
            visual = new Visual(visualIndexed);
        }

        var mipDict = default(ImmutableDictionary<float, Tree>);

        if (tree is CPlugTreeVisualMip mip)
        {
            mipDict = mip.Levels
                .DistinctBy(x => x.FarZ) // FarZ can be duplicated sometimes, epic Nadeo. should be additionally logged
                .ToImmutableDictionary(x => x.FarZ, x => new Tree(x.Tree));
        }

        var light = default(Light);

        if (tree is CPlugTreeLight treeLight)
        {
            light = new Light(treeLight);
        }

        var surface = default(Surface);

        if (tree.Surface is CPlugSurface plugSurface)
        {
            surface = new Surface(null, plugSurface, standalone: false);
        }

        Name = tree.Name ?? "[unnamed]";
        Visual = visual;
        Location = LocationToByteArray(tree);
        Children = tree.Children.Count == 0 ? null : tree.Children.Select(x => new Tree(x)).ToImmutableList();
        VisualMip = mipDict;
        Flags = tree.Flags;
        Light = light;
        Material = tree.GetMaterialName();
        Surface = surface;
        Visible = tree.IsVisible;
    }

    private static byte[]? LocationToByteArray(CPlugTree tree)
    {
        if (tree.Location is not Iso4 t || tree.Location == Iso4.Identity)
        {
            return null;
        }

        return MemoryMarshal.AsBytes([t]).ToArray();
    }
}
