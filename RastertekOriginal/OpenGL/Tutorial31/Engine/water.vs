////////////////////////////////////////////////////////////////////////////////
// Filename: water.vs
////////////////////////////////////////////////////////////////////////////////
#version 400


/////////////////////
// INPUT VARIABLES //
/////////////////////
in vec3 inputPosition;
in vec2 inputTexCoord;


//////////////////////
// OUTPUT VARIABLES //
//////////////////////
out vec2 texCoord;
out vec4 reflectionPosition;
out vec4 refractionPosition;


///////////////////////
// UNIFORM VARIABLES //
///////////////////////
uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 reflectionMatrix;


////////////////////////////////////////////////////////////////////////////////
// Vertex Shader
////////////////////////////////////////////////////////////////////////////////
void main(void)
{
    mat4 reflectProjectWorld;
    mat4 viewProjectWorld;


    // Calculate the position of the vertex against the world, view, and projection matrices.
    gl_Position = vec4(inputPosition, 1.0f) * worldMatrix;
    gl_Position = gl_Position * viewMatrix;
    gl_Position = gl_Position * projectionMatrix;

    // Store the texture coordinates for the pixel shader.
    texCoord = inputTexCoord;

    // Create the reflection projection world matrix.
    reflectProjectWorld = reflectionMatrix * projectionMatrix;
    reflectProjectWorld = worldMatrix * reflectProjectWorld;

    // Calculate the input position against the reflectProjectWorld matrix.
    reflectionPosition = vec4(inputPosition, 1.0f) * reflectProjectWorld;

    // Create the view projection world matrix for refraction.
    viewProjectWorld = viewMatrix * projectionMatrix;
    viewProjectWorld = worldMatrix * viewProjectWorld;

    // Calculate the input position against the viewProjectWorld matrix.
    refractionPosition = vec4(inputPosition, 1.0f) * viewProjectWorld;
}
