////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Camera = 0;
    m_GroundModel = 0;
    m_WallModel = 0;
    m_BathModel = 0;
    m_WaterModel = 0;
    m_Light = 0;
    m_RefractionTexture = 0;
    m_ReflectionTexture = 0;
    m_LightShader = 0;
    m_RefractionShader = 0;
    m_WaterShader = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char modelFilename[128];
    char textureFilename[128];
    bool result;


    // Create and initialize the OpenGL object.
    m_OpenGL = new OpenGLClass;

    result = m_OpenGL->Initialize(display, win, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, VSYNC_ENABLED);
    if(!result)
    {
        cout << "Error: Could not initialize the OpenGL object." << endl;
        return false;
    }

    // Create the camera object.
    m_Camera = new CameraClass;

    // Set the position and rotation of the camera.
    m_Camera->SetPosition(-10.0f, 6.0f, -10.0f);
    m_Camera->SetRotation(0.0f, 45.0f, 0.0f);
    m_Camera->Render();

    // Set the file names of the ground model.
    strcpy(modelFilename, "../Engine/data/ground.txt");
    strcpy(textureFilename, "../Engine/data/ground01.tga");

    // Create and initialize the ground model object.
    m_GroundModel = new ModelClass;

    result = m_GroundModel->Initialize(m_OpenGL, modelFilename, textureFilename, true, NULL, true, NULL, true);
    if(!result)
    {
        cout << "Error: Could not initialize the ground model object." << endl;
	return false;
    }

    // Set the file names of the wall model.
    strcpy(modelFilename, "../Engine/data/wall.txt");
    strcpy(textureFilename, "../Engine/data/wall01.tga");

    // Create and initialize the wall model object.
    m_WallModel = new ModelClass;

    result = m_WallModel->Initialize(m_OpenGL, modelFilename, textureFilename, true, NULL, true, NULL, true);
    if(!result)
    {
        cout << "Error: Could not initialize the wall model object." << endl;
        return false;
    }

    // Set the file names of the bath model.
    strcpy(modelFilename, "../Engine/data/bath.txt");
    strcpy(textureFilename, "../Engine/data/marble01.tga");

    // Create and initialize the bath model object.
    m_BathModel = new ModelClass;

    result = m_BathModel->Initialize(m_OpenGL, modelFilename, textureFilename, true, NULL, true, NULL, true);
    if(!result)
    {
        cout << "Error: Could not initialize the bath model object." << endl;
        return false;
    }

    // Set the file names of the water model.
    strcpy(modelFilename, "../Engine/data/water.txt");
    strcpy(textureFilename, "../Engine/data/water01.tga");

    // Create and initialize the water model object.
    m_WaterModel = new ModelClass;

    result = m_WaterModel->Initialize(m_OpenGL, modelFilename, textureFilename, true, NULL, true, NULL, true);
    if(!result)
    {
        cout << "Error: Could not initialize the water model object." << endl;
        return false;
    }

    // Create and initialize the light object.
    m_Light = new LightClass;

    m_Light->SetAmbientLight(0.15f, 0.15f, 0.15f, 1.0f);
    m_Light->SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
    m_Light->SetDirection(0.0f, -1.0f, 0.5f);

    // Create and initialize the refraction render to texture object.
    m_RefractionTexture = new RenderTextureClass;

    result = m_RefractionTexture->Initialize(m_OpenGL, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, 0);
    if(!result)
    {
        cout << "Error: Could not initialize the refraction texture object." << endl;
        return false;
    }

    // Create and initialize the reflection render to texture object.
    m_ReflectionTexture = new RenderTextureClass;

    result = m_ReflectionTexture->Initialize(m_OpenGL, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, 0);
    if(!result)
    {
        cout << "Error: Could not initialize the reflection texture object." << endl;
        return false;
    }

    // Create and initialize the light shader object.
    m_LightShader = new LightShaderClass;

    result = m_LightShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the light shader object." << endl;
        return false;
    }

    // Create and initialize the refraction shader object.
    m_RefractionShader = new RefractionShaderClass;

    result = m_RefractionShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the refraction shader object." << endl;
        return false;
    }

    // Create and initialize the water shader object.
    m_WaterShader = new WaterShaderClass;

    result = m_WaterShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the water shader object." << endl;
        return false;
    }

    // Set the height of the water.
    m_waterHeight = 2.75f;

    // Initialize the position of the water.
    m_waterTranslation = 0.0f;

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the water shader object.
    if(m_WaterShader)
    {
        m_WaterShader->Shutdown();
        delete m_WaterShader;
        m_WaterShader = 0;
    }

    // Release the refraction shader object.
    if(m_RefractionShader)
    {
        m_RefractionShader->Shutdown();
        delete m_RefractionShader;
        m_RefractionShader = 0;
    }

    // Release the light shader object.
    if(m_LightShader)
    {
        m_LightShader->Shutdown();
        delete m_LightShader;
        m_LightShader = 0;
    }

    // Release the reflection render to texture object.
    if(m_ReflectionTexture)
    {
        m_ReflectionTexture->Shutdown();
        delete m_ReflectionTexture;
        m_ReflectionTexture = 0;
    }

    // Release the refraction render to texture object.
    if(m_RefractionTexture)
    {
        m_RefractionTexture->Shutdown();
        delete m_RefractionTexture;
        m_RefractionTexture = 0;
    }

    // Release the light object.
    if(m_Light)
    {
        delete m_Light;
        m_Light = 0;
    }

    // Release the water model object.
    if(m_WaterModel)
    {
        m_WaterModel->Shutdown();
        delete m_WaterModel;
        m_WaterModel = 0;
    }

    // Release the bath model object.
    if(m_BathModel)
    {
        m_BathModel->Shutdown();
        delete m_BathModel;
        m_BathModel = 0;
    }

    // Release the wall model object.
    if(m_WallModel)
    {
        m_WallModel->Shutdown();
        delete m_WallModel;
        m_WallModel = 0;
    }

    // Release the ground model object.
    if(m_GroundModel)
    {
        m_GroundModel->Shutdown();
        delete m_GroundModel;
        m_GroundModel = 0;
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

    // Update the position of the water to simulate motion.
    m_waterTranslation += 0.001f;
    if(m_waterTranslation > 1.0f)
    {
        m_waterTranslation -= 1.0f;
    }

    // Render the refraction of the scene to a texture.
    result = RenderRefractionToTexture();
    if(!result)
    {
        return false;
    }

    // Render the reflection of the scene to a texture.
    result = RenderReflectionToTexture();
    if(!result)
    {
        return false;
    }

    // Render the final graphics scene to the back buffer.
    result = Render();
    if(!result)
    {
        return false;
    }

    return true;
}


