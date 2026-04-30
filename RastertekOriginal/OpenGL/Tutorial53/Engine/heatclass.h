////////////////////////////////////////////////////////////////////////////////
// Filename: heatclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _HEATCLASS_H_
#define _HEATCLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "textureclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: HeatClass
////////////////////////////////////////////////////////////////////////////////
class HeatClass
{
public:
    HeatClass();
    HeatClass(const HeatClass&);
    ~HeatClass();

    bool Initialize(OpenGLClass*);
    void Shutdown();
    void Frame(float);

    void SetTexture(OpenGLClass*, unsigned int);
    void GetNoiseValues(float*, float*, float*, float*, float*, float&, float&);

private:
    TextureClass* m_HeatNoiseTexture;
    float m_scrollSpeeds[3], m_scales[3];
    float m_distortion1[2], m_distortion2[2], m_distortion3[2];
    float m_emissiveMultiplier, m_noiseFrameTime;
};

#endif
