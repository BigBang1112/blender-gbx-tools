using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Models;

internal sealed class Reusable
{
    public ImmutableDictionary<string, Material?>? Materials { get; }
    public ImmutableList<CPlugSurface.MaterialId>? Surfaces { get; }
    public ImmutableList<Solid>? Solids { get; }
    public ImmutableList<Solid2>? Solid2s { get; }
    public ImmutableList<Prefab>? Prefabs { get; }
}
