////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Camera = 0;
    m_MultiTextureShader = 0;
    m_Model = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char modelFilename[128], textureFilename1[128], textureFilename2[128];
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

    // Create and initialize the multitexture shader object.
    m_MultiTextureShader = new MultiTextureShaderClass;

    result = m_MultiTextureShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the multitexture shader object." << endl;
        return false;
    }

    // Set the file name of the model.
    strcpy(modelFilename, "../Engine/data/square.txt");

    // Set the file name of the textures.
    strcpy(textureFilename1, "../Engine/data/stone01.tga");
    strcpy(textureFilename2, "../Engine/data/dirt01.tga");

    // Create and initialize the model object.
    m_Model = new ModelClass;

    result = m_Model->Initialize(m_OpenGL, modelFilename, textureFilename1, true, textureFilename2, true);
    if(!result)
    {
        cout << "Error: Could not initialize the model object." << endl;
	return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the model object.
    if(m_Model)
    {
        m_Model->Shutdown();
	delete m_Model;
	m_Model = 0;
    }

    // Release the multitexture shader object.
    if(m_MultiTextureShader)
    {
        m_MultiTextureShader->Shutdown();
	delete m_MultiTextureShader;
	m_MultiTextureShader = 0;
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
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Set the multitexture shader as active and set its parameters.
    result = m_MultiTextureShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix);
    if(!result)
    {
        return false;
    }

    // Set the two textures for the model in the pixel shader in texture unit 0 and 1.
    m_Model->SetTexture1(0);
    m_Model->SetTexture2(1);
    
    // Render the model using the multitexture shader.
    m_Model->Render();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
