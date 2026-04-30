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
    m_ShaderManager = 0;
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

    m_Camera->SetPosition(0.0f, 0.0f, -10.0f);
    m_Camera->Render();

    // Set the file name of the model.
    strcpy(modelFilename, "../Engine/data/sphere.txt");

    // Set the file name of the textures.
    strcpy(textureFilename1, "../Engine/data/stone01.tga");
    strcpy(textureFilename2, "../Engine/data/normal01.tga");

    // Create and initialize the model object.
    m_Model = new ModelClass;

    result = m_Model->Initialize(m_OpenGL, modelFilename, textureFilename1, true, textureFilename2, true, NULL, true);
    if(!result)
    {
        cout << "Error: Could not initialize the model object." << endl;
	return false;
    }

    // Create and initialize the light object.
    m_Light = new LightClass;

    m_Light->SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
    m_Light->SetDirection(0.0f, 0.0f, 1.0f);

    // Create and initialize the shader manager object.
    m_ShaderManager = new ShaderManagerClass;

    result = m_ShaderManager->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the shader manager object." << endl;
        return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the shader manager object.
    if(m_ShaderManager)
    {
        m_ShaderManager->Shutdown();
	delete m_ShaderManager;
	m_ShaderManager = 0;
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
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16], rotateMatrix[16], translateMatrix[16];
    float diffuseLightColor[4], lightDirection[3];
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Setup the rotation matrix.
    m_OpenGL->MatrixRotationY(rotateMatrix, rotation);

    // Get the light properties.
    m_Light->GetDirection(lightDirection);
    m_Light->GetDiffuseColor(diffuseLightColor);

    // Setup matrices.
    m_OpenGL->MatrixTranslation(translateMatrix, 0.0f, 1.0f, 0.0f);
    m_OpenGL->MatrixMultiply(worldMatrix, rotateMatrix, translateMatrix);

    // Render the model using the texture shader.
    result = m_ShaderManager->RenderTextureShader(worldMatrix, viewMatrix, projectionMatrix);  if(!result) { return false; }
    m_Model->SetTexture1(0);  // Color
    m_Model->Render();

    // Setup matrices.
    m_OpenGL->MatrixTranslation(translateMatrix, -1.5f, -1.0f, 0.0f);
    m_OpenGL->MatrixMultiply(worldMatrix, rotateMatrix, translateMatrix);

    // Render the model using the light shader.
    result = m_ShaderManager->RenderLightShader(worldMatrix, viewMatrix, projectionMatrix, lightDirection, diffuseLightColor);  if(!result) { return false; }
    m_Model->SetTexture1(0);  // Color
    m_Model->Render();

    // Setup matrices.
    m_OpenGL->MatrixTranslation(translateMatrix, 1.5f, -1.0f, 0.0f);
    m_OpenGL->MatrixMultiply(worldMatrix, rotateMatrix, translateMatrix);

    // Render the model using the normal map shader.
    result = m_ShaderManager->RenderNormalMapShader(worldMatrix, viewMatrix, projectionMatrix, lightDirection, diffuseLightColor);  if(!result) { return false; }
    m_Model->SetTexture1(0);  // Color
    m_Model->SetTexture2(1);  // Normal
    m_Model->Render();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