bool ApplicationClass::RenderRefractionToTexture()
{
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16];
    float clipPlane[4];
    float diffuseLightColor[4], lightDirection[3], ambientLight[4];
    bool result;


    // Setup a clipping plane based on the height of the water to clip everything above it.
    clipPlane[0] =  0.0f;
    clipPlane[1] = -1.0f;
    clipPlane[2] =  0.0f;
    clipPlane[3] = m_waterHeight + 0.1f;

    // Set the render target to be the refraction render texture and clear it.
    m_RefractionTexture->SetRenderTarget();
    m_RefractionTexture->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_RefractionTexture->GetProjectionMatrix(projectionMatrix);

    // Get the light properties.
    m_Light->GetDirection(lightDirection);
    m_Light->GetDiffuseColor(diffuseLightColor);
    m_Light->GetAmbientLight(ambientLight);

    // Enable clip planes in OpenGL.
    m_OpenGL->EnableClipping();
    
    // Translate to where the bath model will be rendered.
    m_OpenGL->MatrixTranslation(worldMatrix, 0.0f, 2.0f, 0.0f);

    // Set the refraction shader as the current shader program and set the variables that it will use for rendering.
    result = m_RefractionShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightDirection, diffuseLightColor, ambientLight, clipPlane);
    if(!result)
    {
        return false;
    }

    // Render the bath model to the refraction render texture using the refraction shader.
    m_BathModel->SetTexture1(0);
    m_BathModel->Render();

    // Disable clip planes in OpenGL.
    m_OpenGL->DisableClipping();
    
    // Reset the render target back to the original back buffer and not the render to texture anymore.  And reset the viewport back to the original.
    m_OpenGL->SetBackBufferRenderTarget();
    m_OpenGL->ResetViewport();

    return true;
}


