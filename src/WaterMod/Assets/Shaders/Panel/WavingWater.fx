float4 frag(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
    return float4(1.0, 1.0, 1.0, 1.0);
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 frag(); 
    }
}