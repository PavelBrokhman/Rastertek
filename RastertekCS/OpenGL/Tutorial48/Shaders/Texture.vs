#version 400

in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 instancePosition;

out vec2 texCoord;

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main(void)
{
    vec3 pos = inputPosition + instancePosition;
    gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4(pos, 1.0);
    texCoord = inputTexCoord;
}
