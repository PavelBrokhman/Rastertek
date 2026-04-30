////////////////////////////////////////////////////////////////////////////////
// Filename: blurclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "blurclass.h"


BlurClass::BlurClass()
{
	m_DownSampleTexure1 = 0;
	m_DownSampleTexure2 = 0;
	m_DownSampleWindow = 0;
	m_UpSampleWindow = 0;
}


BlurClass::BlurClass(const BlurClass& other)
{
}


BlurClass::~BlurClass()
{
}


bool BlurClass::Initialize(D3DClass* Direct3D, int downSampleWidth, int downSampleHeight, float screenNear, float screenDepth, int renderWidth, int renderHeight)
{
	bool result;


	// Store the down sample dimensions.
	m_downSampleWidth = downSampleWidth;
	m_downSampleHeight = downSampleHeight;
	
	// Create and initialize the first down sample render to texture object.
	m_DownSampleTexure1 = new RenderTextureClass;

	result = m_DownSampleTexure1->Initialize(Direct3D->GetDevice(), m_downSampleWidth, m_downSampleHeight, screenDepth, screenNear, 1);
	if(!result)
	{
		return false;
	}

	// Create and initialize the second down sample render to texture object.
	m_DownSampleTexure2 = new RenderTextureClass;

	result = m_DownSampleTexure2->Initialize(Direct3D->GetDevice(), m_downSampleWidth, m_downSampleHeight, screenDepth, screenNear, 1);
	if(!result)
	{
		return false;
	}

	// Create and initialize the down sample screen ortho window object.
	m_DownSampleWindow = new OrthoWindowClass;

	result = m_DownSampleWindow->Initialize(Direct3D->GetDevice(), m_downSampleWidth, m_downSampleHeight);
	if(!result)
	{
		return false;
	}

	// Create and initialize the up sample screen ortho window object.
	m_UpSampleWindow = new OrthoWindowClass;

	result = m_UpSampleWindow->Initialize(Direct3D->GetDevice(), renderWidth, renderHeight);
	if(!result)
	{
		return false;
	}

	return true;
}


void BlurClass::Shutdown()
{
	// Release the up sample screen ortho window object.
	if(m_UpSampleWindow)
	{
		m_UpSampleWindow->Shutdown();
		delete m_UpSampleWindow;
		m_UpSampleWindow = 0;
	}

	// Release the down sample screen ortho window object.
	if(m_DownSampleWindow)
	{
		m_DownSampleWindow->Shutdown();
		delete m_DownSampleWindow;
		m_DownSampleWindow = 0;
	}

	// Release the second down sample render to texture object.
	if(m_DownSampleTexure2)
	{
		m_DownSampleTexure2->Shutdown();
		delete m_DownSampleTexure2;
		m_DownSampleTexure2 = 0;
	}

	// Release the first down sample render to texture object.
	if(m_DownSampleTexure1)
	{
		m_DownSampleTexure1->Shutdown();
		delete m_DownSampleTexure1;
		m_DownSampleTexure1 = 0;
	}

	return;
}


bool BlurClass::BlurTexture(D3DClass* Direct3D, CameraClass* Camera, RenderTextureClass* RenderTexture, TextureShaderClass* TextureShader, BlurShaderClass* BlurShader)
{
	XMMATRIX worldMatrix, baseViewMatrix, orthoMatrix;
	float blurType;
	bool result;


	// Get the world and view matrix.
	Direct3D->GetWorldMatrix(worldMatrix);
	Camera->GetBaseViewMatrix(baseViewMatrix);

	// Begin 2D rendering and turn off the Z buffer.
	Direct3D->TurnZBufferOff();

	/////////////////////////////////////////////
    // STEP 1: Down sample the render to texture.
    /////////////////////////////////////////////
	
	m_DownSampleTexure1->SetRenderTarget(Direct3D->GetDeviceContext());
	m_DownSampleTexure1->ClearRenderTarget(Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 1.0f);
	m_DownSampleTexure1->GetOrthoMatrix(orthoMatrix);

	m_DownSampleWindow->Render(Direct3D->GetDeviceContext());

	result = TextureShader->Render(Direct3D->GetDeviceContext(), m_DownSampleWindow->GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix, RenderTexture->GetShaderResourceView());
	if(!result)
	{
		return false;
	}

	/////////////////////////////////////////////////////////////////
    // STEP 2: Perform a horizontal blur on the down sampled texture.
    /////////////////////////////////////////////////////////////////
	
	// Set the blur type to zero for a horizontal blur from the blur shader.
    blurType = 0.0f;

	m_DownSampleTexure2->SetRenderTarget(Direct3D->GetDeviceContext());
	m_DownSampleTexure2->ClearRenderTarget(Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 1.0f);
	m_DownSampleTexure2->GetOrthoMatrix(orthoMatrix);

	m_DownSampleWindow->Render(Direct3D->GetDeviceContext());

	result = BlurShader->Render(Direct3D->GetDeviceContext(), m_DownSampleWindow->GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix, m_DownSampleTexure1->GetShaderResourceView(),
											 m_downSampleWidth, m_downSampleHeight, blurType);
	if(!result)
	{
		return false;
	}

	//////////////////////////////////////////////////////////////////////
    // STEP 3: Perform a vertical blur on the horizontally blurred texture.
    //////////////////////////////////////////////////////////////////////

	// Set the blur type to one for a vertical blur from the blur shader.
    blurType = 1.0f;

	m_DownSampleTexure1->SetRenderTarget(Direct3D->GetDeviceContext());
	m_DownSampleTexure1->ClearRenderTarget(Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 1.0f);
	m_DownSampleTexure1->GetOrthoMatrix(orthoMatrix);

	m_DownSampleWindow->Render(Direct3D->GetDeviceContext());

	result = BlurShader->Render(Direct3D->GetDeviceContext(), m_DownSampleWindow->GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix, m_DownSampleTexure2->GetShaderResourceView(),
											 m_downSampleWidth, m_downSampleHeight, blurType);
	if(!result)
	{
		return false;
	}

	//////////////////////////////////////////////////////////////////////
    // STEP 4: Up sample the blurred result.
    //////////////////////////////////////////////////////////////////////
	
	RenderTexture->SetRenderTarget(Direct3D->GetDeviceContext());
	RenderTexture->ClearRenderTarget(Direct3D->GetDeviceContext(), 0.0f, 0.0f, 0.0f, 1.0f);
	RenderTexture->GetOrthoMatrix(orthoMatrix);

	m_UpSampleWindow->Render(Direct3D->GetDeviceContext());

	result = TextureShader->Render(Direct3D->GetDeviceContext(), m_UpSampleWindow->GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix, m_DownSampleTexure1->GetShaderResourceView());
	if(!result)
	{
		return false;
	}

	// Re-enable the Z buffer after 2D rendering complete.
	Direct3D->TurnZBufferOn();

	// Reset the render target back to the original back buffer and not the render to texture anymore.  And reset the viewport back to the original.
	Direct3D->SetBackBufferRenderTarget();
	Direct3D->ResetViewport();

	return true;
}