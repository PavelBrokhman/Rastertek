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
#include "cameraclass.h"
#include "lightclass.h"
#include "modelclass.h"
#include "orthowindowclass.h"
#include "deferredbuffersclass.h"
#include "deferredshaderclass.h"
#include "lightshaderclass.h"


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
    bool RenderSceneToTexture(float);
    bool Render();

private:
    OpenGLClass* m_OpenGL;
    CameraClass* m_Camera;
    LightClass* m_Light;
    ModelClass* m_Model;
    OrthoWindowClass* m_FullScreenWindow;
    DeferredBuffersClass* m_DeferredBuffers;
    DeferredShaderClass* m_DeferredShader;
    LightShaderClass* m_LightShader;
};

#endif

