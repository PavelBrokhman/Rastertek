////////////////////////////////////////////////////////////////////////////////
// Filename: light.vs
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

	// Calculate the normal vector against the world matrix only.
	normal = inputNormal * mat3(worldMatrix);

	// Normalize the normal vector.
	normal = normalize(normal);

	// Calculate the position of the vertex in the world.
    worldPosition = vec4(inputPosition, 1.0f) * worldMatrix;

    // Determine the viewing direction based on the position of the camera and the position of the vertex in the world.
    viewDirection = cameraPosition.xyz - worldPosition.xyz;

    // Normalize the viewing direction vector.
    viewDirection = normalize(viewDirection);
}
