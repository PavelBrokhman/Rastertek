////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_ShaderManager = 0;
    m_Timer = 0;
    m_Fps = 0;
    m_Font = 0;
    m_UserInterface = 0;
    m_Zone = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    bool result;


    // Create and initialize the OpenGL object.
    m_OpenGL = new OpenGLClass;

    result = m_OpenGL->Initialize(display, win, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, VSYNC_ENABLED);
    if(!result)
    {
        cout << "Error: Could not initialize the OpenGL object." << endl;
        return false;
    }

    // Create and initialize the shader manager object.
    m_ShaderManager = new ShaderManagerClass;

    result = m_ShaderManager->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the shader manager object." << endl;
        return false;
    }

    // Create and initialize the timer object.
    m_Timer = new TimerClass;
    m_Timer->Initialize();

    // Create and initialize the fps object.
    m_Fps = new FpsClass();
    m_Fps->Initialize();

    // Create and initialize the font object.
    m_Font = new FontClass;

    result = m_Font->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the font object." << endl;
        return false;
    }

    // Create and initialize the user interface object.
    m_UserInterface = new UserInterfaceClass;

    result = m_UserInterface->Initialize(m_OpenGL, m_Font, screenHeight, screenWidth);
    if(!result)
    {
        cout << "Error: Could not initialize the user interface object." << endl;
		return false;
    }

    // Create and initialize the zone object.
    m_Zone = new ZoneClass;

    result = m_Zone->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the zone object." << endl;
        return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the zone object.
    if(m_Zone)
    {
        m_Zone->Shutdown(m_OpenGL);
        delete m_Zone;
        m_Zone = 0;
    }

    // Release the user interface object.
    if(m_UserInterface)
    {
        m_UserInterface->Shutdown();
        delete m_UserInterface;
        m_UserInterface = 0;
    }

    // Release the font object.
    if(m_Font)
    {
        m_Font->Shutdown();
        delete m_Font;
        m_Font = 0;
    }

    // Release the fps object.
    if(m_Fps)
    {
        delete m_Fps;
        m_Fps = 0;
    }

    // Release the timer object.
    if(m_Timer)
    {
        delete m_Timer;
        m_Timer = 0;
    }

    // Release the shader manager object.
    if(m_ShaderManager)
    {
        m_ShaderManager->Shutdown();
        delete m_ShaderManager;
        m_ShaderManager = 0;
    }

    // Release the OpenGL object.
    if(m_OpenGL)
    {
        m_OpenGL->Shutdown();
        delete m_OpenGL;
        m_OpenGL = 0;
    }

    return;
}


bool ApplicationClass::Frame(InputClass* Input)
{
    bool result;


    // Update the system timer.
    m_Timer->Frame();

    // Update the fps each frame.
    m_Fps->Frame();

    // Check if the user pressed escape and wants to exit the application.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

    // Perform the user interface processing.
    result = m_UserInterface->Frame(m_Font, m_Fps->GetFps());
    if(!result)
    {
        return false;
    }

    // Perform the zone frame processing.
    result = m_Zone->Frame(m_OpenGL, m_ShaderManager, m_Font, m_UserInterface, Input, m_Timer->GetTime());
    if(!result)
    {
        return false;
    }

    return true;
}
