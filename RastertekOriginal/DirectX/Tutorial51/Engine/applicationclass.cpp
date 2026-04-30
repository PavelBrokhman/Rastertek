////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
	m_Direct3D = 0;
	m_Camera = 0;
	m_Light = 0;
	m_SphereModel = 0;
	m_GroundModel = 0;
	m_DeferredBuffers = 0;
	m_GBufferShader = 0;
	m_SsaoRenderTexture = 0;
	m_FullScreenWindow = 0;
	m_SsaoShader = 0;
	m_RandomTexture = 0;
	m_BlurSsaoRenderTexture = 0;
	m_SsaoBlurShader = 0;
	m_LightShader = 0;
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


	// Store the screen width and height.
	m_screenWidth = screenWidth;
	m_screenHeight = screenHeight;

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
	m_Camera->RenderBaseViewMatrix();

	m_Camera->SetPosition(0.0f, 7.0f, -10.0f);
    m_Camera->SetRotation(35.0f, 0.0f, 0.0f);
	m_Camera->Render();

	// Create and initialize the light object.
	m_Light = new LightClass;

	m_Light->SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
	m_Light->SetDirection(1.0f, -0.5f, 0.0f);

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

	// Create and initialize the deferred buffers object.
	m_DeferredBuffers = new DeferredBuffersClass;

	result = m_DeferredBuffers->Initialize(m_Direct3D->GetDevice(), screenWidth, screenHeight, SCREEN_DEPTH, SCREEN_NEAR);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the deferred buffers object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the gbuffer shader object.
	m_GBufferShader = new GBufferShaderClass;

	result = m_GBufferShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the gbuffer shader object.", L"Error", MB_OK);
		return false;
	}

	// Create the ssao render to texture object.
	m_SsaoRenderTexture = new RenderTextureClass;

	result = m_SsaoRenderTexture->Initialize(m_Direct3D->GetDevice(), screenWidth, screenHeight, SCREEN_DEPTH, SCREEN_NEAR, 2);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the ssao render texture object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the blur ssao render to texture object.
	m_BlurSsaoRenderTexture = new RenderTextureClass;

	result = m_BlurSsaoRenderTexture->Initialize(m_Direct3D->GetDevice(), screenWidth, screenHeight, SCREEN_DEPTH, SCREEN_NEAR, 2);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the blur ssao render texture object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the full screen ortho window object.
    m_FullScreenWindow = new OrthoWindowClass;

    result = m_FullScreenWindow->Initialize(m_Direct3D->GetDevice(), screenWidth, screenHeight);
    if(!result)
    {
        MessageBox(hwnd, L"Could not initialize the full screen ortho window object.", L"Error", MB_OK);
        return false;
    }

	// Create and initialize the ssao shader object.
	m_SsaoShader = new SsaoShaderClass;

	result = m_SsaoShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the ssao shader object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the random texture object.
	m_RandomTexture = new TextureClass;

	strcpy_s(textureFilename, "../Engine/data/random_vec.tga");

	result = m_RandomTexture->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), textureFilename);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the random texture object.", L"Error", MB_OK);
		return false;
	}

	// Create the ssao blur shader object.
	m_SsaoBlurShader = new SsaoBlurShaderClass;

	result = m_SsaoBlurShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the ssao blur shader object.", L"Error", MB_OK);
		return false;
	}

	// Create the deferred ssao light shader object.
	m_LightShader = new LightShaderClass;

	result = m_LightShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the light shader object.", L"Error", MB_OK);
		return false;
	}

	return true;
}


