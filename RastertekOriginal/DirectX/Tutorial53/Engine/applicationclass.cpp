////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
	m_Direct3D = 0;
	m_Timer = 0;
	m_Camera = 0;
	m_Model = 0;
	m_Light = 0;
	m_LightShader = 0;
	m_RenderTexture = 0;
	m_FullScreenWindow = 0;
	m_TextureShader = 0;
	m_BlurShader = 0;
    m_Blur = 0;
	m_Heat = 0;
	m_HeatShader = 0;
	m_HeatTexture = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(int screenWidth, int screenHeight, HWND hwnd)
{
	char modelFilename[128], diffuseFilename[128];
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

    // Create and initialize the timer object.
    m_Timer = new TimerClass;

    result = m_Timer->Initialize();
    if(!result)
    {
        MessageBox(hwnd, L"Could not initialize the timer object.", L"Error", MB_OK);
        return false;
    }

    // Create and initialize the camera object.
	m_Camera = new CameraClass;

	m_Camera->SetPosition(0.0f, 0.0f, -10.0f);
	m_Camera->Render();
	m_Camera->RenderBaseViewMatrix();

	// Create and initialize the sphere model object.
    m_Model = new ModelClass;

	strcpy_s(modelFilename, "../Engine/data/sphere.txt");
	strcpy_s(diffuseFilename, "../Engine/data/yellowcolor01.tga");

	result = m_Model->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, diffuseFilename);
    if(!result)
    {
        MessageBox(hwnd, L"Could not initialize the model object.", L"Error", MB_OK);
        return false;
    }

    // Create and initialize the light object.
	m_Light = new LightClass;

	m_Light->SetAmbientColor(0.15f, 0.15f, 0.15f, 1.0f);
	m_Light->SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
	m_Light->SetDirection(0.0f, 0.0f, 1.0f);

    // Create and initialize the light shader object.
    m_LightShader  = new LightShaderClass;

    result = m_LightShader->Initialize(m_Direct3D->GetDevice(), hwnd);
    if(!result)
    {
        MessageBox(hwnd, L"Could not initialize the light shader object.", L"Error", MB_OK);
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

	// Create and initialize the full screen ortho window object.
    m_FullScreenWindow = new OrthoWindowClass;

    result = m_FullScreenWindow->Initialize(m_Direct3D->GetDevice(), screenWidth, screenHeight);
    if(!result)
    {
        MessageBox(hwnd, L"Could not initialize the full screen ortho window object.", L"Error", MB_OK);
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

    // Create and initialize the heat object.
	m_Heat = new HeatClass;

	result = m_Heat->Initialize(m_Direct3D);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the heat object.", L"Error", MB_OK);
		return false;
	}

	// Create the heat shader object.
	m_HeatShader = new HeatShaderClass;

	result = m_HeatShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the heat shader object.", L"Error", MB_OK);
		return false;
	}

    // Create and initialize the heat render to texture object.
    m_HeatTexture = new RenderTextureClass;

    result = m_HeatTexture->Initialize(m_Direct3D->GetDevice(), screenWidth, screenHeight, SCREEN_DEPTH, SCREEN_NEAR, 1);
    if(!result)
    {
        MessageBox(hwnd, L"Could not initialize the heat render texture object.", L"Error", MB_OK);
        return false;
    }

	return true;
}


