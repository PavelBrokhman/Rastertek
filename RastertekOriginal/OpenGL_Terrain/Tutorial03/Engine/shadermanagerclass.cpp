////////////////////////////////////////////////////////////////////////////////
// Filename: shadermanagerclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "shadermanagerclass.h"


ShaderManagerClass::ShaderManagerClass()
{
    m_FontShader = 0;
    m_TerrainShader = 0;
}


ShaderManagerClass::ShaderManagerClass(const ShaderManagerClass& other)
{
}


ShaderManagerClass::~ShaderManagerClass()
{
}


bool ShaderManagerClass::Initialize(OpenGLClass* OpenGL)
{
    bool result;


    // Create and initialize the font shader object.
    m_FontShader = new FontShaderClass;

    result = m_FontShader->Initialize(OpenGL);
    if(!result)
    {
        return false;
    }

    // Create and initialize the terrain shader object.
    m_TerrainShader = new TerrainShaderClass;

    result = m_TerrainShader->Initialize(OpenGL);
    if(!result)
    {
        return false;
    }

    return true;
}


void ShaderManagerClass::Shutdown()
{
    // Release the terrain shader object.
    if(m_TerrainShader)
    {
        m_TerrainShader->Shutdown();
        delete m_TerrainShader;
        m_TerrainShader = 0;
    }

    // Release the font shader object.
    if(m_FontShader)
    {
        m_FontShader->Shutdown();
        delete m_FontShader;
        m_FontShader = 0;
    }

    return;
}


bool ShaderManagerClass::RenderFontShader(float* worldMatrix, float* viewMatrix, float* projectionMatrix, float* pixelColor)
{
    return m_FontShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, pixelColor);
}


bool ShaderManagerClass::RenderTerrainShader(float* worldMatrix, float* viewMatrix, float* projectionMatrix)
{
    return m_TerrainShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix);
}
