///////////////////////////////////////////////////////////////////////////////
// Filename: heatclass.cpp
///////////////////////////////////////////////////////////////////////////////
#include "heatclass.h"


HeatClass::HeatClass()
{
    m_HeatNoiseTexture = 0;
}


HeatClass::HeatClass(const HeatClass& other)
{
}


HeatClass::~HeatClass()
{
}


bool HeatClass::Initialize(OpenGLClass* OpenGL)
{
    char textureFilename[128];
    bool result;


    // Create and initialize the heat noise texture object.
    m_HeatNoiseTexture = new TextureClass;

    strcpy(textureFilename, "../Engine/data/heatnoise01.tga");

    result = m_HeatNoiseTexture->Initialize(OpenGL, textureFilename, true);
    if(!result)
    {
        return false;
    }

    // Set the three scrolling speeds for the three different noise textures.
    m_scrollSpeeds[0] = 1.3f;
    m_scrollSpeeds[1] = 2.1f;
    m_scrollSpeeds[2] = 2.3f;

    // Set the three scales which will be used to create the three different noise octave textures.
    m_scales[0] = 1.0f;
    m_scales[1] = 2.0f;
    m_scales[2] = 3.0f;

    // Set the three different x and y distortion factors for the three different noise textures.
    m_distortion1[0] = 0.1f;
    m_distortion1[1] = 0.2f;
    
    m_distortion2[0] = 0.1f;
    m_distortion2[1] = 0.3f;
    
    m_distortion3[0] = 0.1f;
    m_distortion3[1] = 0.1f;

    // Set the emissive multiplier.
    m_emissiveMultiplier = 1.6f;

    // Initialize the noise timing.
    m_noiseFrameTime = 0.0f;

    return true;
}


void HeatClass::Shutdown()
{
    // Release the heat noise texture.
    if(m_HeatNoiseTexture)
    {
        m_HeatNoiseTexture->Shutdown();
	delete m_HeatNoiseTexture;
	m_HeatNoiseTexture = 0;
    }
    
    return;
}


void HeatClass::Frame(float frameTime)
{
    // Increment the frame time counter.
    m_noiseFrameTime += (frameTime * 0.075f);
    if(m_noiseFrameTime > 1000.0f)
    {
        m_noiseFrameTime = 0.0f;
    }

    return;
}


void HeatClass::SetTexture(OpenGLClass* OpenGL, unsigned int textureUnit)
{
    m_HeatNoiseTexture->SetTexture(OpenGL, textureUnit);
    return;
}


void HeatClass::GetNoiseValues(float* scrollSpeeds, float* scales, float* distortion1, float* distortion2, float* distortion3,
			       float& emissiveMultiplier, float& noiseFrameTime)
{
    scrollSpeeds[0] = m_scrollSpeeds[0];
    scrollSpeeds[1] = m_scrollSpeeds[1];
    scrollSpeeds[2] = m_scrollSpeeds[2];
    
    scales[0] = m_scales[0];
    scales[1] = m_scales[1];
    scales[2] = m_scales[2];
	
    distortion1[0] = m_distortion1[0];
    distortion1[1] = m_distortion1[1];

    distortion2[0] = m_distortion2[0];
    distortion2[1] = m_distortion2[1];
    
    distortion3[0] = m_distortion3[0];
    distortion3[1] = m_distortion3[1];

    emissiveMultiplier = m_emissiveMultiplier;

    noiseFrameTime = m_noiseFrameTime;

    return;
}
