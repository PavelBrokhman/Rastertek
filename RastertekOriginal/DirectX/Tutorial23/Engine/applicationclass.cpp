////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
	m_Direct3D = 0;
	m_Camera = 0;
	m_Model = 0;
	m_Light = 0;
	m_LightShader = 0;
	m_FontShader = 0;
    m_Font = 0;
	m_RenderCountString = 0;
	m_ModelList = 0;
	m_Timer = 0;
	m_Position = 0;
	m_Frustum = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(int screenWidth, int screenHeight, HWND hwnd)
{
	char modelFilename[128], textureFilename1[128], renderString[32];
	bool result;


	// Create and initialize the Direct3D object.
	m_Direct3D = new D3DClass;

	result = m_Direct3D->Initialize(screenWidth, screenHeight, VSYNC_ENABLED, hwnd, FULL_SCREEN, SCREEN_DEPTH, SCREEN_NEAR);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize Direct3D", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the camera object.
	m_Camera = new CameraClass;

	m_Camera->SetPosition(0.0f, 0.0f, -10.0f);
	m_Camera->Render();
	m_Camera->GetViewMatrix(m_baseViewMatrix);

	// Set the file name of the model.
    strcpy_s(modelFilename, "../Engine/data/sphere.txt");

    // Set the file name of the textures.
    strcpy_s(textureFilename1, "../Engine/data/stone01.tga");

    // Create and initialize the model object.
    m_Model = new ModelClass;

    result = m_Model->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, textureFilename1);
    if(!result)
    {
        return false;
    }

	 // Create and initialize the light object.
    m_Light = new LightClass;

    m_Light->SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
    m_Light->SetDirection(0.0f, 0.0f, 1.0f);

	// Create and initialize the light shader object.
	m_LightShader = new LightShaderClass;

	result = m_LightShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		return false;
	}

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
	
	// Set the initial render count string.
    strcpy_s(renderString, "Render Count: 0");

    // Create and initialize the text object for the render count string.
    m_RenderCountString = new TextClass;

    result = m_RenderCountString->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), screenWidth, screenHeight, 32, m_Font, renderString, 10, 10, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

	// Create and initialize the model list object.
    m_ModelList = new ModelListClass;
    m_ModelList->Initialize(25);

	// Create and initialize the timer object.
    m_Timer = new TimerClass;

    result = m_Timer->Initialize();
    if(!result)
    {
        return false;
    }

	// Create the position class object.
	m_Position = new PositionClass;

	// Create the frustum class object.
    m_Frustum = new FrustumClass;

	return true;
}


