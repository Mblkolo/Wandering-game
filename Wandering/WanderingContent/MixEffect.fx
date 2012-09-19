sampler ColorMapSampler : register(s0);

texture AlphaMap;
sampler AlphaMapSampler = sampler_state
{
   Texture = <AlphaMap>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;   
   AddressU  = Mirror;
   AddressV  = Mirror;
};


float4 PixelShaderFunction(float2 Tex: TEXCOORD0) : COLOR0
{
	float4 Color = tex2D(ColorMapSampler, Tex);
	Color.a = (tex2D(AlphaMapSampler, Tex) == float4(1,1,1,1)) ? 1 : 0;
    return Color;
}

technique PostProcess
{
    pass P0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
