////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
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


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char modelFilename[128], textureFilename[128];
    bool result;


    // Store the screen width and height.
    m_screenWidth = screenWidth;
    m_screenHeight = screenHeight;

    // Create and initialize the OpenGL object.
    m_OpenGL = new OpenGLClass;

    result = m_OpenGL->Initialize(display, win, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, VSYNC_ENABLED);
    if(!result)
    {
        cout << "Error: Could not initialize the OpenGL object." << endl;
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

    strcpy(modelFilename, "../Engine/data/sphere.txt");
    strcpy(textureFilename, "../Engine/data/ice.tga");

    result = m_SphereModel->Initialize(m_OpenGL, modelFilename, textureFilename, true, NULL, false, NULL, false);
    if(!result)
    {
        cout << "Error: Could not initialize the sphere model object." << endl;
        return false;
    }

    // Create and initialize the ground model object.
    m_GroundModel = new ModelClass;

    strcpy(modelFilename, "../Engine/data/plane01.txt");
    strcpy(textureFilename, "../Engine/data/metal001.tga");

    result = m_GroundModel->Initialize(m_OpenGL, modelFilename, textureFilename, true, NULL, false, NULL, false);
    if(!result)
    {
        cout << "Error: Could not initialize the ground model object." << endl;
        return false;
    }

    // Create and initialize the deferred buffers object.
    m_DeferredBuffers = new DeferredBuffersClass;

    result = m_DeferredBuffers->Initialize(m_OpenGL, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH);
    if(!result)
    {
        cout << "Error: Could not initialize the deferred buffers object." << endl;
        return false;
    }

    // Create the and initialize gbuffer shader object.
    m_GBufferShader = new GBufferShaderClass;

    result = m_GBufferShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the gbuffer shader object." << endl;
        return false;
    }

    // Create the ssao render to texture object.
    m_SsaoRenderTexture = new RenderTextureClass;

    result = m_SsaoRenderTexture->Initialize(m_OpenGL, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, 1);
    if(!result)
    {
        cout << "Error: Could not initialize the ssao render texture object." << endl;
        return false;
    }

    // Create and initialize the blur ssao render to texture object.
    m_BlurSsaoRenderTexture = new RenderTextureClass;

    result = m_BlurSsaoRenderTexture->Initialize(m_OpenGL, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, 1);
    if(!result)
    {
        cout << "Error: Could not initialize the blur ssao render texture object." << endl;
        return false;
    }

    // Create and initialize the full screen ortho window object.
    m_FullScreenWindow = new OrthoWindowClass;

    result = m_FullScreenWindow->Initialize(m_OpenGL, screenWidth, screenHeight);
    if(!result)
    {
        cout << "Error: Could not initialize the full screen window object." << endl;
        return false;
    }

    // Create the and initialize ssao shader object.
    m_SsaoShader = new SsaoShaderClass;

    result = m_SsaoShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the ssao shader object." << endl;
        return false;
    }

    // Create and initialize the random texture object.
    m_RandomTexture = new TextureClass;

    strcpy(textureFilename, "../Engine/data/random_vec.tga");

    result = m_RandomTexture->Initialize(m_OpenGL, textureFilename, true);
    if(!result)
    {
        cout << "Error: Could not initialize the random texture object." << endl;
        return false;
    }

    // Create the and initialize ssao blur shader object.
    m_SsaoBlurShader = new SsaoBlurShaderClass;

    result = m_SsaoBlurShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the ssao blur shader object." << endl;
        return false;
    }

    // Create and initialize the deferred light shader object.
    m_LightShader = new LightShaderClass;

    result = m_LightShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the light shader object." << endl;
        return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the deferred light shader object.
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
        m_BlurSsaoRenderTexture->Shutdown(m_OpenGL);
        delete m_BlurSsaoRenderTexture;
        m_BlurSsaoRenderTexture = 0;
    }

    // Release the ssao render to texture object.
    if(m_SsaoRenderTexture)
    {
        m_SsaoRenderTexture->Shutdown(m_OpenGL);
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
        m_DeferredBuffers->Shutdown(m_OpenGL);
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

    // Release the OpenGL object.
    if(m_OpenGL)
    {
        m_OpenGL->Shutdown();
        delete m_OpenGL;
        m_OpenGL = 0;
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

    // Render the final graphics scene using deferred shading.
    result = Render();
    if(!result)
    {
        return false;
    }

    return true;
}


bool ApplicationClass::RenderGBuffer()
{
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16], translateMatrix[16];
    bool result;


    // Get the view, and projection matrices from the camera and OpenGL objects.
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Set the render target to be the deferred buffers.  Also clear the buffers.
    m_DeferredBuffers->SetRenderTarget(m_OpenGL);
    m_DeferredBuffers->ClearRenderTargets(0.0f, 0.0f, 0.0f, 0.0f);

    // Setup the translation matrix for the sphere model.
    m_OpenGL->MatrixTranslation(translateMatrix, 2.0f, 2.0f, 0.0f);

    // Render the sphere model using the gbuffer shader.
    result = m_GBufferShader->SetShaderParameters(translateMatrix, viewMatrix, projectionMatrix);
    if(!result)
    {
        return false;
    }
    m_SphereModel->SetTexture1(0);
    m_SphereModel->Render();

    // Setup the translation matrix for the sphere model.
    m_OpenGL->MatrixTranslation(translateMatrix, 0.0f, 1.0f, 0.0f);

    // Render the sphere model using the gbuffer shader.
    result = m_GBufferShader->SetShaderParameters(translateMatrix, viewMatrix, projectionMatrix);
    if(!result)
    {
        return false;
    }
    m_GroundModel->SetTexture1(0);
    m_GroundModel->Render();

    // Reset the render target back to the original back buffer and not the deferred buffers anymore.  Also reset the viewport back to the original.
    m_OpenGL->SetBackBufferRenderTarget();
    m_OpenGL->ResetViewport();

    return true;
}


