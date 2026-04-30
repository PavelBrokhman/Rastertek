////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "applicationclass.h"


ApplicationClass::ApplicationClass()
{
    m_OpenGL = 0;
    m_Timer = 0;
    m_Camera = 0;
    m_Model = 0;
    m_Light = 0;
    m_LightShader = 0;
    m_RenderTexture = 0;
    m_FullScreenWindow = 0;
    m_TextureShader = 0;
    m_BlurShader = 0;
    m_Blur = 0;
    m_Heat = 0;
    m_HeatShader = 0;
    m_HeatTexture = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char modelFilename[128], diffuseFilename[128];
    int downSampleWidth, downSampleHeight;
    bool result;


    // Create and initialize the OpenGL object.
    m_OpenGL = new OpenGLClass;

    result = m_OpenGL->Initialize(display, win, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, VSYNC_ENABLED);
    if(!result)
    {
        cout << "Error: Could not initialize the OpenGL object." << endl;
        return false;
    }

    // Create and initialize the timer object.
    m_Timer = new TimerClass;
    m_Timer->Initialize();
    
    // Create and initialize the camera object.
    m_Camera = new CameraClass;

    m_Camera->SetPosition(0.0f, 0.0f, -10.0f);
    m_Camera->Render();
    m_Camera->RenderBaseViewMatrix();

    // Create and initialize the sphere model object.
    m_Model = new ModelClass;

    strcpy(modelFilename, "../Engine/data/sphere.txt");
    strcpy(diffuseFilename, "../Engine/data/yellowcolor01.tga");

    result = m_Model->Initialize(m_OpenGL, modelFilename, diffuseFilename, true, NULL, false, NULL, false);
    if(!result)
    {
        cout << "Error: Could not initialize the model object." << endl;
        return false;
    }

    // Create and initialize the light object.
    m_Light = new LightClass;

    m_Light->SetAmbientLight(0.15f, 0.15f, 0.15f, 1.0f);
    m_Light->SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
    m_Light->SetDirection(0.0f, 0.0f, 1.0f);
    
    // Create and initialize the light shader object.
    m_LightShader = new LightShaderClass;

    result = m_LightShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the light shader object." << endl;
        return false;
    }

    // Create and initialize the render to texture object.
    m_RenderTexture = new RenderTextureClass;

    result = m_RenderTexture->Initialize(m_OpenGL, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, 0);
    if(!result)
    {
        cout << "Error: Could not initialize the render texture object." << endl;
        return false;
    }
    
    // Create and initialize the full screen ortho window object.
    m_FullScreenWindow = new OrthoWindowClass;

    result = m_FullScreenWindow->Initialize(m_OpenGL, screenWidth, screenHeight);
    if(!result)
    {
        cout << "Error: Could not initialize the full screen window object." << endl;
        return false;
    }
    
    // Create and initialize the texture shader object.
    m_TextureShader  = new TextureShaderClass;

    result = m_TextureShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the texture shader object." << endl;
        return false;
    }

    // Create and initialize the blur shader object.
    m_BlurShader  = new BlurShaderClass;

    result = m_BlurShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the blur shader object." << endl;
        return false;
    }

    // Set the size to sample down to.
    downSampleWidth = screenWidth / 2;
    downSampleHeight = screenHeight / 2;
  
    // Create and initialize the blur object.
    m_Blur = new BlurClass;

    result = m_Blur->Initialize(m_OpenGL, downSampleWidth, downSampleHeight, SCREEN_NEAR, SCREEN_DEPTH, screenWidth, screenHeight);
    if(!result)
    {
        cout << "Error: Could not initialize the blur object." << endl;
        return false;
    }
    
    // Create and initialize the heat object.
    m_Heat = new HeatClass;

    result = m_Heat->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the heat object." << endl;
        return false;
    }
    
    // Create the heat shader object.
    m_HeatShader = new HeatShaderClass;

    result = m_HeatShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the heat shader object." << endl;
        return false;
    }
    
    // Create and initialize the heat render to texture object.
    m_HeatTexture = new RenderTextureClass;

    result = m_HeatTexture->Initialize(m_OpenGL, screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH, 0);
    if(!result)
    {
        cout << "Error: Could not initialize the heat render texture object." << endl;
        return false;
    }
    
    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the heat render texture object.
    if(m_HeatTexture)
    {
        m_HeatTexture->Shutdown();
        delete m_HeatTexture;
        m_HeatTexture = 0;
    }

    // Release the heat shader object.
    if(m_HeatShader)
    {
        m_HeatShader->Shutdown();
        delete m_HeatShader;
        m_HeatShader = 0;
    }

    // Release the heat object.
    if(m_Heat)
    {
        m_Heat->Shutdown();
        delete m_Heat;
        m_Heat = 0;
    }

    // Release the blur object.
    if(m_Blur)
    {
        m_Blur->Shutdown();
        delete m_Blur;
        m_Blur = 0;
    }

    // Release the blur shader object.
    if(m_BlurShader)
    {
        m_BlurShader->Shutdown();
        delete m_BlurShader;
        m_BlurShader = 0;
    }

    // Release the texture shader object.
    if(m_TextureShader)
    {
        m_TextureShader->Shutdown();
        delete m_TextureShader;
        m_TextureShader = 0;
    }

    // Release the full screen ortho window object.
    if(m_FullScreenWindow)
    {
        m_FullScreenWindow->Shutdown();
        delete m_FullScreenWindow;
        m_FullScreenWindow = 0;
    }

    // Release the render texture object.
    if(m_RenderTexture)
    {
        m_RenderTexture->Shutdown();
        delete m_RenderTexture;
        m_RenderTexture = 0;
    }

    // Release the light shader object.
    if(m_LightShader)
    {
        m_LightShader->Shutdown();
        delete m_LightShader;
        m_LightShader = 0;
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

    // Release the timer object.
    if(m_Timer)
    {
        delete m_Timer;
        m_Timer = 0;
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
    float frameTime;
    bool result;

  
    // Check if the escape key has been pressed, if so quit.
    if(Input->IsEscapePressed() == true)
    {
        return false;
    }

    // Update the system stats.
    m_Timer->Frame();
    frameTime = m_Timer->GetTime();

    // Do the engine heat frame processing.
    m_Heat->Frame(frameTime);

    // Render the regular scene to a texture.
    result = RenderSceneToTexture();
    if(!result)
    {
        return false;
    }

    // Render the heat map to a texture.
    result = RenderHeatToTexture();
    if(!result)
    {
        return false;
    }

    // Use the blur object to blur the heat map texture.
    result = m_Blur->BlurTexture(m_HeatTexture, m_OpenGL, m_Camera, m_TextureShader, m_BlurShader);
    if(!result)
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


bool ApplicationClass::RenderSceneToTexture()
{
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16];
    float diffuseLightColor[4], lightDirection[3], ambientLight[4];
    bool result;


    // Set the render target to be the render to texture.  Also clear the render to texture to grey.
    m_RenderTexture->SetRenderTarget();
    m_RenderTexture->ClearRenderTarget(0.25f, 0.25f, 0.25f, 1.0f);

    // Get the world, view, and projection matrices from the camera and d3d objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Get the light properties.
    m_Light->GetDirection(lightDirection);
    m_Light->GetDiffuseColor(diffuseLightColor);
    m_Light->GetAmbientLight(ambientLight);
    
    // Render the sphere using the light shader.
    result = m_LightShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightDirection, diffuseLightColor, ambientLight);
    if(!result)
    {
        return false;
    }

    m_Model->SetTexture1(0);
    m_Model->Render();

    // Reset the render target back to the original back buffer and not the render to texture anymore.  Also reset the viewport back to the original.
    m_OpenGL->SetBackBufferRenderTarget();
    m_OpenGL->ResetViewport();

    return true;
}