void ApplicationClass::Shutdown()
{
	// Release the deferred ssao light shader object.
	if(m_LightShader)
	{
		m_LightShader->Shutdown();
		delete m_LightShader;
		m_LightShader = 0;
	}

	// Release the ssao blur shader object.
	if(m_SsaoBlurShader)
	{
		m_SsaoBlurShader->Shutdown();
		delete m_SsaoBlurShader;
		m_SsaoBlurShader = 0;
	}

	// Release the random texture object.
	if(m_RandomTexture)
	{
		m_RandomTexture->Shutdown();
		delete m_RandomTexture;
		m_RandomTexture = 0;
	}

	// Release the ssao shader object.
	if(m_SsaoShader)
	{
		m_SsaoShader->Shutdown();
		delete m_SsaoShader;
		m_SsaoShader = 0;
	}

	// Release the full screen ortho window object.
    if(m_FullScreenWindow)
    {
        m_FullScreenWindow->Shutdown();
        delete m_FullScreenWindow;
        m_FullScreenWindow = 0;
    }

	// Release the blur ssao render to texture object.
	if(m_BlurSsaoRenderTexture)
	{
		m_BlurSsaoRenderTexture->Shutdown();
		delete m_BlurSsaoRenderTexture;
		m_BlurSsaoRenderTexture = 0;
	}

	// Release the ssao render to texture object.
	if(m_SsaoRenderTexture)
	{
		m_SsaoRenderTexture->Shutdown();
		delete m_SsaoRenderTexture;
		m_SsaoRenderTexture = 0;
	}

	// Release the gbuffer shader object.
	if(m_GBufferShader)
	{
		m_GBufferShader->Shutdown();
		delete m_GBufferShader;
		m_GBufferShader = 0;
	}

	// Release the deferred buffers object.
	if(m_DeferredBuffers)
	{
		m_DeferredBuffers->Shutdown();
		delete m_DeferredBuffers;
		m_DeferredBuffers = 0;
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

	// Release the light object.
	if(m_Light)
	{
		delete m_Light;
		m_Light = 0;
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
	

	// Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

	// Render the scene data to the G buffer to setup deferred rendering.
	result = RenderGBuffer();
    if(!result)
    {
        return false;
    }

	// Render the screen space ambient occlusion of the scene to a render texture.
	result = RenderSsao();
    if(!result)
    {
        return false;
    }

	// Blur the ssao render texture.
	result = BlurSsaoTexture();
    if(!result)
    {
        return false;
    }

    // Render the final graphics scene.
    result = Render();
    if(!result)
    {
        return false;
    }

	return true;
}


bool ApplicationClass::RenderGBuffer()
{
	XMMATRIX translateMatrix, viewMatrix, projectionMatrix;
	bool result;


	// Get the matrices from the camera and d3d objects.
	m_Camera->GetViewMatrix(viewMatrix);
	m_Direct3D->GetProjectionMatrix(projectionMatrix);

	// Set the render buffers to be the render target.
	m_DeferredBuffers->SetRenderTargets(m_Direct3D->GetDeviceContext());
	m_DeferredBuffers->ClearRenderTargets(m_Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 0.0f);
	
	// Setup the translation matrix for the sphere model.
    translateMatrix = XMMatrixTranslation(2.0f, 2.0f, 0.0f);

    // Render the sphere model using the gbuffer shader.
    m_SphereModel->Render(m_Direct3D->GetDeviceContext());

	result = m_GBufferShader->Render(m_Direct3D->GetDeviceContext(), m_SphereModel->GetIndexCount(), translateMatrix, viewMatrix, projectionMatrix, m_SphereModel->GetTexture());
	if(!result)
	{
		return false;
	}

	 // Setup the translation matrix for the ground model.
    translateMatrix = XMMatrixTranslation(0.0f, 1.0f, 0.0f);

    // Render the ground model using the gbuffer shader.
    m_GroundModel->Render(m_Direct3D->GetDeviceContext());

	result = m_GBufferShader->Render(m_Direct3D->GetDeviceContext(), m_GroundModel->GetIndexCount(), translateMatrix, viewMatrix, projectionMatrix, m_GroundModel->GetTexture());
	if(!result)
	{
		return false;
	}

	// Reset the render target back to the original back buffer and not the render to texture anymore.  Also reset the viewport back to the original.
	m_Direct3D->SetBackBufferRenderTarget();
	m_Direct3D->ResetViewport();

	return true;
}


bool ApplicationClass::RenderSsao()
{
	XMMATRIX worldMatrix, baseViewMatrix, orthoMatrix;
	float sampleRadius, ssaoScale, ssaoBias, ssaoIntensity, randomTextureSize, screenWidth, screenHeight;
	bool result;


	// Set the sample radius for the ssao shader.
	sampleRadius = 1.0f;
	ssaoScale = 1.0f;
	ssaoBias = 0.1f;
	ssaoIntensity = 2.0f;

	// Set the random texture width in float for the ssao shader.
	randomTextureSize = 64.0f;

	// Convert the screen size to float for the shader.
	screenWidth = (float)m_screenWidth;
	screenHeight = (float)m_screenHeight;

	// Get the matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetBaseViewMatrix(baseViewMatrix);
	m_Direct3D->GetOrthoMatrix(orthoMatrix);

	// Set the render target to be the ssao texture.
	m_SsaoRenderTexture->SetRenderTarget(m_Direct3D->GetDeviceContext());
	m_SsaoRenderTexture->ClearRenderTarget(m_Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 0.0f);

	// Begin 2D rendering.
	m_Direct3D->TurnZBufferOff();

	// Render the ssao effect to a 2D full screen window.
	m_FullScreenWindow->Render(m_Direct3D->GetDeviceContext());

	result = m_SsaoShader->Render(m_Direct3D->GetDeviceContext(), m_FullScreenWindow->GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix, m_DeferredBuffers->GetShaderResourcePositions(),
								  m_DeferredBuffers->GetShaderResourceNormals(), m_RandomTexture->GetTexture(), screenWidth, screenHeight, randomTextureSize, sampleRadius, ssaoScale, ssaoBias, ssaoIntensity);
	if(!result)
	{
		return false;
	}

	// End 2D rendering.
	m_Direct3D->TurnZBufferOn();

	// Reset the render target back to the original back buffer and not the render to texture anymore.  Also reset the viewport back to the original.
	m_Direct3D->SetBackBufferRenderTarget();
	m_Direct3D->ResetViewport();

	return true;
}


bool ApplicationClass::BlurSsaoTexture()
{
	XMMATRIX worldMatrix, baseViewMatrix, orthoMatrix;
	bool result;


	// Get the matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetBaseViewMatrix(baseViewMatrix);
	m_Direct3D->GetOrthoMatrix(orthoMatrix);

	// Begin 2D rendering.
	m_Direct3D->TurnZBufferOff();

	// Set the blur render texture as the render target, and clear it.
	m_BlurSsaoRenderTexture->SetRenderTarget(m_Direct3D->GetDeviceContext());
	m_BlurSsaoRenderTexture->ClearRenderTarget(m_Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 1.0f);

	// Perform a horizontal blur of the ssao texture.
	m_FullScreenWindow->Render(m_Direct3D->GetDeviceContext());

	result = m_SsaoBlurShader->Render(m_Direct3D->GetDeviceContext(), m_FullScreenWindow->GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix, m_SsaoRenderTexture->GetShaderResourceView(),
									  m_DeferredBuffers->GetShaderResourceNormals(), m_screenWidth, m_screenHeight, 0);
	if(!result)
	{
		return false;
	}

	// Set the original ssao render texture as the render target, and clear it.
	m_SsaoRenderTexture->SetRenderTarget(m_Direct3D->GetDeviceContext());
	m_SsaoRenderTexture->ClearRenderTarget(m_Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 1.0f);

	// Now perform a vertical blur of the ssao texture that was already horizontally blurred.
	m_FullScreenWindow->Render(m_Direct3D->GetDeviceContext());

	result = m_SsaoBlurShader->Render(m_Direct3D->GetDeviceContext(), m_FullScreenWindow->GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix, m_BlurSsaoRenderTexture->GetShaderResourceView(), 
									  m_DeferredBuffers->GetShaderResourceNormals(), m_screenWidth, m_screenHeight, 1);
	if(!result)
	{
		return false;
	}

	// End 2D rendering.
	m_Direct3D->TurnZBufferOn();

	// Reset the render target back to the original back buffer and not the render to texture anymore.  Also reset the viewport back to the original.
	m_Direct3D->SetBackBufferRenderTarget();
	m_Direct3D->ResetViewport();

	return true;
}


bool ApplicationClass::Render()
{
	XMMATRIX worldMatrix, viewMatrix, baseViewMatrix, orthoMatrix;
	bool result;


	// Get the matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetViewMatrix(viewMatrix);
	m_Camera->GetBaseViewMatrix(baseViewMatrix);
	m_Direct3D->GetOrthoMatrix(orthoMatrix);

	// Clear the scene.
	m_Direct3D->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

	// Begin 2D rendering.
	m_Direct3D->TurnZBufferOff();

	// Render the scene using the deferred light shader and the blurred ssao texture.
	m_FullScreenWindow->Render(m_Direct3D->GetDeviceContext());

	result = m_LightShader->Render(m_Direct3D->GetDeviceContext(), m_FullScreenWindow->GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix, m_Light->GetDirection(), m_Light->GetAmbientColor(),
								   m_Camera->GetPosition(), m_DeferredBuffers->GetShaderResourceNormals(), m_SsaoRenderTexture->GetShaderResourceView(), viewMatrix, m_DeferredBuffers->GetShaderResourceColors());
	if(!result)
	{
		return false;
	}

	// End 2D rendering.
	m_Direct3D->TurnZBufferOn();

	// Present the rendered scene to the screen.
	m_Direct3D->EndScene();

	return true;
}