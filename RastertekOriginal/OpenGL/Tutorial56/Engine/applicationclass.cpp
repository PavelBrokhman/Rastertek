////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_OpenAL = 0;
    m_TestSound1 = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char filename[256];
    bool result;


    // Create and initialize the OpenGL object.
    m_OpenGL = new OpenGLClass;

    result = m_OpenGL->Initialize(display, win, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, VSYNC_ENABLED);
    if(!result)
    {
        cout << "Error: Could not initialize the OpenGL object." << endl;
        return false;
    }

    // Create and initialize the OpenAL object.
    m_OpenAL = new OpenALClass;

    result = m_OpenAL->Initialize();
    if(!result)
    {
        cout << "Error: Could not initialize the OpenAL object." << endl;
        return false;
    }

    // Set the filename for the sound to load.
    strcpy(filename, "../Engine/data/sound01.wav");

    // Create and initialize the sound object.
    m_TestSound1 = new SoundClass;

    result = m_TestSound1->LoadTrack(filename, 1.0f);
    if(!result)
    {
        cout << "Error: Could not initialize the test sound 1 object." << endl;
        return false;
    }

    // Play the sound.
    m_TestSound1->PlayTrack(true);
    
    return true;
}


void ApplicationClass::Shutdown()
{
    if(m_TestSound1)
    {
        // Stop the sound if it was still playing.
        m_TestSound1->StopTrack();
	
	// Release the sound object.
	m_TestSound1->ReleaseTrack();
        delete m_TestSound1;
        m_TestSound1 = 0;
    }

    // Release the OpenAL object.
    if(m_OpenAL)
    {
        m_OpenAL->Shutdown();
        delete m_OpenAL;
        m_OpenAL = 0;
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


    // Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

    // Render the final graphics scene.
    result = Render();
    if(!result)
    {
        return false;
    }

    return true;
}


bool ApplicationClass::Render()
{
    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);


    
    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
