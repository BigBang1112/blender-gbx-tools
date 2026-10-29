using BlenderGbxTools.Enums;
using GBX.NET.Engines.Graphic;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed record Light
{
    public LightType Type { get; init; }
    public float R { get; init; }
    public float G { get; init; }
    public float B { get; init; }
    public float Intensity { get; init; }
    public float? Radius { get; init; }
    public float? AngleInner { get; init; }
    public float? AngleOuter { get; init; }
    public bool NightOnly { get; init; }

    public static Light From(CPlugTreeLight treeLight)
    {
        if (treeLight.PlugLight is not CPlugLight plugLight)
        {
            return new Light();
        }

        if (plugLight.Light is not GxLight gxLight)
        {
            return new Light { NightOnly = plugLight.NightOnly };
        }

        // GxLightSpot derives from GxLightBall, which derives from GxLightPoint
        var ball = gxLight as GxLightBall;
        var spot = gxLight as GxLightSpot;

        return new Light
        {
            Type = spot is null ? LightType.POINT : LightType.SPOT,
            R = gxLight.Color.X,
            G = gxLight.Color.Y,
            B = gxLight.Color.Z,
            Intensity = gxLight.Intensity,
            Radius = ball?.Radius,
            AngleInner = spot?.AngleInner,
            AngleOuter = spot?.AngleOuter,
            NightOnly = plugLight.NightOnly,
        };
    }
}
