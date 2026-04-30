////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Camera = 0;
    m_Model = 0;
    m_WindowModel = 0;
    m_RenderTexture = 0;
    m_TextureShader = 0;
    m_GlassShader = 0;
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

    m_Camera->SetPosition(0.0f, 0.0f, -5.0f);
    m_Camera->Render();

    // Set the file name of the model.
    strcpy(modelFilename, "../Engine/data/cube.txt");

    // Set the file name of the textures.
    strcpy(textureFilename1, "../Engine/data/stone01.tga");
    strcpy(textureFilename2, "../Engine/data/normal03.tga");

    // Create and initialize the model object.
    m_Model = new ModelClass;

    result = m_Model->Initialize(m_OpenGL, modelFilename, textureFilename1, false, textureFilename2, false, NULL, false);
    if(!result)
    {
        cout << "Error: Could not initialize the model object." << endl;
        return false;
    }

    // Set the file name of the model.
    strcpy(modelFilename, "../Engine/data/square.txt");

    // Set the file name of the textures.
    strcpy(textureFilename1, "../Engine/data/glass01.tga");
    strcpy(textureFilename2, "../Engine/data/normal03.tga");

    // Create and initialize the model object.
    m_WindowModel = new ModelClass;

    result = m_WindowModel->Initialize(m_OpenGL, modelFilename, textureFilename1, false, textureFilename2, false, NULL, false);
    if(!result)
    {
      cout << "Error: Could not initialize the window model object." << endl;
        return false;
    }

    // Create and initialize the reflection render to texture object.
    m_RenderTexture = new RenderTextureClass;

    result = m_RenderTexture->Initialize(m_OpenGL, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, 0);
    if(!result)
    {
        cout << "Error: Could not initialize the render texture object." << endl;
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

    // Create and initialize the glass shader object.
    m_GlassShader = new GlassShaderClass;

    result = m_GlassShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the glass shader object." << endl;
	return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the glass shader object.
    if(m_GlassShader)
    {
        m_GlassShader->Shutdown();
        delete m_GlassShader;
        m_GlassShader = 0;
    }

    // Release the texture shader object.
    if(m_TextureShader)
    {
        m_TextureShader->Shutdown();
        delete m_TextureShader;
        m_TextureShader = 0;
    }

    // Release the render to texture object.
    if(m_RenderTexture)
    {
        m_RenderTexture->Shutdown();
	delete m_RenderTexture;
	m_RenderTexture = 0;
    }

    // Release the window model object.
    if(m_WindowModel)
    {
        m_WindowModel->Shutdown();
	delete m_WindowModel;
	m_WindowModel = 0;
    }

    // Release the model object.
    if(m_Model)
    {
        m_Model->Shutdown();
        delete m_Model;
        m_Model = 0;
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

    // Render the cube spinning scene to texture first.
    result = RenderSceneToTexture(rotation);
    if(!result)
    {
        return false;
    }

    // Render the graphics scene.
    result = Render(rotation);
    if(!result)
    {
        return false;
    }

    return true;
}


bool ApplicationClass::RenderSceneToTexture(float rotation)
{
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16];
    bool result;


    // Set the render target to be the render to texture and clear it.
    m_RenderTexture->SetRenderTarget();
    m_RenderTexture->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);

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

    // Render the model.
    m_Model->SetTexture1(0);
    m_Model->Render();

    // Reset the render target back to the original back buffer and not the render to texture anymore.  And reset the viewport back to the original.
    m_OpenGL->SetBackBufferRenderTarget();
    m_OpenGL->ResetViewport();

    return true;
}


bool ApplicationClass::Render(float rotation)
{
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16];
    float refractionScale;
    bool result;


    // Set the refraction scale for the glass shader.
    refractionScale = 0.01f;

    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Rotate the world matrix by the rotation value so that the triangle will spin.
    m_OpenGL->MatrixRotationY(worldMatrix, rotation);

    // Set the texture shader as the current shader program and set the matrices that it will use for rendering.
    result = m_TextureShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix);
    if(!result)
    {
        return false;
    }

    // Render the model.
    m_Model->SetTexture1(0);
    m_Model->Render();

    // Translate to back where the window model will be rendered.
    m_OpenGL->MatrixTranslation(worldMatrix, 0.0f, 0.0f, -1.5f);

    // Set the glass shader as the current shader program and set the parameters that it will use for rendering.
    result = m_GlassShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, refractionScale);
    if(!result)
    {
        return false;
    }

    // Set the render texture into texture slot 2 for the glass shader.
    m_RenderTexture->SetTexture(2);
    
    // Render the window model using the glass shader.
    m_WindowModel->SetTexture1(0);
    m_WindowModel->SetTexture2(1);
    m_WindowModel->Render();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
