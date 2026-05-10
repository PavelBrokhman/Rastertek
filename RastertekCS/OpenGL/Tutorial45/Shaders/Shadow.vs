#version 400

in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 inputNormal;

out vec2 texCoord;
out vec3 normal;
out vec4 lightViewPosition;

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 lightViewMatrix;
uniform mat4 lightProjectionMatrix;

void main(void)
{
    gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4(inputPosition, 1.0f);
    lightViewPosition = lightProjectionMatrix * lightViewMatrix * worldMatrix * vec4(inputPosition, 1.0f);

    texCoord = inputTexCoord;
    normal = mat3(worldMatrix) * inputNormal;
    normal = normalize(normal);
}
