////////////////////////////////////////////////////////////////////////////////
// Filename: shadermanagerclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "shadermanagerclass.h"


ShaderManagerClass::ShaderManagerClass()
{
    m_TextureShader = 0;
    m_LightShader = 0;
    m_NormalMapShader = 0;
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


    // Create and initialize the texture shader object.
    m_TextureShader = new TextureShaderClass;

    result = m_TextureShader->Initialize(OpenGL);
    if(!result)
    {
        return false;
    }

    // Create and initialize the light shader object.
    m_LightShader = new LightShaderClass;

    result = m_LightShader->Initialize(OpenGL);
    if(!result)
    {
        return false;
    }

    // Create and initialize the normal map shader object.
    m_NormalMapShader = new NormalMapShaderClass;

    result = m_NormalMapShader->Initialize(OpenGL);
    if(!result)
    {
        return false;
    }

    return true;
}


void ShaderManagerClass::Shutdown()
{
    // Release the normal map shader object.
    if(m_NormalMapShader)
    {
        m_NormalMapShader->Shutdown();
	delete m_NormalMapShader;
	m_NormalMapShader = 0;
    }

    // Release the light shader object.
    if(m_LightShader)
    {
        m_LightShader->Shutdown();
	delete m_LightShader;
	m_LightShader = 0;
    }

    // Release the texture shader object.
    if(m_TextureShader)
    {
        m_TextureShader->Shutdown();
	delete m_TextureShader;
	m_TextureShader = 0;
    }

    return;
}


bool ShaderManagerClass::RenderTextureShader(float* worldMatrix, float* viewMatrix, float* projectionMatrix)
{
    return m_TextureShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix);
}


bool ShaderManagerClass::RenderLightShader(float* worldMatrix, float* viewMatrix, float* projectionMatrix, float* lightDirection, float* diffuseLightColor)
{
    return m_LightShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightDirection, diffuseLightColor);
}


bool ShaderManagerClass::RenderNormalMapShader(float* worldMatrix, float* viewMatrix, float* projectionMatrix, float* lightDirection, float* diffuseLightColor)
{
    return m_NormalMapShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightDirection, diffuseLightColor);
}
