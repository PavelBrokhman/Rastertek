cbuffer MatrixBuffer
{
    matrix worldMatrix;
    matrix viewMatrix;
    matrix projectionMatrix;
    matrix viewMatrix2;
    matrix projectionMatrix2;
};

cbuffer LightPositionBuffer
{
    float3 lightPosition;
    float padding;
};

struct VertexInputType
{
    float4 position : POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
};

struct PixelInputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float4 viewPosition : TEXCOORD1;
    float3 lightPos : TEXCOORD2;
};

PixelInputType ProjectionVertexShader(VertexInputType input)
{
    PixelInputType output;
    input.position.w = 1.0f;

    output.position = mul(input.position, worldMatrix);
    output.position = mul(output.position, viewMatrix);
    output.position = mul(output.position, projectionMatrix);

    output.viewPosition = mul(input.position, worldMatrix);
    output.viewPosition = mul(output.viewPosition, viewMatrix2);
    output.viewPosition = mul(output.viewPosition, projectionMatrix2);

    output.tex = input.tex;

    output.normal = mul(input.normal, (float3x3)worldMatrix);
    output.normal = normalize(output.normal);

    float4 worldPosition = mul(input.position, worldMatrix);
    output.lightPos = lightPosition.xyz - worldPosition.xyz;
    output.lightPos = normalize(output.lightPos);

    return output;
}
