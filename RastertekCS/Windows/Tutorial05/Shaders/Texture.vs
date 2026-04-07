cbuffer MatrixBuffer : register(b0)
{
    matrix worldMatrix;
    matrix viewMatrix;
    matrix projectionMatrix;
};

struct VertexInputType
{
    float3 position : POSITION;
    float2 tex : TEXCOORD0;
};

struct PixelInputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
};

PixelInputType TextureVertexShader(VertexInputType input)
{
    PixelInputType output;

    float4 pos = float4(input.position, 1.0f);
    pos = mul(pos, worldMatrix);
    pos = mul(pos, viewMatrix);
    pos = mul(pos, projectionMatrix);
    output.position = pos;

    output.tex = input.tex;

    return output;
}
