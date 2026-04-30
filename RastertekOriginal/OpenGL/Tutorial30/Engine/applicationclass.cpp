////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_CubeModel = 0;
    m_FloorModel = 0;
    m_Camera = 0;
    m_TextureShader = 0;
    m_ReflectionShader = 0;
    m_RenderTexture = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char modelFilename1[128], modelFilename2[128];
    char textureFilename1[128], textureFilename2[128];
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

    // Set the file names of the models.
    strcpy(modelFilename1, "../Engine/data/cube.txt");
    strcpy(modelFilename2, "../Engine/data/floor.txt");

    // Set the file names of the textures.
    strcpy(textureFilename1, "../Engine/data/stone01.tga");
    strcpy(textureFilename2, "../Engine/data/blue01.tga");

    // Create and initialize the cube model object.
    m_CubeModel = new ModelClass;

    result = m_CubeModel->Initialize(m_OpenGL, modelFilename1, textureFilename1, true, NULL, true, NULL, true);
    if(!result)
    {
        cout << "Error: Could not initialize the cube model object." << endl;
	return false;
    }

    // Create and initialize the floor model object.
    m_FloorModel = new ModelClass;

    result = m_FloorModel->Initialize(m_OpenGL, modelFilename2, textureFilename2, true, NULL, true, NULL, true);
    if(!result)
    {
        cout << "Error: Could not initialize the floor model object." << endl;
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

    // Create and initialize the reflection shader object.
    m_ReflectionShader = new ReflectionShaderClass;

    result = m_ReflectionShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the reflection shader object." << endl;
        return false;
    }

    // Create and initialize the render to texture object.
    m_RenderTexture = new RenderTextureClass;

    result = m_RenderTexture->Initialize(m_OpenGL, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, 0);
    if(!result)
    {
      
        return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the render to texture object.
    if(m_RenderTexture)
    {
        m_RenderTexture->Shutdown();
        delete m_RenderTexture;
        m_RenderTexture = 0;
    }

    // Release the reflection shader object.
    if(m_ReflectionShader)
    {
        m_ReflectionShader->Shutdown();
        delete m_ReflectionShader;
        m_ReflectionShader = 0;
    }

    // Release the texture shader object.
    if(m_TextureShader)
    {
        m_TextureShader->Shutdown();
        delete m_TextureShader;
        m_TextureShader = 0;
    }

    // Release the floor model object.
    if(m_FloorModel)
    {
        m_FloorModel->Shutdown();
        delete m_FloorModel;
        m_FloorModel = 0;
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
    static float rotation = 360.0f;
    bool result;


    // Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

    // Update the rotation variable each frame.
    rotation -= 0.0174532925f * 1.0f;
    if(rotation <= 0.0f)
    {
        rotation += 360.0f;
    }

    // Render the entire scene as a reflection to the texture first.
    result = RenderReflectionToTexture(rotation);
    if(!result)
    {
        return false;
    }

    // Render the final graphics scene to the back buffer.
    result = Render(rotation);
    if(!result)
    {
        return false;
    }

    return true;
}


bool ApplicationClass::RenderReflectionToTexture(float rotation)
{
    float worldMatrix[16], reflectionViewMatrix[16], projectionMatrix[16];
    bool result;


    // Set the render target to be the render texture and clear it.
    m_RenderTexture->SetRenderTarget();
    m_RenderTexture->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);

    // Use the camera to calculate the reflection matrix.
    m_Camera->RenderReflection(-1.5f);

    // Get the reflection view matrix.
    m_Camera->GetReflectionViewMatrix(reflectionViewMatrix);

    // Get the camera reflection view matrix instead of the regular camera view matrix.  Also good practice to use the projection matrix from the render texture.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_RenderTexture->GetProjectionMatrix(projectionMatrix);

    // Rotate the world matrix by the rotation value so that the cube will spin.
    m_OpenGL->MatrixRotationY(worldMatrix, rotation);

    // Set the texture shader as the current shader program and set the matrices that it will use for rendering.
    result = m_TextureShader->SetShaderParameters(worldMatrix, reflectionViewMatrix, projectionMatrix);
    if(!result)
    {
        return false;
    }

    // Render the cube model using the texture shader and the reflection view matrix.
    m_CubeModel->SetTexture1(0);
    m_CubeModel->Render();

    // Reset the render target back to the original back buffer and not the render to texture anymore.  And reset the viewport back to the original.
    m_OpenGL->SetBackBufferRenderTarget();
    m_OpenGL->ResetViewport();

    return true;
}


bool ApplicationClass::Render(float rotation)
{
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16], reflectionViewMatrix[16];
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Rotate the world matrix by the rotation value so that the cube will spin.
    m_OpenGL->MatrixRotationY(worldMatrix, rotation);

    // Set the texture shader as the current shader program and set the matrices that it will use for rendering.
    result = m_TextureShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix);
    if(!result)
    {
        return false;
    }

    // Render the cube model as normal with the texture shader.
    m_CubeModel->SetTexture1(0);
    m_CubeModel->Render();

    // Now get the world matrix again and translate down for the floor model to render underneath the cube.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_OpenGL->MatrixTranslation(worldMatrix, 0.0f, -1.5f, 0.0f);

    // Get the camera reflection view matrix for the reflection shader.
    m_Camera->GetReflectionViewMatrix(reflectionViewMatrix);

    // Render the floor model using the reflection shader, reflection texture, and reflection view matrix.
    result = m_ReflectionShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, reflectionViewMatrix);
    if(!result)
    {
        return false;
    }

    // Set the render texture as the texture to be used in texture unit 1 for the reflection shader.
    m_RenderTexture->SetTexture(1);

    // Render the floor model using the reflection shader and using texture unit 0 for the floor texture and texture unit 1 for the reflection render texture.
    m_FloorModel->SetTexture1(0);
    m_FloorModel->Render();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
