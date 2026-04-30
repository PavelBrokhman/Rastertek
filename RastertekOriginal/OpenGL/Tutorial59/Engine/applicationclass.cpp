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
    m_LightShader = 0;
    m_RenderTexture = 0;
    m_FullScreenWindow = 0;
    m_ColorLUT = 0;
    m_ColorGradeShader = 0;
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
    m_Camera->RenderBaseViewMatrix();

    // Create and initialize the cube model object.
    m_Model = new ModelClass;

    strcpy(modelFilename, "../Engine/data/cube.txt");
    strcpy(diffuseFilename, "../Engine/data/stone01.tga");

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

    // Create and initialize the color lookup table object.
	m_ColorLUT = new ClutClass;

	result = m_ColorLUT->Initialize(m_OpenGL);
	if(!result)
	{
        cout << "Error: Could not initialize the CLUT object." << endl;
		return false;
	}

	// Create and initialize the color grade shader object.
    m_ColorGradeShader  = new ColorGradeShaderClass;

    result = m_ColorGradeShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the color grade shader object." << endl;
        return false;
    }

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the color grade shader object.
    if(m_ColorGradeShader)
    {
        m_ColorGradeShader->Shutdown();
        delete m_ColorGradeShader;
        m_ColorGradeShader = 0;
    }

    // Release the color lookup table object.
    if(m_ColorLUT)
    {
        m_ColorLUT->Shutdown(m_OpenGL);
        delete m_ColorLUT;
        m_ColorLUT = 0;
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

    // Render the regular scene to a texture.
    result = RenderSceneToTexture(rotation);
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


bool ApplicationClass::RenderSceneToTexture(float rotation)
{
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16];
    float diffuseLightColor[4], lightDirection[3], ambientLight[4];
    bool result;


    // Set the render target to be the render to texture.
    m_RenderTexture->SetRenderTarget();
    m_RenderTexture->ClearRenderTarget(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the camera and d3d objects.
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);

    // Rotate the world matrix by the rotation value so that the cube will spin.
    m_OpenGL->MatrixRotationY(worldMatrix, rotation);

    // Get the light properties.
    m_Light->GetDirection(lightDirection);
    m_Light->GetDiffuseColor(diffuseLightColor);
    m_Light->GetAmbientLight(ambientLight);

    // Render the cube using the light shader.
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
    bool result;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and ortho matrices from the camera and OpenGL objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetBaseViewMatrix(baseViewMatrix);
    m_OpenGL->GetOrthoMatrix(orthoMatrix);

    // Begin 2D rendering and turn off the Z buffer.
    m_OpenGL->TurnZBufferOff();

    // Set the color grade shader parameters.
    result = m_ColorGradeShader->SetShaderParameters(worldMatrix, baseViewMatrix, orthoMatrix);
    if(!result)
    {
        return false;
    }

    // Set the render texture and 3D CLUT texture as the two textures resources in the color grade shader.
    m_RenderTexture->SetTexture(0);
    m_ColorLUT->SetTexture(m_OpenGL, 1);

    // Render the full screen ortho window to perform 2D post processing on the render texture using the color grade shader and the CLUT 3D texture data.
    m_FullScreenWindow->Render();

    // Re-enable the Z buffer after 2D rendering complete.
    m_OpenGL->TurnZBufferOn();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}
