#version 400

in vec3 inputPosition;
in vec2 inputTexCoord;

out vec2 texCoord;
out vec4 reflectionPosition;
out vec4 refractionPosition;

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 reflectionMatrix;

void main(void)
{
    gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4(inputPosition, 1.0f);
    texCoord = inputTexCoord;

    mat4 reflectProjectWorld = projectionMatrix * reflectionMatrix * worldMatrix;
    reflectionPosition = reflectProjectWorld * vec4(inputPosition, 1.0f);

    mat4 viewProjectWorld = projectionMatrix * viewMatrix * worldMatrix;
    refractionPosition = viewProjectWorld * vec4(inputPosition, 1.0f);
}
