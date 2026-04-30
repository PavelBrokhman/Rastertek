////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
	m_Direct3D = 0;
	m_Camera = 0;
	m_CubeModel = 0;
	m_SphereModel = 0;
	m_GroundModel = 0;
	m_Light = 0;
	m_RenderTexture = 0;
	m_DepthShader = 0;
	m_ShadowShader = 0;
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

	// Create and initialize the cube model object.
	m_CubeModel = new ModelClass;

	strcpy_s(modelFilename, "../Engine/data/cube.txt");
	strcpy_s(textureFilename, "../Engine/data/wall01.tga");

	result = m_CubeModel->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, textureFilename);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the cube model object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the sphere model object.
	m_SphereModel = new ModelClass;

	strcpy_s(modelFilename, "../Engine/data/sphere.txt");
	strcpy_s(textureFilename, "../Engine/data/ice.tga");

	result = m_SphereModel->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, textureFilename);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the sphere model object.", L"Error", MB_OK);
		return false;
	}

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

	// Create and initialize the light object.
	m_Light = new LightClass;

	m_Light->SetAmbientColor(0.15f, 0.15f, 0.15f, 1.0f);
	m_Light->SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
	m_Light->GenerateOrthoMatrix(20.0f, SHADOWMAP_NEAR, SHADOWMAP_DEPTH);

	// Create and initialize the render to texture object.
	m_RenderTexture = new RenderTextureClass;

	result = m_RenderTexture->Initialize(m_Direct3D->GetDevice(), SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT, SHADOWMAP_DEPTH, SHADOWMAP_NEAR, 1);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the render texture object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the depth shader object.
	m_DepthShader = new DepthShaderClass;

	result = m_DepthShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the depth shader object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the shadow shader object.
	m_ShadowShader = new ShadowShaderClass;

	result = m_ShadowShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the shadow shader object.", L"Error", MB_OK);
		return false;
	}

	// Set the shadow map bias to fix the floating point precision issues (shadow acne/lines artifacts).
	m_shadowMapBias = 0.0022f;

	return true;
}


