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
