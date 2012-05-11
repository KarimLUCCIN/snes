uniform float2 halfPixel;
uniform float4 OverlayColor;

struct VS_OUTPUT
{
	float4 Pos      : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
};

VS_OUTPUT vs_main(in float4 pos      : POSITION,
					 in float2 texCoord : TEXCOORD)
{
	VS_OUTPUT Out;
   
	Out.Pos      = pos;
	Out.TexCoord = texCoord + halfPixel;
   
	return Out;
}

struct PS_OUTPUT
{
	float4 color : SV_TARGET;
};

PS_OUTPUT ps_main(VS_OUTPUT input)
{
	PS_OUTPUT result = (PS_OUTPUT)0;
	result.color = float4(cos(input.TexCoord.x + OverlayColor.x), input.TexCoord.y, cos(input.TexCoord.x + OverlayColor.x + 1.54), 1);

	return result;
}

technique10 Render
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, vs_main() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, ps_main() ) );
    }
}