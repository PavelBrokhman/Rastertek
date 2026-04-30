////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
	m_Direct3D = 0;
	m_DirectSound = 0;
	m_TestSound2 = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(int screenWidth, int screenHeight, HWND hwnd)
{
	char soundFilename[128];
	bool result;


	// Create and initialize the Direct3D object.
	m_Direct3D = new D3DClass;

	result = m_Direct3D->Initialize(screenWidth, screenHeight, VSYNC_ENABLED, hwnd, FULL_SCREEN, SCREEN_DEPTH, SCREEN_NEAR);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize Direct3D.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the direct sound object.
	m_DirectSound = new DirectSoundClass;

	result = m_DirectSound->Initialize(hwnd);
	if(!result)
	{
		MessageBox(hwnd, L"Could not initialize direct sound.", L"Error", MB_OK);
		return false;
	}

	// Create and initialize the test sound.
	m_TestSound2 = new Sound3DClass;

	strcpy_s(soundFilename, "../Engine/data/sound02.wav");

	result = m_TestSound2->LoadTrack(m_DirectSound->GetDirectSound(), soundFilename, 0);
	if(!result)
	{
		MessageBox(hwnd, L"Could not load the test sound.", L"Error", MB_OK);
		return false;
	}

	// Set the 3D position of the sound.
	m_TestSound2->Update3DPosition(-2.0f, 0.0f, 0.0f);

	// Play the sound.
	m_TestSound2->PlayTrack();

	return true;
}


void ApplicationClass::Shutdown()
{
	if(m_TestSound2)
	{
		// Stop the sound if it was still playing.
		m_TestSound2->StopTrack();

		// Release the test sound object.
		m_TestSound2->ReleaseTrack();
		delete m_TestSound2;
		m_TestSound2 = 0;
	}

	// Release the direct sound object.
	if(m_DirectSound)
	{
		m_DirectSound->Shutdown();
		delete m_DirectSound;
		m_DirectSound = 0;
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
	// Clear the buffers to begin the scene.
	m_Direct3D->BeginScene(0.25f, 0.25f, 0.25f, 1.0f);


	// Present the rendered scene to the screen.
	m_Direct3D->EndScene();

	return true;
}