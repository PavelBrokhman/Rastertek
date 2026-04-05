#version 400

#define NUM_LIGHTS 4

in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 inputNormal;

out vec2 texCoord;
out vec3 normal;
out vec3 lightPos[NUM_LIGHTS];

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform vec3 lightPosition[NUM_LIGHTS];

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

    for (int i = 0; i < NUM_LIGHTS; i++)
    {
        lightPos[i] = lightPosition[i] - worldPosition.xyz;
        lightPos[i] = normalize(lightPos[i]);
    }
}
