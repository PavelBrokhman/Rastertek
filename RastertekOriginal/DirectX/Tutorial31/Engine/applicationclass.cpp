////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
	m_Direct3D = 0;
	m_Camera = 0;
	m_GroundModel = 0;
	m_WallModel = 0;
	m_BathModel = 0;
	m_WaterModel = 0;
	m_Light = 0;
	m_RefractionTexture = 0;
	m_ReflectionTexture = 0;
	m_LightShader = 0;
	m_RefractionShader = 0;
	m_WaterShader = 0;
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

	// Update the position and rotation of the camera for this scene.
	m_Camera->SetPosition(-10.0f, 6.0f, -10.0f);
	m_Camera->SetRotation(0.0f, 45.0f, 0.0f);

	// Set the file names of the ground model.
    strcpy_s(modelFilename, "../Engine/data/ground.txt");
    strcpy_s(textureFilename, "../Engine/data/ground01.tga");

    // Create and initialize the ground model object.
    m_GroundModel = new ModelClass;

    result = m_GroundModel->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, textureFilename);
    if(!result)
    {
		MessageBox(hwnd, L"Could not initialize the ground model object.", L"Error", MB_OK);
        return false;
    }

	// Set the file names of the wall model.
    strcpy_s(modelFilename, "../Engine/data/wall.txt");
    strcpy_s(textureFilename, "../Engine/data/wall01.tga");

    // Create and initialize the wall model object.
    m_WallModel = new ModelClass;

    result = m_WallModel->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, textureFilename);
    if(!result)
    {
		MessageBox(hwnd, L"Could not initialize the wall model object.", L"Error", MB_OK);
        return false;
    }

	// Set the file names of the bath model.
    strcpy_s(modelFilename, "../Engine/data/bath.txt");
    strcpy_s(textureFilename, "../Engine/data/marble01.tga");

    // Create and initialize the bath model object.
    m_BathModel = new ModelClass;

    result = m_BathModel->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, textureFilename);
    if(!result)
    {
		MessageBox(hwnd, L"Could not initialize the bath model object.", L"Error", MB_OK);
        return false;
    }

	// Set the file names of the water model.
    strcpy_s(modelFilename, "../Engine/data/water.txt");
    strcpy_s(textureFilename, "../Engine/data/water01.tga");

    // Create and initialize the water model object.
    m_WaterModel = new ModelClass;

    result = m_WaterModel->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, textureFilename);
    if(!result)
    {
		MessageBox(hwnd, L"Could not initialize the water model object.", L"Error", MB_OK);
        return false;
    }

	// Create and initialize the light object.
	m_Light = new LightClass;

	m_Light->SetAmbientColor(0.15f, 0.15f, 0.15f, 1.0f);
	m_Light->SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
	m_Light->SetDirection(0.0f, -1.0f, 0.5f);

	// Create and initialize the refraction render to texture object.
	m_RefractionTexture = new RenderTextureClass;

	result = m_RefractionTexture->Initialize(m_Direct3D->GetDevice(), screenWidth, screenHeight, SCREEN_DEPTH, SCREEN_NEAR, 1);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the refraction render texture object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the reflection render to texture object.
	m_ReflectionTexture = new RenderTextureClass;

	result = m_ReflectionTexture->Initialize(m_Direct3D->GetDevice(), screenWidth, screenHeight, SCREEN_DEPTH, SCREEN_NEAR, 1);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the reflection render texture object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the light shader object.
	m_LightShader = new LightShaderClass;

	result = m_LightShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the light shader object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the refraction shader object.
	m_RefractionShader = new RefractionShaderClass;

	result = m_RefractionShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the refraction shader object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the water shader object.
	m_WaterShader = new WaterShaderClass;

	result = m_WaterShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the water shader object.", L"Error", MB_OK);
		return false;
	}

	// Set the height of the water.
	m_waterHeight = 2.75f;

	// Initialize the position of the water.
	m_waterTranslation = 0.0f;

	return true;
}


