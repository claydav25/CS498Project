int Get12PanelCh(float2 uv, int dmx)
{
    if(uv.x > (0.835) && uv.x < (0.865))
    {
        if(uv.y > 0.55 && uv.y < 0.575)      { return dmx+(0*3); }
        else if(uv.y > 0.58 && uv.y < 0.605) { return dmx+(1*3); }
        else if(uv.y > 0.61 && uv.y < 0.635) { return dmx+(2*3); }
        else if(uv.y > 0.64 && uv.y < 0.665) { return dmx+(3*3); }
        else if(uv.y > 0.67 && uv.y < 0.695) { return dmx+(4*3); }
        else if(uv.y > 0.7 && uv.y < 0.725)  { return dmx+(5*3); }
        else if(uv.y > 0.73 && uv.y < 0.755) { return dmx+(6*3); }
        else if(uv.y > 0.76 && uv.y < 0.785) { return dmx+(7*3); }
        else if(uv.y > 0.79 && uv.y < 0.815) { return dmx+(8*3); }
        else if(uv.y > 0.82 && uv.y < 0.845) { return dmx+(9*3); }
        else if(uv.y > 0.85 && uv.y < 0.875) { return dmx+(10*3); }
        else if(uv.y > 0.88 && uv.y < 0.905) { return dmx+(11*3); }
        else { return 0; }
    }
    else
    { return 0; }
}

int Get12BeamCh(float2 uv, int dmx)
{
    if(uv.x > (0.875) && uv.x < (0.905))
    {
        if(uv.y > 0.55 && uv.y < 0.575)      { return dmx; }
        else if(uv.y > 0.58 && uv.y < 0.605) { return dmx+1; }
        else if(uv.y > 0.61 && uv.y < 0.635) { return dmx+2; }
        else if(uv.y > 0.64 && uv.y < 0.665) { return dmx+3; }
        else if(uv.y > 0.67 && uv.y < 0.695) { return dmx+4; }
        else if(uv.y > 0.7 && uv.y < 0.725)  { return dmx+5; }
        else if(uv.y > 0.73 && uv.y < 0.755) { return dmx+6; }
        else if(uv.y > 0.76 && uv.y < 0.785) { return dmx+7; }
        else if(uv.y > 0.79 && uv.y < 0.815) { return dmx+8; }
        else if(uv.y > 0.82 && uv.y < 0.845) { return dmx+9; }
        else if(uv.y > 0.85 && uv.y < 0.875) { return dmx+10; }
        else if(uv.y > 0.88 && uv.y < 0.905) { return dmx+11; }
        else { return 0; }
    }
    else
    { return 0; }
}

float4 GetDMXColor_Fixed(uint DMXChannel)
{
    #ifdef _VRSL_LEGACY_TEXTURES
        float redchannel = getValueAtCoords(DMXChannel, _OSCGridRenderTextureRAW);
        float greenchannel = getValueAtCoords(DMXChannel + 1, _OSCGridRenderTextureRAW);
        float bluechannel = getValueAtCoords(DMXChannel + 2, _OSCGridRenderTextureRAW);
    #else
        float redchannel = getValueAtCoords(DMXChannel, _Udon_DMXGridRenderTexture);
        float greenchannel = getValueAtCoords(DMXChannel + 1, _Udon_DMXGridRenderTexture);
        float bluechannel = getValueAtCoords(DMXChannel + 2, _Udon_DMXGridRenderTexture);
    #endif


    #if defined(PROJECTION_YES)
        redchannel = redchannel * _RedMultiplier;
        bluechannel = bluechannel * _BlueMultiplier;
        greenchannel = greenchannel * _GreenMultiplier;
    #endif


    //return IF(isOSC() == 1,lerp(fixed4(0,0,0,1), float4(redchannel,greenchannel,bluechannel,1), GetOSCIntensity(DMXChannel, _FixtureMaxIntensity)), float4(redchannel,greenchannel,bluechannel,1) * GetOSCIntensity(DMXChannel, _FixtureMaxIntensity));
    return float4(redchannel,greenchannel,bluechannel,1);
}