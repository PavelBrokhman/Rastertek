////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Model1 = 0;
    m_Model2 = 0;
    m_Camera = 0;
    m_TextureShader = 0;
    m_TransparentShader = 0;
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
    strcpy(modelFilename, "../Engine/data/square.txt");

    // Set the file names of the textures.
    strcpy(textureFilename1, "../Engine/data/dirt01.tga");
    strcpy(textureFilename2, "../Engine/data/stone01.tga");

    // Create and initialize the first model object that will use the dirt texture.
    m_Model1 = new ModelClass;

    result = m_Model1->Initialize(m_OpenGL, modelFilename, textureFilename1, false, NULL, false, NULL, false);
    if(!result)
    {
        cout << "Error: Could not initialize the model 1 object." << endl;
	return false;
    }

    // Create and initialize the second model object that will use the stone texture.
    m_Model2 = new ModelClass;

    result = m_Model2->Initialize(m_OpenGL, modelFilename, textureFilename2, false, NULL, false, NULL, false);
    if(!result)
    {
        cout << "Error: Could not initialize the model 2 object." << endl;
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

    // Create and initialize the transparent shader object.
    m_TransparentShader = new TransparentShaderClass;

    result = m_TransparentShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the transparent shader object." << endl;
	return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the transparent shader object.
    if(m_TransparentShader)
    {
        m_TransparentShader->Shutdown();
        delete m_TransparentShader;
        m_TransparentShader = 0;
    }

    // Release the texture shader object.
    if(m_TextureShader)
    {
        m_TextureShader->Shutdown();
        delete m_TextureShader;
        m_TextureShader = 0;
    }

    // Release the model objects.
    if(m_Model2)
    {
        m_Model2->Shutdown();
        delete m_Model2;
        m_Model2 = 0;
    }

    if(m_Model1)
    {
        m_Model1->Shutdown();
        delete m_Model1;
        m_Model1 = 0;
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
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16];
    float blendAmount;
    bool result;


    // Set the blending amount to 50%.
    blendAmount = 0.5f;

    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Set the texture shader as the current shader program and set the matrices that it will use for rendering.
    result = m_TextureShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix);
    if(!result)
    {
        return false;
    }

    // Render the first model that is using the dirt texture using the regular texture shader.
    m_Model1->SetTexture1(0);
    m_Model1->Render();

    // Translate to the right by one unit and towards the camera by one unit.
    m_OpenGL->MatrixTranslation(worldMatrix, 1.0f, 0.0f, -1.0f);

    // Turn on alpha blending for the transparency to work.
    m_OpenGL->EnableAlphaBlending();

    // Render the second square model with the stone texture and use the 50% blending amount for transparency.
    result = m_TransparentShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, blendAmount);
    if(!result)
    {
        return false;
    }

    // Render the model.
    m_Model2->SetTexture1(0);
    m_Model2->Render();

    // Turn off alpha blending.
    m_OpenGL->DisableAlphaBlending();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