void ApplicationClass::Shutdown()
{
	// Release the water shader object.
	if(m_WaterShader)
	{
		m_WaterShader->Shutdown();
		delete m_WaterShader;
		m_WaterShader = 0;
	}

	// Release the refraction shader object.
	if(m_RefractionShader)
	{
		m_RefractionShader->Shutdown();
		delete m_RefractionShader;
		m_RefractionShader = 0;
	}

	// Release the light shader object.
	if(m_LightShader)
	{
		m_LightShader->Shutdown();
		delete m_LightShader;
		m_LightShader = 0;
	}

	// Release the reflection render texture object.
    if(m_ReflectionTexture)
    {
        m_ReflectionTexture->Shutdown();
        delete m_ReflectionTexture;
        m_ReflectionTexture = 0;
    }

	// Release the refraction render texture object.
    if(m_RefractionTexture)
    {
        m_RefractionTexture->Shutdown();
        delete m_RefractionTexture;
        m_RefractionTexture = 0;
    }

	// Release the light object.
	if(m_Light)
	{
		delete m_Light;
		m_Light = 0;
	}

	// Release the water model object.
    if(m_WaterModel)
    {
        m_WaterModel->Shutdown();
        delete m_WaterModel;
        m_WaterModel = 0;
    }

	// Release the bath model object.
    if(m_BathModel)
    {
        m_BathModel->Shutdown();
        delete m_BathModel;
        m_BathModel = 0;
    }

	// Release the wall model object.
    if(m_WallModel)
    {
        m_WallModel->Shutdown();
        delete m_WallModel;
        m_WallModel = 0;
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

	// Update the position of the water to simulate motion.
	m_waterTranslation += 0.001f;
	if(m_waterTranslation > 1.0f)
	{
		m_waterTranslation -= 1.0f;
	}

	// Render the refraction of the scene to a texture.
	result = RenderRefractionToTexture();
	if(!result)
	{
		return false;
	}

	// Render the reflection of the scene to a texture.
	result = RenderReflectionToTexture();
	if(!result)
	{
		return false;
	}

	// Render the scene as normal to the back buffer.
	result = Render();
	if(!result)
	{
		return false;
	}

	return true;
}


bool ApplicationClass::RenderRefractionToTexture()
{
	XMMATRIX worldMatrix, viewMatrix, projectionMatrix;
	XMFLOAT4 clipPlane;
	bool result;


	// Setup a clipping plane based on the height of the water to clip everything above it.
	clipPlane = XMFLOAT4(0.0f, -1.0f, 0.0f, m_waterHeight + 0.1f);

	// Set the render target to be the refraction render to texture and clear it.
	m_RefractionTexture->SetRenderTarget(m_Direct3D->GetDeviceContext());
	m_RefractionTexture->ClearRenderTarget(m_Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 1.0f);

	// Generate the view matrix based on the camera's position.
	m_Camera->Render();

	// Get the world, view, and projection matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetViewMatrix(viewMatrix);
	m_Direct3D->GetProjectionMatrix(projectionMatrix);

	// Translate to where the bath model will be rendered.
	worldMatrix = XMMatrixTranslation(0.0f, 2.0f, 0.0f);

	// Render the bath model using the refraction shader.
	m_BathModel->Render(m_Direct3D->GetDeviceContext());

	result = m_RefractionShader->Render(m_Direct3D->GetDeviceContext(), m_BathModel->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, m_BathModel->GetTexture(),
										m_Light->GetDirection(), m_Light->GetAmbientColor(), m_Light->GetDiffuseColor(), clipPlane);
	if(!result)
	{
		return false;
	}

	// Reset the render target back to the original back buffer and not the render to texture anymore.  And reset the viewport back to the original.
	m_Direct3D->SetBackBufferRenderTarget();
	m_Direct3D->ResetViewport();

	return true;
}


bool ApplicationClass::RenderReflectionToTexture()
{
	XMMATRIX worldMatrix, reflectionViewMatrix, projectionMatrix;
	bool result;


	// Set the render target to be the reflection render to texture and clear it.
	m_ReflectionTexture->SetRenderTarget(m_Direct3D->GetDeviceContext());
	m_ReflectionTexture->ClearRenderTarget(m_Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 1.0f);

	// Use the camera to render the reflection and create a reflection view matrix.
	m_Camera->RenderReflection(m_waterHeight);

	// Get the camera reflection view matrix instead of the normal view matrix.
	m_Camera->GetReflectionViewMatrix(reflectionViewMatrix);

	// Get the world and projection matrices from the d3d object.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Direct3D->GetProjectionMatrix(projectionMatrix);

	// Translate to where the wall model will be rendered.
	worldMatrix = XMMatrixTranslation(0.0f, 6.0f, 8.0f);

	// Render the wall model using the light shader and the reflection view matrix.
	m_WallModel->Render(m_Direct3D->GetDeviceContext());

	result = m_LightShader->Render(m_Direct3D->GetDeviceContext(), m_WallModel->GetIndexCount(), worldMatrix, reflectionViewMatrix, projectionMatrix, m_WallModel->GetTexture(),
								   m_Light->GetDirection(), m_Light->GetAmbientColor(), m_Light->GetDiffuseColor());
	if(!result)
	{
		return false;
	}

	// Reset the render target back to the original back buffer and not the render to texture anymore.  And reset the viewport back to the original.
	m_Direct3D->SetBackBufferRenderTarget();
	m_Direct3D->ResetViewport();

	return true;
}


bool ApplicationClass::Render()
{
	XMMATRIX worldMatrix, viewMatrix, projectionMatrix, reflectionMatrix;
	bool result;


	// Clear the buffers to begin the scene.
	m_Direct3D->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

	// Generate the view matrix based on the camera's position.
	m_Camera->Render();

	// Get the world, view, and projection matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetViewMatrix(viewMatrix);
	m_Direct3D->GetProjectionMatrix(projectionMatrix);

	// Translate to where the ground model will be rendered.
	worldMatrix = XMMatrixTranslation(0.0f, 1.0f, 0.0f);

	// Put the ground model vertex and index buffers on the graphics pipeline to prepare them for drawing.
	m_GroundModel->Render(m_Direct3D->GetDeviceContext());

	// Render the ground model using the light shader.
	result = m_LightShader->Render(m_Direct3D->GetDeviceContext(), m_GroundModel->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, m_GroundModel->GetTexture(),
								   m_Light->GetDirection(), m_Light->GetAmbientColor(), m_Light->GetDiffuseColor());
	if(!result)
	{
		return false;
	}

	// Reset the world matrix.
	m_Direct3D->GetWorldMatrix(worldMatrix);

	// Translate to where the wall model will be rendered.
	worldMatrix = XMMatrixTranslation(0.0f, 6.0f, 8.0f);

	// Put the wall model vertex and index buffers on the graphics pipeline to prepare them for drawing.
	m_WallModel->Render(m_Direct3D->GetDeviceContext());

	// Render the wall model using the light shader.
	result = m_LightShader->Render(m_Direct3D->GetDeviceContext(), m_WallModel->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, m_WallModel->GetTexture(),
								   m_Light->GetDirection(), m_Light->GetAmbientColor(), m_Light->GetDiffuseColor());
	if(!result)
	{
		return false;
	}

	// Reset the world matrix.
	m_Direct3D->GetWorldMatrix(worldMatrix);

	// Translate to where the bath model will be rendered.
	worldMatrix = XMMatrixTranslation(0.0f, 2.0f, 0.0f);

	// Put the bath model vertex and index buffers on the graphics pipeline to prepare them for drawing.
	m_BathModel->Render(m_Direct3D->GetDeviceContext());

	// Render the bath model using the light shader.
	result = m_LightShader->Render(m_Direct3D->GetDeviceContext(), m_BathModel->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, m_BathModel->GetTexture(),
								   m_Light->GetDirection(), m_Light->GetAmbientColor(), m_Light->GetDiffuseColor());
	if(!result)
	{
		return false;
	}

	// Reset the world matrix.
	m_Direct3D->GetWorldMatrix(worldMatrix);

	// Get the camera reflection view matrix.
	m_Camera->GetReflectionViewMatrix(reflectionMatrix);

	// Translate to where the water model will be rendered.
	worldMatrix = XMMatrixTranslation(0.0f, m_waterHeight, 0.0f);

	// Put the water model vertex and index buffers on the graphics pipeline to prepare them for drawing.
	m_WaterModel->Render(m_Direct3D->GetDeviceContext());

	// Render the water model using the water shader.
	result = m_WaterShader->Render(m_Direct3D->GetDeviceContext(), m_WaterModel->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, reflectionMatrix,
								   m_ReflectionTexture->GetShaderResourceView(), m_RefractionTexture->GetShaderResourceView(), m_WaterModel->GetTexture(),
								   m_waterTranslation, 0.01f);
	if(!result)
	{
		return false;
	}

	// Present the rendered scene to the screen.
	m_Direct3D->EndScene();

	return true;
}