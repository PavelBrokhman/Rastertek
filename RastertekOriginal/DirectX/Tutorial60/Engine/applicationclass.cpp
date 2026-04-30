////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
	m_Direct3D = 0;
	m_XInput = 0;
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


bool ApplicationClass::Initialize(int screenWidth, int screenHeight, HWND hwnd)
{
	char activeString[32], buttonString[32], triggerString[32], thumbString[32];
	bool result;


	// Create and initialize the Direct3D object.
	m_Direct3D = new D3DClass;

	result = m_Direct3D->Initialize(screenWidth, screenHeight, VSYNC_ENABLED, hwnd, FULL_SCREEN, SCREEN_DEPTH, SCREEN_NEAR);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize Direct3D", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the XInput object.
	m_XInput = new XInputClass;

	result = m_XInput->Initialize();
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the XInput object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the camera object.
	m_Camera = new CameraClass;

	m_Camera->SetPosition(0.0f, 0.0f, -10.0f);
	m_Camera->Render();

	// Create and initialize the font shader object.
	m_FontShader = new FontShaderClass;

	result = m_FontShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the font shader object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the font object.
    m_Font = new FontClass;

    result = m_Font->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), 0);
    if(!result)
    {
        return false;
    }

	// Create and initialize the text objects for the controllers active strings.
    m_ActiveStrings = new TextClass[4];

    strcpy_s(activeString, "Controller Active: No");

	result = m_ActiveStrings[0].Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), screenWidth, screenHeight, 32, m_Font, activeString, 10, 10, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

	result = m_ActiveStrings[1].Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), screenWidth, screenHeight, 32, m_Font, activeString, 10, 35, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

	result = m_ActiveStrings[2].Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), screenWidth, screenHeight, 32, m_Font, activeString, 10, 60, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

	result = m_ActiveStrings[3].Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), screenWidth, screenHeight, 32, m_Font, activeString, 10, 85, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

	// Create and initialize the text objects for the button strings.
    m_ButtonStrings = new TextClass[2];

    strcpy_s(buttonString, "A Button Down: No");

    result = m_ButtonStrings[0].Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), screenWidth, screenHeight, 32, m_Font, buttonString, 10, 120, 1.0f, 0.0f, 0.0f);
    if(!result)
    {
        return false;
    }

	strcpy_s(buttonString, "B Button Down: No");

    result = m_ButtonStrings[1].Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), screenWidth, screenHeight, 32, m_Font, buttonString, 10, 145, 1.0f, 0.0f, 0.0f);
    if(!result)
    {
        return false;
    }

	// Create and initialize the text objects for the trigger strings.
    m_TriggerStrings = new TextClass[2];

    strcpy_s(triggerString, "Left Trigger: 0.0");

    result = m_TriggerStrings[0].Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), screenWidth, screenHeight, 32, m_Font, triggerString, 10, 180, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

    strcpy_s(triggerString, "Right Trigger: 0.0");

	result = m_TriggerStrings[1].Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), screenWidth, screenHeight, 32, m_Font, triggerString, 10, 205, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

	// Create and initialize the text objects for the thumb stick strings.
    m_ThumbStrings = new TextClass[2];

    strcpy_s(thumbString, "Left Thumb X: 0");

    result = m_ThumbStrings[0].Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), screenWidth, screenHeight, 32, m_Font, thumbString, 10, 240, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

    strcpy_s(thumbString, "Left Thumb Y: 0");

	result = m_ThumbStrings[1].Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), screenWidth, screenHeight, 32, m_Font, thumbString, 10, 265, 1.0f, 1.0f, 1.0f);
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

	// Release the XInput object.
	if(m_XInput)
	{
		m_XInput->Shutdown();
		delete m_XInput;
		m_XInput = 0;
	}

	// Release the Direct3D object.
	if(m_Direct3D)
	{
		m_Direct3D->Shutdown();
		delete m_Direct3D;
		m_Direct3D = 0;
	}

	return;
}


