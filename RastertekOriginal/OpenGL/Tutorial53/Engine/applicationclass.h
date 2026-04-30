////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _APPLICATIONCLASS_H_
#define _APPLICATIONCLASS_H_


/////////////
// GLOBALS //
/////////////
const bool FULL_SCREEN = false;
const bool VSYNC_ENABLED = true;
const float SCREEN_NEAR = 0.3f;
const float SCREEN_DEPTH = 1000.0f;


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "openglclass.h"
#include "inputclass.h"
#include "timerclass.h"
#include "cameraclass.h"
#include "modelclass.h"
#include "lightclass.h"
#include "orthowindowclass.h"
#include "rendertextureclass.h"
#include "blurclass.h"
#include "lightshaderclass.h"
#include "heatclass.h"
#include "heatshaderclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class Name: ApplicationClass
////////////////////////////////////////////////////////////////////////////////
class ApplicationClass
{
public:
    ApplicationClass();
    ApplicationClass(const ApplicationClass&);
    ~ApplicationClass();

    bool Initialize(Display*, Window, int, int);
    void Shutdown();
    bool Frame(InputClass*);

private:
    bool RenderSceneToTexture();
    bool RenderHeatToTexture();
    bool Render();

private:
    OpenGLClass* m_OpenGL;
    TimerClass* m_Timer;
    CameraClass* m_Camera;
    ModelClass* m_Model;
    LightClass* m_Light;
    LightShaderClass* m_LightShader;
    RenderTextureClass* m_RenderTexture;
    OrthoWindowClass* m_FullScreenWindow;
    TextureShaderClass* m_TextureShader;
    BlurShaderClass* m_BlurShader;
    BlurClass* m_Blur;
    HeatClass* m_Heat;
    HeatShaderClass* m_HeatShader;
    RenderTextureClass* m_HeatTexture;
};

#endif

