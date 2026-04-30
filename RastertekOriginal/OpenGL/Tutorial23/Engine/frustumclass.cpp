////////////////////////////////////////////////////////////////////////////////
// Filename: frustumclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "frustumclass.h"


FrustumClass::FrustumClass()
{
}


FrustumClass::FrustumClass(const FrustumClass& other)
{
}


FrustumClass::~FrustumClass()
{
}


void FrustumClass::ConstructFrustum(OpenGLClass* OpenGL, float screenDepth, float* viewMatrix, float* projectionMatrix)
{
    float zMinimum, r, t;
    float matrix[16];


    // Calculate the minimum Z distance in the frustum.
    zMinimum = -projectionMatrix[14] / projectionMatrix[10];
    r = screenDepth / (screenDepth - zMinimum);
    projectionMatrix[10] = r;
    projectionMatrix[14] = -r * zMinimum;

    // Create the frustum matrix from the view matrix and updated projection matrix.
    OpenGL->MatrixMultiply(matrix, viewMatrix, projectionMatrix);

    // Get the near plane of the frustum.
    m_planes[0][0] = matrix[2];
    m_planes[0][1] = matrix[6];
    m_planes[0][2] = matrix[10];
    m_planes[0][3] = matrix[14];

    // Normalize it.
    t = (float)sqrt((m_planes[0][0] * m_planes[0][0]) + (m_planes[0][1] * m_planes[0][1]) + (m_planes[0][2] * m_planes[0][2]));
    m_planes[0][0] /= t;
    m_planes[0][1] /= t;
    m_planes[0][2] /= t;
    m_planes[0][3] /= t;

    // Calculate the far plane of the frustum.
    m_planes[1][0] = matrix[3] - matrix[2];
    m_planes[1][1] = matrix[7] - matrix[6];
    m_planes[1][2] = matrix[11] - matrix[10];
    m_planes[1][3] = matrix[15] - matrix[14];

    // Normalize it.
    t = (float)sqrt((m_planes[1][0] * m_planes[1][0]) + (m_planes[1][1] * m_planes[1][1]) + (m_planes[1][2] * m_planes[1][2]));
    m_planes[1][0] /= t;
    m_planes[1][1] /= t;
    m_planes[1][2] /= t;
    m_planes[1][3] /= t;

    // Calculate the left plane of the frustum.
    m_planes[2][0] = matrix[3] + matrix[0];
    m_planes[2][1] = matrix[7] + matrix[4];
    m_planes[2][2] = matrix[11] + matrix[8];
    m_planes[2][3] = matrix[15] + matrix[12];

    // Normalize it.
    t = (float)sqrt((m_planes[2][0] * m_planes[2][0]) + (m_planes[2][1] * m_planes[2][1]) + (m_planes[2][2] * m_planes[2][2]));
    m_planes[2][0] /= t;
    m_planes[2][1] /= t;
    m_planes[2][2] /= t;
    m_planes[2][3] /= t;

    // Calculate the right plane of the frustum.
    m_planes[3][0] = matrix[3] - matrix[0];
    m_planes[3][1] = matrix[7] - matrix[4];
    m_planes[3][2] = matrix[11] - matrix[8];
    m_planes[3][3] = matrix[15] - matrix[12];

    // Normalize it.
    t = (float)sqrt((m_planes[3][0] * m_planes[3][0]) + (m_planes[3][1] * m_planes[3][1]) + (m_planes[3][2] * m_planes[3][2]));
    m_planes[3][0] /= t;
    m_planes[3][1] /= t;
    m_planes[3][2] /= t;
    m_planes[3][3] /= t;

    // Calculate the top plane of the frustum.
    m_planes[4][0] = matrix[3] - matrix[1];
    m_planes[4][1] = matrix[7] - matrix[5];
    m_planes[4][2] = matrix[11] - matrix[9];
    m_planes[4][3] = matrix[15] - matrix[13];

    // Normalize it.
    t = (float)sqrt((m_planes[4][0] * m_planes[4][0]) + (m_planes[4][1] * m_planes[4][1]) + (m_planes[4][2] * m_planes[4][2]));
    m_planes[4][0] /= t;
    m_planes[4][1] /= t;
    m_planes[4][2] /= t;
    m_planes[4][3] /= t;

    // Calculate the bottom plane of the frustum.
    m_planes[5][0] = matrix[3] + matrix[1];
    m_planes[5][1] = matrix[7] + matrix[5];
    m_planes[5][2] = matrix[11] + matrix[9];
    m_planes[5][3] = matrix[15] + matrix[13];

    // Normalize it.
    t = (float)sqrt((m_planes[5][0] * m_planes[5][0]) + (m_planes[5][1] * m_planes[5][1]) + (m_planes[5][2] * m_planes[5][2]));
    m_planes[5][0] /= t;
    m_planes[5][1] /= t;
    m_planes[5][2] /= t;
    m_planes[5][3] /= t;

    return;
}


