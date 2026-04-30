////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Model = 0;
    m_Camera = 0;
    m_FogShader = 0;
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

    // Create and initialize the camera object.
    m_Camera = new CameraClass;

    m_Camera->SetPosition(0.0f, 0.0f, -10.0f);
    m_Camera->Render();

    // Set the file name of the model.
    strcpy(modelFilename, "../Engine/data/cube.txt");

    // Set the file name of the texture.
    strcpy(textureFilename, "../Engine/data/stone01.tga");

    // Create and initialize the model object.
    m_Model = new ModelClass;

    result = m_Model->Initialize(m_OpenGL, modelFilename, textureFilename, false, NULL, false, NULL, false);
    if(!result)
    {
        cout << "Error: Could not initialize the model object." << endl;
	return false;
    }

    // Create and initialize the fog shader object.
    m_FogShader = new FogShaderClass;

    result = m_FogShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the fog shader object." << endl;
        return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the fog shader object.
    if(m_FogShader)
    {
        m_FogShader->Shutdown();
        delete m_FogShader;
        m_FogShader = 0;
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

    // Render the graphics scene.
    result = Render(rotation);
    if(!result)
    {
        return false;
    }

    return true;
}


bool ApplicationClass::Render(float rotation)
{
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16], fogPosition[2];
    float fogColor, fogStart, fogEnd;
    bool result;


    // Set the color of the fog to grey.
    fogColor = 0.5f;

    // Set the start and end of the fog.
    fogStart = 0.0f;
    fogEnd = 10.0f;

    fogPosition[0] = fogStart;
    fogPosition[1] = fogEnd;

    // Clear the buffers to the color of fog to begin the scene.
    m_OpenGL->BeginScene(fogColor, fogColor, fogColor, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Rotate the world matrix by the rotation value so that the cube will spin.
    m_OpenGL->MatrixRotationY(worldMatrix, rotation);

    // Set the fog shader as the current shader program and set the matrices and fog position that it will use for rendering.
    result = m_FogShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, fogPosition);
    if(!result)
    {
        return false;
    }

    // Render the model.
    m_Model->SetTexture1(0);
    m_Model->Render();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
