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
    m_MouseStrings = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char mouseString1[32], mouseString2[32], mouseString3[32];
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

    // Set the initial mouse strings.
    strcpy(mouseString1, "Mouse X: 0");
    strcpy(mouseString2, "Mouse Y: 0");
    strcpy(mouseString3, "Mouse Button: No");

    // Create and initialize the text objects for the mouse strings.
    m_MouseStrings = new TextClass[3];

    result = m_MouseStrings[0].Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, mouseString1, 10, 10, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        cout << "Error: Could not initialize mouse string 0." << endl;
	return false;
    }

    result = m_MouseStrings[1].Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, mouseString2, 10, 35, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        cout << "Error: Could not initialize mouse string 1." << endl;
	return false;
    }

    result = m_MouseStrings[2].Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, mouseString3, 10, 60, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        cout << "Error: Could not initialize mouse string 2." << endl;
	return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the text objects for the mouse strings.
    if(m_MouseStrings)
    {
        m_MouseStrings[0].Shutdown();
	m_MouseStrings[1].Shutdown();
	m_MouseStrings[2].Shutdown();
	
	delete [] m_MouseStrings;
	m_MouseStrings = 0;
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
    int mouseX, mouseY;
    bool result, mouseDown;


    // Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

    // Get the location of the mouse from the input object.
    Input->GetMouseLocation(mouseX, mouseY);

    // Check if the mouse has been pressed.
    mouseDown = Input->IsMousePressed();

    // Update the mouse strings each frame.
    result = UpdateMouseStrings(mouseX, mouseY, mouseDown);
    if(!result)
    {
        cout << "Error: Could not UpdateMouseStrings." << endl;
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

    // Get the color to render the mouse text as.
    m_MouseStrings[0].GetPixelColor(pixelColor);

    // Set the font shader as active and set its parameters.
    result = m_FontShader->SetShaderParameters(worldMatrix, viewMatrix, orthoMatrix, pixelColor);
    if(!result)
    {
        return false;
    }

    // Set the font texture as the active texture.
    m_Font->SetTexture(0);

    // Render the mouse text strings using the font shader.
    m_MouseStrings[0].Render();
    m_MouseStrings[1].Render();
    m_MouseStrings[2].Render();

    // Enable the Z buffer and disable alpha blending now that 2D rendering is complete.
    m_OpenGL->TurnZBufferOn();
    m_OpenGL->DisableAlphaBlending();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}


bool ApplicationClass::UpdateMouseStrings(int mouseX, int mouseY, bool mouseDown)
{
    char tempString[16], finalString[32];
    bool result;


    // Convert the mouse X integer to string format.
    sprintf(tempString, "%d", mouseX);

    // Setup the mouse X string.
    strcpy(finalString, "Mouse X: ");
    strcat(finalString, tempString);

    // Update the sentence vertex buffer with the new string information.
    result = m_MouseStrings[0].UpdateText(m_Font, finalString, 10, 10, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

    // Convert the mouse Y integer to string format.
    sprintf(tempString, "%d", mouseY);

    // Setup the mouse Y string.
    strcpy(finalString, "Mouse Y: ");
    strcat(finalString, tempString);
    
    // Update the sentence vertex buffer with the new string information.
    result = m_MouseStrings[1].UpdateText(m_Font, finalString, 10, 35, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

    // Setup the mouse button string.
    if(mouseDown)
    {
        strcpy(finalString, "Mouse Button: Yes");
    }
    else
    {
        strcpy(finalString, "Mouse Button: No");
    }

    // Update the sentence vertex buffer with the new string information.
    result = m_MouseStrings[2].UpdateText(m_Font, finalString, 10, 60, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

    return true;
}
