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


void LightClass::SetDiffuseColor(float red, float green, float blue, float alpha)
{
    m_diffuseColor[0] = red;
    m_diffuseColor[1] = green;
    m_diffuseColor[2] = blue;
    m_diffuseColor[3] = alpha;
    return;
}


void LightClass::SetDirection(float x, float y, float z)
{
    m_direction[0] = x;
    m_direction[1] = y;
    m_direction[2] = z;
    return;
}


void LightClass::SetAmbientLight(float red, float green, float blue, float alpha)
{
    m_ambientLight[0] = red;
    m_ambientLight[1] = green;
    m_ambientLight[2] = blue;
    m_ambientLight[3] = alpha;
    return;
}


void LightClass::SetSpecularColor(float red, float green, float blue, float alpha)
{
    m_specularColor[0] = red;
    m_specularColor[1] = green;
    m_specularColor[2] = blue;
    m_specularColor[3] = alpha;
    return;
}


void LightClass::SetSpecularPower(float power)
{
    m_specularPower = power;
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


void LightClass::GetDiffuseColor(float* color)
{
    color[0] = m_diffuseColor[0];
    color[1] = m_diffuseColor[1];
    color[2] = m_diffuseColor[2];
    color[3] = m_diffuseColor[3];
    return;
}


void LightClass::GetDirection(float* direction)
{
    direction[0] = m_direction[0];
    direction[1] = m_direction[1];
    direction[2] = m_direction[2];
    return;
}


void LightClass::GetAmbientLight(float* ambient)
{
    ambient[0] = m_ambientLight[0];
    ambient[1] = m_ambientLight[1];
    ambient[2] = m_ambientLight[2];
    ambient[3] = m_ambientLight[3];
    return;
}


void LightClass::GetSpecularColor(float* specular)
{
    specular[0] = m_specularColor[0];
    specular[1] = m_specularColor[1];
    specular[2] = m_specularColor[2];
    specular[3] = m_specularColor[3];
    return;
}


void LightClass::GetSpecularPower(float& power)
{
    power = m_specularPower;
    return;
}


void LightClass::GetPosition(float* position)
{
    position[0] = m_position[0];
    position[1] = m_position[1];
    position[2] = m_position[2];
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


void LightClass::GenerateProjectionMatrix(float screenDepth, float screenNear)
{
    float fieldOfView, screenAspect;


    // Setup field of view and screen aspect for a square light source.
    fieldOfView = 3.14159265358979323846f / 2.0f;
    screenAspect = 1.0f;

    // Create the projection matrix for the view point.
    BuildMatrixPerspectiveFovLH(fieldOfView, screenAspect, screenNear, screenDepth);

    return;
}


void LightClass::BuildMatrixPerspectiveFovLH(float fieldOfView, float screenAspect, float screenNear, float screenDepth)
{
    m_projectionMatrix[0]  = 1.0f / (screenAspect * tan(fieldOfView * 0.5f));
    m_projectionMatrix[1]  = 0.0f;
    m_projectionMatrix[2]  = 0.0f;
    m_projectionMatrix[3]  = 0.0f;

    m_projectionMatrix[4]  = 0.0f;
    m_projectionMatrix[5]  = 1.0f / tan(fieldOfView * 0.5f);
    m_projectionMatrix[6]  = 0.0f;
    m_projectionMatrix[7]  = 0.0f;

    m_projectionMatrix[8]  = 0.0f;
    m_projectionMatrix[9]  = 0.0f;
    m_projectionMatrix[10] = screenDepth / (screenDepth - screenNear);
    m_projectionMatrix[11] = 1.0f;

    m_projectionMatrix[12] = 0.0f;
    m_projectionMatrix[13] = 0.0f;
    m_projectionMatrix[14] = (-screenNear * screenDepth) / (screenDepth - screenNear);
    m_projectionMatrix[15] = 0.0f;

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


void LightClass::GetProjectionMatrix(float* matrix)
{
    matrix[0]  = m_projectionMatrix[0];
    matrix[1]  = m_projectionMatrix[1];
    matrix[2]  = m_projectionMatrix[2];
    matrix[3]  = m_projectionMatrix[3];

    matrix[4]  = m_projectionMatrix[4];
    matrix[5]  = m_projectionMatrix[5];
    matrix[6]  = m_projectionMatrix[6];
    matrix[7]  = m_projectionMatrix[7];

    matrix[8]  = m_projectionMatrix[8];
    matrix[9]  = m_projectionMatrix[9];
    matrix[10] = m_projectionMatrix[10];
    matrix[11] = m_projectionMatrix[11];

    matrix[12] = m_projectionMatrix[12];
    matrix[13] = m_projectionMatrix[13];
    matrix[14] = m_projectionMatrix[14];
    matrix[15] = m_projectionMatrix[15];

    return;
}
