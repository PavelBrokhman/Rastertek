#version 400

in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 inputNormal;

out vec2 texCoord;
out vec3 normal;
out float gl_ClipDistance[1];

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform vec4 clipPlane;

void main(void)
{
    vec4 worldPos = worldMatrix * vec4(inputPosition, 1.0f);
    gl_Position = projectionMatrix * viewMatrix * worldPos;
    texCoord = inputTexCoord;
    normal = normalize(mat3(worldMatrix) * inputNormal);
    gl_ClipDistance[0] = dot(worldPos, clipPlane);
}
