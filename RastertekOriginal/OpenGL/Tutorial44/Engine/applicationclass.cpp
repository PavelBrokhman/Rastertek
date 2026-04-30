////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Camera = 0;
    m_CubeModel = 0;
    m_SphereModel = 0;
    m_GroundModel = 0;
    m_Light = 0;
    m_RenderTexture = 0;
    m_BlackWhiteRenderTexture = 0;
    m_DepthShader = 0;
    m_ShadowShader = 0;
    m_SoftShadowShader = 0;
    m_Blur = 0;
    m_TextureShader = 0;
    m_BlurShader = 0;
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
    int downSampleWidth, downSampleHeight;
    bool result;


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
    m_Camera->Render();
    m_Camera->RenderBaseViewMatrix();

    // Set the camera up a bit to view the shadows better.
    m_Camera->SetPosition(0.0f, 7.0f, -10.0f);
    m_Camera->SetRotation(35.0f, 0.0f, 0.0f);
    m_Camera->Render();

    // Create and initialize the cube model object.
    m_CubeModel = new ModelClass;

    strcpy(modelFilename, "../Engine/data/cube.txt");
    strcpy(textureFilename, "../Engine/data/wall01.tga");

    result = m_CubeModel->Initialize(m_OpenGL, modelFilename, textureFilename, false, NULL, false, NULL, false);
    if(!result)
    {
        cout << "Error: Could not initialize the cube model object." << endl;
        return false;
    }

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

    result = m_GroundModel->Initialize(m_OpenGL, modelFilename, textureFilename, false, NULL, false, NULL, false);
    if(!result)
    {
        cout << "Error: Could not initialize the ground model object." << endl;
        return false;
    }

    // Create and initialize the light object.
    m_Light = new LightClass;

    m_Light->SetAmbientLight(0.15f, 0.15f, 0.15f, 1.0f);
    m_Light->SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
    m_Light->SetLookAt(0.0f, 0.0f, 0.0f);
    m_Light->GenerateProjectionMatrix(SCREEN_DEPTH, SCREEN_NEAR);

    // Create and initialize the render to texture object.
    m_RenderTexture = new RenderTextureClass;

    result = m_RenderTexture->Initialize(m_OpenGL, SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT, SCREEN_NEAR, SCREEN_DEPTH, 0);
    if(!result)
    {
        cout << "Error: Could not initialize the render texture object." << endl;
        return false;
    }

    // Create and initialize the black and white render to texture object.
    m_BlackWhiteRenderTexture = new RenderTextureClass;

    result = m_BlackWhiteRenderTexture->Initialize(m_OpenGL, SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT, SCREEN_NEAR, SCREEN_DEPTH, 0);
    if(!result)
    {
        cout << "Error: Could not initialize the render texture object." << endl;
        return false;
    }

    // Create and initialize the depth shader object.
    m_DepthShader = new DepthShaderClass;

    result = m_DepthShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the depth shader object." << endl;
        return false;
    }

    // Create and initialize the shadow shader object.
    m_ShadowShader = new ShadowShaderClass;

    result = m_ShadowShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the shadow shader object." << endl;
        return false;
    }

    // Create and initialize the soft shadow shader object.
    m_SoftShadowShader = new SoftShadowShaderClass;

    result = m_SoftShadowShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the soft shadow shader object." << endl;
        return false;
    }

    // Set the size to sample down to.
    downSampleWidth = SHADOWMAP_WIDTH / 2;
    downSampleHeight = SHADOWMAP_HEIGHT / 2;

    // Create and initialize the blur object.
    m_Blur = new BlurClass;

    result = m_Blur->Initialize(m_OpenGL, downSampleWidth, downSampleHeight, SCREEN_NEAR, SCREEN_DEPTH, SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT);
    if(!result)
    {
        cout << "Error: Could not initialize the blur object." << endl;
        return false;
    }

    // Create and initialize the texture shader object.
    m_TextureShader = new TextureShaderClass;

    result = m_TextureShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the texture shader object." << endl;
        return false;
    }

    // Create and initialize the blur shader object.
    m_BlurShader = new BlurShaderClass;

    result = m_BlurShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the blur shader object." << endl;
        return false;
    }

    // Set the shadow map bias to fix the floating point precision issues (shadow acne/lines artifacts).
    m_shadowMapBias = 0.0022f;

    return true;
}


