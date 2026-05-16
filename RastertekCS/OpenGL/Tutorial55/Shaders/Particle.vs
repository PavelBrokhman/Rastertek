////////////////////////////////////////////////////////////////////////////////
// Filename: particle.vs
////////////////////////////////////////////////////////////////////////////////
#version 400


/////////////////////
// INPUT VARIABLES //
/////////////////////
in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 inputData1;


//////////////////////
// OUTPUT VARIABLES //
//////////////////////
out vec2 texCoord;
out vec4 pixColor;
out vec3 data1;
out vec2 texCoords1;


///////////////////////
// UNIFORM VARIABLES //
///////////////////////
uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;


////////////////////////////////////////////////////////////////////////////////
// Vertex Shader
////////////////////////////////////////////////////////////////////////////////
void main(void)
{
    float scroll1X, scroll1Y;


    // Calculate the position of the vertex against the world, view, and projection matrices.
    gl_Position = vec4(inputPosition, 1.0f) * worldMatrix;
    gl_Position = gl_Position * viewMatrix;
    gl_Position = gl_Position * projectionMatrix;

    // Store the texture coordinates for the pixel shader.
    texCoord = inputTexCoord;

    // Store the particle data for the pixel shader.
    data1 = inputData1;

    // Get the scrolling values from the data1.yz portion of the vertex input data.
    scroll1X = data1.y;
    scroll1Y = data1.z;

    // Calculate the first texture scroll speed texture sampling coordinates.
    texCoords1.x = inputTexCoord.x - scroll1X;
    texCoords1.y = inputTexCoord.y + scroll1Y;
}
