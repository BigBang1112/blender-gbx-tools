using BlenderGbxTools.Enums;
using GBX.NET.Engines.Graphic;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed class Light
{
    public LightType Type { get; }
    public float Intensity { get; }
    public float? SpotSize { get; }
    public float? SpotBlend { get; }

    public Light()
    {
        
    }

    public Light(CPlugTreeLight treeLight)
    {
        var gxLight = treeLight.PlugLight?.Light;

        if (gxLight is null)
        {
            return;
        }

        Intensity = gxLight.Intensity;

        switch (gxLight)
        {
            case GxLightSpot spot:
                Type = LightType.Spot;
                SpotSize = spot.AngleOuter;
                SpotBlend = 1 - spot.AngleInner / spot.AngleOuter;
                break;
            case GxLightBall ball:
                break;
            case GxLightPoint point:
                Type = LightType.Point;
                break;
        }
    }
}