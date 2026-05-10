#version 400

in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 inputNormal;

out vec2 texCoord;
out vec3 normal;
out vec4 lightViewPosition;
out vec3 lightPos;
out vec4 lightViewPosition2;
out vec3 lightPos2;

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 lightViewMatrix;
uniform mat4 lightProjectionMatrix;
uniform mat4 lightViewMatrix2;
uniform mat4 lightProjectionMatrix2;
uniform vec3 lightPosition;
uniform vec3 lightPosition2;

void main(void)
{
    gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4(inputPosition, 1.0f);

    lightViewPosition  = lightProjectionMatrix  * lightViewMatrix  * worldMatrix * vec4(inputPosition, 1.0f);
    lightViewPosition2 = lightProjectionMatrix2 * lightViewMatrix2 * worldMatrix * vec4(inputPosition, 1.0f);

    texCoord = inputTexCoord;

    normal = mat3(worldMatrix) * inputNormal;
    normal = normalize(normal);

    vec4 worldPosition = worldMatrix * vec4(inputPosition, 1.0f);
    lightPos  = normalize(lightPosition  - worldPosition.xyz);
    lightPos2 = normalize(lightPosition2 - worldPosition.xyz);
}
