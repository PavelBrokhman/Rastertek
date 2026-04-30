////////////////////////////////////////////////////////////////////////////////
// Filename: blurclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "blurclass.h"


BlurClass::BlurClass()
{
    m_DownSampleTexture1 = 0;
    m_DownSampleTexture2 = 0;
    m_DownSampleWindow = 0;
    m_UpSampleWindow = 0;
}


BlurClass::BlurClass(const BlurClass& other)
{
}


BlurClass::~BlurClass()
{
}


bool BlurClass::Initialize(OpenGLClass* OpenGL, int downSampleWidth, int downSampleHeight, float screenNear, float screenDepth, int renderWidth, int renderHeight)
{
    bool result;


    // Store the down sample dimensions.
    m_downSampleWidth = downSampleWidth;
    m_downSampleHeight = downSampleHeight;

    // Create and initialize the first down sample render to texture object.
    m_DownSampleTexture1 = new RenderTextureClass;

    result = m_DownSampleTexture1->Initialize(OpenGL, m_downSampleWidth, m_downSampleHeight, screenNear, screenDepth, 0);
    if(!result)
    {
        return false;
    }

    // Create and initialize the second down sample render to texture object.
    m_DownSampleTexture2 = new RenderTextureClass;

    result = m_DownSampleTexture2->Initialize(OpenGL, m_downSampleWidth, m_downSampleHeight, screenNear, screenDepth, 0);
    if(!result)
    {
        return false;
    }

    // Create and initialize the down sample ortho window object.
    m_DownSampleWindow = new OrthoWindowClass;

    result = m_DownSampleWindow->Initialize(OpenGL, m_downSampleWidth, m_downSampleHeight);
    if(!result)
    {
        return false;
    }

    // Create and initialize the up sample ortho window object.
    m_UpSampleWindow = new OrthoWindowClass;

    result = m_UpSampleWindow->Initialize(OpenGL, renderWidth, renderHeight);
    if(!result)
    {
        return false;
    }

    return true;
}


void BlurClass::Shutdown()
{
    // Release the up sample ortho window object.
    if(m_UpSampleWindow)
    {
        m_UpSampleWindow->Shutdown();
        delete m_UpSampleWindow;
        m_UpSampleWindow = 0;
    }

    // Release the down sample ortho window object.
    if(m_DownSampleWindow)
    {
        m_DownSampleWindow->Shutdown();
        delete m_DownSampleWindow;
        m_DownSampleWindow = 0;
    }

    // Release the second down sample render to texture object.
    if(m_DownSampleTexture2)
    {
        m_DownSampleTexture2->Shutdown();
        delete m_DownSampleTexture2;
        m_DownSampleTexture2 = 0;
    }

    // Release the first down sample render to texture object.
    if(m_DownSampleTexture1)
    {
        m_DownSampleTexture1->Shutdown();
        delete m_DownSampleTexture1;
        m_DownSampleTexture1 = 0;
    }

    return;
}


bool BlurClass::BlurTexture(RenderTextureClass* RenderTexture, OpenGLClass* OpenGL, CameraClass* Camera, TextureShaderClass* TextureShader, BlurShaderClass* BlurShader)
{
    float worldMatrix[16], baseViewMatrix[16], orthoMatrix[16];
    float blurType;
    bool result;


    // Get the matrices.
    OpenGL->GetWorldMatrix(worldMatrix);
    Camera->GetBaseViewMatrix(baseViewMatrix);

    // Begin 2D rendering and turn off the Z buffer.
    OpenGL->TurnZBufferOff();

    /////////////////////////////////////////////
    // STEP 1: Down sample the render to texture.
    /////////////////////////////////////////////

    // Set the first down sample render texture as the target render texture.
    m_DownSampleTexture1->SetRenderTarget();
    m_DownSampleTexture1->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);
    m_DownSampleTexture1->GetOrthoMatrix(orthoMatrix);

    // Set the texture shader as the current shader program and set the matrices that it will use for rendering.
    result = TextureShader->SetShaderParameters(worldMatrix, baseViewMatrix, orthoMatrix);
    if(!result)
    {
        return false;
    }

    // Set the render texture that will be used for the down sample ortho window rendering.
    RenderTexture->SetTexture(0);

    // Render the down sample ortho window.
    m_DownSampleWindow->Render();

    /////////////////////////////////////////////////////////////////
    // STEP 2: Perform a horizontal blur on the down sampled texture.
    /////////////////////////////////////////////////////////////////

    // Set the blur type to zero for a horizontal blur from the blur shader.
    blurType = 0.0f;

    // Set the second down sample render texture as the target render texture.
    m_DownSampleTexture2->SetRenderTarget();
    m_DownSampleTexture2->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);
    m_DownSampleTexture2->GetOrthoMatrix(orthoMatrix);

    // Set the blur shader as the current shader program and set the parameters that it will use for rendering.
    result = BlurShader->SetShaderParameters(worldMatrix, baseViewMatrix, orthoMatrix, m_downSampleWidth, m_downSampleHeight, blurType);
    if(!result)
    {
        return false;
    }

    // Use the down sampled render texture from the previous step as the texture that will be blurred horizontally.
    m_DownSampleTexture1->SetTexture(0);

    // Render the down sample ortho window.
    m_DownSampleWindow->Render();

    //////////////////////////////////////////////////////////////////////
    // STEP 3: Perform a vertical blur on the horizontally blurred texture.
    //////////////////////////////////////////////////////////////////////

    // Set the blur type to one for a vertical blur from the blur shader.
    blurType = 1.0f;

    // Set the first down sample render texture as the target render location this time.
    m_DownSampleTexture1->SetRenderTarget();
    m_DownSampleTexture1->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);
    m_DownSampleTexture1->GetOrthoMatrix(orthoMatrix);

    // Set the blur shader as the current shader program and set the parameters that it will use for rendering.
    result = BlurShader->SetShaderParameters(worldMatrix, baseViewMatrix, orthoMatrix, m_downSampleWidth, m_downSampleHeight, blurType);
    if(!result)
    {
        return false;
    }

    // Use the horizontally blurred render texture from the previous step as the texture that will be vertically blurred.
    m_DownSampleTexture2->SetTexture(0);

    // Render the down sample ortho window.
    m_DownSampleWindow->Render();

    //////////////////////////////////////////////////////////////////////
    // STEP 4: Up sample the blurred result.
    //////////////////////////////////////////////////////////////////////

    // Set the input/output render texture as the target render location this time.
    RenderTexture->SetRenderTarget();
    RenderTexture->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);
    RenderTexture->GetOrthoMatrix(orthoMatrix);

    // Set the texture shader as the current shader program and set the matrices that it will use for rendering.
    result = TextureShader->SetShaderParameters(worldMatrix, baseViewMatrix, orthoMatrix);
    if(!result)
    {
        return false;
    }

    // Use the fully blurred render texture from the previous step as the texture that will be up sampled.
    m_DownSampleTexture1->SetTexture(0);

    // Render the up sample ortho window.
    m_UpSampleWindow->Render();

    // Re-enable the Z buffer after 2D rendering complete.
    OpenGL->TurnZBufferOn();

    // Reset the render target back to the original back buffer and not the render to texture anymore.  And reset the viewport back to the original.
    OpenGL->SetBackBufferRenderTarget();
    OpenGL->ResetViewport();

    return true;
}
