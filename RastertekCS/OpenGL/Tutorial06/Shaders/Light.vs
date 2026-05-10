#version 400

in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 inputNormal;

out vec2 texCoord;
out vec3 normal;

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main(void)
{
    gl_Position = vec4(inputPosition, 1.0f) * worldMatrix;
    gl_Position = gl_Position * viewMatrix;
    gl_Position = gl_Position * projectionMatrix;

    texCoord = inputTexCoord;

    normal = inputNormal * mat3(worldMatrix);
    normal = normalize(normal);
}