void ApplicationClass::Shutdown()
{
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

    // Release the blur object.
    if(m_Blur)
    {
        m_Blur->Shutdown();
        delete m_Blur;
        m_Blur = 0;
    }

    // Release the soft shadow shader object.
    if(m_SoftShadowShader)
    {
        m_SoftShadowShader->Shutdown();
        delete m_SoftShadowShader;
        m_SoftShadowShader = 0;
    }

    // Release the shadow shader object.
    if(m_ShadowShader)
    {
        m_ShadowShader->Shutdown();
        delete m_ShadowShader;
        m_ShadowShader = 0;
    }

    // Release the depth shader object.
    if(m_DepthShader)
    {
        m_DepthShader->Shutdown();
        delete m_DepthShader;
        m_DepthShader = 0;
    }

    // Release the black and white render texture object.
    if(m_BlackWhiteRenderTexture)
    {
        m_BlackWhiteRenderTexture->Shutdown();
        delete m_BlackWhiteRenderTexture;
        m_BlackWhiteRenderTexture = 0;
    }

    // Release the render texture object.
    if(m_RenderTexture)
    {
        m_RenderTexture->Shutdown();
        delete m_RenderTexture;
        m_RenderTexture = 0;
    }

    // Release the light object.
    if(m_Light)
    {
        delete m_Light;
	m_Light = 0;
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

    // Release the cube model object.
    if(m_CubeModel)
    {
        m_CubeModel->Shutdown();
        delete m_CubeModel;
        m_CubeModel = 0;
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
    static float lightPositionX = -5.0f;
    bool result;


    // Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

    // Update the position of the light each frame.
    lightPositionX += 0.05f;
    if(lightPositionX > 5.0f)
    {
        lightPositionX = -5.0f;
    }

    // Update the position of the light.
    m_Light->SetPosition(lightPositionX, 8.0f, -5.0f);
    m_Light->GenerateViewMatrix();

    // Render the scene depth for the first light to it's render texture.
    result = RenderDepthToTexture();
    if(!result)
    {
        return false;
    }

    // Next render the shadowed scene in black and white.
    result = RenderBlackAndWhiteShadows();
    if(!result)
    {
        return false;
    }

    // Blur the black and white shadow render texture using the BlurClass object.
    result = m_Blur->BlurTexture(m_BlackWhiteRenderTexture, m_OpenGL, m_Camera, m_TextureShader, m_BlurShader);
    if(!result)
    {
        return true;
    }

    // Render the final graphics scene.
    result = Render();
    if(!result)
    {
        return false;
    }

    return true;
}


bool ApplicationClass::RenderDepthToTexture()
{
    float translateMatrix[16], lightViewMatrix[16], lightProjectionMatrix[16];
    bool result;


    // Set the render target to be the render texture and clear it.
    m_RenderTexture->SetRenderTarget();
    m_RenderTexture->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the view and projection matrices from the light object.
    m_Light->GetViewMatrix(lightViewMatrix);
    m_Light->GetProjectionMatrix(lightProjectionMatrix);

    // Setup the translation matrix for the cube model.
    m_OpenGL->MatrixTranslation(translateMatrix, -2.0f, 2.0f, 0.0f);

    // Render the cube model using the depth shader and the light matrices.
    result = m_DepthShader->SetShaderParameters(translateMatrix, lightViewMatrix, lightProjectionMatrix);
    if(!result)
    {
        return false;
    }

    m_CubeModel->Render();

    // Setup the translation matrix for the sphere model.
    m_OpenGL->MatrixTranslation(translateMatrix, 2.0f, 2.0f, 0.0f);

    // Render the sphere model using the depth shader and the light matrices.
    result = m_DepthShader->SetShaderParameters(translateMatrix, lightViewMatrix, lightProjectionMatrix);
    if(!result)
    {
        return false;
    }

    m_SphereModel->Render();

    // Setup the translation matrix for the ground model.
    m_OpenGL->MatrixTranslation(translateMatrix, 0.0f, 1.0f, 0.0f);

    // Render the ground model using the depth shader and the light matrices.
    result = m_DepthShader->SetShaderParameters(translateMatrix, lightViewMatrix, lightProjectionMatrix);
    if(!result)
    {
        return false;
    }

    m_GroundModel->Render();

    // Reset the render target back to the original back buffer and not the render to texture anymore.  And reset the viewport back to the original.
    m_OpenGL->SetBackBufferRenderTarget();
    m_OpenGL->ResetViewport();

    return true;
}


bool ApplicationClass::RenderBlackAndWhiteShadows()
{
    float viewMatrix[16], projectionMatrix[16], lightViewMatrix[16], lightProjectionMatrix[16], translateMatrix[16];
    float lightPosition[3];
    bool result;


    // Set the render target to be the black and white render texture and clear it.
    m_BlackWhiteRenderTexture->SetRenderTarget();
    m_BlackWhiteRenderTexture->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the view matrix from the camera, and get the projection matrix from the OpenGL object.
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Get the view and projection matrices from the light object.
    m_Light->GetViewMatrix(lightViewMatrix);
    m_Light->GetProjectionMatrix(lightProjectionMatrix);

    // Get the light position.
    m_Light->GetPosition(lightPosition);

    // Setup the translation matrix for the cube model.
    m_OpenGL->MatrixTranslation(translateMatrix, -2.0f, 2.0f, 0.0f);

    // Render the cube model using the shadow shader.
    result = m_ShadowShader->SetShaderParameters(translateMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightProjectionMatrix, lightPosition, m_shadowMapBias);
    if(!result)
    {
        return false;
    }

    // Set the render texture in texture unit 0.
    m_RenderTexture->SetTexture(0);

    m_CubeModel->Render();

    // Setup the translation matrix for the sphere model.
    m_OpenGL->MatrixTranslation(translateMatrix, 2.0f, 2.0f, 0.0f);

    // Render the sphere model using the shadow shader.
    result = m_ShadowShader->SetShaderParameters(translateMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightProjectionMatrix, lightPosition, m_shadowMapBias);
    if(!result)
    {
        return false;
    }

    // Set the render texture in texture unit 0.
    m_RenderTexture->SetTexture(0);

    m_SphereModel->Render();

    // Setup the translation matrix for the ground model.
    m_OpenGL->MatrixTranslation(translateMatrix, 0.0f, 1.0f, 0.0f);

    // Render the ground model using the shadow shader.
    result = m_ShadowShader->SetShaderParameters(translateMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightProjectionMatrix, lightPosition, m_shadowMapBias);
    if(!result)
    {
        return false;
    }

    // Set the render texture in texture unit 0.
    m_RenderTexture->SetTexture(0);

    m_GroundModel->Render();

    // Reset the render target back to the original back buffer and not the render to texture anymore.  And reset the viewport back to the original.
    m_OpenGL->SetBackBufferRenderTarget();
    m_OpenGL->ResetViewport();

    return true;
}


bool ApplicationClass::Render()
{
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16];
    float diffuseColor[4], ambientColor[4], lightPosition[3];
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Get the light properties.
    m_Light->GetPosition(lightPosition);
    m_Light->GetAmbientLight(ambientColor);
    m_Light->GetDiffuseColor(diffuseColor);

    // Setup the translation matrix for the cube model.
    m_OpenGL->MatrixTranslation(worldMatrix, -2.0f, 2.0f, 0.0f);

    // Set the soft shadow shader as the current shader program and set the parameters that it will use for rendering.
    result = m_SoftShadowShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightPosition, ambientColor, diffuseColor);
    if(!result)
    {
        return false;
    }

    // Set the blurred black and white shadow texture in texture unit 1.
    m_BlackWhiteRenderTexture->SetTexture(1);

    // Render the cube model using the soft shadow shader.
    m_CubeModel->SetTexture1(0);
    m_CubeModel->Render();

    // Setup the translation matrix for the sphere model.
    m_OpenGL->MatrixTranslation(worldMatrix, 2.0f, 2.0f, 0.0f);

    // Set the shadow shader as the current shader program and set the parameters that it will use for rendering.
    result = m_SoftShadowShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightPosition, ambientColor, diffuseColor);
    if(!result)
    {
        return false;
    }

    // Set the blurred black and white shadow texture in texture unit 1.
    m_BlackWhiteRenderTexture->SetTexture(1);

    // Render the sphere model using the soft shadow shader.
    m_SphereModel->SetTexture1(0);
    m_SphereModel->Render();

    // Setup the translation matrix for the ground model.
    m_OpenGL->MatrixTranslation(worldMatrix, 0.0f, 1.0f, 0.0f);

    // Set the shadow shader as the current shader program and set the parameters that it will use for rendering.
    result = m_SoftShadowShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightPosition, ambientColor, diffuseColor);
    if(!result)
    {
        return false;
    }

    // Set the blurred black and white shadow texture in texture unit 1.
    m_BlackWhiteRenderTexture->SetTexture(1);

    // Render the ground model using the soft shadow shader.
    m_GroundModel->SetTexture1(0);
    m_GroundModel->Render();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
