////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Camera = 0;
    m_TextureShader = 0;
    m_Model = 0;
    m_RenderTexture = 0;
    m_FullScreenWindow = 0;
    m_FadeShader = 0;
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
    char modelFilename[128], textureFilename[128];
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
 
    // Create and initialize the texture shader object.
    m_TextureShader = new TextureShaderClass;

    result = m_TextureShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the texture shader object." << endl;
        return false;
    }

    // Set the filename for the model object.
    strcpy(modelFilename, "../Engine/data/cube.txt");

    // Set the file name of the texture.
    strcpy(textureFilename, "../Engine/data/stone01.tga");

    // Create and initialize the model object.
    m_Model = new ModelClass;

    result = m_Model->Initialize(m_OpenGL, modelFilename, textureFilename, false, NULL, false, NULL, false);
    if(!result)
    {
        cout << "Error: Could not initialize the Model object." << endl;
        return false;
    }

    // Create and initialize the render to texture object.
    m_RenderTexture = new RenderTextureClass;

    result = m_RenderTexture->Initialize(m_OpenGL, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, 0);
    if(!result)
    {
        cout << "Error: Could not initialize the render texture object." << endl;
        return false;
    }

    // Create and initialize the full screen ortho window object.
    m_FullScreenWindow = new OrthoWindowClass;

    result = m_FullScreenWindow->Initialize(m_OpenGL, screenWidth, screenHeight);
    if(!result)
    {
        cout << "Error: Could not initialize the full screen ortho window object." << endl;
        return false;
    }

    // Create and initialize the fade shader object.
    m_FadeShader = new FadeShaderClass;

    result = m_FadeShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the fade shader object." << endl;
        return false;
    }

    // Create and initialize the timer object.
    m_Timer = new TimerClass;
    m_Timer->Initialize();

    // Initialize the accumulated time to zero milliseconds.
    m_accumulatedTime = 0;

    // Set the fade in time to 5 seconds.
    m_fadeInTime = 5.0f;

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

    // Release the fade shader object.
    if(m_FadeShader)
    {
        m_FadeShader->Shutdown();
        delete m_FadeShader;
        m_FadeShader = 0;
    }

    // Release the full screen ortho window object.
    if(m_FullScreenWindow)
    {
        m_FullScreenWindow->Shutdown();
        delete m_FullScreenWindow;
        m_FullScreenWindow = 0;
    }

    // Release the render texture object.
    if(m_RenderTexture)
    {
        m_RenderTexture->Shutdown();
        delete m_RenderTexture;
        m_RenderTexture = 0;
    }

    // Release the model object.
    if(m_Model)
    {
        m_Model->Shutdown();
        delete m_Model;
        m_Model = 0;
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
    static float rotation = 360.0f;
    float frameTime;
    float fadePercentage;
    bool result;


    // Update the system stats.
    m_Timer->Frame();

    // Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

    // Update the rotation variable each frame.
    rotation -= 0.0174532925f * 1.0f;
    if(rotation <= 0.0f)
    {
        rotation += 360.0f;
    }

    // Update the accumulated time with the extra frame time addition.
    frameTime = m_Timer->GetTime();
    m_accumulatedTime += frameTime;

    // While the time goes on increase the fade in amount by the time that is passing each frame.
    if(m_accumulatedTime < m_fadeInTime)
    {
        // Calculate the percentage that the screen should be faded in based on the accumulated time.
        fadePercentage = m_accumulatedTime / m_fadeInTime;
    }
    else
    {
        fadePercentage = 1.0f;
    }

    // Render the scene to a render texture.
    result = RenderSceneToTexture(rotation);
    if(!result)
    {
        return false;
    }

    // Render the faded version of the scene.
    result = Render(fadePercentage);
    if(!result)
    {
        return false;
    }

    return true;
}


bool ApplicationClass::RenderSceneToTexture(float rotation)
{
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16];
    bool result;


    // Set the render target to be the render texture and clear it.
    m_RenderTexture->SetRenderTarget();
    m_RenderTexture->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the matrices.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_RenderTexture->GetProjectionMatrix(projectionMatrix);

    // Rotate the world matrix by the rotation value so that the cube will spin.
    m_OpenGL->MatrixRotationY(worldMatrix, rotation);

    // Set the texture shader as the current shader program and set the matrices that it will use for rendering.
    result = m_TextureShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix);
    if(!result)
    {
        return false;
    }

    // Render the model.
    m_Model->SetTexture1(0);
    m_Model->Render();

    // Reset the render target back to the original back buffer and not the render to texture anymore.  And reset the viewport back to the original.
    m_OpenGL->SetBackBufferRenderTarget();
    m_OpenGL->ResetViewport();

    return true;
}


bool ApplicationClass::Render(float fadeAmount)
{
    float worldMatrix[16], baseViewMatrix[16], orthoMatrix[16];
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Begin 2D rendering and turn off the Z buffer.
    m_OpenGL->TurnZBufferOff();
    
    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetBaseViewMatrix(baseViewMatrix);
    m_OpenGL->GetOrthoMatrix(orthoMatrix);

    // Set the fade shader as the current shader program and set the variables that it will use for rendering.
    result = m_FadeShader->SetShaderParameters(worldMatrix, baseViewMatrix, orthoMatrix, fadeAmount);
    if(!result)
    {
        return false;
    }

    // Set the render texture for the ortho window rendering.
    m_RenderTexture->SetTexture(0);

    // Render the full screen ortho window.
    m_FullScreenWindow->Render();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