void ApplicationClass::Shutdown()
{
    // Release the heat render texture object.
    if(m_HeatTexture)
    {
        m_HeatTexture->Shutdown();
        delete m_HeatTexture;
        m_HeatTexture = 0;
    }

    // Release the heat shader object.
	if(m_HeatShader)
	{
		m_HeatShader->Shutdown();
		delete m_HeatShader;
		m_HeatShader = 0;
	}

	// Release the heat object.
	if(m_Heat)
	{
		m_Heat->Shutdown();
		delete m_Heat;
		m_Heat = 0;
	}

	// Release the blur object.
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

    // Release the render texture object.
    if(m_RenderTexture)
    {
        m_RenderTexture->Shutdown();
        delete m_RenderTexture;
        m_RenderTexture = 0;
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

	// Release the timer object.
    if(m_Timer)
    {
        delete m_Timer;
        m_Timer = 0;
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
	float frameTime;
    bool result;
	

	// Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

	// Update the system stats.
    m_Timer->Frame();
	frameTime = m_Timer->GetTime();

	// Do the engine heat frame processing.
	m_Heat->Frame(frameTime);

	// Render the regular scene to a texture.
    result = RenderSceneToTexture();
    if(!result)
    {
        return false;
    }

	// Render the heat map to a texture.
    result = RenderHeatToTexture();
    if(!result)
    {
        return false;
    }

    // Use the blur object to blur the heat map texture.
    result = m_Blur->BlurTexture(m_Direct3D, m_Camera, m_HeatTexture, m_TextureShader, m_BlurShader);
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


bool ApplicationClass::RenderSceneToTexture()
{
    XMMATRIX worldMatrix, viewMatrix, projectionMatrix;
    bool result;


    // Set the render target to be the render to texture.  Also clear the render to texture to grey.
    m_RenderTexture->SetRenderTarget(m_Direct3D->GetDeviceContext());
    m_RenderTexture->ClearRenderTarget(m_Direct3D->GetDeviceContext(), 0.25f, 0.25f, 0.25f, 1.0f);

    // Get the world, view, and projection matrices from the camera and d3d objects.
    m_Direct3D->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_Direct3D->GetProjectionMatrix(projectionMatrix);

    // Render the sphere using the light shader.
    m_Model->Render(m_Direct3D->GetDeviceContext());

   	result = m_LightShader->Render(m_Direct3D->GetDeviceContext(), m_Model->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, m_Model->GetTexture(), 
                                   m_Light->GetDirection(), m_Light->GetAmbientColor(), m_Light->GetDiffuseColor());
	if(!result)
	{
		return false;
	}

    // Reset the render target back to the original back buffer and not the render to texture anymore.  Also reset the viewport back to the original.
    m_Direct3D->SetBackBufferRenderTarget();
    m_Direct3D->ResetViewport();

    return true;
}


bool ApplicationClass::RenderHeatToTexture()
{
    XMMATRIX worldMatrix, viewMatrix, projectionMatrix;
    bool result;


    // Set the render target to be the glow render texture and clear it.
    m_HeatTexture->SetRenderTarget(m_Direct3D->GetDeviceContext());
    m_HeatTexture->ClearRenderTarget(m_Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the camera and d3d objects.
    m_Direct3D->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_Direct3D->GetProjectionMatrix(projectionMatrix);

    // Render the sphere using the light shader.
    m_Model->Render(m_Direct3D->GetDeviceContext());

	result = m_LightShader->Render(m_Direct3D->GetDeviceContext(), m_Model->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, m_Model->GetTexture(),
                                   m_Light->GetDirection(), m_Light->GetAmbientColor(), m_Light->GetDiffuseColor());
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
	XMFLOAT3 scrollSpeeds, scales;
	XMFLOAT2 distortion1, distortion2, distortion3;
	float distortionScale, distortionBias, emessiveMultiplier, noiseFrameTime;
	bool result;


	// Get the heat noise values.
	m_Heat->GetNoiseValues(scrollSpeeds, scales, distortion1, distortion2, distortion3, distortionScale, distortionBias, emessiveMultiplier, noiseFrameTime);

	// Clear the buffers to begin the scene.
	m_Direct3D->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

	// Get the world, view, and ortho matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetBaseViewMatrix(baseViewMatrix);
	m_Direct3D->GetOrthoMatrix(orthoMatrix);

	// Begin 2D rendering and turn off the Z buffer.
	m_Direct3D->TurnZBufferOff();

	// Put the full screen ortho window vertex and index buffers on the graphics pipeline to prepare them for drawing.
	m_FullScreenWindow->Render(m_Direct3D->GetDeviceContext());

	// Render the full screen ortho window using the heat shader and the rendered to texture resources.
	result = m_HeatShader->Render(m_Direct3D->GetDeviceContext(), m_FullScreenWindow->GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix, m_RenderTexture->GetShaderResourceView(), 
								  m_HeatTexture->GetShaderResourceView(), emessiveMultiplier, noiseFrameTime, scrollSpeeds, scales, distortion1, distortion2, distortion3, 
								  distortionScale, distortionBias, m_Heat->GetTexture());
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