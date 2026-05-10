#version 400

in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 inputNormal;

out vec2 texCoord;
out vec3 normal;
out vec4 viewPosition;
out vec3 lightPos;

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform vec3 lightPosition;

void main(void)
{
    gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4(inputPosition, 1.0f);
    viewPosition = gl_Position;

    texCoord = inputTexCoord;
    normal = mat3(worldMatrix) * inputNormal;
    normal = normalize(normal);

    vec4 worldPosition = worldMatrix * vec4(inputPosition, 1.0f);
    lightPos = normalize(lightPosition - worldPosition.xyz);
}
