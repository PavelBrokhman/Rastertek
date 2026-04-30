////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Camera = 0;
    m_FontShader = 0;
    m_Font = 0;
    m_Fps = 0;
    m_FpsString = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char fpsString[32];
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

    // Create and initialize the font shader object.
    m_FontShader = new FontShaderClass;

    result = m_FontShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the font shader object." << endl;
        return false;
    }

    // Create and initialize the font object.
    m_Font = new FontClass;

    result = m_Font->Initialize(m_OpenGL, 0);
    if(!result)
    {
        cout << "Error: Could not initialize the font object." << endl;
        return false;
    }

    // Create and initialize the fps object.
    m_Fps = new FpsClass();

    m_Fps->Initialize();

    // Set the initial fps and fps string.
    m_previousFps = -1;
    strcpy(fpsString, "Fps: 0");

    // Create and initialize the text object for the fps string.
    m_FpsString = new TextClass;

    result = m_FpsString->Initialize(m_OpenGL, screenWidth, screenHeight, 16, m_Font, fpsString, 10, 50, 0.0f, 1.0f, 0.0f);
    if(!result)
    {
        cout << "Error: Could not initialize the fps string object." << endl;
        return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the text object for the fps string.
    if(m_FpsString)
    {
        m_FpsString->Shutdown();
	delete m_FpsString;
	m_FpsString = 0;
    }

    // Release the fps object.
    if(m_Fps)
    {
        delete m_Fps;
        m_Fps = 0;
    }

    // Release the font object.
    if(m_Font)
    {
        m_Font->Shutdown();
	delete m_Font;
	m_Font = 0;
    }

    // Release the font shader object.
    if(m_FontShader)
    {
        m_FontShader->Shutdown();
	delete m_FontShader;
	m_FontShader = 0;
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


    // Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

    // Update the frames per second each frame.
    result = UpdateFps();
    if(!result)
    {
        return false;
    }

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
    float pixelColor[4];
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and ortho matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetOrthoMatrix(orthoMatrix);

    // Disable the Z buffer and enable alpha blending for 2D rendering.
    m_OpenGL->TurnZBufferOff();
    m_OpenGL->EnableAlphaBlending();

    // Get the color to render the fps text as.
    m_FpsString->GetPixelColor(pixelColor);

    // Set the font shader as active and set its parameters.
    result = m_FontShader->SetShaderParameters(worldMatrix, viewMatrix, orthoMatrix, pixelColor);
    if(!result)
    {
        return false;
    }

    // Set the font texture as the active texture.
    m_Font->SetTexture(0);

    // Render the fps text string using the font shader.
    m_FpsString->Render();

    // Enable the Z buffer and disable alpha blending now that 2D rendering is complete.
    m_OpenGL->TurnZBufferOn();
    m_OpenGL->DisableAlphaBlending();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}


bool ApplicationClass::UpdateFps()
{
    int fps;
    char tempString[16], finalString[16];
    float red, green, blue;
    bool result;


    // Update the fps each frame.
    m_Fps->Frame();

    // Get the current fps.
    fps = m_Fps->GetFps();

    // Check if the fps from the previous frame was the same, if so don't need to update the text string.
    if(m_previousFps == fps)
    {
        return true;
    }

    // Store the fps for checking next frame.
    m_previousFps = fps;

    // Truncate the fps to below 100,000.
    if(fps > 99999)
    {
        fps = 99999;
    }

    // Convert the fps integer to string format.
    sprintf(tempString, "%d", fps);

    // Setup the fps string.
    strcpy(finalString, "Fps: ");
    strcat(finalString, tempString);

    // If fps is 60 or above set the fps color to green.
    if(fps >= 60)
    {
        red = 0.0f;
	green = 1.0f;
	blue = 0.0f;
    }

    // If fps is below 60 set the fps color to yellow.
    if(fps < 60)
    {
        red = 1.0f;
	green = 1.0f;
	blue = 0.0f;
    }

    // If fps is below 30 set the fps color to red.
    if(fps < 30)
    {
        red = 1.0f;
	green = 0.0f;
	blue = 0.0f;
    }

    // Update the sentence vertex buffer with the new string information.
    result = m_FpsString->UpdateText(m_Font, finalString, 10, 10, red, green, blue);
    if(!result)
    {
        return false;
    }

    return true;
}