void ApplicationClass::Shutdown()
{
	// Release the shadow shader object.
	if(m_ShadowShader)
	{
		m_ShadowShader->Shutdown();
		delete m_ShadowShader;
		m_ShadowShader = 0;
	}

	// Release the depth shader object.
	if(m_DepthShader)
	{
		m_DepthShader->Shutdown();
		delete m_DepthShader;
		m_DepthShader = 0;
	}

	// Release the render texture object.
	if(m_RenderTexture)
	{
		m_RenderTexture->Shutdown();
		delete m_RenderTexture;
		m_RenderTexture = 0;
	}

	// Release the light object.
	if(m_Light)
	{
		delete m_Light;
		m_Light = 0;
	}

    // Release the ground model object.
    if(m_GroundModel)
    {
        m_GroundModel->Shutdown();
        delete m_GroundModel;
        m_GroundModel = 0;
    }

	// Release the sphere model object.
	if(m_SphereModel)
	{
		m_SphereModel->Shutdown();
		delete m_SphereModel;
		m_SphereModel = 0;
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
	static float lightAngle = 270.0f;
	float radians;
	static float lightPosX = 9.0f;
	float frameTime;
	bool result;

	
	// Check if the user pressed escape and wants to exit the application.
	if(Input->IsEscapePressed())
	{
		return false;
	}

	// Set the frame time manually assumming 60 fps.
	frameTime = 10.0f;

	// Update the position of the light each frame.
	lightPosX -= 0.003f * frameTime;

	// Update the angle of the light each frame.
	lightAngle -= 0.03f * frameTime;
	if(lightAngle < 90.0f)
	{
		lightAngle = 270.0f;

		// Reset the light position also.
		lightPosX = 9.0f;
	}
	radians = lightAngle * 0.0174532925f;

	// Update the direction of the light.
	m_Light->SetDirection(sinf(radians), cosf(radians), 0.0f);

	// Set the position and lookat for the light.
	m_Light->SetPosition(lightPosX, 8.0f, -0.1f);
	m_Light->SetLookAt(-lightPosX, 0.0f, 0.0f);
	m_Light->GenerateViewMatrix();

	// Render the scene depth of the first light to the render texture.
	result = RenderDepthToTexture();
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


bool ApplicationClass::RenderDepthToTexture()
{
	XMMATRIX translateMatrix, lightViewMatrix, lightOrthoMatrix;
	bool result;


	// Set the render target to be the render to texture.  Also clear the render to texture.
	m_RenderTexture->SetRenderTarget(m_Direct3D->GetDeviceContext());
	m_RenderTexture->ClearRenderTarget(m_Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 1.0f);

	// Get the view and orthographic matrices from the light object.
	m_Light->GetViewMatrix(lightViewMatrix);
	m_Light->GetOrthoMatrix(lightOrthoMatrix);

	// Setup the translation matrix for the cube model.
	translateMatrix = XMMatrixTranslation(-2.0f, 2.0f, 0.0f);

	// Render the cube model using the depth shader.
	m_CubeModel->Render(m_Direct3D->GetDeviceContext());

	result = m_DepthShader->Render(m_Direct3D->GetDeviceContext(), m_CubeModel->GetIndexCount(), translateMatrix, lightViewMatrix, lightOrthoMatrix);
	if(!result)
	{
		return false;
	}

	// Setup the translation matrix for the sphere model.
	translateMatrix = XMMatrixTranslation(2.0f, 2.0f, 0.0f);

	// Render the sphere model using the depth shader.
	m_SphereModel->Render(m_Direct3D->GetDeviceContext());

	result = m_DepthShader->Render(m_Direct3D->GetDeviceContext(), m_SphereModel->GetIndexCount(), translateMatrix, lightViewMatrix, lightOrthoMatrix);
	if(!result)
	{
		return false;
	}

	// Setup the translation matrix for the ground model.
	translateMatrix = XMMatrixTranslation(0.0f, 1.0f, 0.0f);

	// Render the ground model using the depth shader.
	m_GroundModel->Render(m_Direct3D->GetDeviceContext());

	result = m_DepthShader->Render(m_Direct3D->GetDeviceContext(), m_GroundModel->GetIndexCount(), translateMatrix, lightViewMatrix, lightOrthoMatrix);
	if(!result)
	{
		return false;
	}

	// Reset the render target back to the original back buffer and not the render to texture anymore.  Also reset the viewport back to the original.
	m_Direct3D->SetBackBufferRenderTarget();
	m_Direct3D->ResetViewport();

	return true;
}


bool ApplicationClass::Render()
{
	XMMATRIX worldMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightOrthoMatrix;
	bool result;


	// Clear the buffers to begin the scene.
	m_Direct3D->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

	// Get the world, view, and projection matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetViewMatrix(viewMatrix);
	m_Direct3D->GetProjectionMatrix(projectionMatrix);

	// Get the view and projection matrices from the first light.
	m_Light->GetViewMatrix(lightViewMatrix);
	m_Light->GetOrthoMatrix(lightOrthoMatrix);

	// Setup the translation matrix for the cube model.
	worldMatrix = XMMatrixTranslation(-2.0f, 2.0f, 0.0f);

	// Render the cube model using the shadow shader.
	m_CubeModel->Render(m_Direct3D->GetDeviceContext());

	result = m_ShadowShader->Render(m_Direct3D->GetDeviceContext(), m_CubeModel->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightOrthoMatrix,
									m_CubeModel->GetTexture(), m_RenderTexture->GetShaderResourceView(), m_Light->GetAmbientColor(), m_Light->GetDiffuseColor(), m_Light->GetDirection(), m_shadowMapBias);
	if(!result)
	{
		return false;
	}

	// Setup the translation matrix for the sphere model.
	worldMatrix = XMMatrixTranslation(2.0f, 2.0f, 0.0f);

	// Render the sphere model using the shadow shader.
	m_SphereModel->Render(m_Direct3D->GetDeviceContext());

	result = m_ShadowShader->Render(m_Direct3D->GetDeviceContext(), m_SphereModel->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightOrthoMatrix,
									m_SphereModel->GetTexture(), m_RenderTexture->GetShaderResourceView(), m_Light->GetAmbientColor(), m_Light->GetDiffuseColor(), m_Light->GetDirection(), m_shadowMapBias);
	if(!result)
	{
		return false;
	}

	// Setup the translation matrix for the ground model.
	worldMatrix = XMMatrixTranslation(0.0f, 1.0f, 0.0f);

	// Render the ground model using the shadow shader.
	m_GroundModel->Render(m_Direct3D->GetDeviceContext());

	result = m_ShadowShader->Render(m_Direct3D->GetDeviceContext(), m_GroundModel->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightOrthoMatrix,
									m_GroundModel->GetTexture(), m_RenderTexture->GetShaderResourceView(), m_Light->GetAmbientColor(), m_Light->GetDiffuseColor(), m_Light->GetDirection(), m_shadowMapBias);
	if(!result)
	{
		return false;
	}

	// Present the rendered scene to the screen.
	m_Direct3D->EndScene();

	return true;
}