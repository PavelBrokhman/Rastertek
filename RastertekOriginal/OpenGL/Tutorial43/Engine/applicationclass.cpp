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
    m_DepthShader = 0;
    m_ShadowShader = 0;
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

    // Position camera to view scene from behind to see the shadow effect better.
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
    m_Light->GenerateOrthoMatrix(20.0f, SHADOWMAP_NEAR, SHADOWMAP_DEPTH);

    // Create and initialize the render to texture object.
    m_RenderTexture = new RenderTextureClass;

    result = m_RenderTexture->Initialize(m_OpenGL, SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT, SHADOWMAP_NEAR, SHADOWMAP_DEPTH, 0);
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

    // Set the shadow map bias to fix the floating point precision issues (shadow acne/lines artifacts).
    m_shadowMapBias = 0.0022f;

    return true;
}


void ApplicationClass::Shutdown()
{
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
    static float lightAngle = 270.0f;
    static float lightPosX = 9.0f;
    float radians;
    float frameTime;
    bool result;


    // Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

    // Set the frame time manually assumming 60 fps.
    frameTime = 10.0f;

    // Update the position of the light each frame.
    lightPosX -= 0.003f * frameTime;

    // Update the angle of the light each frame.
    lightAngle -= 0.03f * frameTime;
    if(lightAngle < 90.0f)
    {
        lightAngle = 270.0f;
	
	// Reset the light position also.
	lightPosX = 9.0f;
    }
    radians = lightAngle * 0.0174532925f;

    // Update the direction of the light.
    m_Light->SetDirection(sinf(radians), cosf(radians), 0.0f);

    // Set the position and lookat for the light.
    m_Light->SetPosition(lightPosX, 8.0f, -0.1f);
    m_Light->SetLookAt(-lightPosX, 0.0f, 0.0f);
    m_Light->GenerateViewMatrix();

    // Render the scene depth for the first light to it's render texture.
    result = RenderDepthToTexture();
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


bool ApplicationClass::RenderDepthToTexture()
{
    float translateMatrix[16], lightViewMatrix[16], lightOrthoMatrix[16];
    bool result;


    // Set the render target to be the render texture and clear it.
    m_RenderTexture->SetRenderTarget();
    m_RenderTexture->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the view and orthographic matrices from the light object.
    m_Light->GetViewMatrix(lightViewMatrix);
    m_Light->GetOrthoMatrix(lightOrthoMatrix);

    // Setup the translation matrix for the cube model.
    m_OpenGL->MatrixTranslation(translateMatrix, -2.0f, 2.0f, 0.0f);

    // Render the cube model using the depth shader and the light matrices.
    result = m_DepthShader->SetShaderParameters(translateMatrix, lightViewMatrix, lightOrthoMatrix);
    if(!result)
    {
        return false;
    }

    m_CubeModel->Render();

    // Setup the translation matrix for the sphere model.
    m_OpenGL->MatrixTranslation(translateMatrix, 2.0f, 2.0f, 0.0f);

    // Render the sphere model using the depth shader and the light matrices.
    result = m_DepthShader->SetShaderParameters(translateMatrix, lightViewMatrix, lightOrthoMatrix);
    if(!result)
    {
        return false;
    }

    m_SphereModel->Render();

    // Setup the translation matrix for the ground model.
    m_OpenGL->MatrixTranslation(translateMatrix, 0.0f, 1.0f, 0.0f);

    // Render the ground model using the depth shader and the light matrices.
    result = m_DepthShader->SetShaderParameters(translateMatrix, lightViewMatrix, lightOrthoMatrix);
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


bool ApplicationClass::Render()
{
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16], lightViewMatrix[16], lightOrthoMatrix[16];
    float diffuseColor[4], ambientColor[4], lightDirection[3];
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Get the light view and projection matrices.
    m_Light->GetViewMatrix(lightViewMatrix);
    m_Light->GetOrthoMatrix(lightOrthoMatrix);

    // Get the light properties.
    m_Light->GetDiffuseColor(diffuseColor);
    m_Light->GetAmbientLight(ambientColor);
    m_Light->GetDirection(lightDirection);

    // Setup the translation matrix for the cube model.
    m_OpenGL->MatrixTranslation(worldMatrix, -2.0f, 2.0f, 0.0f);

    // Set the shadow shader as the current shader program and set the parameters that it will use for rendering.
    result = m_ShadowShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightOrthoMatrix,
						 diffuseColor, ambientColor, lightDirection, m_shadowMapBias);
    if(!result)
    {
        return false;
    }

    // Set the render texture in texture unit 1.
    m_RenderTexture->SetTexture(1);

    // Render the cube model using the shadow shader.
    m_CubeModel->SetTexture1(0);
    m_CubeModel->Render();

    // Setup the translation matrix for the sphere model.
    m_OpenGL->MatrixTranslation(worldMatrix, 2.0f, 2.0f, 0.0f);

    // Set the shadow shader as the current shader program and set the parameters that it will use for rendering.
    result = m_ShadowShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightOrthoMatrix,
						 diffuseColor, ambientColor, lightDirection, m_shadowMapBias);
    if(!result)
    {
        return false;
    }

    // Set the render texture in texture unit 1.
    m_RenderTexture->SetTexture(1);

    // Render the sphere model using the shadow shader.
    m_SphereModel->SetTexture1(0);
    m_SphereModel->Render();

    // Setup the translation matrix for the ground model.
    m_OpenGL->MatrixTranslation(worldMatrix, 0.0f, 1.0f, 0.0f);

    // Set the shadow shader as the current shader program and set the parameters that it will use for rendering.
    result = m_ShadowShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightOrthoMatrix,
						 diffuseColor, ambientColor, lightDirection, m_shadowMapBias);
    if(!result)
    {
        return false;
    }

    // Set the render texture in texture unit 1.
    m_RenderTexture->SetTexture(1);

    // Render the ground model using the shadow shader.
    m_GroundModel->SetTexture1(0);
    m_GroundModel->Render();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
