////////////////////////////////////////////////////////////////////////////////
// Filename: shadow.vs
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
out vec4 lightViewPosition;
out vec3 lightPos;
out vec4 lightViewPosition2;
out vec3 lightPos2;


///////////////////////
// UNIFORM VARIABLES //
///////////////////////
uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 lightViewMatrix;
uniform mat4 lightProjectionMatrix;
uniform vec3 lightPosition;
uniform mat4 lightViewMatrix2;
uniform mat4 lightProjectionMatrix2;
uniform vec3 lightPosition2;


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
    lightViewPosition = vec4(inputPosition, 1.0f) * worldMatrix;
    lightViewPosition = lightViewPosition * lightViewMatrix;
    lightViewPosition = lightViewPosition * lightProjectionMatrix;

    // Store the position of the vertice as viewed by the second projection view point in a separate variable.
    lightViewPosition2 = vec4(inputPosition, 1.0f) * worldMatrix;
    lightViewPosition2 = lightViewPosition2 * lightViewMatrix2;
    lightViewPosition2 = lightViewPosition2 * lightProjectionMatrix2;

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

    // Determine the second light position based on the position of the second light and the position of the vertex in the world.
    lightPos2 = lightPosition2.xyz - worldPosition.xyz;

    // Normalize the second light position vector.
    lightPos2 = normalize(lightPos2);
}
