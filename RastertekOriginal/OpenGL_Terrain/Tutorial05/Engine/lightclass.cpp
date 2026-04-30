////////////////////////////////////////////////////////////////////////////////
// Filename: lightclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "lightclass.h"


LightClass::LightClass()
{
}


LightClass::LightClass(const LightClass& other)
{
}


LightClass::~LightClass()
{
}


void LightClass::SetDirection(float x, float y, float z)
{
    m_direction[0] = x;
    m_direction[1] = y;
    m_direction[2] = z;
    return;
}



void LightClass::SetDiffuseColor(float red, float green, float blue, float alpha)
{
    m_diffuseColor[0] = red;
    m_diffuseColor[1] = green;
    m_diffuseColor[2] = blue;
    m_diffuseColor[3] = alpha;
    return;
}


void LightClass::SetDiffuseIntensity(float intensity)
{
	m_diffuseIntensity = intensity;
	return;
}


void LightClass::SetPosition(float x, float y, float z)
{
    m_position[0] = x;
    m_position[1] = y;
    m_position[2] = z;
    return;
}


void LightClass::SetLookAt(float x, float y, float z)
{
    m_lookAt[0] = x;
    m_lookAt[1] = y;
    m_lookAt[2] = z;
    return;
}


void LightClass::GetDirection(float* direction)
{
    direction[0] = m_direction[0];
    direction[1] = m_direction[1];
    direction[2] = m_direction[2];
    return;
}


void LightClass::GetDiffuseColor(float* color)
{
    color[0] = m_diffuseColor[0];
    color[1] = m_diffuseColor[1];
    color[2] = m_diffuseColor[2];
    color[3] = m_diffuseColor[3];
    return;
}


void LightClass::GetDiffuseIntensity(float& intensity)
{
	intensity = m_diffuseIntensity;
	return;
}


void LightClass::GenerateViewMatrix()
{
    VectorType up, position, lookAt;


    // Setup the vectors.
    up.x = 0.0f;
    up.y = 1.0f;
    up.z = 0.0f;

    position.x = m_position[0];
    position.y = m_position[1];
    position.z = m_position[2];

    lookAt.x = m_lookAt[0];
    lookAt.y = m_lookAt[1];
    lookAt.z = m_lookAt[2];

    // Create the view matrix from the three vectors.
    BuildMatrixLookAtLH(position, lookAt, up);

    return;
}


void LightClass::BuildMatrixLookAtLH(VectorType position, VectorType lookAt, VectorType up)
{
    VectorType zAxis, xAxis, yAxis;
    float length, result1, result2, result3;


    // zAxis = normal(lookAt - position)
    zAxis.x = lookAt.x - position.x;
    zAxis.y = lookAt.y - position.y;
    zAxis.z = lookAt.z - position.z;
    length = sqrt((zAxis.x * zAxis.x) + (zAxis.y * zAxis.y) + (zAxis.z * zAxis.z));
    zAxis.x = zAxis.x / length;
    zAxis.y = zAxis.y / length;
    zAxis.z = zAxis.z / length;

    // xAxis = normal(cross(up, zAxis))
    xAxis.x = (up.y * zAxis.z) - (up.z * zAxis.y);
    xAxis.y = (up.z * zAxis.x) - (up.x * zAxis.z);
    xAxis.z = (up.x * zAxis.y) - (up.y * zAxis.x);
    length = sqrt((xAxis.x * xAxis.x) + (xAxis.y * xAxis.y) + (xAxis.z * xAxis.z));
    xAxis.x = xAxis.x / length;
    xAxis.y = xAxis.y / length;
    xAxis.z = xAxis.z / length;

    // yAxis = cross(zAxis, xAxis)
    yAxis.x = (zAxis.y * xAxis.z) - (zAxis.z * xAxis.y);
    yAxis.y = (zAxis.z * xAxis.x) - (zAxis.x * xAxis.z);
    yAxis.z = (zAxis.x * xAxis.y) - (zAxis.y * xAxis.x);

    // -dot(xAxis, position)
    result1 = ((xAxis.x * position.x) + (xAxis.y * position.y) + (xAxis.z * position.z)) * -1.0f;

    // -dot(yaxis, position)
    result2 = ((yAxis.x * position.x) + (yAxis.y * position.y) + (yAxis.z * position.z)) * -1.0f;

    // -dot(zaxis, position)
    result3 = ((zAxis.x * position.x) + (zAxis.y * position.y) + (zAxis.z * position.z)) * -1.0f;

    // Set the computed values in the view matrix.
    m_viewMatrix[0]  = xAxis.x;
    m_viewMatrix[1]  = yAxis.x;
    m_viewMatrix[2]  = zAxis.x;
    m_viewMatrix[3]  = 0.0f;

    m_viewMatrix[4]  = xAxis.y;
    m_viewMatrix[5]  = yAxis.y;
    m_viewMatrix[6]  = zAxis.y;
    m_viewMatrix[7]  = 0.0f;

    m_viewMatrix[8]  = xAxis.z;
    m_viewMatrix[9]  = yAxis.z;
    m_viewMatrix[10] = zAxis.z;
    m_viewMatrix[11] = 0.0f;

    m_viewMatrix[12] = result1;
    m_viewMatrix[13] = result2;
    m_viewMatrix[14] = result3;
    m_viewMatrix[15] = 1.0f;

    return;
}