bool ApplicationClass::RenderHeatToTexture()
{
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16];
    float diffuseLightColor[4], lightDirection[3], ambientLight[4];
    bool result;


    // Set the render target to be the glow render texture and clear it.
    m_HeatTexture->SetRenderTarget();
    m_HeatTexture->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the camera and d3d objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Get the light properties.
    m_Light->GetDirection(lightDirection);
    m_Light->GetDiffuseColor(diffuseLightColor);
    m_Light->GetAmbientLight(ambientLight);
    
    // Render the sphere using the light shader.
    result = m_LightShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightDirection, diffuseLightColor, ambientLight);
    if(!result)
    {
        return false;
    }

    m_Model->SetTexture1(0);
    m_Model->Render();

    // Reset the render target back to the original back buffer and not the render to texture anymore.  Also reset the viewport back to the original.
    m_OpenGL->SetBackBufferRenderTarget();
    m_OpenGL->ResetViewport();

    return true;
}


bool ApplicationClass::Render()
{
    float worldMatrix[16], baseViewMatrix[16], orthoMatrix[16];
    float scrollSpeeds[3], scales[3];
    float distortion1[2], distortion2[2], distortion3[2];
    float emessiveMultiplier, noiseFrameTime;
    bool result;


    // Get the heat noise values.
    m_Heat->GetNoiseValues(scrollSpeeds, scales, distortion1, distortion2, distortion3, emessiveMultiplier, noiseFrameTime);
    
    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and ortho matrices from the camera and d3d objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetBaseViewMatrix(baseViewMatrix);
    m_OpenGL->GetOrthoMatrix(orthoMatrix);

    // Begin 2D rendering and turn off the Z buffer.
    m_OpenGL->TurnZBufferOff();

    // Set the parameters for the heat shader.
    result = m_HeatShader->SetShaderParameters(worldMatrix, baseViewMatrix, orthoMatrix, emessiveMultiplier, noiseFrameTime, scrollSpeeds, scales,
					       distortion1, distortion2, distortion3);
    if(!result)
    {
        return false;
    }

    // Set the three textures for the heat pixel shader.
    m_RenderTexture->SetTexture(0);   // colorTexture - slot 0
    m_HeatTexture->SetTexture(1);     // glowTexture  - slot 1
    m_Heat->SetTexture(m_OpenGL, 2);  // noiseTexture - slot 2

    // Render the full screen ortho window using the heat shader and the rendered to texture resources.
    m_FullScreenWindow->Render();

    // Re-enable the Z buffer after 2D rendering complete.
    m_OpenGL->TurnZBufferOn();
	    
    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
