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
    m_FontShader = 0;
    m_Font = 0;
    m_RenderCountString = 0;
    m_Position = 0;
    m_Timer = 0;
    m_ModelList = 0;
    m_Frustum = 0;
}


ApplicationClass::ApplicationClass(const ApplicationClass& other)
{
}


ApplicationClass::~ApplicationClass()
{
}


bool ApplicationClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight)
{
    char modelFilename[128], textureFilename[128], renderString[32];
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
    m_Camera->GetViewMatrix(m_baseViewMatrix);

    // Set the file name of the model.
    strcpy(modelFilename, "../Engine/data/sphere.txt");

    // Set the file name of the textures.
    strcpy(textureFilename, "../Engine/data/stone01.tga");

    // Create and initialize the model object.
    m_Model = new ModelClass;

    result = m_Model->Initialize(m_OpenGL, modelFilename, textureFilename, true, NULL, true, NULL, true);
    if(!result)
    {
        cout << "Error: Could not initialize the model object." << endl;
	return false;
    }

    // Create and initialize the light object.
    m_Light = new LightClass;

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

    // Create and initialize the font shader object.
    m_FontShader = new FontShaderClass;

    result = m_FontShader->Initialize(m_OpenGL);
    if(!result)
    {
        cout << "Error: Could not initialize the font shader object." << endl;
        return false;
    }

    // Create and initialize the font object.
    m_Font = new FontClass;

    result = m_Font->Initialize(m_OpenGL, 0);
    if(!result)
    {
        cout << "Error: Could not initialize the font object." << endl;
	return false;
    }

    // Set the initial render count string.
    strcpy(renderString, "Render Count: 0");

    // Create and initialize the text object for the render count string.
    m_RenderCountString = new TextClass;

    result = m_RenderCountString->Initialize(m_OpenGL, screenWidth, screenHeight, 32, m_Font, renderString, 10, 10, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        cout << "Error: Could not initialize the render count string object." << endl;
        return false;
    }

    // Create the position object.
    m_Position = new PositionClass;

    // Create and initialize the timer object.
    m_Timer = new TimerClass;
    m_Timer->Initialize();

    // Create and initialize the model list object.
    m_ModelList = new ModelListClass;
    m_ModelList->Initialize(25);

    // Create the frustum object.
    m_Frustum = new FrustumClass;

    return true;
}


void ApplicationClass::Shutdown()
{
    // Release the frustum object.
    if(m_Frustum)
    {
        delete m_Frustum;
	m_Frustum = 0;
    }

    // Release the model list object.
    if(m_ModelList)
    {
        m_ModelList->Shutdown();
	delete m_ModelList;
	m_ModelList = 0;
    }

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

    // Release the text objects for the render count string.
    if(m_RenderCountString)
    {
        m_RenderCountString->Shutdown();
        delete m_RenderCountString;
        m_RenderCountString = 0;
    }

    // Release the font object.
    if(m_Font)
    {
        m_Font->Shutdown();
        delete m_Font;
        m_Font = 0;
    }

    // Release the font shader object.
    if(m_FontShader)
    {
        m_FontShader->Shutdown();
        delete m_FontShader;
        m_FontShader = 0;
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
    float rotationY;
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

    // Check if the left or right arrow key has been pressed, if so rotate the camera accordingly.
    keyDown = Input->IsLeftArrowPressed();
    m_Position->TurnLeft(keyDown);

    keyDown = Input->IsRightArrowPressed();
    m_Position->TurnRight(keyDown);

    // Get the current view point rotation.
    m_Position->GetRotation(rotationY);

    // Set the rotation of the camera.
    m_Camera->SetRotation(0.0f, rotationY, 0.0f);
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
    float worldMatrix[16], viewMatrix[16], projectionMatrix[16], orthoMatrix[16];
    float diffuseLightColor[4], lightDirection[3], pixelColor[4];
    float positionX, positionY, positionZ, radius;
    int modelCount, renderCount, i;
    bool result, renderModel;


    // Clear the buffers to begin the scene.
    m_OpenGL->BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

    // Get the world, view, and projection matrices from the opengl and camera objects.
    m_OpenGL->GetWorldMatrix(worldMatrix);
    m_Camera->GetViewMatrix(viewMatrix);
    m_OpenGL->GetProjectionMatrix(projectionMatrix);
    m_OpenGL->GetOrthoMatrix(orthoMatrix);

    // Construct the frustum.
    m_Frustum->ConstructFrustum(m_OpenGL, SCREEN_DEPTH, viewMatrix, projectionMatrix);

    // Get the light properties.
    m_Light->GetDirection(lightDirection);
    m_Light->GetDiffuseColor(diffuseLightColor);

    // Get the number of models that will be rendered.
    modelCount = m_ModelList->GetModelCount();

    // Initialize the count of models that have been rendered.
    renderCount = 0;

    // Go through all the models and render them only if they can be seen by the camera view.
    for(i=0; i<modelCount; i++)
    {
        // Get the position and color of the sphere model at this index.
        m_ModelList->GetData(i, positionX, positionY, positionZ);

	// Set the radius of the sphere to 1.0 since this is already known.
	radius = 1.0f;

	// Check if the sphere model is in the view frustum.
	renderModel = m_Frustum->CheckSphere(positionX, positionY, positionZ, radius);

	// If it can be seen then render it, if not skip this model and check the next sphere.
	if(renderModel)
	{
	    // Move the model to the location it should be rendered at.
	    m_OpenGL->MatrixTranslation(worldMatrix, positionX, positionY, positionZ);

	    // Set the light shader parameters.
            result = m_LightShader->SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix, lightDirection, diffuseLightColor);
            if(!result)
            {
                return false;
            }

            // Render the sphere model using the light shader.
	    m_Model->SetTexture1(0); 
            m_Model->Render();

	    // Reset to the original world matrix.
	    m_OpenGL->GetWorldMatrix(worldMatrix);

	    // Since this model was rendered then increase the count for this frame.
	    renderCount++;
	}
    }

    // Disable the Z buffer and enable alpha blending for 2D rendering.
    m_OpenGL->TurnZBufferOff();
    m_OpenGL->EnableAlphaBlending();

    // Update the render count text.
    result = UpdateRenderCountString(renderCount);
    if(!result)
    {
        return false;
    }

    // Get the color to render the render count text as.
    m_RenderCountString->GetPixelColor(pixelColor);

    // Set the font shader as active and set its parameters.
    result = m_FontShader->SetShaderParameters(worldMatrix, m_baseViewMatrix, orthoMatrix, pixelColor);
    if(!result)
    {
        return false;
    }

    // Set the font texture as the active texture.
    m_Font->SetTexture(0);

    // Render the render count text string using the font shader.
    m_RenderCountString->Render();

    // Enable the Z buffer and disable alpha blending now that 2D rendering is complete.
    m_OpenGL->TurnZBufferOn();
    m_OpenGL->DisableAlphaBlending();

    // Present the rendered scene to the screen.
    m_OpenGL->EndScene();

    return true;
}


bool ApplicationClass::UpdateRenderCountString(int renderCount)
{
    char tempString[16], finalString[32];
    bool result;


    // Convert the render count integer to string format.
    sprintf(tempString, "%d", renderCount);

    // Setup the render count string.
    strcpy(finalString, "Render Count: ");
    strcat(finalString, tempString);

    // Update the sentence vertex buffer with the new string information.
    result = m_RenderCountString->UpdateText(m_Font, finalString, 10, 10, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

    return true;
}
