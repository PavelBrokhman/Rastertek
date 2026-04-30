////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Camera = 0;
    m_Model = 0;
    m_Light = 0;
    m_PbrShader = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char modelFilename[128], diffuseFilename[128], normalFilename[128], rmFilename[128];
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

    // Create and initialize the light object.
    m_Light = new LightClass;

    m_Light->SetDirection(0.5f, 0.5f, 0.5f);

    // Create and initialize the sphere model object.
    m_Model = new ModelClass;

    strcpy(modelFilename, "../Engine/data/sphere.txt");

    strcpy(diffuseFilename, "../Engine/data/pbr_albedo.tga");
    strcpy(normalFilename, "../Engine/data/pbr_normal.tga");
    strcpy(rmFilename, "../Engine/data/pbr_roughmetal.tga");

    result = m_Model->Initialize(m_OpenGL, modelFilename, diffuseFilename, true, normalFilename, true, rmFilename, true);
    if(!result)
    {
        cout << "Error: Could not initialize the model object." << endl;
        return false;
    }
    
    // Create and initialize the PBR shader object.
    m_PbrShader = new PbrShaderClass;

    result = m_PbrShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the PBR shader object." << endl;
        return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the PBR shader object.
    if(m_PbrShader)
    {
        m_PbrShader->Shutdown();
        delete m_PbrShader;
        m_PbrShader = 0;
    }

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
    static float rotation = 0.0f;
    bool result;

  
    // Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

    // Update the rotation variable each frame.
    rotation -= 0.0174532925f * 0.1f;
    if(rotation < 0.0f)
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
    float cameraPosition[3], lightDirection[3];
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Rotate the world matrix by the rotation value so that the model will spin.
    m_OpenGL->MatrixRotationY(worldMatrix, rotation);

    // Get the camera position.
    m_Camera->GetPosition(cameraPosition);
    
    // Get the light properties.
    m_Light->GetDirection(lightDirection);
    
    // Set the PBR shader as the current shader program and set the parameters that it will use for rendering.
    result = m_PbrShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, cameraPosition, lightDirection);
    if(!result)
    {
	return false;
    }

    // Set the three textures for the PBR pixel shader.
    m_Model->SetTexture1(0);  // Diffuse
    m_Model->SetTexture2(1);  // Normal
    m_Model->SetTexture3(2);  // Roughess and Metalness

    // Render the model using the PBR shader.
    m_Model->Render();
	    
    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
