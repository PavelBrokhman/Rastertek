////////////////////////////////////////////////////////////////////////////////
// Filename: systemclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _SYSTEMCLASS_H_
#define _SYSTEMCLASS_H_


//////////////
// INCLUDES //
//////////////
#include <unistd.h>


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "applicationclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class Name: SystemClass
////////////////////////////////////////////////////////////////////////////////
class SystemClass
{
public:
    SystemClass();
    SystemClass(const SystemClass&);
    ~SystemClass();

    bool Initialize();
    void Shutdown();
    void Frame();

private:
    bool InitializeWindow(int&, int&);
    void ShutdownWindow();
    void ReadInput();

private:
    ApplicationClass* m_Application;
    InputClass* m_Input;
    Display* m_videoDisplay;
    Window m_hwnd;
    GLXContext m_renderingContext;
};

#endif