void ApplicationClass::Shutdown()
{
	// Release the frustum class object.
    if(m_Frustum)
    {
        delete m_Frustum;
        m_Frustum = 0;
    }

	// Release the position object.
	if(m_Position)
	{
		delete m_Position;
		m_Position = 0;
	}

	// Release the timer object.
    if(m_Timer)
    {
        delete m_Timer;
        m_Timer = 0;
    }

	// Release the model list object.
    if(m_ModelList)
    {
        m_ModelList->Shutdown();
        delete m_ModelList;
        m_ModelList = 0;
    }

	// Release the text objects for the render count string.
    if(m_RenderCountString)
    {
        m_RenderCountString->Shutdown();
        delete m_RenderCountString;
        m_RenderCountString = 0;
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

	// Release the light shader object.
	if(m_LightShader)
	{
		m_LightShader->Shutdown();
		delete m_LightShader;
		m_LightShader = 0;
	}

	// Release the light object.
    if(m_Light)
    {
        delete m_Light;
        m_Light = 0;
    }

	// Release the model object.
    if(m_Model)
    {
        m_Model->Shutdown();
        delete m_Model;
        m_Model = 0;
    }

	// Release the camera object.
	if(m_Camera)
	{
		delete m_Camera;
		m_Camera = 0;
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
	float rotationY;
	bool result, keyDown;


	// Update the system stats.
    m_Timer->Frame();

	// Check if the user pressed escape and wants to exit the application.
	if(Input->IsEscapePressed())
	{
		return false;
	}

	// Set the frame time for calculating the updated position.
	m_Position->SetFrameTime(m_Timer->GetTime());

	// Check if the left or right arrow key has been pressed, if so rotate the camera accordingly.
    keyDown = Input->IsLeftArrowPressed();
    m_Position->TurnLeft(keyDown);

    keyDown = Input->IsRightArrowPressed();
    m_Position->TurnRight(keyDown);

	// Get the current view point rotation.
    m_Position->GetRotation(rotationY);

	// Set the rotation of the camera.
    m_Camera->SetRotation(0.0f, rotationY, 0.0f);
    m_Camera->Render();

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
	XMMATRIX worldMatrix, viewMatrix, projectionMatrix, orthoMatrix;
	float positionX, positionY, positionZ, radius;
	int modelCount, renderCount, i;
	bool renderModel, result;


	// Clear the buffers to begin the scene.
	m_Direct3D->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

	// Get the world, view, and projection matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetViewMatrix(viewMatrix);
	m_Direct3D->GetProjectionMatrix(projectionMatrix);
	m_Direct3D->GetOrthoMatrix(orthoMatrix);

	// Construct the frustum.
    m_Frustum->ConstructFrustum(viewMatrix, projectionMatrix, SCREEN_DEPTH);

	// Get the number of models that will be rendered.
    modelCount = m_ModelList->GetModelCount();

	// Initialize the count of models that have been rendered.
    renderCount = 0;

	// Go through all the models and render them only if they can be seen by the camera view.
    for(i=0; i<modelCount; i++)
    {
		// Get the position and color of the sphere model at this index.
        m_ModelList->GetData(i, positionX, positionY, positionZ);

		// Set the radius of the sphere to 1.0 since this is already known.
        radius = 1.0f;

		// Check if the sphere model is in the view frustum.
        renderModel = m_Frustum->CheckSphere(positionX, positionY, positionZ, radius);

		// If it can be seen then render it, if not skip this model and check the next sphere.
        if(renderModel)
        {
			// Move the model to the location it should be rendered at.
			worldMatrix = XMMatrixTranslation(positionX, positionY, positionZ);

			// Render the model using the light shader.
			m_Model->Render(m_Direct3D->GetDeviceContext());

			result = m_LightShader->Render(m_Direct3D->GetDeviceContext(), m_Model->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix,
										   m_Model->GetTexture(), m_Light->GetDirection(), m_Light->GetDiffuseColor());
			if(!result)
			{
				return false;
			}

			// Since this model was rendered then increase the count for this frame.
            renderCount++;
		}
	}

	// Update the render count text.
    result = UpdateRenderCountString(renderCount);
    if(!result)
    {
        return false;
    }

	 // Disable the Z buffer and enable alpha blending for 2D rendering.
    m_Direct3D->TurnZBufferOff();
    m_Direct3D->EnableAlphaBlending();

	// Reset the world matrix.
	m_Direct3D->GetWorldMatrix(worldMatrix);

	// Render the render count text string using the font shader.
	m_RenderCountString->Render(m_Direct3D->GetDeviceContext());

	result = m_FontShader->Render(m_Direct3D->GetDeviceContext(), m_RenderCountString->GetIndexCount(), worldMatrix, m_baseViewMatrix, orthoMatrix, 
								  m_Font->GetTexture(), m_RenderCountString->GetPixelColor());
	if(!result)
	{
		return false;
	}	

	// Enable the Z buffer and disable alpha blending now that 2D rendering is complete.
    m_Direct3D->TurnZBufferOn();
    m_Direct3D->DisableAlphaBlending();

	// Present the rendered scene to the screen.
	m_Direct3D->EndScene();

	return true;
}


bool ApplicationClass::UpdateRenderCountString(int renderCount)
{
    char tempString[16], finalString[32];
    bool result;


    // Convert the render count integer to string format.
    sprintf_s(tempString, "%d", renderCount);

    // Setup the render count string.
    strcpy_s(finalString, "Render Count: ");
    strcat_s(finalString, tempString);

    // Update the sentence vertex buffer with the new string information.
    result = m_RenderCountString->UpdateText(m_Direct3D->GetDeviceContext(), m_Font, finalString, 10, 10, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

    return true;
}