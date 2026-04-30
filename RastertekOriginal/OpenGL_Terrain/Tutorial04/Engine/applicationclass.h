///////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.h
///////////////////////////////////////////////////////////////////////////////
#ifndef _APPLICATIONCLASS_H_
#define _APPLICATIONCLASS_H_


/////////////
// GLOBALS //
/////////////
const bool FULL_SCREEN = true;
const bool VSYNC_ENABLED = true;
const float SCREEN_DEPTH = 1000.0f;
const float SCREEN_NEAR = 0.3f;


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "openglclass.h"
#include "shadermanagerclass.h"
#include "inputclass.h"
#include "timerclass.h"
#include "fpsclass.h"
#include "fontclass.h"
#include "userinterfaceclass.h"
#include "zoneclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: ApplicationClass
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
    OpenGLClass* m_OpenGL;
    ShaderManagerClass* m_ShaderManager;
    TimerClass* m_Timer;
    FpsClass* m_Fps;
    FontClass* m_Font;
    UserInterfaceClass* m_UserInterface;
    ZoneClass* m_Zone;
};

#endif
