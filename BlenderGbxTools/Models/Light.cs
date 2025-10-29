using BlenderGbxTools.Enums;
using GBX.NET.Engines.Graphic;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed class Light
{
    public LightType Type { get; }
    public float R { get; }
    public float G { get; }
    public float B { get; }
    public float Intensity { get; }
    public float? Radius { get; }
    public float? AngleInner { get; }
    public float? AngleOuter { get; }
    public bool NightOnly { get; }

    public Light()
    {
        
    }

    public Light(CPlugTreeLight treeLight)
    {
        if (treeLight.PlugLight is not CPlugLight plugLight)
        {
            return;
        }

        NightOnly = plugLight.NightOnly;

        var gxLight = plugLight.Light;
        
        if (gxLight is null)
        {
            return;
        }

        R = gxLight.Color.X;
        G = gxLight.Color.Y;
        B = gxLight.Color.Z;
        Intensity = gxLight.Intensity;

        if (gxLight is GxLightPoint point)
        {
            Type = LightType.POINT;
        }

        if (gxLight is GxLightBall ball)
        {
            Radius = ball.Radius;
        }

        if (gxLight is GxLightSpot spot)
        {
            Type = LightType.SPOT;
            AngleInner = spot.AngleInner;
            AngleOuter = spot.AngleOuter;
        }
    }
}