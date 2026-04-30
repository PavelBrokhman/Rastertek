////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
	m_Direct3D = 0;
	m_Camera = 0;
    m_GroundModel = 0;
    m_CubeModel = 0;
    m_ProjectionShader = 0;
	m_ProjectionTexture = 0;
	m_ViewPoint = 0;
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

	m_Camera->SetPosition(0.0f, 7.0f, -10.0f);
	m_Camera->SetRotation(35.0f, 0.0f, 0.0f);
	m_Camera->Render();

    // Create and initialize the ground model object.
    m_GroundModel = new ModelClass;

	strcpy_s(modelFilename, "../Engine/data/plane01.txt");
    strcpy_s(textureFilename, "../Engine/data/metal001.tga");

    result = m_GroundModel->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, textureFilename);
    if(!result)
    {
		MessageBox(hwnd, L"Could not initialize the ground model object.", L"Error", MB_OK);
        return false;
    }

    // Create and initialize the cube model object.
    m_CubeModel = new ModelClass;

    strcpy_s(modelFilename, "../Engine/data/cube.txt");
    strcpy_s(textureFilename, "../Engine/data/stone01.tga");

    result = m_CubeModel->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, textureFilename);
    if (!result)
    {
        MessageBox(hwnd, L"Could not initialize the cube model object.", L"Error", MB_OK);
        return false;
    }

	// Create and initialize the projection shader object.
    m_ProjectionShader = new ProjectionShaderClass;

	result = m_ProjectionShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the projection shader object.", L"Error", MB_OK);
		return false;
	}

	// Create the projection texture object.
	m_ProjectionTexture = new TextureClass;

	strcpy_s(textureFilename, "../Engine/data/directx_logo.tga");

	result = m_ProjectionTexture->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), textureFilename);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the projection texture object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the view point object.
	m_ViewPoint = new ViewPointClass;

	m_ViewPoint->SetPosition(2.0f, 5.0f, -2.0f);
	m_ViewPoint->SetLookAt(0.0f, 0.0f, 0.0f);
	m_ViewPoint->SetProjectionParameters((3.14159265358979323846f / 2.0f), 1.0f, 0.1f, 100.0f);
	m_ViewPoint->GenerateViewMatrix();
	m_ViewPoint->GenerateProjectionMatrix();

	return true;
}


void ApplicationClass::Shutdown()
{
	// Release the view point object.
	if(m_ViewPoint)
	{
		delete m_ViewPoint;
		m_ViewPoint = 0;
	}

	// Release the projection texture object.
	if(m_ProjectionTexture)
	{
		m_ProjectionTexture->Shutdown();
		delete m_ProjectionTexture;
		m_ProjectionTexture = 0;
	}

	// Release the projection shader object.
    if(m_ProjectionShader)
    {
        m_ProjectionShader->Shutdown();
        delete m_ProjectionShader;
        m_ProjectionShader = 0;
    }

	// Release the cube model object.
    if(m_CubeModel)
    {
        m_CubeModel->Shutdown();
        delete m_CubeModel;
        m_CubeModel = 0;
    }

    // Release the ground model object.
    if(m_GroundModel)
    {
        m_GroundModel->Shutdown();
        delete m_GroundModel;
        m_GroundModel = 0;
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
	bool result;

	
	// Check if the user pressed escape and wants to exit the application.
	if(Input->IsEscapePressed())
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
	XMMATRIX worldMatrix, viewMatrix, projectionMatrix, viewMatrix2, projectionMatrix2;
	bool result;


	// Clear the buffers to begin the scene.
	m_Direct3D->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

	// Get the world, view, and projection matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetViewMatrix(viewMatrix);
	m_Direct3D->GetProjectionMatrix(projectionMatrix);

	// Get the view and projection matrices from the view point object.
	m_ViewPoint->GetViewMatrix(viewMatrix2);
	m_ViewPoint->GetProjectionMatrix(projectionMatrix2);

	// Setup the translation for the ground model.
	worldMatrix = XMMatrixTranslation(0.0f, 1.0f, 0.0f);

    // Render the ground model using the projection shader..
	m_GroundModel->Render(m_Direct3D->GetDeviceContext());

    result = m_ProjectionShader->Render(m_Direct3D->GetDeviceContext(), m_GroundModel->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, viewMatrix2, projectionMatrix2, m_GroundModel->GetTexture(), m_ProjectionTexture->GetTexture());
	if(!result)
	{
		return false;
	}

	// Setup the translation for the cube model.
	worldMatrix = XMMatrixTranslation(0.0f, 2.0f, 0.0f);

	// Render the ground model using the projection shader..
	m_CubeModel->Render(m_Direct3D->GetDeviceContext());

	result = m_ProjectionShader->Render(m_Direct3D->GetDeviceContext(), m_CubeModel->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, viewMatrix2, projectionMatrix2, m_CubeModel->GetTexture(), m_ProjectionTexture->GetTexture());
	if(!result)
	{
		return false;
	}

	// Present the rendered scene to the screen.
	m_Direct3D->EndScene();

	return true;
}