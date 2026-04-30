////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Camera = 0;
    m_Timer = 0;
    m_FullScreenWindow = 0;
    m_ParallaxForest = 0;
    m_ScrollShader = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char configFilename[256];
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

    // Create and initialize the full screen ortho window object.
    m_FullScreenWindow = new OrthoWindowClass;

    result = m_FullScreenWindow->Initialize(m_OpenGL, screenWidth, screenHeight);
    if(!result)
    {
        cout << "Error: Could not initialize the full screen ortho window object." << endl;
        return false;
    }

    // Create and initialize the parallax scroll forest object.
    m_ParallaxForest = new ParallaxScrollClass;
    
    // Set the file name of the config file.
    strcpy(configFilename, "../Engine/data/config.txt");

    result = m_ParallaxForest->Initialize(m_OpenGL, configFilename);
    if(!result)
    {
        cout << "Error: Could not initialize the parallax forest scroll object." << endl;
        return false;
    }
    
    // Create and initialize the scroll shader object.
    m_ScrollShader = new ScrollShaderClass;

    result = m_ScrollShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the scroll shader object." << endl;
        return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the scroll shader object.
    if(m_ScrollShader)
    {
        m_ScrollShader->Shutdown();
        delete m_ScrollShader;
        m_ScrollShader = 0;
    }

    // Release the parallax scroll forest object.
    if(m_ParallaxForest)
    {
        m_ParallaxForest->Shutdown();
        delete m_ParallaxForest;
        m_ParallaxForest = 0;
    }

    // Release the full screen ortho window object.
    if(m_FullScreenWindow)
    {
        m_FullScreenWindow->Shutdown();
        delete m_FullScreenWindow;
        m_FullScreenWindow = 0;
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
    float frameTime;
    bool result;

  
    // Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }
    
    // Update the system stats.
    m_Timer->Frame();
    frameTime = m_Timer->GetTime();

    // Do the frame processing for the parallax scroll forest object.
    m_ParallaxForest->Frame(frameTime);
    
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
    float worldMatrix[16], baseViewMatrix[16], orthoMatrix[16];
    int textureCount, i;
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetBaseViewMatrix(baseViewMatrix);
    m_OpenGL->GetOrthoMatrix(orthoMatrix);

    // Turn on alpha blending and disable the Z buffer.
    m_OpenGL->EnableAlphaBlending();
    m_OpenGL->TurnZBufferOff();

    // Get the number of textures the parallax object uses.
    textureCount = m_ParallaxForest->GetTextureCount();

    // Render each of the parallax scroll textures in order using the scroll shader.
    for(i=0; i<textureCount; i++)
    {
        // Set the scroll shader as the current shader program and set the matrices that it will use for rendering.
        result = m_ScrollShader->SetShaderParameters(worldMatrix, baseViewMatrix, orthoMatrix, m_ParallaxForest->GetTranslation(i));
	if(!result)
	{
	    return false;
	}

	// Set the texture.
	m_ParallaxForest->SetTexture(m_OpenGL, i, 0);
	    
	// Render the full screen ortho window using the scroll shader.
	m_FullScreenWindow->Render();
    }
    
    // Enable the Z buffer and disable alpha blending.
    m_OpenGL->TurnZBufferOn();
    m_OpenGL->DisableAlphaBlending();
    
    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
