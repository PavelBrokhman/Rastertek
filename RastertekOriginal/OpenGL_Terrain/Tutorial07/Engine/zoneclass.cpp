////////////////////////////////////////////////////////////////////////////////
// Filename: zoneclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "zoneclass.h"


ZoneClass::ZoneClass()
{
    m_Camera = 0;
    m_Position = 0;
    m_Terrain = 0;
    m_Light = 0;
}


ZoneClass::ZoneClass(const ZoneClass& other)
{
}


ZoneClass::~ZoneClass()
{
}


bool ZoneClass::Initialize(OpenGLClass* OpenGL)
{
    char configFilename[256];
    bool result;


    // Create and initialize the camera object.
    m_Camera = new CameraClass;

    m_Camera->SetPosition(0.0f, 0.0f, -10.0f);
    m_Camera->Render();
    m_Camera->RenderBaseViewMatrix();

    // Create and initialize the position object.
    m_Position = new PositionClass;

    m_Position->SetPosition(500.0f, 50.0f, -10.0f);
    m_Position->SetRotation(0.0f, 0.0f, 0.0f);

    // Create and initialize the terrain object.
    m_Terrain = new TerrainClass;

    strcpy(configFilename, "../Engine/data/setup.txt");

    result = m_Terrain->Initialize(OpenGL, configFilename);
    if(!result)
    {
        cout << "Error: Could not initialize the terrain object." << endl;
        return false;
    }

    // Create and initialize the light object.
    m_Light = new LightClass;

    m_Light->SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
    m_Light->SetDirection(-0.5f, -1.0f, -0.5f);

    return true;
}


void ZoneClass::Shutdown(OpenGLClass* OpenGL)
{
    // Release the light object.
    if(m_Light)
    {
        delete m_Light;
        m_Light = 0;
    }

    // Release the terrain object.
    if(m_Terrain)
    {
        m_Terrain->Shutdown(OpenGL);
        delete m_Terrain;
        m_Terrain = 0;
    }

    // Release the position object.
    if(m_Position)
    {
        delete m_Position;
        m_Position = 0;
    }

    // Release the camera object.
    if(m_Camera)
    {
        delete m_Camera;
        m_Camera = 0;
    }

    return;
}


bool ZoneClass::Frame(OpenGLClass* OpenGL, ShaderManagerClass* ShaderManager, FontClass* Font, UserInterfaceClass* UserInterface, InputClass* Input, float frameTime)
{
    float posX, posY, posZ, rotX, rotY, rotZ;
    bool result;


    // Do the frame input processing for the movement.
    HandleMovementInput(Input, frameTime);

    // Get the view point position/rotation.
    m_Position->GetPosition(posX, posY, posZ);
    m_Position->GetRotation(rotX, rotY, rotZ);

    // Update the UI with the position.
    UserInterface->UpdatePositonStrings(Font, posX, posY, posZ, rotX, rotY, rotZ);

    // Render the zone.
    result = Render(OpenGL, ShaderManager, Font, UserInterface);
    if(!result)
    {
        return false;
    }

    return true;
}


bool ZoneClass::Render(OpenGLClass* OpenGL, ShaderManagerClass* ShaderManager, FontClass* Font, UserInterfaceClass* UserInterface)
{
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16], baseViewMatrix[16], orthoMatrix[16];
    bool result;


    // Get the world, view, and ortho matrices from the opengl and camera objects.
    OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    OpenGL->GetProjectionMatrix(projectionMatrix);
    m_Camera->GetBaseViewMatrix(baseViewMatrix);
    OpenGL->GetOrthoMatrix(orthoMatrix);

    // Clear the scene.
    OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Render the terrain using the terrain shader.
    result = m_Terrain->Render(OpenGL, ShaderManager, m_Light, worldMatrix, viewMatrix, projectionMatrix);
    if(!result)
    {
        return false;
    }

    // Render the user interface.
    result = UserInterface->Render(OpenGL, ShaderManager, Font, worldMatrix, baseViewMatrix, orthoMatrix);
    if(!result)
    {
        return false;
    }

    // Present the rendered scene to the screen.
    OpenGL->EndScene();

    return true;
}


void ZoneClass::HandleMovementInput(InputClass* Input, float frameTime)
{
    float posX, posY, posZ, rotX, rotY, rotZ;
    bool keyDown;


    // Set the frame time for calculating the updated position.
    m_Position->SetFrameTime(frameTime);

    // Check if the input keys have been pressed.  If so, then update the position accordingly.
    keyDown = Input->IsLeftPressed();
    m_Position->TurnLeft(keyDown);

    keyDown = Input->IsRightPressed();
    m_Position->TurnRight(keyDown);

    keyDown = Input->IsUpPressed();
    m_Position->MoveForward(keyDown);

    keyDown = Input->IsDownPressed();
    m_Position->MoveBackward(keyDown);

    keyDown = Input->IsAPressed();
    m_Position->MoveUpward(keyDown);

    keyDown = Input->IsZPressed();
    m_Position->MoveDownward(keyDown);

    keyDown = Input->IsPgUpPressed();
    m_Position->LookUpward(keyDown);

    keyDown = Input->IsPgDownPressed();
    m_Position->LookDownward(keyDown);

    // Get the view point position/rotation.
    m_Position->GetPosition(posX, posY, posZ);
    m_Position->GetRotation(rotX, rotY, rotZ);

    // Set the position of the camera.
    m_Camera->SetPosition(posX, posY, posZ);
    m_Camera->SetRotation(rotX, rotY, rotZ);

    // Update the camera view matrix for rendering.
    m_Camera->Render();

    return;
}
