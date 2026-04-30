////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
	m_Direct3D = 0;
	m_Camera = 0;
	m_TextureShader = 0;
	m_Model = 0;
	m_RenderTexture = 0;
    m_FullScreenWindow = 0;
    m_FadeShader = 0;
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

	m_Camera->SetPosition(0.0f, 0.0f, -10.0f);
	m_Camera->Render();

    // Create and initialize the model object.
    m_Model = new ModelClass;

	strcpy_s(modelFilename, "../Engine/data/cube.txt");
    strcpy_s(textureFilename, "../Engine/data/stone01.tga");

    result = m_Model->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), modelFilename, textureFilename);
    if(!result)
    {
		MessageBox(hwnd, L"Could not initialize the model object.", L"Error", MB_OK);
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

	// Create and initialize the render to texture object.
    m_RenderTexture = new RenderTextureClass;

    result = m_RenderTexture->Initialize(m_Direct3D->GetDevice(), screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, 0);
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

	// Create and initialize the fade shader object.
	m_FadeShader = new FadeShaderClass;

	result = m_FadeShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the fade shader object.", L"Error", MB_OK);
		return false;
	}

    // Create and initialize the timer object.
    m_Timer = new TimerClass;
    
    result = m_Timer->Initialize();
    if (!result)
    {
        MessageBox(hwnd, L"Could not initialize the timer object.", L"Error", MB_OK);
        return false;
    }

    // Initialize the accumulated time to zero.
    m_accumulatedTime = 0.0f;

    // Set the fade in time to 5 seconds.
    m_fadeInTime = 5.0f;

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

	// Release the blur shader object.
    if(m_FadeShader)
    {
        m_FadeShader->Shutdown();
        delete m_FadeShader;
        m_FadeShader = 0;
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

	// Release the texture shader object.
    if(m_TextureShader)
    {
        m_TextureShader->Shutdown();
        delete m_TextureShader;
        m_TextureShader = 0;
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
	static float rotation = 0.0f;
    float frameTime;
    float fadePercentage;
	bool result;

	
    // Update the system stats.
    m_Timer->Frame();

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

    // Update the accumulated time with the extra frame time addition.
    frameTime = m_Timer->GetTime();
    m_accumulatedTime += frameTime;

    // While the time goes on increase the fade in amount by the time that is passing each frame.
    if(m_accumulatedTime < m_fadeInTime)
    {
        // Calculate the percentage that the screen should be faded in based on the accumulated time.
        fadePercentage = m_accumulatedTime / m_fadeInTime;
    }
    else
    {
        fadePercentage = 1.0f;
    }

	// Render the scene to a render texture.
    result = RenderSceneToTexture(rotation);
    if(!result)
    {
        return false;
    }

    // Render the faded version of the scene.
    result = Render(fadePercentage);
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


    // Set the render target to be the render texture and clear it.
    m_RenderTexture->SetRenderTarget(m_Direct3D->GetDeviceContext());
    m_RenderTexture->ClearRenderTarget(m_Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 1.0f);

    // Get the matrices.
    m_Direct3D->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_RenderTexture->GetProjectionMatrix(projectionMatrix);

    // Rotate the world matrix by the rotation value so that the triangle will spin.
    worldMatrix = XMMatrixRotationY(rotation);

	// Put the model vertex and index buffers on the graphics pipeline to prepare them for drawing.
	m_Model->Render(m_Direct3D->GetDeviceContext());

	result = m_TextureShader->Render(m_Direct3D->GetDeviceContext(), m_Model->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, m_Model->GetTexture());
	if(!result)
	{
		return false;
	}

    // Reset the render target back to the original back buffer and not the render to texture anymore.  And reset the viewport back to the original.
    m_Direct3D->SetBackBufferRenderTarget();
    m_Direct3D->ResetViewport();

    return true;
}


bool ApplicationClass::Render(float fadePercentage)
{
	XMMATRIX worldMatrix, viewMatrix, orthoMatrix;
	bool result;


	// Clear the buffers to begin the scene.
	m_Direct3D->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

	// Get the world, view, and projection matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetViewMatrix(viewMatrix);
	m_Direct3D->GetOrthoMatrix(orthoMatrix);

    // Render the full screen ortho window.
    m_FullScreenWindow->Render(m_Direct3D->GetDeviceContext());

    // Render the full screen ortho window using the texture shader and the full screen sized blurred render to texture resource.
    result = m_FadeShader->Render(m_Direct3D->GetDeviceContext(), m_Model->GetIndexCount(), worldMatrix, viewMatrix, orthoMatrix, m_RenderTexture->GetShaderResourceView(), fadePercentage);
	if(!result)
	{
		return false;
	}

	// Present the rendered scene to the screen.
	m_Direct3D->EndScene();

	return true;
}