////////////////////////////////////////////////////////////////////////////////
// Filename: projection.vs
////////////////////////////////////////////////////////////////////////////////
#version 400


/////////////////////
// INPUT VARIABLES //
/////////////////////
in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 inputNormal;


//////////////////////
// OUTPUT VARIABLES //
//////////////////////
out vec2 texCoord;
out vec3 normal;
out vec4 viewPosition;
out vec3 lightPos;


///////////////////////
// UNIFORM VARIABLES //
///////////////////////
uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 viewMatrix2;
uniform mat4 projectionMatrix2;
uniform vec3 lightPosition;


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

    // Store the position of the vertice as viewed by the projection view point in a separate variable.
    viewPosition = vec4(inputPosition, 1.0f) * worldMatrix;
    viewPosition = viewPosition * viewMatrix2;
    viewPosition = viewPosition * projectionMatrix2;

    // Store the texture coordinates for the pixel shader.
    texCoord = inputTexCoord;

    // Calculate the normal vector against the world matrix only.
    normal = inputNormal * mat3(worldMatrix);

    // Normalize the normal vector.
    normal = normalize(normal);

    // Calculate the position of the vertex in the world.
    worldPosition = vec4(inputPosition, 1.0f) * worldMatrix;

    // Determine the light position based on the position of the light and the position of the vertex in the world.
    lightPos = lightPosition.xyz - worldPosition.xyz;

    // Normalize the light position vector.
    lightPos = normalize(lightPos);
}
