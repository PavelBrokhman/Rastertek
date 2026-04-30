////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Camera = 0;
    m_Timer = 0;
    m_ParticleSystem = 0;
    m_ParticleShader = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char configFilename[128];
    bool result;


    // Create and initialize the OpenGL object.
    m_OpenGL = new OpenGLClass;

    result = m_OpenGL->Initialize(display, win, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, VSYNC_ENABLED);
    if(!result)
    {
        cout << "Error: Could not initialize the OpenGL object." << endl;
        return false;
    }

    // Create and initialize the camera object.
    m_Camera = new CameraClass;

    m_Camera->SetPosition(0.0f, 0.0f, -10.0f);
    m_Camera->Render();
    m_Camera->RenderBaseViewMatrix();

    // Create and initialize the timer object.
    m_Timer = new TimerClass;
    m_Timer->Initialize();

    // Set the file name of the texture for the particle system.
    strcpy(configFilename, "../Engine/data/particle_config_01.txt");

    // Create and initialize the partcile system object.
    m_ParticleSystem = new ParticleSystemClass;

    result = m_ParticleSystem->Initialize(m_OpenGL, configFilename);
    if(!result)
    {
        cout << "Error: Could not initialize the particle system object." << endl;
        return false;
    }
    
    // Create and initialize the particle shader object.
    m_ParticleShader = new ParticleShaderClass;

    result = m_ParticleShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the particle shader object." << endl;
        return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the particle shader object.
    if(m_ParticleShader)
    {
        m_ParticleShader->Shutdown();
        delete m_ParticleShader;
        m_ParticleShader = 0;
    }

    // Release the particle system object.
    if(m_ParticleSystem)
    {
        m_ParticleSystem->Shutdown();
        delete m_ParticleSystem;
        m_ParticleSystem = 0;
    }

    // Release the timer object.
    if(m_Timer)
    {
        delete m_Timer;
        m_Timer = 0;
    }

    // Release the camera object.
    if(m_Camera)
    {
        delete m_Camera;
        m_Camera = 0;
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

  
    // Update the system stats.
    m_Timer->Frame();

    // Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

    // Check if the user wants to reload the particle system config and restart the particles.
    if(Input->IsRPressed() == true)
    {
        result = m_ParticleSystem->Reload();
        if(!result)
        {
            return false;
        }
    }

    // Run the frame processing for the particle system.
    m_ParticleSystem->Frame(m_Timer->GetTime());
    
    // Render the graphics scene.
    result = Render();
    if(!result)
    {
        return false;
    }

    return true;
}


bool ApplicationClass::Render()
{
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16];
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Turn on particle alpha blending and disable the Z buffer.
    m_OpenGL->EnableParticleAlphaBlending();
    m_OpenGL->TurnZBufferOff();
    
    // Set the particle shader as the current shader program and set the matrices that it will use for rendering.
    result = m_ParticleShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix);
    if(!result)
    {
        return false;
    }

    // Render the particles using the particle shader.
    m_ParticleSystem->Render();
    
    // Enable the Z buffer and disable alpha blending.
    m_OpenGL->TurnZBufferOn();
    m_OpenGL->DisableAlphaBlending();
    
    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
