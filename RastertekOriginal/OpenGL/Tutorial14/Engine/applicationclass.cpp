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
    m_TextString1 = 0;
    m_TextString2 = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char testString1[32], testString2[32];
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

    // Set the strings we want to display.
    strcpy(testString1, "Hello");
    strcpy(testString2, "Goodbye");

    // Create and initialize the first text object.
    m_TextString1 = new TextClass;

    result = m_TextString1->Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, testString1, 10, 10, 0.0f, 1.0f, 0.0f);
    if(!result)
    {
        cout << "Error: Could not initialize the text string 1 object." << endl;
        return false;
    }

    // Create and initialize the second text object.
    m_TextString2 = new TextClass;

    result = m_TextString2->Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, testString2, 10, 50, 1.0f, 1.0f, 0.0f);
    if(!result)
    {
        cout << "Error: Could not initialize the text string 2 object." << endl;
        return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the text string objects.
    if(m_TextString2)
    {
        m_TextString2->Shutdown();
	delete m_TextString2;
	m_TextString2 = 0;
    }
    
    if(m_TextString1)
    {
	m_TextString1->Shutdown();
	delete m_TextString1;
	m_TextString1 = 0;
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

    // Get the color to render the text as.
    m_TextString1->GetPixelColor(pixelColor);

    // Set the font shader as active and set its parameters.
    result = m_FontShader->SetShaderParameters(worldMatrix, viewMatrix, orthoMatrix, pixelColor);
    if(!result)
    {
        return false;
    }

    // Set the font texture as the active texture.
    m_Font->SetTexture(0);

    // Render the first text string using the font shader.
    m_TextString1->Render();

    // Get the color to render the text as.
    m_TextString2->GetPixelColor(pixelColor);

    // Set the font shader as active and set its parameters.
    result = m_FontShader->SetShaderParameters(worldMatrix, viewMatrix, orthoMatrix, pixelColor);
    if(!result)
    {
        return false;
    }
    
    // Set the font texture as the active texture.
    m_Font->SetTexture(0);

    // Render the first text string using the font shader.
    m_TextString2->Render();

    // Enable the Z buffer and disable alpha blending now that 2D rendering is complete.
    m_OpenGL->TurnZBufferOn();
    m_OpenGL->DisableAlphaBlending();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
