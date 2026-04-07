#version 400

in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 inputNormal;

out vec2 texCoord;
out vec3 normal;
out vec4 viewPosition;

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main(void)
{
    gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4(inputPosition, 1.0f);
    texCoord = inputTexCoord;
    viewPosition = viewMatrix * worldMatrix * vec4(inputPosition, 1.0f);
    normal = mat3(viewMatrix) * mat3(worldMatrix) * inputNormal;
}
