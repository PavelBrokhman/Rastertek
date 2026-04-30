////////////////////////////////////////////////////////////////////////////////
// Filename: refraction.vs
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

    // Calculate the normal vector against the world matrix only.
    normal = inputNormal * mat3(worldMatrix);

    // Normalize the normal vector.
    normal = normalize(normal);

    // Set the clipping plane.
    gl_ClipDistance[0] = dot(vec4(inputPosition, 1.0f) * worldMatrix, clipPlane);
}