bool ApplicationClass::Frame(InputClass* Input)
{
	bool result;


	// Check if the user pressed escape and wants to exit the application.
	if(Input->IsEscapePressed())
	{
		return false;
	}

	// Do the frame processing for the XInput object.
	m_XInput->Frame();

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
	XMMATRIX worldMatrix, viewMatrix, orthoMatrix;
	int i;
	bool result;


	// Clear the buffers to begin the scene.
	m_Direct3D->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

	// Get the world, view, and projection matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetViewMatrix(viewMatrix);
	m_Direct3D->GetOrthoMatrix(orthoMatrix);

	// Disable the Z buffer and enable alpha blending for 2D rendering.
	m_Direct3D->TurnZBufferOff();
	m_Direct3D->EnableAlphaBlending();

	// Render the controllers active text strings using the font shader.
	for(i=0; i<4; i++)
	{
		m_ActiveStrings[i].Render(m_Direct3D->GetDeviceContext());

		result = m_FontShader->Render(m_Direct3D->GetDeviceContext(), m_ActiveStrings[i].GetIndexCount(), worldMatrix, viewMatrix, orthoMatrix, 
									  m_Font->GetTexture(), m_ActiveStrings[i].GetPixelColor());
		if(!result)
		{
			return false;
		}	
	}

	// Render the button down strings.
	for(i=0; i<2; i++)
	{
		m_ButtonStrings[i].Render(m_Direct3D->GetDeviceContext());

		result = m_FontShader->Render(m_Direct3D->GetDeviceContext(), m_ButtonStrings[i].GetIndexCount(), worldMatrix, viewMatrix, orthoMatrix, 
									  m_Font->GetTexture(), m_ButtonStrings[i].GetPixelColor());
		if(!result)
		{
			return false;
		}	
	}

	// Render the trigger strings.
	for(i=0; i<2; i++)
	{
		m_TriggerStrings[i].Render(m_Direct3D->GetDeviceContext());

		result = m_FontShader->Render(m_Direct3D->GetDeviceContext(), m_TriggerStrings[i].GetIndexCount(), worldMatrix, viewMatrix, orthoMatrix, 
									  m_Font->GetTexture(), m_TriggerStrings[i].GetPixelColor());
		if(!result)
		{
			return false;
		}	
	}

	for(i=0; i<2; i++)
	{
		m_ThumbStrings[i].Render(m_Direct3D->GetDeviceContext());

		result = m_FontShader->Render(m_Direct3D->GetDeviceContext(), m_ThumbStrings[i].GetIndexCount(), worldMatrix, viewMatrix, orthoMatrix, 
									  m_Font->GetTexture(), m_ThumbStrings[i].GetPixelColor());
		if(!result)
		{
			return false;
		}	
	}

	// Enable the Z buffer and disable alpha blending now that 2D rendering is complete.
	m_Direct3D->TurnZBufferOn();
	m_Direct3D->DisableAlphaBlending();

	// Present the rendered scene to the screen.
	m_Direct3D->EndScene();

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
		if(m_XInput->IsControllerActive(i) == true)
		{
			strcpy_s(activeString, "Controller Active: Yes");
			result = m_ActiveStrings[i].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, activeString, 10, yPos, 0.0f, 1.0f, 0.0f);
		}
		else
		{
			strcpy_s(activeString, "Controller Active: No");
			result = m_ActiveStrings[i].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, activeString, 10, yPos, 1.0f, 0.0f, 0.0f);
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
	if(m_XInput->IsControllerActive(0) == true)
	{
		// A button.
		if(m_XInput->IsButtonADown(0) == true)
		{
			strcpy_s(buttonString, "A Button Down: Yes");
			result = m_ButtonStrings[0].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, buttonString, 10, yPos, 0.0f, 1.0f, 0.0f);
		}
		else
		{
			strcpy_s(buttonString, "A Button Down: No");
			result = m_ButtonStrings[0].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, buttonString, 10, yPos, 1.0f, 0.0f, 0.0f);
		}
		if(!result)
		{
			return false;
		}
		yPos += 25;

		// B button.
		if(m_XInput->IsButtonBDown(0) == true)
		{
			strcpy_s(buttonString, "B Button Down: Yes");
			result = m_ButtonStrings[1].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, buttonString, 10, yPos, 0.0f, 1.0f, 0.0f);
		}
		else
		{
			strcpy_s(buttonString, "B Button Down: No");
			result = m_ButtonStrings[1].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, buttonString, 10, yPos, 1.0f, 0.0f, 0.0f);
		}
		if(!result)
		{
			return false;
		}
		yPos += 25;
	}
	else
	{
		strcpy_s(buttonString, "A Button Down: No");
		
		result = m_ButtonStrings[0].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, buttonString, 10, yPos, 1.0f, 0.0f, 0.0f);
		if(!result)
		{
			return false;
		}
		yPos += 25;

		strcpy_s(buttonString, "B Button Down: No");
		
		result = m_ButtonStrings[1].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, buttonString, 10, yPos, 1.0f, 0.0f, 0.0f);
		if(!result)
		{
			return false;
		}
		yPos += 25;
	}
	yPos += 10;

	// Controller 1 triggers.
	if(m_XInput->IsControllerActive(0) == true)
	{
		// Left trigger.
		value = m_XInput->GetLeftTrigger(0);
		sprintf_s(triggerString, "Left Trigger: %f", value);

		result = m_TriggerStrings[0].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
		if(!result)
		{
			return false;
		}
		yPos += 25;

		// Right trigger.
		value = m_XInput->GetRightTrigger(0);
		sprintf_s(triggerString, "Right Trigger: %f", value);

		result = m_TriggerStrings[1].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
		if(!result)
		{
			return false;
		}
		yPos += 25;
	}
	else
	{
		strcpy_s(triggerString, "Left Trigger: 0.0");

		result = m_TriggerStrings[0].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
		if(!result)
		{
			return false;
		}
		yPos += 25;

		strcpy_s(triggerString, "Right Trigger: 0.0");

		result = m_TriggerStrings[1].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
		if(!result)
		{
			return false;
		}
		yPos += 25;
	}
	yPos += 10;

	// Controller 1 left thumb.
	if(m_XInput->IsControllerActive(0) == true)
	{
		m_XInput->GetLeftThumbStickLocation(0, leftX, leftY);

		// Left X.
		sprintf_s(triggerString, "Left Thumb X: %d", leftX);

		result = m_ThumbStrings[0].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
		if(!result)
		{
			return false;
		}
		yPos += 25;

		// Left Y.
		sprintf_s(triggerString, "Left Thumb Y: %d", leftY);

		result = m_ThumbStrings[1].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
		if(!result)
		{
			return false;
		}
		yPos += 25;
	}
	else
	{
		strcpy_s(triggerString, "Left Thumb X: 0");

		result = m_ThumbStrings[0].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
		if(!result)
		{
			return false;
		}
		yPos += 25;

		strcpy_s(triggerString, "Left Thumb Y: 0");

		result = m_ThumbStrings[1].UpdateText(m_Direct3D->GetDeviceContext(), m_Font, triggerString, 10, yPos, 1.0f, 1.0f, 1.0f);
		if(!result)
		{
			return false;
		}
		yPos += 25;
	}

    return true;
}