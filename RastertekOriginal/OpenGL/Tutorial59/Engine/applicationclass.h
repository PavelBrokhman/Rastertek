////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _APPLICATIONCLASS_H_
#define _APPLICATIONCLASS_H_


/////////////
// GLOBALS //
/////////////
const bool FULL_SCREEN = true;
const bool VSYNC_ENABLED = true;
const float SCREEN_NEAR = 0.3f;
const float SCREEN_DEPTH = 1000.0f;


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "openglclass.h"
#include "inputclass.h"
#include "cameraclass.h"
#include "modelclass.h"
#include "lightclass.h"
#include "orthowindowclass.h"
#include "rendertextureclass.h"
#include "lightshaderclass.h"
#include "clutclass.h"
#include "colorgradeshaderclass.h"


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
    ModelClass* m_Model;
    LightClass* m_Light;
    LightShaderClass* m_LightShader;
    RenderTextureClass* m_RenderTexture;
    OrthoWindowClass* m_FullScreenWindow;
    ClutClass* m_ColorLUT;
    ColorGradeShaderClass* m_ColorGradeShader;
};

#endif

