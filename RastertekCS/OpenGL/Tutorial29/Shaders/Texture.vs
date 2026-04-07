#version 400

in vec3 inputPosition;
in vec2 inputTexCoord;

out vec2 texCoord;

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main(void)
{
    // Calculate the position of the vertex against the world, view, and projection matrices.
    gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4(inputPosition, 1.0f);

    // Store the texture coordinates for the pixel shader.
    texCoord = inputTexCoord;
}
