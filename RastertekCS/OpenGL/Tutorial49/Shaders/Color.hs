////////////////////////////////////////////////////////////////////////////////
// Filename: Color.hs
////////////////////////////////////////////////////////////////////////////////
#version 400


/////////////
// GLOBALS //
/////////////
layout(vertices = 3) out;


///////////////////////
// UNIFORM VARIABLES //
///////////////////////
uniform float tessellationAmount;


/////////////////////
// INPUT VARIABLES //
/////////////////////
in vec3 hullPosition[];
in vec3 hullColor[];


//////////////////////
// OUTPUT VARIABLES //
//////////////////////
out vec3 domainPosition[];
out vec3 domainColor[];


////////////////////////////////////////////////////////////////////////////////
// Hull (Tessellation Control) Shader
////////////////////////////////////////////////////////////////////////////////
void main(void)
{
	// Pass the control point's position and color through to the domain shader.
	domainPosition[gl_InvocationID] = hullPosition[gl_InvocationID];
	domainColor[gl_InvocationID] = hullColor[gl_InvocationID];

	// Only the first invocation per patch needs to write the tessellation factors.
	if(gl_InvocationID == 0)
	{
		// Set the tessellation factors for the three edges of the triangle.
		gl_TessLevelOuter[0] = tessellationAmount;
		gl_TessLevelOuter[1] = tessellationAmount;
		gl_TessLevelOuter[2] = tessellationAmount;

		// Set the tessellation factor for tessellating inside the triangle.
		gl_TessLevelInner[0] = tessellationAmount;
	}
}
