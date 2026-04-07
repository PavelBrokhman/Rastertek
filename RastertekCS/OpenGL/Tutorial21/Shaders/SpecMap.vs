#version 400

in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 inputNormal;
in vec3 inputTangent;
in vec3 inputBinormal;

out vec2 texCoord;
out vec3 normal;
out vec3 tangent;
out vec3 binormal;
out vec3 viewDirection;

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform vec3 cameraPosition;

void main(void)
{
    vec4 worldPosition;

    // Calculate the position of the vertex against the world, view, and projection matrices.
    gl_Position = worldMatrix * vec4(inputPosition, 1.0);
    gl_Position = viewMatrix * gl_Position;
    gl_Position = projectionMatrix * gl_Position;

    // Store the texture coordinates for the pixel shader.
    texCoord = inputTexCoord;

    // Calculate the normal vector against the world matrix only and then normalize the final value.
    normal = mat3(worldMatrix) * inputNormal;
    normal = normalize(normal);

    // Calculate the tangent vector against the world matrix only and then normalize the final value.
    tangent = mat3(worldMatrix) * inputTangent;
    tangent = normalize(tangent);

    // Calculate the binormal vector against the world matrix only and then normalize the final value.
    binormal = mat3(worldMatrix) * inputBinormal;
    binormal = normalize(binormal);

    // Calculate the position of the vertex in the world.
    worldPosition = worldMatrix * vec4(inputPosition, 1.0);

    // Determine the viewing direction based on the position of the camera and the position of the vertex in the world.
    viewDirection = cameraPosition.xyz - worldPosition.xyz;

    // Normalize the viewing direction vector.
    viewDirection = normalize(viewDirection);
}
