////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Camera = 0;
    m_SpecMapShader = 0;
    m_Model = 0;
    m_Light = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char modelFilename[128], textureFilename1[128], textureFilename2[128], textureFilename3[128];
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

    // Create and initialize the spec map shader object.
    m_SpecMapShader = new SpecMapShaderClass;

    result = m_SpecMapShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the specmap shader object." << endl;
	return false;
    }

    // Set the file name of the model.
    strcpy(modelFilename, "../Engine/data/cube.txt");

    // Set the file name of the textures.
    strcpy(textureFilename1, "../Engine/data/stone02.tga");
    strcpy(textureFilename2, "../Engine/data/normal02.tga");
    strcpy(textureFilename3, "../Engine/data/spec02.tga");

    // Create and initialize the model object.
    m_Model = new ModelClass;

    result = m_Model->Initialize(m_OpenGL, modelFilename, textureFilename1, true, textureFilename2, true, textureFilename3, false);
    if(!result)
    {
        cout << "Error: Could not initialize the model object." << endl;
	return false;
    }

    // Create and initialize the light object.
    m_Light = new LightClass;

    m_Light->SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
    m_Light->SetDirection(0.0f, 0.0f, 1.0f);
    m_Light->SetSpecularColor(1.0f, 1.0f, 1.0f, 1.0f);
    m_Light->SetSpecularPower(16.0f);

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the light object.
    if(m_Light)
    {
        delete m_Light;
	m_Light = 0;
    }

    // Release the model object.
    if(m_Model)
    {
        m_Model->Shutdown();
	delete m_Model;
	m_Model = 0;
    }

    // Release the spec map shader object.
    if(m_SpecMapShader)
    {
        m_SpecMapShader->Shutdown();
	delete m_SpecMapShader;
	m_SpecMapShader = 0;
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
    float diffuseLightColor[4], lightDirection[3], cameraPosition[3], specularColor[4];
    float specularPower;
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Rotate the world matrix by the rotation value so that the triangle will spin.
    m_OpenGL->MatrixRotationY(worldMatrix, rotation);

    // Get the light properties.
    m_Light->GetDirection(lightDirection);
    m_Light->GetDiffuseColor(diffuseLightColor);
    m_Light->GetSpecularColor(specularColor);
    m_Light->GetSpecularPower(specularPower);

    // Get the camera position.
    m_Camera->GetPosition(cameraPosition);

    // Set the spec map shader as active and set its parameters.
    result = m_SpecMapShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightDirection, diffuseLightColor, cameraPosition, specularColor, specularPower);
    if(!result)
    {
        return false;
    }

    // Set the color, normal, and specular map textures in the pixel shader.
    m_Model->SetTexture1(0);  // Color
    m_Model->SetTexture2(1);  // Normal
    m_Model->SetTexture2(2);  // Specular Map
    
    // Render the model using the spec map shader.
    m_Model->Render();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
