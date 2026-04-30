////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Model = 0;
    m_Camera = 0;
    m_LightShader = 0;
    m_Lights = 0;
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

    m_Camera->SetPosition(0.0f, 2.0f, -12.0f);
    m_Camera->Render();

    // Set the file name of the model.
    strcpy(modelFilename, "../Engine/data/plane.txt");

    // Set the file name of the texture.
    strcpy(textureFilename, "../Engine/data/stone01.tga");

    // Create and initialize the model object.
    m_Model = new ModelClass;

    result = m_Model->Initialize(m_OpenGL, modelFilename, textureFilename, false);
    if(!result)
    {
        cout << "Error: Could not initialize the model object." << endl;
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

    // Set the number of lights we will use.
    m_numLights = 4;

    // Create and initialize the light objects array.
    m_Lights = new LightClass[m_numLights];

    // Manually set the color and position of each light.
    m_Lights[0].SetDiffuseColor(1.0f, 0.0f, 0.0f, 1.0f);  // Red
    m_Lights[0].SetPosition(-3.0f, 1.0f, 3.0f);

    m_Lights[1].SetDiffuseColor(0.0f, 1.0f, 0.0f, 1.0f);  // Green
    m_Lights[1].SetPosition(3.0f, 1.0f, 3.0f);

    m_Lights[2].SetDiffuseColor(0.0f, 0.0f, 1.0f, 1.0f);  // Blue
    m_Lights[2].SetPosition(-3.0f, 1.0f, -3.0f);
    
    m_Lights[3].SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);  // White
    m_Lights[3].SetPosition(3.0f, 1.0f, -3.0f);

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the light objects array.
    if(m_Lights)
    {
        delete [] m_Lights;
        m_Lights = 0;
    }

    // Release the light shader object.
    if(m_LightShader)
    {
        m_LightShader->Shutdown();
        delete m_LightShader;
        m_LightShader = 0;
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
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16];
    float diffuseColor[4], diffuseColorArray[16];
    float lightPosition[3], lightPositionArray[12];
    int i, j, k;
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Get the light properties.
    j=0;
    k=0;
    for(i=0; i<m_numLights; i++)
    {
        // Get the color and position of each light.
        m_Lights[i].GetDiffuseColor(diffuseColor);
        m_Lights[i].GetPosition(lightPosition);

        // Copy the individual light data into the single larger arrays.
        diffuseColorArray[j+0] = diffuseColor[0];
        diffuseColorArray[j+1] = diffuseColor[1];
        diffuseColorArray[j+2] = diffuseColor[2];
        diffuseColorArray[j+3] = diffuseColor[3];
        j+=4;

        lightPositionArray[k+0] = lightPosition[0];
        lightPositionArray[k+1] = lightPosition[1];
        lightPositionArray[k+2] = lightPosition[2];
        k+=3;
    }

    // Set the light shader as the current shader program and set the matrices and light value arrays that it will use for rendering, including the number of lights.
    result = m_LightShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, diffuseColorArray, lightPositionArray, m_numLights);
    if(!result)
    {
        return false;
    }

    // Set the texture for the model in the pixel shader.
    m_Model->SetTexture(0);
    
    // Render the model.
    m_Model->Render();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
