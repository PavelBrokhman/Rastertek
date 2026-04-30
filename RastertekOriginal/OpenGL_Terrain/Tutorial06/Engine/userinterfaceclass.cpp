////////////////////////////////////////////////////////////////////////////////
// Filename: userinterfaceclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "userinterfaceclass.h"


UserInterfaceClass::UserInterfaceClass()
{
    m_VideoCardString = 0;
    m_FpsString = 0;
    m_PositionStrings = 0;
}


UserInterfaceClass::UserInterfaceClass(const UserInterfaceClass& other)
{
}


UserInterfaceClass::~UserInterfaceClass()
{
}


bool UserInterfaceClass::Initialize(OpenGLClass* OpenGL, FontClass* Font, int screenHeight, int screenWidth)
{
    char videoCard[256], fpsString[32], posString[16];
    int i;
    bool result;


    // Get the video card string.
    OpenGL->GetVideoCardString(videoCard);

    // Setup the video card text object.
    m_VideoCardString = new TextClass;

    result = m_VideoCardString->Initialize(OpenGL, screenWidth, screenHeight, 256, Font, videoCard, 10, 10, 1.0f, 1.0f, 1.0f);
    if(!result)
    {
        return false;
    }

    // Initialize the previous frame fps.
    m_previousFps = -1;

    // Create and initialize the text object for the fps string.
    m_FpsString = new TextClass;

    strcpy(fpsString, "Fps: 0");

    result = m_FpsString->Initialize(OpenGL, screenWidth, screenHeight, 16, Font, fpsString, 10, 35, 0.0f, 1.0f, 0.0f);
    if(!result)
    {
        return false;
    }

    // Create and initialize the text object for the fps string.
    m_PositionStrings = new TextClass[6];

    strcpy(posString, "X: 0");
    result = m_PositionStrings[0].Initialize(OpenGL, screenWidth, screenHeight, 16, Font, posString, 10, 80, 1.0f, 1.0f, 1.0f);  if(!result) { return false; }

    strcpy(posString, "Y: 0");
    result = m_PositionStrings[1].Initialize(OpenGL, screenWidth, screenHeight, 16, Font, posString, 10, 105, 1.0f, 1.0f, 1.0f);  if(!result) { return false; }

    strcpy(posString, "Z: 0");
    result = m_PositionStrings[2].Initialize(OpenGL, screenWidth, screenHeight, 16, Font, posString, 10, 130, 1.0f, 1.0f, 1.0f);  if(!result) { return false; }

    strcpy(posString, "rX: 0");
    result = m_PositionStrings[3].Initialize(OpenGL, screenWidth, screenHeight, 16, Font, posString, 10, 165, 1.0f, 1.0f, 1.0f);  if(!result) { return false; }

    strcpy(posString, "rY: 0");
    result = m_PositionStrings[4].Initialize(OpenGL, screenWidth, screenHeight, 16, Font, posString, 10, 190, 1.0f, 1.0f, 1.0f);  if(!result) { return false; }

    strcpy(posString, "rZ: 0");
    result = m_PositionStrings[5].Initialize(OpenGL, screenWidth, screenHeight, 16, Font, posString, 10, 215, 1.0f, 1.0f, 1.0f);  if(!result) { return false; }

    // Initialize the previous frame position.
    for(i=0; i<6; i++)
    {
        m_previousPosition[i] = -1;
    }

    return true;
}


void UserInterfaceClass::Shutdown()
{
    int i;


    // Release the text objects for the position strings.
    if(m_PositionStrings)
    {
        for(i=0; i<6; i++)
        {
            m_PositionStrings[i].Shutdown();
        }

        delete [] m_PositionStrings;
        m_PositionStrings = 0;
    }

    // Release the text object for the fps string.
    if(m_FpsString)
    {
        m_FpsString->Shutdown();
        delete m_FpsString;
        m_FpsString = 0;
    }

    // Release the text object for the video card string.
    if(m_VideoCardString)
    {
        m_VideoCardString->Shutdown();
        delete m_VideoCardString;
        m_VideoCardString = 0;
    }

    return;
}


bool UserInterfaceClass::Frame(FontClass* Font, int fps)
{
    bool result;


    result = UpdateFpsString(Font, fps);
    if(!result)
    {
        return false;
    }

    return true;
}


bool UserInterfaceClass::Render(OpenGLClass* OpenGL, ShaderManagerClass* ShaderManager, FontClass* Font, float* worldMatrix, float* viewMatrix, float* orthoMatrix)
{
    float pixelColor[4];
    int i;
    bool result;


    // Disable the Z buffer and enable alpha blending for 2D rendering.
    OpenGL->TurnZBufferOff();
    OpenGL->EnableAlphaBlending();

    // Render the video card text object using the font shader.
    m_VideoCardString->GetPixelColor(pixelColor);

    result = ShaderManager->RenderFontShader(worldMatrix, viewMatrix, orthoMatrix, pixelColor);
    if(!result)
    {
        return false;
    }

    m_VideoCardString->Render(Font);

    // Render the fps string using the font shader.
    m_FpsString->GetPixelColor(pixelColor);

    result = ShaderManager->RenderFontShader(worldMatrix, viewMatrix, orthoMatrix, pixelColor);
    if(!result)
    {
        return false;
    }

    m_FpsString->Render(Font);

    // Render the position strings.
    for(i=0; i<6; i++)
    {
        m_PositionStrings[i].GetPixelColor(pixelColor);

        result = ShaderManager->RenderFontShader(worldMatrix, viewMatrix, orthoMatrix, pixelColor);
        if(!result)
        {
            return false;
        }

        m_PositionStrings[i].Render(Font);
    }

    // Enable the Z buffer and disable alpha blending now that 2D rendering is complete.
    OpenGL->DisableAlphaBlending();
    OpenGL->TurnZBufferOn();

    return true;
}