bool ApplicationClass::RenderReflectionToTexture()
{
    float worldMatrix[16], reflectionViewMatrix[16], projectionMatrix[16];
    float diffuseLightColor[4], lightDirection[3], ambientLight[4];
    bool result;


    // Set the render target to be the reflection render texture and clear it.
    m_ReflectionTexture->SetRenderTarget();
    m_ReflectionTexture->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);

    // Use the camera to render the reflection and create a reflection view matrix.  Then get the camera reflection view matrix instead of the normal view matrix.
    m_Camera->RenderReflection(m_waterHeight);
    m_Camera->GetReflectionViewMatrix(reflectionViewMatrix);

    // Get the world and projection matrices from the opengl and reflection render texture objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_ReflectionTexture->GetProjectionMatrix(projectionMatrix);

    // Get the light properties.
    m_Light->GetDirection(lightDirection);
    m_Light->GetDiffuseColor(diffuseLightColor);
    m_Light->GetAmbientLight(ambientLight);

    // Translate to where the wall model will be rendered.
    m_OpenGL->MatrixTranslation(worldMatrix, 0.0f, 6.0f, 8.0f);

    // Set the light shader as the current shader program and set the variables that it will use for rendering.
    result = m_LightShader->SetShaderParameters(worldMatrix, reflectionViewMatrix, projectionMatrix, lightDirection, diffuseLightColor, ambientLight);
    if(!result)
    {
        return false;
    }

    // Render the wall model using the regular light shader and the reflection view matrix to create a reflection of just the wall.
    m_WallModel->SetTexture1(0);
    m_WallModel->Render();

    // Reset the render target back to the original back buffer and not the render to texture anymore.  And reset the viewport back to the original.
    m_OpenGL->SetBackBufferRenderTarget();
    m_OpenGL->ResetViewport();

    return true;
}


bool ApplicationClass::Render()
{
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16], reflectionViewMatrix[16];
    float diffuseLightColor[4], lightDirection[3], ambientLight[4];
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Get the light properties.
    m_Light->GetDirection(lightDirection);
    m_Light->GetDiffuseColor(diffuseLightColor);
    m_Light->GetAmbientLight(ambientLight);

    // Translate to where the ground model will be rendered.
    m_OpenGL->MatrixTranslation(worldMatrix, 0.0f, 1.0f, 0.0f);

    // Set the light shader as the current shader program and set the matrices that it will use for rendering.
    result = m_LightShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightDirection, diffuseLightColor, ambientLight);
    if(!result)
    {
        return false;
    }

    // Render the ground model using the light shader.
    m_GroundModel->SetTexture1(0);
    m_GroundModel->Render();

    // Translate to where the wall model will be rendered.
    m_OpenGL->MatrixTranslation(worldMatrix, 0.0f, 6.0f, 8.0f);

    // Set the light shader as the current shader program and set the matrices that it will use for rendering.
    result = m_LightShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightDirection, diffuseLightColor, ambientLight);
    if(!result)
    {
        return false;
    }

    // Render the wall model using the light shader.
    m_WallModel->SetTexture1(0);
    m_WallModel->Render();

    // Translate to where the bath model will be rendered.
    m_OpenGL->MatrixTranslation(worldMatrix, 0.0f, 2.0f, 0.0f);

    // Set the light shader as the current shader program and set the variables that it will use for rendering.
    result = m_LightShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightDirection, diffuseLightColor, ambientLight);
    if(!result)
    {
        return false;
    }

    // Render the bath model using the light shader.
    m_BathModel->SetTexture1(0);
    m_BathModel->Render();

    // Get the camera reflection view matrix for the water shader.
    m_Camera->GetReflectionViewMatrix(reflectionViewMatrix);

    // Translate to where the water model will be rendered.
    m_OpenGL->MatrixTranslation(worldMatrix, 0.0f, m_waterHeight, 0.0f);

    // Set the water shader as the current shader program and set the variables that it will use for rendering.
    result = m_WaterShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, reflectionViewMatrix, m_waterTranslation, 0.01f);
    if(!result)
    {
        return false;
    }

    // Set the refraction render texture as the texture to be used in texture unit 1 for the water shader.
    m_RefractionTexture->SetTexture(1);

    // Set the reflection render texture as the texture to be used in texture unit 2 for the water shader.
    m_ReflectionTexture->SetTexture(2);

    // Render the water model using the water shader.
    m_WaterModel->SetTexture1(0);
    m_WaterModel->Render();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
