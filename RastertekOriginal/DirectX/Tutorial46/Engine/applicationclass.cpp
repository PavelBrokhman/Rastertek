////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
	m_Direct3D = 0;
	m_Camera = 0;
    m_Model = 0;
    m_RenderTexture = 0;
    m_GlowTexture = 0;
    m_FullScreenWindow = 0;
    m_TextureShader = 0;
    m_BlurShader = 0;
    m_Blur = 0;
    m_GlowShader = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(int screenWidth, int screenHeight, HWND hwnd)
{
	char modelFilename[128], textureFilename[128], glowMapFilename[128];
	int downSampleWidth, downSampleHeight;
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
	m_Camera->RenderBaseViewMatrix();

    // Create and initialize the cube model object.
    m_Model = new ModelClass;

	strcpy_s(modelFilename, "../Engine/data/cube.txt");
    strcpy_s(textureFilename, "../Engine/data/stone01.tga");
    strcpy_s(glowMapFilename, "../Engine/data/glowmap001.tga");

    result = m_Model->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, textureFilename, glowMapFilename);
    if(!result)
    {
		MessageBox(hwnd, L"Could not initialize the cube model object.", L"Error", MB_OK);
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

	// Create and initialize the glow render to texture object.
	m_GlowTexture = new RenderTextureClass;

	result = m_GlowTexture->Initialize(m_Direct3D->GetDevice(), screenWidth, screenHeight, SCREEN_DEPTH, SCREEN_NEAR, 1);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the glow render texture object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the full screen ortho window object.
    m_FullScreenWindow = new OrthoWindowClass;

    result = m_FullScreenWindow->Initialize(m_Direct3D->GetDevice(), screenWidth, screenHeight);
    if(!result)
    {
        MessageBox(hwnd, L"Could not initialize the full screen window object.", L"Error", MB_OK);
        return false;
    }

	// Create and initialize the texture shader object.
	m_TextureShader  = new TextureShaderClass;

	result = m_TextureShader ->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the texture shader object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the blur shader object.
	m_BlurShader  = new BlurShaderClass;

	result = m_BlurShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the blur shader object.", L"Error", MB_OK);
		return false;
	}

	 // Set the size to sample down to.
    downSampleWidth = screenWidth / 2;
    downSampleHeight = screenHeight / 2;

	// Create and initialize the blur object.
    m_Blur = new BlurClass;

    result = m_Blur->Initialize(m_Direct3D, downSampleWidth, downSampleHeight, SCREEN_NEAR, SCREEN_DEPTH, screenWidth, screenHeight);
    if(!result)
    {
        MessageBox(hwnd, L"Could not initialize the blur object.", L"Error", MB_OK);
        return false;
    }

	// Create and initialize the glow shader object.
	m_GlowShader  = new GlowShaderClass;

	result = m_GlowShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the glow shader object.", L"Error", MB_OK);
		return false;
	}

	return true;
}


