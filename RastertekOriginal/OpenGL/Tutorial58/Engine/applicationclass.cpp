////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_XboxInput = 0;
    m_OpenGL = 0;
    m_Camera = 0;
    m_FontShader = 0;
    m_Font = 0;
    m_ActiveStrings = 0;
    m_ButtonStrings = 0;
    m_TriggerStrings = 0;
    m_ThumbStrings = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char activeString[32], buttonString[32], triggerString[32], thumbString[32];
    bool result;


    // Create and initialize the OpenGL object.
    m_OpenGL = new OpenGLClass;

    result = m_OpenGL->Initialize(display, win, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, VSYNC_ENABLED);
    if(!result)
    {
        cout << "Error: Could not initialize the OpenGL object." << endl;
        return false;
    }

    // Create and initialize the Xbox input object.
    m_XboxInput = new XboxInputClass;

    result = m_XboxInput->Initialize();
    if(!result)
    {
        cout << "Could not initialize the Xbox input object.  Make sure a controller is actually connected." << endl;
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

    // Create and initialize the text objects for the controllers active strings.
    m_ActiveStrings = new TextClass[4];

    strcpy(activeString, "Controller Active: No");

    result = m_ActiveStrings[0].Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, activeString, 10, 10, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

    result = m_ActiveStrings[1].Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, activeString, 10, 35, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

    result = m_ActiveStrings[2].Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, activeString, 10, 60, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

    result = m_ActiveStrings[3].Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, activeString, 10, 85, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

    // Create and initialize the text objects for the button strings.
    m_ButtonStrings = new TextClass[2];

    strcpy(buttonString, "A Button Down: No");

    result = m_ButtonStrings[0].Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, buttonString, 10, 120, 1.0f, 0.0f, 0.0f);
    if(!result)
    {
        return false;
    }

    strcpy(buttonString, "B Button Down: No");

    result = m_ButtonStrings[1].Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, buttonString, 10, 145, 1.0f, 0.0f, 0.0f);
    if(!result)
    {
        return false;
    }

    // Create and initialize the text objects for the trigger strings.
    m_TriggerStrings = new TextClass[2];

    strcpy(triggerString, "Left Trigger: 0.0");

    result = m_TriggerStrings[0].Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, triggerString, 10, 180, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

    strcpy(triggerString, "Right Trigger: 0.0");

    result = m_TriggerStrings[1].Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, triggerString, 10, 205, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

    // Create and initialize the text objects for the thumb stick strings.
    m_ThumbStrings = new TextClass[2];

    strcpy(thumbString, "Left Thumb X: 0");

    result = m_ThumbStrings[0].Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, thumbString, 10, 240, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

    strcpy(thumbString, "Left Thumb Y: 0");

    result = m_ThumbStrings[1].Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, thumbString, 10, 265, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }
    
    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the text objects for the thumb stick strings.
    if(m_ThumbStrings)
    {
        m_ThumbStrings[0].Shutdown();
        m_ThumbStrings[1].Shutdown();

        delete [] m_ThumbStrings;
        m_ThumbStrings = 0;
    }

    // Release the text objects for the trigger strings.
    if(m_TriggerStrings)
    {
        m_TriggerStrings[0].Shutdown();
        m_TriggerStrings[1].Shutdown();

        delete [] m_TriggerStrings;
        m_TriggerStrings = 0;
    }

    // Release the text objects for the button strings.
    if(m_ButtonStrings)
    {
        m_ButtonStrings[0].Shutdown();
        m_ButtonStrings[1].Shutdown();

        delete [] m_ButtonStrings;
        m_ButtonStrings = 0;
    }

    // Release the text objects for the controllers active strings.
    if(m_ActiveStrings)
    {
        m_ActiveStrings[0].Shutdown();
        m_ActiveStrings[1].Shutdown();
        m_ActiveStrings[2].Shutdown();
        m_ActiveStrings[3].Shutdown();

        delete [] m_ActiveStrings;
        m_ActiveStrings = 0;
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

    // Release the Xbox input object.
    if(m_XboxInput)
    {
        m_XboxInput->Shutdown();
        delete m_XboxInput;
        m_XboxInput = 0;
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

    // Do the frame processing for the Xbox input object.
    m_XboxInput->Frame();

    // Update the controller strings each frame.
    result = UpdateControllerStrings();
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
    int i;
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetOrthoMatrix(orthoMatrix);

    // Disable the Z buffer and enable alpha blending for 2D rendering.
    m_OpenGL->TurnZBufferOff();
    m_OpenGL->EnableAlphaBlending();


    // Render the controllers active text strings using the font shader.
    for(i=0; i<4; i++)
    {
        m_ActiveStrings[i].GetPixelColor(pixelColor);

        result = m_FontShader->SetShaderParameters(worldMatrix, viewMatrix, orthoMatrix, pixelColor);
        if(!result)
        {
            return false;
        }	

	m_Font->SetTexture(0);
	
        m_ActiveStrings[i].Render();
    }

    // Render the button down strings.
    for(i=0; i<2; i++)
    {
        m_ButtonStrings[i].GetPixelColor(pixelColor);

        result = m_FontShader->SetShaderParameters(worldMatrix, viewMatrix, orthoMatrix, pixelColor);
        if(!result)
        {
            return false;
        }	

	m_Font->SetTexture(0);
	
        m_ButtonStrings[i].Render();
    }

    // Render the trigger strings.
    for(i=0; i<2; i++)
    {
        m_TriggerStrings[i].GetPixelColor(pixelColor);

        result = m_FontShader->SetShaderParameters(worldMatrix, viewMatrix, orthoMatrix, pixelColor);
        if(!result)
        {
            return false;
        }	

	m_Font->SetTexture(0);
	
        m_TriggerStrings[i].Render();
    }

    for(i=0; i<2; i++)
    {
        m_ThumbStrings[i].GetPixelColor(pixelColor);

        result = m_FontShader->SetShaderParameters(worldMatrix, viewMatrix, orthoMatrix, pixelColor);
        if(!result)
        {
            return false;
        }	

	m_Font->SetTexture(0);
	
        m_ThumbStrings[i].Render();
    }

    // Enable the Z buffer and disable alpha blending now that 2D rendering is complete.
    m_OpenGL->TurnZBufferOn();
    m_OpenGL->DisableAlphaBlending();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}


bool ApplicationClass::UpdateControllerStrings()
{
    char activeString[32], buttonString[32], triggerString[32];
    int i, yPos, leftX, leftY;
    float value;
    bool result;


    // Update the controllers active strings.
    yPos = 10;
    for(i=0; i<4; i++)
    {
        if(m_XboxInput->IsControllerActive(i) == true)
        {
            strcpy(activeString, "Controller Active: Yes");
            result = m_ActiveStrings[i].UpdateText(m_Font, activeString, 10, yPos, 0.0f, 1.0f, 0.0f);
        }
        else
        {
            strcpy(activeString, "Controller Active: No");
            result = m_ActiveStrings[i].UpdateText(m_Font, activeString, 10, yPos, 1.0f, 0.0f, 0.0f);
        }

        // Confirm the string did update.
        if(!result)
        {
            return false;
        }

        // Increment the string drawing location.
        yPos += 25;
    }
    yPos += 10;

    // Update the buttons pressed strings.
    if(m_XboxInput->IsControllerActive(0) == true)
    {
        // A button.
        if(m_XboxInput->IsButtonADown(0) == true)
        {
            strcpy(buttonString, "A Button Down: Yes");
            result = m_ButtonStrings[0].UpdateText(m_Font, buttonString, 10, yPos, 0.0f, 1.0f, 0.0f);
        }
        else
        {
            strcpy(buttonString, "A Button Down: No");
            result = m_ButtonStrings[0].UpdateText(m_Font, buttonString, 10, yPos, 1.0f, 0.0f, 0.0f);
        }
        if(!result)
        {
            return false;
        }
        yPos += 25;

        // B button.
        if(m_XboxInput->IsButtonBDown(0) == true)
        {
            strcpy(buttonString, "B Button Down: Yes");
            result = m_ButtonStrings[1].UpdateText(m_Font, buttonString, 10, yPos, 0.0f, 1.0f, 0.0f);
        }
        else
        {
            strcpy(buttonString, "B Button Down: No");
            result = m_ButtonStrings[1].UpdateText(m_Font, buttonString, 10, yPos, 1.0f, 0.0f, 0.0f);
        }
        if(!result)
        {
            return false;
        }
        yPos += 25;
    }
    else
    {
        strcpy(buttonString, "A Button Down: No");
		
        result = m_ButtonStrings[0].UpdateText(m_Font, buttonString, 10, yPos, 1.0f, 0.0f, 0.0f);
        if(!result)
        {
            return false;
        }
        yPos += 25;

        strcpy(buttonString, "B Button Down: No");
		
        result = m_ButtonStrings[1].UpdateText(m_Font, buttonString, 10, yPos, 1.0f, 0.0f, 0.0f);
        if(!result)
        {
            return false;
        }
        yPos += 25;
    }
    yPos += 10;

    // Controller 1 triggers.
    if(m_XboxInput->IsControllerActive(0) == true)
    {
        // Left trigger.
        value = m_XboxInput->GetLeftTrigger(0);
        sprintf(triggerString, "Left Trigger: %f", value);

        result = m_TriggerStrings[0].UpdateText(m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
        if(!result)
        {
            return false;
        }
        yPos += 25;

        // Right trigger.
        value = m_XboxInput->GetRightTrigger(0);
        sprintf(triggerString, "Right Trigger: %f", value);

        result = m_TriggerStrings[1].UpdateText(m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
        if(!result)
        {
            return false;
        }
        yPos += 25;
    }
    else
    {
        strcpy(triggerString, "Left Trigger: 0.0");

        result = m_TriggerStrings[0].UpdateText(m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
        if(!result)
        {
            return false;
        }
        yPos += 25;

        strcpy(triggerString, "Right Trigger: 0.0");

        result = m_TriggerStrings[1].UpdateText(m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
        if(!result)
        {
            return false;
        }
        yPos += 25;
    }
    yPos += 10;

    // Controller 1 left thumb.
    if(m_XboxInput->IsControllerActive(0) == true)
    {
        m_XboxInput->GetLeftThumbStickLocation(0, leftX, leftY);

        // Left X.
        sprintf(triggerString, "Left Thumb X: %d", leftX);

        result = m_ThumbStrings[0].UpdateText(m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
        if(!result)
        {
            return false;
        }
        yPos += 25;

        // Left Y.
        sprintf(triggerString, "Left Thumb Y: %d", leftY);

        result = m_ThumbStrings[1].UpdateText(m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
        if(!result)
        {
            return false;
        }
        yPos += 25;
    }
    else
    {
        strcpy(triggerString, "Left Thumb X: 0");

        result = m_ThumbStrings[0].UpdateText(m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
        if(!result)
        {
            return false;
        }
        yPos += 25;

        strcpy(triggerString, "Left Thumb Y: 0");

        result = m_ThumbStrings[1].UpdateText(m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
        if(!result)
        {
            return false;
        }
        yPos += 25;
    }
  
    return true;
}
