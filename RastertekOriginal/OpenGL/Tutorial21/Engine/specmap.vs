////////////////////////////////////////////////////////////////////////////////
// Filename: specmap.vs
////////////////////////////////////////////////////////////////////////////////
#version 400


/////////////////////
// INPUT VARIABLES //
/////////////////////
in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 inputNormal;
in vec3 inputTangent;
in vec3 inputBinormal;


//////////////////////
// OUTPUT VARIABLES //
//////////////////////
out vec2 texCoord;
out vec3 normal;
out vec3 tangent;
out vec3 binormal;
out vec3 viewDirection;


///////////////////////
// UNIFORM VARIABLES //
///////////////////////
uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform vec3 cameraPosition;


////////////////////////////////////////////////////////////////////////////////
// Vertex Shader
////////////////////////////////////////////////////////////////////////////////
void main(void)
{
    vec4 worldPosition;


    // Calculate the position of the vertex against the world, view, and projection matrices.
    gl_Position = vec4(inputPosition, 1.0f) * worldMatrix;
    gl_Position = gl_Position * viewMatrix;
    gl_Position = gl_Position * projectionMatrix;

    // Store the texture coordinates for the pixel shader.
    texCoord = inputTexCoord;

    // Calculate the normal vector against the world matrix only and then normalize the final value.
    normal = inputNormal * mat3(worldMatrix);
    normal = normalize(normal);

    // Calculate the tangent vector against the world matrix only and then normalize the final value.
    tangent = inputTangent * mat3(worldMatrix);
    tangent = normalize(tangent);

    // Calculate the binormal vector against the world matrix only and then normalize the final value.
    binormal = inputBinormal * mat3(worldMatrix);
    binormal = normalize(binormal);

    // Calculate the position of the vertex in the world.
    worldPosition = vec4(inputPosition, 1.0f) * worldMatrix;

    // Determine the viewing direction based on the position of the camera and the position of the vertex in the world.
    viewDirection = cameraPosition.xyz - worldPosition.xyz;

    // Normalize the viewing direction vector.
    viewDirection = normalize(viewDirection);
}
