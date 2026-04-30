////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Camera = 0;
    m_TextureShader = 0;
    m_Sprite = 0;
    m_Timer = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char spriteFilename[128];
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

    // Create and initialize the texture shader object.
    m_TextureShader = new TextureShaderClass;

    result = m_TextureShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the texture shader object." << endl;
	return false;
    }

    // Set the sprite info file we will be using.
    strcpy(spriteFilename, "../Engine/data/sprite_data_01.txt");

    // Create and initialize the sprite object.
    m_Sprite = new SpriteClass;

    result = m_Sprite->Initialize(m_OpenGL, screenWidth, screenHeight, 50, 50, spriteFilename);
    if(!result)
    {
        cout << "Error: Could not initialize the sprite object." << endl;
	return false;
    }

    // Create and initialize the timer object.
    m_Timer = new TimerClass;

    m_Timer->Initialize();

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the timer object.
    if(m_Timer)
    {
        delete m_Timer;
        m_Timer = 0;
    }

    // Release the sprite object.
    if(m_Sprite)
    {
        m_Sprite->Shutdown();
	delete m_Sprite;
	m_Sprite = 0;
    }

    // Release the texture shader object.
    if(m_TextureShader)
    {
        m_TextureShader->Shutdown();
        delete m_TextureShader;
        m_TextureShader = 0;
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
    float frameTime;
    bool result;


    // Update the system stats.
    m_Timer->Frame();

    // Get the current frame time.
    frameTime = m_Timer->GetTime();

    // Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

    // Update the sprite object using the frame time.
    m_Sprite->Update(frameTime);

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
    float worldMatrix[16], viewMatrix[16], orthoMatrix[16];
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and ortho matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetOrthoMatrix(orthoMatrix);

    // Disable the Z buffer for 2D rendering.
    m_OpenGL->TurnZBufferOff();

    // Set the texture shader as active and set its parameters.
    result = m_TextureShader->SetShaderParameters(worldMatrix, viewMatrix, orthoMatrix);
    if(!result)
    {
        return false;
    }
	    
    // Set the current sprite texture in the pixel shader.
    m_Sprite->SetTexture(0);
    
    // Render the sprite using the texture shader.
    m_Sprite->Render();

    // Enable the Z buffer now that 2D rendering is complete.
    m_OpenGL->TurnZBufferOn();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
