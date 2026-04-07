#version 400

in vec3 inputPosition;
in vec2 inputTexCoord;

out vec2 texCoord;
out vec3 lightDir;

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 cameraViewMatrix;
uniform vec3 lightDirection;

void main(void)
{
    gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4(inputPosition, 1.0f);
    texCoord = inputTexCoord;
    lightDir = -lightDirection;
    lightDir = mat3(cameraViewMatrix) * lightDir;
}
