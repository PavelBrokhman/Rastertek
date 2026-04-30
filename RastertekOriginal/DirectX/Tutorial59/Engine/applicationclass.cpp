////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
	m_Direct3D = 0;
	m_Camera = 0;
    m_Timer = 0;
	m_ParticleSystem = 0;
	m_ParticleShader = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(int screenWidth, int screenHeight, HWND hwnd)
{
	char configFilename[128];
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
	m_Camera->RenderBaseViewMatrix();

	// Create and initialize the timer object.
	m_Timer = new TimerClass;

	result = m_Timer->Initialize();
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the timer object.", L"Error", MB_OK);
		return false;
	}

	// Set the file name of the texture for the particle system.
	strcpy_s(configFilename, "../Engine/data/particle_config_01.txt");

	// Create and initialize the partcile system object.
	m_ParticleSystem = new ParticleSystemClass;

	result = m_ParticleSystem->Initialize(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext(), configFilename);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the particle system object.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the particle shader object.
	m_ParticleShader = new ParticleShaderClass;

	result = m_ParticleShader->Initialize(m_Direct3D->GetDevice(), hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize the particle shader object.", L"Error", MB_OK);
		return false;
	}

	return true;
}


void ApplicationClass::Shutdown()
{
	// Release the particle shader object.
	if(m_ParticleShader)
	{
		m_ParticleShader->Shutdown();
		delete m_ParticleShader;
		m_ParticleShader = 0;
	}

	// Release the particle system object.
	if(m_ParticleSystem)
	{
		m_ParticleSystem->Shutdown();
		delete m_ParticleSystem;
		m_ParticleSystem = 0;
	}

    // Release the timer object.
    if(m_Timer)
    {
        delete m_Timer;
        m_Timer = 0;
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

	
    // Update the system stats.
    m_Timer->Frame();

	// Check if the user pressed escape and wants to exit the application.
	if(Input->IsEscapePressed())
	{
		return false;
	}

	// Check if the user wants to reload the particle system config and restart the particles.
	if(Input->IsRPressed())
	{
		result = m_ParticleSystem->Reload(m_Direct3D->GetDevice(), m_Direct3D->GetDeviceContext());
		if(!result)
		{
			return false;
		}
	}

	// Run the frame processing for the particle system.
	result = m_ParticleSystem->Frame(m_Timer->GetTime(), m_Direct3D->GetDeviceContext());
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


bool ApplicationClass::Render()
{
	XMMATRIX worldMatrix, viewMatrix, projectionMatrix;
	bool result;


	// Clear the buffers to begin the scene.
	m_Direct3D->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

	// Get the world, view, and projection matrices from the camera and d3d objects.
	m_Direct3D->GetWorldMatrix(worldMatrix);
	m_Camera->GetViewMatrix(viewMatrix);
	m_Direct3D->GetProjectionMatrix(projectionMatrix);

	// Turn on particle alpha blending and disable the Z buffer.
	m_Direct3D->EnableParticleAlphaBlending();
	m_Direct3D->TurnZBufferOff();

    // Render the particles using the particle shader.
	m_ParticleSystem->Render(m_Direct3D->GetDeviceContext());

    result = m_ParticleShader->Render(m_Direct3D->GetDeviceContext(), m_ParticleSystem->GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, 
									  m_ParticleSystem->GetTexture());
	if(!result)
	{
		return false;
	}

	// Turn off alpha blending and enable the Z buffer.
	m_Direct3D->DisableAlphaBlending();
	m_Direct3D->TurnZBufferOn();

	// Present the rendered scene to the screen.
	m_Direct3D->EndScene();

	return true;
}