bool FrustumClass::CheckPoint(float x, float y, float z)
{
    int i;


    // Check if the point is inside all six planes of the view frustum.
    for(i=0; i<6; i++)
    {
        if(((m_planes[i][0] * x) + (m_planes[i][1] * y) + (m_planes[i][2] * z) + m_planes[i][3]) < 0.0f)
	{
	    return false;
	}
    }

    return true;
}


bool FrustumClass::CheckCube(float xCenter, float yCenter, float zCenter, float radius)
{
    int i;


    // Check if any one point of the cube is in the view frustum.
    for(i=0; i<6; i++)
    {
        if(m_planes[i][0] * (xCenter - radius) +
           m_planes[i][1] * (yCenter - radius) +
           m_planes[i][2] * (zCenter - radius) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

        if(m_planes[i][0] * (xCenter + radius) +
           m_planes[i][1] * (yCenter - radius) +
           m_planes[i][2] * (zCenter - radius) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

        if(m_planes[i][0] * (xCenter - radius) +
           m_planes[i][1] * (yCenter + radius) +
           m_planes[i][2] * (zCenter - radius) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

        if(m_planes[i][0] * (xCenter + radius) +
           m_planes[i][1] * (yCenter + radius) +
           m_planes[i][2] * (zCenter - radius) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

        if(m_planes[i][0] * (xCenter - radius) +
           m_planes[i][1] * (yCenter - radius) +
           m_planes[i][2] * (zCenter + radius) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

        if(m_planes[i][0] * (xCenter + radius) +
           m_planes[i][1] * (yCenter - radius) +
           m_planes[i][2] * (zCenter + radius) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

        if(m_planes[i][0] * (xCenter - radius) +
           m_planes[i][1] * (yCenter + radius) +
           m_planes[i][2] * (zCenter + radius) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

        if(m_planes[i][0] * (xCenter + radius) +
           m_planes[i][1] * (yCenter + radius) +
           m_planes[i][2] * (zCenter + radius) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

	return false;
    }

    return true;
}


bool FrustumClass::CheckSphere(float xCenter, float yCenter, float zCenter, float radius)
{
    int i;


    // Check if the radius of the sphere is inside the view frustum.
    for(i=0; i<6; i++)
    {
        if(((m_planes[i][0] * xCenter) + (m_planes[i][1] * yCenter) + (m_planes[i][2] * zCenter) + m_planes[i][3]) < -radius)
	{
	    return false;
	}
    }

    return true;
}


bool FrustumClass::CheckRectangle(float xCenter, float yCenter, float zCenter, float xSize, float ySize, float zSize)
{
    int i;


    // Check if any of the 6 planes of the rectangle are inside the view frustum.
    for(i=0; i<6; i++)
    {
        if(m_planes[i][0] * (xCenter - xSize) +
           m_planes[i][1] * (yCenter - ySize) +
           m_planes[i][2] * (zCenter - zSize) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

        if(m_planes[i][0] * (xCenter + xSize) +
           m_planes[i][1] * (yCenter - ySize) +
           m_planes[i][2] * (zCenter - zSize) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

        if(m_planes[i][0] * (xCenter - xSize) +
           m_planes[i][1] * (yCenter + ySize) +
           m_planes[i][2] * (zCenter - zSize) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

        if(m_planes[i][0] * (xCenter - xSize) +
           m_planes[i][1] * (yCenter - ySize) +
           m_planes[i][2] * (zCenter + zSize) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

        if(m_planes[i][0] * (xCenter + xSize) +
           m_planes[i][1] * (yCenter + ySize) +
           m_planes[i][2] * (zCenter - zSize) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

        if(m_planes[i][0] * (xCenter + xSize) +
           m_planes[i][1] * (yCenter - ySize) +
           m_planes[i][2] * (zCenter + zSize) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

        if(m_planes[i][0] * (xCenter - xSize) +
           m_planes[i][1] * (yCenter + ySize) +
           m_planes[i][2] * (zCenter + zSize) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

        if(m_planes[i][0] * (xCenter + xSize) +
           m_planes[i][1] * (yCenter + ySize) +
           m_planes[i][2] * (zCenter + zSize) + m_planes[i][3] >= 0.0f)
	{
	    continue;
	}

	return false;
    }

    return true;
}
