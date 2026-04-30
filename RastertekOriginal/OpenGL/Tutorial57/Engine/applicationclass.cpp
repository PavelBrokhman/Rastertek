////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_OpenAL = 0;
    m_TestSound2 = 0;
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
    strcpy(filename, "../Engine/data/sound02.wav");

    // Create and initialize the sound object.
    m_TestSound2 = new Sound3DClass;

    result = m_TestSound2->LoadTrack(filename, 1.0f);
    if(!result)
    {
        cout << "Error: Could not initialize the test sound 2 object." << endl;
        return false;
    }

    // Set the 3D position of the sound.
    result = m_TestSound2->Update3DPosition(-2.0f, 0.0f, 0.0f);
    if(!result)
    {
        cout << "Error: Could not set the sound position." << endl;
        return false;
    }
    
    // Play the sound.
    result = m_TestSound2->PlayTrack(true);
    if(!result)
    {
        cout << "Error: Could not play the sound." << endl;
        return false;
    }
    
    return true;
}


void ApplicationClass::Shutdown()
{
    if(m_TestSound2)
    {
        // Stop the sound if it was still playing.
        m_TestSound2->StopTrack();
	
	// Release the sound object.
	m_TestSound2->ReleaseTrack();
        delete m_TestSound2;
        m_TestSound2 = 0;
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
