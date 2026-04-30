////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Camera = 0;
    m_TextureShader = 0;
    m_FloorModel = 0;
    m_BillboardModel = 0;
    m_Position = 0;
    m_Timer = 0;
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

    // Create and initialize the texture shader object.
    m_TextureShader = new TextureShaderClass;

    result = m_TextureShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the texture shader object." << endl;
        return false;
    }

    // Set the filenames for the floor model object.
    strcpy(modelFilename, "../Engine/data/floor.txt");
    strcpy(textureFilename, "../Engine/data/grid01.tga");

    // Create and initialize the floor model object.
    m_FloorModel = new ModelClass;

    result = m_FloorModel->Initialize(m_OpenGL, modelFilename, textureFilename, false, NULL, false, NULL, false);
    if(!result)
    {
        cout << "Error: Could not initialize the floor model object." << endl;
        return false;
    }

    // Set the filenames for the billboard model object.
    strcpy(modelFilename, "../Engine/data/square.txt");
    strcpy(textureFilename, "../Engine/data/stone01.tga");

    // Create and initialize the billboard model object.
    m_BillboardModel = new ModelClass;

    result = m_BillboardModel->Initialize(m_OpenGL, modelFilename, textureFilename, false, NULL, false, NULL, false);
    if(!result)
    {
        cout << "Error: Could not initialize the billboard model object." << endl;
        return false;
    }

    // Create the position object and set the initial viewing position.
    m_Position = new PositionClass;
    m_Position->SetPosition(0.0f, 1.5f, -11.0f);

    // Create and initialize the timer object.
    m_Timer = new TimerClass;
    m_Timer->Initialize();

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the timer object.
    if(m_Timer)
    {
        delete m_Timer;
        m_Timer = 0;
    }

    // Release the position object.
    if(m_Position)
    {
        delete m_Position;
        m_Position = 0;
    }

    // Release the billboard model object.
    if(m_BillboardModel)
    {
        m_BillboardModel->Shutdown();
        delete m_BillboardModel;
        m_BillboardModel = 0;
    }

    // Release the floor model object.
    if(m_FloorModel)
    {
        m_FloorModel->Shutdown();
        delete m_FloorModel;
        m_FloorModel = 0;
    }

    // Release the texture shader object.
    if(m_TextureShader)
    {
        m_TextureShader->Shutdown();
        delete m_TextureShader;
        m_TextureShader = 0;
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
    float positionX, positionY, positionZ;
    bool result, keyDown;


    // Update the system stats.
    m_Timer->Frame();

    // Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

    // Set the frame time for calculating the updated position.
    m_Position->SetFrameTime(m_Timer->GetTime());

    // Check if the user is pressing the left or right arrow keys and update the position object accordingly.
    keyDown = Input->IsLeftArrowPressed();
    m_Position->MoveLeft(keyDown);

    keyDown = Input->IsRightArrowPressed();
    m_Position->MoveRight(keyDown);

    // Get the current view point position
    m_Position->GetPosition(positionX, positionY, positionZ);

    // Set the position of the camera.
    m_Camera->SetPosition(positionX, positionY, positionZ);
    m_Camera->Render();

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
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16], rotateMatrix[16], translateMatrix[16];
    float cameraPosition[3], modelPosition[3];
    double angle;
    float pi, rotation;
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Set the texture shader as the current shader program and set the parameters that it will use for rendering.
    result = m_TextureShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix);
    if(!result)
    {
        return false;
    }

    // Render the floor model using the texture shader.
    m_FloorModel->SetTexture1(0);
    m_FloorModel->Render();

    // Get the position of the camera.
    m_Camera->GetPosition(cameraPosition);

    // Set the position of the billboard model.
    modelPosition[0] = 0.0f;
    modelPosition[1] = 1.5f;
    modelPosition[2] = 0.0f;

    // Calculate the rotation angle that needs to be applied to the billboard model to face the current camera position using the arc tangent function.
    pi = 3.14159265358979323846f;
    angle = atan2(modelPosition[0] - cameraPosition[0], modelPosition[2] - cameraPosition[2]) * (180.0f / pi);

    // Convert rotation angle into radians.
    rotation = (float)angle * 0.0174532925f;

    // Setup the rotation matrix for the billboard model.
    m_OpenGL->MatrixRotationY(rotateMatrix, rotation);

    // Setup the translation matrix for the billboard model.
    m_OpenGL->MatrixTranslation(translateMatrix, modelPosition[0], modelPosition[1], modelPosition[2]);

    // Finally combine the rotation and translation matrices to create the final world matrix for the billboard model.
    m_OpenGL->MatrixMultiply(worldMatrix, rotateMatrix, translateMatrix);

    // Set the texture shader as the current shader program and set the parameters that it will use for rendering.
    result = m_TextureShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix);
    if(!result)
    {
        return false;
    }

    // Render the bilboard model using the texture shader.
    m_BillboardModel->SetTexture1(0);
    m_BillboardModel->Render();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