void ApplicationClass::Shutdown()
{
	// Release the glow shader object.
	if(m_GlowShader)
	{
		m_GlowShader->Shutdown();
		delete m_GlowShader;
		m_GlowShader = 0;
	}

	// Release the blur  object.
	if(m_Blur)
	{
		m_Blur->Shutdown();
		delete m_Blur;
		m_Blur = 0;
	}

	// Release the blur shader object.
	if(m_BlurShader)
	{
		m_BlurShader->Shutdown();
		delete m_BlurShader;
		m_BlurShader = 0;
	}

	// Release the texture shader object.
    if(m_TextureShader)
    {
        m_TextureShader->Shutdown();
        delete m_TextureShader;
        m_TextureShader = 0;
    }

    // Release the full screen ortho window object.
    if(m_FullScreenWindow)
    {
        m_FullScreenWindow->Shutdown();
        delete m_FullScreenWindow;
        m_FullScreenWindow = 0;
    }

	// Release the glow render texture object.
    if(m_GlowTexture)
    {
        m_GlowTexture->Shutdown();
        delete m_GlowTexture;
        m_GlowTexture = 0;
    }

	// Release the render texture object.
	if(m_RenderTexture)
	{
		m_RenderTexture->Shutdown();
		delete m_RenderTexture;
		m_RenderTexture = 0;
	}

    // Release the cube model object.
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
	static float rotation = 0.0f;
    bool result;
	

	// Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

	// Update the rotation variable each frame.
    rotation -= 0.0174532925f * 0.25f;
    if(rotation < 0.0f)
    {
        rotation += 360.0f;
    }

	// Render the regular scene to a texture.
    result = RenderSceneToTexture(rotation);
    if(!result)
    {
        return false;
    }

    // Render the glow map to a texture.
    result = RenderGlowToTexture(rotation);
    if(!result)
    {
        return false;
    }

    // Use the blur object to blur the glow map texture.
    result = m_Blur->BlurTexture(m_Direct3D, m_Camera, m_GlowTexture, m_TextureShader, m_BlurShader);
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


bool ApplicationClass::RenderSceneToTexture(float rotation)
{
	XMMATRIX worldMatrix, viewMatrix, projectionMatrix;
	bool result;


	// Set the render target to be the render to texture.  Also clear the render to texture.
	m_RenderTexture->SetRenderTarget(m_Direct3D->GetDeviceContext());
	m_RenderTexture->ClearRenderTarget(m_Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 1.0f);

	// Get the world, view, and projection matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetViewMatrix(viewMatrix);
	m_Direct3D->GetProjectionMatrix(projectionMatrix);

	// Rotate the world matrix by the rotation value so that the cube will spin.
	worldMatrix = XMMatrixRotationY(rotation);

	// Render the cube using the texture shader.
	m_Model->Render(m_Direct3D->GetDeviceContext());

	result = m_TextureShader->Render(m_Direct3D->GetDeviceContext(), m_Model->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, m_Model->GetTexture1());
	if(!result)
	{
		return false;
	}

	// Reset the render target back to the original back buffer and not the render to texture anymore.  Also reset the viewport back to the original.
	m_Direct3D->SetBackBufferRenderTarget();
	m_Direct3D->ResetViewport();

	return true;
}


bool ApplicationClass::RenderGlowToTexture(float rotation)
{
	XMMATRIX worldMatrix, viewMatrix, projectionMatrix;
	bool result;


	// Set the render target to be the glow render texture and clear it.
	m_GlowTexture->SetRenderTarget(m_Direct3D->GetDeviceContext());
	m_GlowTexture->ClearRenderTarget(m_Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 1.0f);

	// Get the world, view, and projection matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetViewMatrix(viewMatrix);
	m_Direct3D->GetProjectionMatrix(projectionMatrix);

	// Rotate the world matrix by the rotation value so that the cube will spin.
	worldMatrix = XMMatrixRotationY(rotation);

	// Render the cube using the texture shader using the glow texture this time (m_Model->GetTexture2).
	m_Model->Render(m_Direct3D->GetDeviceContext());

	result = m_TextureShader->Render(m_Direct3D->GetDeviceContext(), m_Model->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, m_Model->GetTexture2());
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
	XMMATRIX worldMatrix, baseViewMatrix, orthoMatrix;
	float glowValue;
	bool result;


	// Clear the buffers to begin the scene.
	m_Direct3D->BeginScene(0.0f, 0.5f, 0.8f, 1.0f);

	// Get the world, view, and projection matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetBaseViewMatrix(baseViewMatrix);
	m_Direct3D->GetOrthoMatrix(orthoMatrix);

	// Begin 2D rendering and turn off the Z buffer.
    m_Direct3D->TurnZBufferOff();

	// Set the glow strength we want.
    glowValue = 2.0f;

	// Render the full screen window using the glow shader.
	m_FullScreenWindow->Render(m_Direct3D->GetDeviceContext());

	result = m_GlowShader->Render(m_Direct3D->GetDeviceContext(), m_FullScreenWindow->GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix, 
								  m_RenderTexture->GetShaderResourceView(), m_GlowTexture->GetShaderResourceView(), glowValue);
	if(!result)
	{
		return false;
	}

	// Re-enable the Z buffer after 2D rendering complete.
    m_Direct3D->TurnZBufferOn();

	// Present the rendered scene to the screen.
	m_Direct3D->EndScene();

	return true;
}