bool UserInterfaceClass::UpdateFpsString(FontClass* Font, int fps)
{
    char tempString[16], finalString[16];
    float red, green, blue;
    bool result;


    // Check if the fps from the previous frame was the same, if so don't need to update the text string.
    if(m_previousFps == fps)
    {
        return true;
    }

    // Store the fps for checking next frame.
    m_previousFps = fps;

    // Truncate the fps to below 100,000.
    if(fps > 99999)
    {
        fps = 99999;
    }

    // Convert the fps integer to string format.
    sprintf(tempString, "%d", fps);

    // Setup the fps string.
    strcpy(finalString, "Fps: ");
    strcat(finalString, tempString);

    // If fps is 60 or above set the fps color to green.
    if(fps >= 60)
    {
        red = 0.0f;
        green = 1.0f;
        blue = 0.0f;
    }

    // If fps is below 60 set the fps color to yellow.
    if(fps < 60)
    {
        red = 1.0f;
        green = 1.0f;
        blue = 0.0f;
    }

    // If fps is below 30 set the fps color to red.
    if(fps < 30)
    {
        red = 1.0f;
        green = 0.0f;
        blue = 0.0f;
    }

    // Update the sentence vertex buffer with the new string information.
    result = m_FpsString->UpdateText(Font, finalString, 10, 35, red, green, blue);
    if(!result)
    {
        return false;
    }

    return true;
}


bool UserInterfaceClass::UpdatePositonStrings(FontClass* Font, float posX, float posY, float posZ, float rotX, float rotY, float rotZ)
{
    int positionX, positionY, positionZ, rotationX, rotationY, rotationZ;
    char tempString[16], finalString[16];
    bool result;


    // Convert the float values to integers.
    positionX = (int)posX;
    positionY = (int)posY;
    positionZ = (int)posZ;
    rotationX = (int)rotX;
    rotationY = (int)rotY;
    rotationZ = (int)rotZ;

    // Update the position strings if the value has changed since the last frame.
    if(positionX != m_previousPosition[0])
    {
        m_previousPosition[0] = positionX;
        sprintf(tempString, "%d", positionX);
        strcpy(finalString, "X: ");
        strcat(finalString, tempString);

        result = m_PositionStrings[0].UpdateText(Font, finalString, 10, 80, 1.0f, 1.0f, 1.0f);
        if(!result)
        {
            return false;
        }
    }

    if(positionY != m_previousPosition[1])
    {
        m_previousPosition[1] = positionY;
        sprintf(tempString, "%d", positionY);
        strcpy(finalString, "Y: ");
        strcat(finalString, tempString);

        result = m_PositionStrings[1].UpdateText(Font, finalString, 10, 105, 1.0f, 1.0f, 1.0f);
        if(!result)
        {
            return false;
        }
    }

    if(positionZ != m_previousPosition[2])
    {
        m_previousPosition[2] = positionZ;
        sprintf(tempString, "%d", positionZ);
        strcpy(finalString, "Z: ");
        strcat(finalString, tempString);

        result = m_PositionStrings[2].UpdateText(Font, finalString, 10, 130, 1.0f, 1.0f, 1.0f);
        if(!result)
        {
            return false;
        }
    }

    if(rotationX != m_previousPosition[3])
    {
        m_previousPosition[3] = rotationX;
        sprintf(tempString, "%d", rotationX);
        strcpy(finalString, "rX: ");
        strcat(finalString, tempString);

        result = m_PositionStrings[3].UpdateText(Font, finalString, 10, 165, 1.0f, 1.0f, 1.0f);
        if(!result)
        {
            return false;
        }
    }

    if(rotationY != m_previousPosition[4])
    {
        m_previousPosition[4] = rotationY;
        sprintf(tempString, "%d", rotationY);
        strcpy(finalString, "rY: ");
        strcat(finalString, tempString);

        result = m_PositionStrings[4].UpdateText(Font, finalString, 10, 190, 1.0f, 1.0f, 1.0f);
        if(!result)
        {
            return false;
        }
    }

    if(rotationZ != m_previousPosition[5])
    {
        m_previousPosition[5] = rotationZ;
        sprintf(tempString, "%d", rotationZ);
        strcpy(finalString, "rZ: ");
        strcat(finalString, tempString);

        result = m_PositionStrings[5].UpdateText(Font, finalString, 10, 215, 1.0f, 1.0f, 1.0f);
        if(!result)
        {
            return false;
        }
    }

    return true;
}
