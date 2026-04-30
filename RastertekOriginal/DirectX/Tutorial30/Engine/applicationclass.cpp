////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
	m_Direct3D = 0;
	m_Camera = 0;
	m_CubeModel = 0;
	m_FloorModel = 0;
	m_RenderTexture = 0;
	m_TextureShader = 0;
	m_ReflectionShader = 0;
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

	m_Camera->SetPosition(0.0f, 0.0f, -10.0f);
	m_Camera->Render();

	// Set the file name of the cube model.
    strcpy_s(modelFilename, "../Engine/data/cube.txt");

    // Set the file name of the texture.
    strcpy_s(textureFilename, "../Engine/data/stone01.tga");

    // Create and initialize the cube model object.
    m_CubeModel = new ModelClass;

    result = m_CubeModel->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, textureFilename);
    if(!result)
    {
		MessageBox(hwnd, L"Could not initialize the cube model object.", L"Error", MB_OK);
        return false;
    }

	// Set the file name of the floor model.
    strcpy_s(modelFilename, "../Engine/data/floor.txt");

    // Set the file name of the texture.
    strcpy_s(textureFilename, "../Engine/data/blue01.tga");

    // Create and initialize the floor model object.
    m_FloorModel  = new ModelClass;

    result = m_FloorModel ->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, textureFilename);
    if(!result)
    {
		MessageBox(hwnd, L"Could not initialize the floor model object.", L"Error", MB_OK);
        return false;
    }

	// Create and initialize the render to texture object.
	m_RenderTexture = new RenderTextureClass;

	result = m_RenderTexture->Initialize(m_Direct3D->GetDevice(), screenWidth, screenHeight, SCREEN_DEPTH, SCREEN_NEAR, 1);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the render texture object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the texture shader object.
	m_TextureShader = new TextureShaderClass;

	result = m_TextureShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the texture shader object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the reflection shader object.
	m_ReflectionShader = new ReflectionShaderClass;

	result = m_ReflectionShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the reflection shader object.", L"Error", MB_OK);
		return false;
	}

	return true;
}


void ApplicationClass::Shutdown()
{
	// Release the reflection shader object.
	if(m_ReflectionShader)
	{
		m_ReflectionShader->Shutdown();
		delete m_ReflectionShader;
		m_ReflectionShader = 0;
	}

	// Release the texture shader object.
	if(m_TextureShader)
	{
		m_TextureShader->Shutdown();
		delete m_TextureShader;
		m_TextureShader = 0;
	}

	// Release the render texture object.
    if(m_RenderTexture)
    {
        m_RenderTexture->Shutdown();
        delete m_RenderTexture;
        m_RenderTexture = 0;
    }

	// Release the floor model object.
    if(m_FloorModel)
    {
        m_FloorModel->Shutdown();
        delete m_FloorModel;
        m_FloorModel = 0;
    }

	// Release the cube model object.
    if(m_CubeModel)
    {
        m_CubeModel->Shutdown();
        delete m_CubeModel;
        m_CubeModel = 0;
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
	static float rotation = 0.0f;
	bool result;


	// Check if the user pressed escape and wants to exit the application.
	if(Input->IsEscapePressed())
	{
		return false;
	}

	// Update the rotation variable each frame.
    rotation -= 0.0174532925f * 0.25f;
    if(rotation < 0.0f)
    {
        rotation += 360.0f;
    }

	// Render the entire scene as a reflection to the texture first.
    result = RenderReflectionToTexture(rotation);
    if(!result)
    {
        return false;
    }

	// Render the final graphics scene to the back buffer.
	result = Render(rotation);
	if(!result)
	{
		return false;
	}

	return true;
}


bool ApplicationClass::RenderReflectionToTexture(float rotation)
{
	XMMATRIX worldMatrix, reflectionViewMatrix, projectionMatrix;
	bool result;


	// Set the render target to be the render to texture and clear it.
	m_RenderTexture->SetRenderTarget(m_Direct3D->GetDeviceContext());
	m_RenderTexture->ClearRenderTarget(m_Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 1.0f);

	// Use the camera to calculate the reflection matrix.
	m_Camera->RenderReflection(-1.5f);

	// Get the camera reflection view matrix instead of the normal view matrix.
	m_Camera->GetReflectionViewMatrix(reflectionViewMatrix);

	// Get the world and projection matrices.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Direct3D->GetProjectionMatrix(projectionMatrix);

	// Rotate the world matrix by the rotation value so that the cube will spin.
    worldMatrix = XMMatrixRotationY(rotation);

	// Render the cube model using the texture shader and the reflection view matrix.
	m_CubeModel->Render(m_Direct3D->GetDeviceContext());

	result = m_TextureShader->Render(m_Direct3D->GetDeviceContext(), m_CubeModel->GetIndexCount(), worldMatrix, reflectionViewMatrix, projectionMatrix, m_CubeModel->GetTexture());
	if(!result)
	{
		return false;
	}

	// Reset the render target back to the original back buffer and not the render to texture anymore.  And reset the viewport back to the original.
	m_Direct3D->SetBackBufferRenderTarget();
	m_Direct3D->ResetViewport();

	return true;
}


bool ApplicationClass::Render(float rotation)
{
	XMMATRIX worldMatrix, viewMatrix, projectionMatrix, reflectionViewMatrix;
	bool result;


	// Clear the buffers to begin the scene.
	m_Direct3D->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

	// Get the world, view, and projection matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetViewMatrix(viewMatrix);
	m_Direct3D->GetProjectionMatrix(projectionMatrix);

	// Rotate the world matrix by the rotation value so that the cube will spin.
    worldMatrix = XMMatrixRotationY(rotation);

	// Render the cube model using the texture shader and the regular view matrix.
	m_CubeModel->Render(m_Direct3D->GetDeviceContext());

	result = m_TextureShader->Render(m_Direct3D->GetDeviceContext(), m_CubeModel->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, m_CubeModel->GetTexture());
	if(!result)
	{
		return false;
	}

	// Now get the world matrix again and translate down for the floor model to render underneath the cube.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	worldMatrix = XMMatrixTranslation(0.0f, -1.5f, 0.0f); 

	// Get the camera reflection view matrix for the reflection shader.
    m_Camera->GetReflectionViewMatrix(reflectionViewMatrix);

	// Render the floor model using the reflection shader, reflection render texture, and reflection view matrix.
	m_FloorModel->Render(m_Direct3D->GetDeviceContext());

	result = m_ReflectionShader->Render(m_Direct3D->GetDeviceContext(), m_FloorModel->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, m_FloorModel->GetTexture(), m_RenderTexture->GetShaderResourceView(), reflectionViewMatrix);
	if(!result)
	{
		return false;
	}

	// Present the rendered scene to the screen.
	m_Direct3D->EndScene();

	return true;
}