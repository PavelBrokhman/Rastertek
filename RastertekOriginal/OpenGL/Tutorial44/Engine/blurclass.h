////////////////////////////////////////////////////////////////////////////////
// Filename: blurclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _BLURCLASS_H_
#define _BLURCLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "rendertextureclass.h"
#include "cameraclass.h"
#include "orthowindowclass.h"
#include "textureshaderclass.h"
#include "blurshaderclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class Name: BlurClass
////////////////////////////////////////////////////////////////////////////////
class BlurClass
{
public:
    BlurClass();
    BlurClass(const BlurClass&);
    ~BlurClass();

    bool Initialize(OpenGLClass*, int, int, float, float, int, int);
    void Shutdown();

    bool BlurTexture(RenderTextureClass*, OpenGLClass*, CameraClass*, TextureShaderClass*, BlurShaderClass*);

private:
    RenderTextureClass *m_DownSampleTexture1, *m_DownSampleTexture2;
    OrthoWindowClass *m_DownSampleWindow, *m_UpSampleWindow;
    int m_downSampleWidth, m_downSampleHeight;
};

#endif

