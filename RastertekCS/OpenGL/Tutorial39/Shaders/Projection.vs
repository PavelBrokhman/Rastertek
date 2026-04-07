#version 400

in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 inputNormal;

out vec2 texCoord;
out vec4 viewPosition;

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 viewMatrix2;
uniform mat4 projectionMatrix2;

void main(void)
{
    gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4(inputPosition, 1.0f);
    viewPosition = projectionMatrix2 * viewMatrix2 * worldMatrix * vec4(inputPosition, 1.0f);
    texCoord = inputTexCoord;
}
