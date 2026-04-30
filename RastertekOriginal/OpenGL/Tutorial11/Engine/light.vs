////////////////////////////////////////////////////////////////////////////////
// Filename: light.vs
////////////////////////////////////////////////////////////////////////////////
#version 400


/////////////
// DEFINES //
/////////////
#define NUM_LIGHTS 4


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
out vec3 lightPos[NUM_LIGHTS];


///////////////////////
// UNIFORM VARIABLES //
///////////////////////
uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform vec3 lightPosition[NUM_LIGHTS];


////////////////////////////////////////////////////////////////////////////////
// Vertex Shader
////////////////////////////////////////////////////////////////////////////////
void main(void)
{
    vec4 worldPosition;
    int i;


    // Calculate the position of the vertex against the world, view, and projection matrices.
    gl_Position = vec4(inputPosition, 1.0f) * worldMatrix;
    gl_Position = gl_Position * viewMatrix;
    gl_Position = gl_Position * projectionMatrix;

    // Store the texture coordinates for the pixel shader.
    texCoord = inputTexCoord;

    // Calculate the normal vector against the world matrix only.
    normal = inputNormal * mat3(worldMatrix);

    // Normalize the normal vector.
    normal = normalize(normal);

    // Calculate the position of the vertex in the world.
    worldPosition = vec4(inputPosition, 1.0f) * worldMatrix;

    for(i=0; i<NUM_LIGHTS; i++)
    {
        // Determine the light positions based on the position of the lights and the position of the vertex in the world.
        lightPos[i] = lightPosition[i].xyz - worldPosition.xyz;

        // Normalize the light position vectors.
        lightPos[i] = normalize(lightPos[i]);
    }
}
