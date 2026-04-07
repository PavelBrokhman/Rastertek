#version 400

in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 inputNormal;
in vec3 inputTangent;
in vec3 inputBinormal;

out vec2 texCoord;
out vec3 normal;
out vec3 tangent;
out vec3 binormal;

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main(void)
{
    gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4(inputPosition, 1.0);

    texCoord = inputTexCoord;

    normal = normalize(mat3(worldMatrix) * inputNormal);
    tangent = normalize(mat3(worldMatrix) * inputTangent);
    binormal = normalize(mat3(worldMatrix) * inputBinormal);
}
