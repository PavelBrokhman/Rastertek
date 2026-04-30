////////////////////////////////////////////////////////////////////////////////
// Filename: texture.vs
////////////////////////////////////////////////////////////////////////////////
#version 400


/////////////////////
// INPUT VARIABLES //
/////////////////////
in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 instancePosition;


//////////////////////
// OUTPUT VARIABLES //
//////////////////////
out vec2 texCoord;


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
    vec3 position;


    // Make a copy of the input position.
    position = inputPosition;
    
    // Update the position of the vertices based on the data for this particular instance.
    position.x += instancePosition.x;
    position.y += instancePosition.y;
    position.z += instancePosition.z;

    // Calculate the position of the vertex against the world, view, and projection matrices.
    gl_Position = vec4(position, 1.0f) * worldMatrix;
    gl_Position = gl_Position * viewMatrix;
    gl_Position = gl_Position * projectionMatrix;

    // Store the texture coordinates for the pixel shader.
    texCoord = inputTexCoord;
}
