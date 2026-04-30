////////////////////////////////////////////////////////////////////////////////
// Filename: clipplane.vs
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
out float gl_ClipDistance[1];


///////////////////////
// UNIFORM VARIABLES //
///////////////////////
uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform vec4 clipPlane;


////////////////////////////////////////////////////////////////////////////////
// Vertex Shader
////////////////////////////////////////////////////////////////////////////////
void main(void)
{
    // Calculate the position of the vertex against the world, view, and projection matrices.
    gl_Position = vec4(inputPosition, 1.0f) * worldMatrix;
    gl_Position = gl_Position * viewMatrix;
    gl_Position = gl_Position * projectionMatrix;

    // Store the texture coordinates for the pixel shader.
    texCoord = inputTexCoord;

    // Set the clipping plane.
    gl_ClipDistance[0] = dot(vec4(inputPosition, 1.0f) * worldMatrix, clipPlane);
}
