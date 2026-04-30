////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
	m_Direct3D = 0;
	m_Camera = 0;
	m_TextureShader = 0;
    m_FloorModel = 0;
    m_BillboardModel = 0;
    m_Position = 0;
    m_Timer = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(int screenWidth, int screenHeight, HWND hwnd)
{
	char modelFilename[128], textureFilename[128];
	bool result;


	// Create and initialize the Direct3D object.
	m_Direct3D = new D3DClass;

	result = m_Direct3D->Initialize(screenWidth, screenHeight, VSYNC_ENABLED, hwnd, FULL_SCREEN, SCREEN_DEPTH, SCREEN_NEAR);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize Direct3D.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the camera object.
	m_Camera = new CameraClass;

	m_Camera->SetPosition(0.0f, 0.0f, -5.0f);
	m_Camera->Render();

	// Create and initialize the texture shader object.
	m_TextureShader  = new TextureShaderClass;

	result = m_TextureShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the texture shader object.", L"Error", MB_OK);
		return false;
	}

	// Set the filenames for the floor model object.
    strcpy_s(modelFilename, "../Engine/data/floor.txt");
    strcpy_s(textureFilename, "../Engine/data/grid01.tga");

    // Create and initialize the floor model object.
    m_FloorModel  = new ModelClass;

    result = m_FloorModel ->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, textureFilename);
    if(!result)
    {
		MessageBox(hwnd, L"Could not initialize the floor model object.", L"Error", MB_OK);
        return false;
    }

	// Set the filenames for the billboard model object.
    strcpy_s(modelFilename, "../Engine/data/square.txt");
    strcpy_s(textureFilename, "../Engine/data/stone01.tga");

    // Create and initialize the billboard model object.
    m_BillboardModel = new ModelClass;

    result = m_BillboardModel->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, textureFilename);
    if(!result)
    {
		MessageBox(hwnd, L"Could not initialize the billboard model object.", L"Error", MB_OK);
        return false;
    }

	// Create the position object and set the initial viewing position.
    m_Position = new PositionClass;
    m_Position->SetPosition(0.0f, 1.5f, -11.0f);

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

    // Release the position object.
    if(m_Position)
    {
        delete m_Position;
        m_Position = 0;
    }

	// Release the billboard model object.
    if(m_BillboardModel)
    {
        m_BillboardModel->Shutdown();
        delete m_BillboardModel;
        m_BillboardModel = 0;
    }

    // Release the floor model object.
    if(m_FloorModel)
    {
        m_FloorModel->Shutdown();
        delete m_FloorModel;
        m_FloorModel = 0;
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
	float positionX, positionY, positionZ;
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

	// Check if the user is pressing the left or right arrow keys and update the position object accordingly.
    keyDown = Input->IsLeftArrowPressed();
    m_Position->MoveLeft(keyDown);

    keyDown = Input->IsRightArrowPressed();
    m_Position->MoveRight(keyDown);

    // Get the current view point position
    m_Position->GetPosition(positionX, positionY, positionZ);

	// Set the position of the camera.
    m_Camera->SetPosition(positionX, positionY, positionZ);
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
	XMMATRIX worldMatrix, viewMatrix, projectionMatrix, translateMatrix;
	XMFLOAT3 cameraPosition, modelPosition;
	double angle;
	float pi, rotation;
	bool result;


	// Clear the buffers to begin the scene.
	m_Direct3D->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

	// Get the world, view, and projection matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetViewMatrix(viewMatrix);
	m_Direct3D->GetProjectionMatrix(projectionMatrix);

	// Put the floor model vertex and index buffers on the graphics pipeline to prepare them for drawing.
	m_FloorModel->Render(m_Direct3D->GetDeviceContext());

	result = m_TextureShader->Render(m_Direct3D->GetDeviceContext(), m_FloorModel->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, m_FloorModel->GetTexture());
	if(!result)
	{
		return false;
	}

	// Get the position of the camera.
    cameraPosition = m_Camera->GetPosition();

    // Set the position of the billboard model.
    modelPosition.x = 0.0f;
    modelPosition.y = 1.5f;
    modelPosition.z = 0.0f;

	// Calculate the rotation angle that needs to be applied to the billboard model to face the current camera position using the arc tangent function.
    pi = 3.14159265358979323846f;
    angle = atan2(modelPosition.x - cameraPosition.x, modelPosition.y - cameraPosition.z) * (180.0f / pi);

	// Convert rotation angle into radians.
    rotation = (float)angle * 0.0174532925f;

	// Setup the rotation the billboard at the origin using the world matrix.
	worldMatrix = XMMatrixRotationY(rotation);

	// Setup the translation matrix from the billboard model.
	translateMatrix = XMMatrixTranslation(modelPosition.x, modelPosition.y, modelPosition.z);

	// Finally combine the rotation and translation matrices to create the final world matrix for the billboard model.
	worldMatrix = XMMatrixMultiply(worldMatrix, translateMatrix);

	// Put the floor model vertex and index buffers on the graphics pipeline to prepare them for drawing.
	m_BillboardModel->Render(m_Direct3D->GetDeviceContext());

	result = m_TextureShader->Render(m_Direct3D->GetDeviceContext(), m_BillboardModel->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, m_BillboardModel->GetTexture());
	if(!result)
	{
		return false;
	}

	// Present the rendered scene to the screen.
	m_Direct3D->EndScene();

	return true;
}