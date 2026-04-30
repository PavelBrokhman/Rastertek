////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
	m_Direct3D = 0;
	m_Timer = 0;
	m_Camera = 0;
	m_FullScreenWindow = 0;
    m_ParallaxForest = 0;
	m_ScrollShader = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(int screenWidth, int screenHeight, HWND hwnd)
{
    char configFilename[256];
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

	// Create and initialize the full screen ortho window object.
    m_FullScreenWindow = new OrthoWindowClass;

    result = m_FullScreenWindow->Initialize(m_Direct3D->GetDevice(), screenWidth, screenHeight);
    if(!result)
    {
        MessageBox(hwnd, L"Could not initialize the full screen ortho window object.", L"Error", MB_OK);
        return false;
    }

    // Create and initialize the parallax scroll forest object.
    m_ParallaxForest = new ParallaxScrollClass;

    // Set the filename of the config file.
	strcpy_s(configFilename, "../Engine/data/config.txt");

    result = m_ParallaxForest->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), configFilename);
    if(!result)
    {
        MessageBox(hwnd, L"Could not initialize the parallax scroll object.", L"Error", MB_OK);
        return false;
    }

    // Create and initialize the scroll shader object.
    m_ScrollShader  = new ScrollShaderClass;

    result = m_ScrollShader ->Initialize(m_Direct3D->GetDevice(), hwnd);
    if(!result)
    {
        MessageBox(hwnd, L"Could not initialize the scroll shader object.", L"Error", MB_OK);
        return false;
    }

	return true;
}


void ApplicationClass::Shutdown()
{
    // Release the scroll shader object.
    if(m_ScrollShader)
    {
        m_ScrollShader->Shutdown();
        delete m_ScrollShader;
        m_ScrollShader = 0;
    }

    // Release the parallax scroll forest object.
    if(m_ParallaxForest)
    {
        m_ParallaxForest->Shutdown();
        delete m_ParallaxForest;
        m_ParallaxForest = 0;
    }

	// Release the full screen ortho window object.
    if(m_FullScreenWindow)
    {
        m_FullScreenWindow->Shutdown();
        delete m_FullScreenWindow;
        m_FullScreenWindow = 0;
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

    // Do the frame processing for the parallax scroll forest object.
    m_ParallaxForest->Frame(frameTime);

    // Render the final graphics scene.
    result = Render();
    if(!result)
    {
        return false;
    }

	return true;
}


bool ApplicationClass::Render()
{
	XMMATRIX worldMatrix, baseViewMatrix, orthoMatrix;
    int textureCount, i;
	bool result;


	// Clear the buffers to begin the scene.
	m_Direct3D->BeginScene(1.0f, 0.0f, 0.0f, 1.0f);

	// Get the world, view, and ortho matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetBaseViewMatrix(baseViewMatrix);
	m_Direct3D->GetOrthoMatrix(orthoMatrix);

	// Begin 2D rendering and turn off the Z buffer.  Also enable alpha blending as the textures have transparency in the alpha channels.
	m_Direct3D->TurnZBufferOff();
    m_Direct3D->EnableAlphaBlending();

    // Get the number of textures the parallax object uses.
    textureCount = m_ParallaxForest->GetTextureCount();

    // Render each of the parallax scroll textures in order using the scroll shader.
    for(i=0; i<textureCount; i++)
    {
        // Render the full screen ortho window using the scroll shader.
        m_FullScreenWindow->Render(m_Direct3D->GetDeviceContext());

	    result = m_ScrollShader->Render(m_Direct3D->GetDeviceContext(), m_FullScreenWindow->GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix,
                                        m_ParallaxForest->GetTexture(i), m_ParallaxForest->GetTranslation(i));
	    if(!result)
	    {
		    return false;
        }
    }

    // Re-enable the Z buffer after 2D rendering complete.  Also disable alpha blending.
	m_Direct3D->TurnZBufferOn();
    m_Direct3D->DisableAlphaBlending();

	// Present the rendered scene to the screen.
	m_Direct3D->EndScene();

	return true;
}