bool ApplicationClass::RenderSsao()
{
    float worldMatrix[16], baseViewMatrix[16], orthoMatrix[16];
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

    // Get the matrices from the camera and OpenGL objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetBaseViewMatrix(baseViewMatrix);
    m_OpenGL->GetOrthoMatrix(orthoMatrix);

    // Set the render target to be the ssao texture.
    m_SsaoRenderTexture->SetRenderTarget(m_OpenGL);
    m_SsaoRenderTexture->ClearRenderTarget(0.0f, 0.0f, 0.0f, 0.0f);

    // Begin 2D rendering.
    m_OpenGL->TurnZBufferOff();

    // Render the ssao effect to a 2D full screen window.
    result = m_SsaoShader->SetShaderParameters(worldMatrix, baseViewMatrix, orthoMatrix, screenWidth, screenHeight, randomTextureSize, sampleRadius, ssaoScale, ssaoBias, ssaoIntensity);
    if(!result)
    {
        return false;
    }

    m_DeferredBuffers->SetShaderResourcePositions(m_OpenGL, 0);
    m_DeferredBuffers->SetShaderResourceNormals(m_OpenGL, 1);
    m_RandomTexture->SetTexture(m_OpenGL, 2);

    m_FullScreenWindow->Render();

    // End 2D rendering.
    m_OpenGL->TurnZBufferOn();

    // Reset the render target back to the original back buffer and not the render to texture anymore.  Also reset the viewport back to the original.
    m_OpenGL->SetBackBufferRenderTarget();
    m_OpenGL->ResetViewport();

    return true;
}


bool ApplicationClass::BlurSsaoTexture()
{
    float worldMatrix[16], baseViewMatrix[16], orthoMatrix[16];
    bool result;


    // Get the matrices from the camera and d3d objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetBaseViewMatrix(baseViewMatrix);
    m_OpenGL->GetOrthoMatrix(orthoMatrix);

    // Begin 2D rendering.
    m_OpenGL->TurnZBufferOff();

    // Set the blur render texture as the render target, and clear it.
    m_BlurSsaoRenderTexture->SetRenderTarget(m_OpenGL);
    m_BlurSsaoRenderTexture->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);

    // Perform a horizontal blur of the ssao texture.
    result = m_SsaoBlurShader->SetShaderParameters(worldMatrix, baseViewMatrix, orthoMatrix, m_screenWidth, m_screenHeight, 0);
    if(!result)
    {
        return false;
    }

    m_SsaoRenderTexture->SetTexture(m_OpenGL, 0);
    m_DeferredBuffers->SetShaderResourceNormals(m_OpenGL, 1);

    m_FullScreenWindow->Render();

    // Set the original ssao render texture as the render target, and clear it.
    m_SsaoRenderTexture->SetRenderTarget(m_OpenGL);
    m_SsaoRenderTexture->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);

    // Now perform a vertical blur of the ssao texture that was already horizontally blurred.
    result = m_SsaoBlurShader->SetShaderParameters(worldMatrix, baseViewMatrix, orthoMatrix, m_screenWidth, m_screenHeight, 1);
    if(!result)
    {
        return false;
    }

    m_BlurSsaoRenderTexture->SetTexture(m_OpenGL, 0);
    m_DeferredBuffers->SetShaderResourceNormals(m_OpenGL, 1);

    m_FullScreenWindow->Render();

    // End 2D rendering.
    m_OpenGL->TurnZBufferOn();

    // Reset the render target back to the original back buffer and not the render to texture anymore.  Also reset the viewport back to the original.
    m_OpenGL->SetBackBufferRenderTarget();
    m_OpenGL->ResetViewport();

    return true;
}


bool ApplicationClass::Render()
{
    float worldMatrix[16], baseViewMatrix[16], orthoMatrix[16], viewMatrix[16];
    float lightDirection[3];
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and ortho matrices from the camera and OpenGL objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetBaseViewMatrix(baseViewMatrix);
    m_OpenGL->GetOrthoMatrix(orthoMatrix);
    m_Camera->GetViewMatrix(viewMatrix);

    // Get the light properties.
    m_Light->GetDirection(lightDirection);

    // Begin 2D rendering and turn off the Z buffer.
    m_OpenGL->TurnZBufferOff();

    // Set the parameters for the deferred light shader.
    result = m_LightShader->SetShaderParameters(worldMatrix, baseViewMatrix, orthoMatrix, viewMatrix, lightDirection);
    if(!result)
    {
        return false;
    }

    m_DeferredBuffers->SetShaderResourceNormals(m_OpenGL, 0);
    m_SsaoRenderTexture->SetTexture(m_OpenGL, 1);
    m_DeferredBuffers->SetShaderResourceColors(m_OpenGL, 2);

    // Render the full screen ortho window using the deferred light shader and the deferred buffer texture resources.
    m_FullScreenWindow->Render();

    // Re-enable the Z buffer after 2D rendering complete.
    m_OpenGL->TurnZBufferOn();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
