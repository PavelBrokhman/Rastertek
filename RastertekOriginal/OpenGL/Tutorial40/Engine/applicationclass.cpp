////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Camera = 0;
    m_GroundModel = 0;
    m_CubeModel = 0;
    m_ProjectionShader = 0;
    m_ProjectionTexture = 0;
    m_ViewPoint = 0;
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

    m_Camera->SetPosition(0.0f, 7.0f, -10.0f);
    m_Camera->SetRotation(35.0f, 0.0f, 0.0f);
    m_Camera->Render();

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

    // Create and initialize the cube model object.
    m_CubeModel = new ModelClass;

    strcpy(modelFilename, "../Engine/data/cube.txt");
    strcpy(textureFilename, "../Engine/data/stone01.tga");

    result = m_CubeModel->Initialize(m_OpenGL, modelFilename, textureFilename, false, NULL, false, NULL, false);
    if(!result)
    {
        cout << "Error: Could not initialize the cube model object." << endl;
        return false;
    }

    // Create and initialize the projection shader object.
    m_ProjectionShader = new ProjectionShaderClass;

    result = m_ProjectionShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the projection shader object." << endl;
        return false;
    }

    // Create and initialize the projection texture object.
    m_ProjectionTexture = new TextureClass;

    strcpy(textureFilename, "../Engine/data/grate.tga");

    result = m_ProjectionTexture->Initialize(m_OpenGL, textureFilename, false);
    if(!result)
    {
        cout << "Error: Could not initialize the projection texture object." << endl;
        return false;
    }

    // Create and initialize the view point object.
    m_ViewPoint = new ViewPointClass;

    m_ViewPoint->SetPosition(2.0f, 5.0f, -2.0f);
    m_ViewPoint->SetLookAt(0.0f, 0.0f, 0.0f);
    m_ViewPoint->SetProjectionParameters((3.14159265358979323846f / 2.0f), 1.0f, 0.1f, 100.0f);
    m_ViewPoint->GenerateViewMatrix();
    m_ViewPoint->GenerateProjectionMatrix();
    
    // Create and initialize the light object.
    m_Light = new LightClass;

    m_Light->SetAmbientLight(0.15f, 0.15f, 0.15f, 1.0f);
    m_Light->SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
    m_Light->SetPosition(2.0f, 5.0f, -2.0f);

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

    // Release the view point object.
    if(m_ViewPoint)
    {
        delete m_ViewPoint;
	m_ViewPoint = 0;
    }

    // Release the projection texture object.
    if(m_ProjectionTexture)
    {
        m_ProjectionTexture->Shutdown();
        delete m_ProjectionTexture;
        m_ProjectionTexture = 0;
    }

    // Release the projection shader object.
    if(m_ProjectionShader)
    {
        m_ProjectionShader->Shutdown();
        delete m_ProjectionShader;
        m_ProjectionShader = 0;
    }

    // Release the cube model object.
    if(m_CubeModel)
    {
        m_CubeModel->Shutdown();
        delete m_CubeModel;
        m_CubeModel = 0;
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
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16], viewMatrix2[16], projectionMatrix2[16];
    float diffuseColor[4], ambientColor[4], lightPosition[3];
    float brightness;
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Get the secondary view and projection matrices from the view point object.
    m_ViewPoint->GetViewMatrix(viewMatrix2);
    m_ViewPoint->GetProjectionMatrix(projectionMatrix2);

    // Get the light properties.
    m_Light->GetDiffuseColor(diffuseColor);
    m_Light->GetAmbientLight(ambientColor);
    m_Light->GetPosition(lightPosition);
    brightness = 1.5f;

    // Setup the translation for the ground model.
    m_OpenGL->MatrixTranslation(worldMatrix, 0.0f, 1.0f, 0.0f);

    // Set the projection shader as the current shader program and set the parameters that it will use for rendering.
    result = m_ProjectionShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, viewMatrix2, projectionMatrix2,
                                                     diffuseColor, ambientColor, lightPosition, brightness);
    if(!result)
    {
        return false;
    }

    // Set the projection texture in texture unit 1.
    m_ProjectionTexture->SetTexture(m_OpenGL, 1);

    // Render the model.
    m_GroundModel->SetTexture1(0);
    m_GroundModel->Render();

    // Setup the translation for the cube model.
    m_OpenGL->MatrixTranslation(worldMatrix, 0.0f, 2.0f, 0.0f);

    // Set the projection shader as the current shader program and set the parameters that it will use for rendering.
    result = m_ProjectionShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, viewMatrix2, projectionMatrix2,
                                                     diffuseColor, ambientColor, lightPosition, brightness);
    if(!result)
    {
        return false;
    }

    // Set the projection texture in texture unit 1.
    m_ProjectionTexture->SetTexture(m_OpenGL, 1);

    // Render the model.
    m_CubeModel->SetTexture1(0);
    m_CubeModel->Render();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
