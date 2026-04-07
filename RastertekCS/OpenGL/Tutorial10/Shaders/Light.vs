#version 400

in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 inputNormal;

out vec2 texCoord;
out vec3 normal;
out vec3 viewDirection;

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform vec3 cameraPosition;

void main(void)
{
    vec4 worldPosition;

    gl_Position = worldMatrix * vec4(inputPosition, 1.0f);
    worldPosition = gl_Position;
    gl_Position = viewMatrix * gl_Position;
    gl_Position = projectionMatrix * gl_Position;

    texCoord = inputTexCoord;

    normal = mat3(worldMatrix) * inputNormal;
    normal = normalize(normal);

    viewDirection = cameraPosition - worldPosition.xyz;
    viewDirection = normalize(viewDirection);
}