void LightClass::GetViewMatrix(float* matrix)
{
    matrix[0]  = m_viewMatrix[0];
    matrix[1]  = m_viewMatrix[1];
    matrix[2]  = m_viewMatrix[2];
    matrix[3]  = m_viewMatrix[3];

    matrix[4]  = m_viewMatrix[4];
    matrix[5]  = m_viewMatrix[5];
    matrix[6]  = m_viewMatrix[6];
    matrix[7]  = m_viewMatrix[7];

    matrix[8]  = m_viewMatrix[8];
    matrix[9]  = m_viewMatrix[9];
    matrix[10] = m_viewMatrix[10];
    matrix[11] = m_viewMatrix[11];

    matrix[12] = m_viewMatrix[12];
    matrix[13] = m_viewMatrix[13];
    matrix[14] = m_viewMatrix[14];
    matrix[15] = m_viewMatrix[15];

    return;
}


void LightClass::GenerateOrthoMatrix(float width, float nearPlane, float farPlane)
{
    m_orthoMatrix[0]  = 2.0f / width;
    m_orthoMatrix[1]  = 0.0f;
    m_orthoMatrix[2]  = 0.0f;
    m_orthoMatrix[3]  = 0.0f;

    m_orthoMatrix[4]  = 0.0f;
    m_orthoMatrix[5]  = 2.0f / width;
    m_orthoMatrix[6]  = 0.0f;
    m_orthoMatrix[7]  = 0.0f;

    m_orthoMatrix[8]  = 0.0f;
    m_orthoMatrix[9]  = 0.0f;
    m_orthoMatrix[10] = 1.0f / (farPlane - nearPlane);
    m_orthoMatrix[11] = 0.0f;

    m_orthoMatrix[12] = 0.0f;
    m_orthoMatrix[13] = 0.0f;
    m_orthoMatrix[14] = nearPlane / (nearPlane - farPlane);
    m_orthoMatrix[15] = 1.0f;

    return;
}


void LightClass::GetOrthoMatrix(float* matrix)
{
    matrix[0]  = m_orthoMatrix[0];
    matrix[1]  = m_orthoMatrix[1];
    matrix[2]  = m_orthoMatrix[2];
    matrix[3]  = m_orthoMatrix[3];

    matrix[4]  = m_orthoMatrix[4];
    matrix[5]  = m_orthoMatrix[5];
    matrix[6]  = m_orthoMatrix[6];
    matrix[7]  = m_orthoMatrix[7];

    matrix[8]  = m_orthoMatrix[8];
    matrix[9]  = m_orthoMatrix[9];
    matrix[10] = m_orthoMatrix[10];
    matrix[11] = m_orthoMatrix[11];

    matrix[12] = m_orthoMatrix[12];
    matrix[13] = m_orthoMatrix[13];
    matrix[14] = m_orthoMatrix[14];
    matrix[15] = m_orthoMatrix[15];

    return;
}
