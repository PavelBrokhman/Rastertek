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
#include "inputclass.h"
#include "openglclass.h"
#include "cameraclass.h"
#include "modelclass.h"
#include "lightclass.h"
#include "lightshaderclass.h"
#include "fontshaderclass.h"
#include "fontclass.h"
#include "textclass.h"
#include "positionclass.h"
#include "timerclass.h"
#include "modellistclass.h"
#include "frustumclass.h"


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
    bool Render();
    bool UpdateRenderCountString(int);

private:
    OpenGLClass* m_OpenGL;
    CameraClass* m_Camera;
    ModelClass* m_Model;
    LightClass* m_Light;
    LightShaderClass* m_LightShader;
    FontShaderClass* m_FontShader;
    FontClass* m_Font;
    TextClass* m_RenderCountString;
    PositionClass* m_Position;
    TimerClass* m_Timer;
    ModelListClass* m_ModelList;
    FrustumClass* m_Frustum;
    float m_baseViewMatrix[16];
};

#endif

