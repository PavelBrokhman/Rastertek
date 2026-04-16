////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Camera = 0;
    m_Model = 0;
    m_FireShader = 0;
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
    char textureFilename1[128], textureFilename2[128], textureFilename3[128];
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
    strcpy(modelFilename, "data/square.txt");

    // Set the file name of the textures.
    strcpy(textureFilename1, "data/fire01.tga");
    strcpy(textureFilename2, "data/noise01.tga");
    strcpy(textureFilename3, "data/alpha01.tga");

    // Create and initialize the model object.
    m_Model = new ModelClass;

    result = m_Model->Initialize(m_OpenGL, modelFilename, textureFilename1, false, textureFilename2, true, textureFilename3, false);
    if(!result)
    {
        cout << "Error: Could not initialize the model object." << endl;
        return false;
    }

    // Create and initialize the fire shader object.
    m_FireShader = new FireShaderClass;

    result = m_FireShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the fire shader object." << endl;
        return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the fire shader object.
    if(m_FireShader)
    {
        m_FireShader->Shutdown();
        delete m_FireShader;
        m_FireShader = 0;
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
    float scrollSpeeds[3], scales[3], distortion1[2], distortion2[2], distortion3[2];
    float distortionScale, distortionBias;
    bool result;
    static float frameTime = 0.0f;


    // Increment the frame time counter.
    frameTime += 0.01f;
    if(frameTime > 1000.0f)
    {
        frameTime = 0.0f;
    }

    // Set the three scrolling speeds for the three different noise textures.
    scrollSpeeds[0] = 1.3f;
    scrollSpeeds[1] = 2.1f;
    scrollSpeeds[2] = 2.3f;

    // Set the three scales which will be used to create the three different noise octave textures.
    scales[0] = 1.0f;
    scales[1] = 2.0f;
    scales[2] = 3.0f;

    // Set the three different x and y distortion factors for the three different noise textures.
    distortion1[0] = 0.1f;
    distortion1[1] = 0.2f;

    distortion2[0] = 0.1f;
    distortion2[1] = 0.3f;

    distortion3[0] = 0.1f;
    distortion3[1] = 0.1f;

    // The the scale and bias of the texture coordinate sampling perturbation.
    distortionScale = 0.8f;
    distortionBias = 0.5f;

    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Turn on alpha blending for the fire transparency.
    m_OpenGL->EnableAlphaBlending();

    // Set the fire shader as the current shader program and set the parameters that it will use for rendering.
    result = m_FireShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, frameTime, scrollSpeeds, scales,
                                               distortion1, distortion2, distortion3, distortionScale, distortionBias);
    if(!result)
    {
        return false;
    }

    // Render the square model using the fire shader.
    m_Model->SetTexture1(0);
    m_Model->SetTexture2(1);
    m_Model->SetTexture3(2);
    m_Model->Render();

    // Turn off alpha blending.
    m_OpenGL->DisableAlphaBlending();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
