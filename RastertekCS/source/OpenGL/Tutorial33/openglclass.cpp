////////////////////////////////////////////////////////////////////////////////
// Filename: openglclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "openglclass.h"


OpenGLClass::OpenGLClass()
{
}


OpenGLClass::OpenGLClass(const OpenGLClass& other)
{
}


OpenGLClass::~OpenGLClass()
{
}


bool OpenGLClass::Initialize(Display* display, Window win, int screenWidth, int screenHeight, float screenNear, float screenDepth, bool vsync)
{
    GLXDrawable drawable;
    float fieldOfView, screenAspect;
    bool result;


    // Store the screen size.
    m_screenWidth = screenWidth;
    m_screenHeight = screenHeight;

    // Load the OpenGL extensions that will be used by this program.
    result = LoadExtensionList();
    if(!result)
    {
        return false;
    }

    // Store copies of the display and window pointers.
    m_display = display;
    m_hwnd = win;

    // Set the depth buffer to be entirely cleared to 1.0 values.
    glClearDepth(1.0f);

    // Enable depth testing.
    glEnable(GL_DEPTH_TEST);

    // Set the polygon winding to clockwise front facing for the left handed system.
    glFrontFace(GL_CW);

    // Enable back face culling.
    glEnable(GL_CULL_FACE);
    glCullFace(GL_BACK);

    // Get the current drawable so we can modify the vertical sync swapping.
    drawable = glXGetCurrentDrawable();

    // Turn on or off the vertical sync depending on the input bool value.
    if(vsync)
    {
        glXSwapIntervalEXT(m_display, drawable, 1);
    }
    else
    {
	glXSwapIntervalEXT(m_display, drawable, 0);
    }

    // Initialize the world/model matrix to the identity matrix.
    BuildIdentityMatrix(m_worldMatrix);

    // Set the field of view and screen aspect ratio.
    fieldOfView = 3.14159265358979323846f / 4.0f;
    screenAspect = (float)screenWidth / (float)screenHeight;

    // Build the perspective projection matrix.
    BuildPerspectiveFovMatrix(m_projectionMatrix, fieldOfView, screenAspect, screenNear, screenDepth);

    // Create an orthographic projection matrix for 2D rendering.
    BuildOrthoMatrix(m_orthoMatrix, (float)screenWidth, (float)screenHeight, screenNear, screenDepth);

    return true;
}


void OpenGLClass::Shutdown()
{

    return;
}


void OpenGLClass::BeginScene(float red, float green, float blue, float alpha)
{
    // Set the color to clear the screen to.
    glClearColor(red, green, blue, alpha);

    // Clear the screen and depth buffer.
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    return;
}


void OpenGLClass::EndScene()
{
    // Present the back buffer to the screen since rendering is complete.
    glXSwapBuffers(m_display, m_hwnd);

    return;
}


void OpenGLClass::BuildIdentityMatrix(float* matrix)
{
    matrix[0]  = 1.0f;
    matrix[1]  = 0.0f;
    matrix[2]  = 0.0f;
    matrix[3]  = 0.0f;

    matrix[4]  = 0.0f;
    matrix[5]  = 1.0f;
    matrix[6]  = 0.0f;
    matrix[7]  = 0.0f;

    matrix[8]  = 0.0f;
    matrix[9]  = 0.0f;
    matrix[10] = 1.0f;
    matrix[11] = 0.0f;

    matrix[12] = 0.0f;
    matrix[13] = 0.0f;
    matrix[14] = 0.0f;
    matrix[15] = 1.0f;

    return;
}


void OpenGLClass::BuildPerspectiveFovMatrix(float* matrix, float fieldOfView, float screenAspect, float screenNear, float screenDepth)
{
    matrix[0]  = 1.0f / (screenAspect * tan(fieldOfView * 0.5f));
    matrix[1]  = 0.0f;
    matrix[2]  = 0.0f;
    matrix[3]  = 0.0f;

    matrix[4]  = 0.0f;
    matrix[5]  = 1.0f / tan(fieldOfView * 0.5f);
    matrix[6]  = 0.0f;
    matrix[7]  = 0.0f;

    matrix[8]  = 0.0f;
    matrix[9]  = 0.0f;
    matrix[10] = screenDepth / (screenDepth - screenNear);
    matrix[11] = 1.0f;

    matrix[12] = 0.0f;
    matrix[13] = 0.0f;
    matrix[14] = (-screenNear * screenDepth) / (screenDepth - screenNear);
    matrix[15] = 0.0f;

    return;
}


void OpenGLClass::BuildOrthoMatrix(float* matrix, float screenWidth,  float screenHeight, float screenNear, float screenDepth)
{
    matrix[0]  = 2.0f / screenWidth;
    matrix[1]  = 0.0f;
    matrix[2]  = 0.0f;
    matrix[3]  = 0.0f;

    matrix[4]  = 0.0f;
    matrix[5]  = 2.0f / screenHeight;
    matrix[6]  = 0.0f;
    matrix[7]  = 0.0f;

    matrix[8]  = 0.0f;
    matrix[9]  = 0.0f;
    matrix[10] = 1.0f / (screenDepth - screenNear);
    matrix[11] = 0.0f;

    matrix[12] = 0.0f;
    matrix[13] = 0.0f;
    matrix[14] = screenNear / (screenNear - screenDepth);
    matrix[15] = 1.0f;

    return;
}


void OpenGLClass::GetWorldMatrix(float* matrix)
{
    matrix[0]  = m_worldMatrix[0];
    matrix[1]  = m_worldMatrix[1];
    matrix[2]  = m_worldMatrix[2];
    matrix[3]  = m_worldMatrix[3];

    matrix[4]  = m_worldMatrix[4];
    matrix[5]  = m_worldMatrix[5];
    matrix[6]  = m_worldMatrix[6];
    matrix[7]  = m_worldMatrix[7];

    matrix[8]  = m_worldMatrix[8];
    matrix[9]  = m_worldMatrix[9];
    matrix[10] = m_worldMatrix[10];
    matrix[11] = m_worldMatrix[11];

    matrix[12] = m_worldMatrix[12];
    matrix[13] = m_worldMatrix[13];
    matrix[14] = m_worldMatrix[14];
    matrix[15] = m_worldMatrix[15];

    return;
}


void OpenGLClass::GetProjectionMatrix(float* matrix)
{
    matrix[0]  = m_projectionMatrix[0];
    matrix[1]  = m_projectionMatrix[1];
    matrix[2]  = m_projectionMatrix[2];
    matrix[3]  = m_projectionMatrix[3];

    matrix[4]  = m_projectionMatrix[4];
    matrix[5]  = m_projectionMatrix[5];
    matrix[6]  = m_projectionMatrix[6];
    matrix[7]  = m_projectionMatrix[7];

    matrix[8]  = m_projectionMatrix[8];
    matrix[9]  = m_projectionMatrix[9];
    matrix[10] = m_projectionMatrix[10];
    matrix[11] = m_projectionMatrix[11];

    matrix[12] = m_projectionMatrix[12];
    matrix[13] = m_projectionMatrix[13];
    matrix[14] = m_projectionMatrix[14];
    matrix[15] = m_projectionMatrix[15];

    return;
}


void OpenGLClass::GetOrthoMatrix(float* matrix)
{
    matrix[0]  = m_orthoMatrix[0];
    matrix[1]  = m_orthoMatrix[1];
    matrix[2]  = m_orthoMatrix[2];
    matrix[3]  = m_orthoMatrix[3];

    matrix[4]  = m_orthoMatrix[4];
    matrix[5]  = m_orthoMatrix[5];
    matrix[6]  = m_orthoMatrix[6];
    matrix[7]  = m_orthoMatrix[7];

    matrix[8]  = m_orthoMatrix[8];
    matrix[9]  = m_orthoMatrix[9];
    matrix[10] = m_orthoMatrix[10];
    matrix[11] = m_orthoMatrix[11];

    matrix[12] = m_orthoMatrix[12];
    matrix[13] = m_orthoMatrix[13];
    matrix[14] = m_orthoMatrix[14];
    matrix[15] = m_orthoMatrix[15];

    return;
}


bool OpenGLClass::LoadExtensionList()
{
    glCreateShader = (PFNGLCREATESHADERPROC)glXGetProcAddress((unsigned char*)"glCreateShader");
    if(!glCreateShader)
    {
        return false;
    }

    glShaderSource = (PFNGLSHADERSOURCEPROC)glXGetProcAddress((unsigned char*)"glShaderSource");
    if(!glShaderSource)
    {
        return false;
    }

    glCompileShader = (PFNGLCOMPILESHADERPROC)glXGetProcAddress((unsigned char*)"glCompileShader");
    if(!glCompileShader)
    {
        return false;
    }

    glGetShaderiv = (PFNGLGETSHADERIVPROC)glXGetProcAddress((unsigned char*)"glGetShaderiv");
    if(!glGetShaderiv)
    {
        return false;
    }

    glGetShaderInfoLog = (PFNGLGETSHADERINFOLOGPROC)glXGetProcAddress((unsigned char*)"glGetShaderInfoLog");
    if(!glGetShaderInfoLog)
    {
        return false;
    }

    glCreateProgram = (PFNGLCREATEPROGRAMPROC)glXGetProcAddress((unsigned char*)"glCreateProgram");
    if(!glCreateProgram)
    {
        return false;
    }

    glAttachShader = (PFNGLATTACHSHADERPROC)glXGetProcAddress((unsigned char*)"glAttachShader");
    if(!glAttachShader)
    {
        return false;
    }

    glBindAttribLocation = (PFNGLBINDATTRIBLOCATIONPROC)glXGetProcAddress((unsigned char*)"glBindAttribLocation");
    if(!glBindAttribLocation)
    {
        return false;
    }

    glLinkProgram = (PFNGLLINKPROGRAMPROC)glXGetProcAddress((unsigned char*)"glLinkProgram");
    if(!glLinkProgram)
    {
        return false;
    }

    glGetProgramiv = (PFNGLGETPROGRAMIVPROC)glXGetProcAddress((unsigned char*)"glGetProgramiv");
    if(!glGetProgramiv)
    {
        return false;
    }

    glGetProgramInfoLog = (PFNGLGETPROGRAMINFOLOGPROC)glXGetProcAddress((unsigned char*)"glGetProgramInfoLog");
    if(!glGetProgramInfoLog)
    {
        return false;
    }

    glDetachShader = (PFNGLDETACHSHADERPROC)glXGetProcAddress((unsigned char*)"glDetachShader");
    if(!glDetachShader)
    {
        return false;
    }

    glDeleteShader = (PFNGLDELETESHADERPROC)glXGetProcAddress((unsigned char*)"glDeleteShader");
    if(!glDeleteShader)
    {
        return false;
    }

    glDeleteProgram = (PFNGLDELETEPROGRAMPROC)glXGetProcAddress((unsigned char*)"glDeleteProgram");
    if(!glDeleteProgram)
    {
        return false;
    }

    glUseProgram = (PFNGLUSEPROGRAMPROC)glXGetProcAddress((unsigned char*)"glUseProgram");
    if(!glUseProgram)
    {
        return false;
    }

    glGetUniformLocation = (PFNGLGETUNIFORMLOCATIONPROC)glXGetProcAddress((unsigned char*)"glGetUniformLocation");
    if(!glGetUniformLocation)
    {
        return false;
    }

    glUniformMatrix4fv = (PFNGLUNIFORMMATRIX4FVPROC)glXGetProcAddress((unsigned char*)"glUniformMatrix4fv");
    if(!glUniformMatrix4fv)
    {
        return false;
    }

    glGenVertexArrays = (PFNGLGENVERTEXARRAYSPROC)glXGetProcAddress((unsigned char*)"glGenVertexArrays");
    if(!glGenVertexArrays)
    {
        return false;
    }

    glBindVertexArray = (PFNGLBINDVERTEXARRAYPROC)glXGetProcAddress((unsigned char*)"glBindVertexArray");
    if(!glBindVertexArray)
    {
        return false;
    }

    glGenBuffers = (PFNGLGENBUFFERSPROC)glXGetProcAddress((unsigned char*)"glGenBuffers");
    if(!glGenBuffers)
    {
        return false;
    }

    glBindBuffer = (PFNGLBINDBUFFERPROC)glXGetProcAddress((unsigned char*)"glBindBuffer");
    if(!glBindBuffer)
    {
        return false;
    }

    glBufferData = (PFNGLBUFFERDATAPROC)glXGetProcAddress((unsigned char*)"glBufferData");
    if(!glBufferData)
    {
        return false;
    }

    glEnableVertexAttribArray = (PFNGLENABLEVERTEXATTRIBARRAYPROC)glXGetProcAddress((unsigned char*)"glEnableVertexAttribArray");
    if(!glEnableVertexAttribArray)
    {
        return false;
    }

    glVertexAttribPointer = (PFNGLVERTEXATTRIBPOINTERPROC)glXGetProcAddress((unsigned char*)"glVertexAttribPointer");
    if(!glVertexAttribPointer)
    {
        return false;
    }

    glDeleteBuffers = (PFNGLDELETEBUFFERSPROC)glXGetProcAddress((unsigned char*)"glDeleteBuffers");
    if(!glDeleteBuffers)
    {
        return false;
    }

    glDeleteVertexArrays = (PFNGLDELETEVERTEXARRAYSPROC)glXGetProcAddress((unsigned char*)"glDeleteVertexArrays");
    if(!glDeleteVertexArrays)
    {
        return false;
    }

    glUniform1i = (PFNGLUNIFORM1IPROC)glXGetProcAddress((unsigned char*)"glUniform1i");
    if(!glUniform1i)
    {
        return false;
    }

    glActiveTexture = (PFNGLACTIVETEXTUREPROC)glXGetProcAddress((unsigned char*)"glActiveTexture");
    if(!glActiveTexture)
    {
        return false;
    }

    glGenerateMipmap = (PFNGLGENERATEMIPMAPPROC)glXGetProcAddress((unsigned char*)"glGenerateMipmap");
    if(!glGenerateMipmap)
    {
        return false;
    }

    glUniform2fv = (PFNGLUNIFORM2FVPROC)glXGetProcAddress((unsigned char*)"glUniform2fv");
    if(!glUniform2fv)
    {
        return false;
    }

    glUniform3fv = (PFNGLUNIFORM3FVPROC)glXGetProcAddress((unsigned char*)"glUniform3fv");
    if(!glUniform3fv)
    {
        return false;
    }

    glUniform4fv = (PFNGLUNIFORM4FVPROC)glXGetProcAddress((unsigned char*)"glUniform4fv");
    if(!glUniform4fv)
    {
        return false;
    }

    glMapBuffer = (PFNGLMAPBUFFERPROC)glXGetProcAddress((unsigned char*)"glMapBuffer");
    if(!glMapBuffer)
    {
        return false;
    }

    glUnmapBuffer = (PFNGLUNMAPBUFFERPROC)glXGetProcAddress((unsigned char*)"glUnmapBuffer");
    if(!glUnmapBuffer)
    {
        return false;
    }

    glXSwapIntervalEXT = (PFNGLXSWAPINTERVALEXTPROC)glXGetProcAddress((unsigned char*)"glXSwapIntervalEXT");
    if(!glXSwapIntervalEXT)
    {
        return false;
    }

    glUniform1f = (PFNGLUNIFORM1FPROC)glXGetProcAddress((unsigned char*)"glUniform1f");
    if(!glUniform1f)
    {
        return false;
    }

    glGenFramebuffers = (PFNGLGENFRAMEBUFFERSPROC)glXGetProcAddress((unsigned char*)"glGenFramebuffers");
    if(!glGenFramebuffers)
    {
        return false;
    }

    glDeleteFramebuffers = (PFNGLDELETEFRAMEBUFFERSPROC)glXGetProcAddress((unsigned char*)"glDeleteFramebuffers");
    if(!glDeleteFramebuffers)
    {
        return false;
    }

    glBindFramebuffer = (PFNGLBINDFRAMEBUFFERPROC)glXGetProcAddress((unsigned char*)"glBindFramebuffer");
    if(!glBindFramebuffer)
    {
        return false;
    }

    glFramebufferTexture2D = (PFNGLFRAMEBUFFERTEXTURE2DPROC)glXGetProcAddress((unsigned char*)"glFramebufferTexture2D");
    if(!glFramebufferTexture2D)
    {
        return false;
    }

    glGenRenderbuffers = (PFNGLGENRENDERBUFFERSPROC)glXGetProcAddress((unsigned char*)"glGenRenderbuffers");
    if(!glGenRenderbuffers)
    {
        return false;
    }

    glBindRenderbuffer = (PFNGLBINDRENDERBUFFERPROC)glXGetProcAddress((unsigned char*)"glBindRenderbuffer");
    if(!glBindRenderbuffer)
    {
        return false;
    }

    glRenderbufferStorage = (PFNGLRENDERBUFFERSTORAGEPROC)glXGetProcAddress((unsigned char*)"glRenderbufferStorage");
    if(!glRenderbufferStorage)
    {
        return false;
    }

    glFramebufferRenderbuffer = (PFNGLFRAMEBUFFERRENDERBUFFERPROC)glXGetProcAddress((unsigned char*)"glFramebufferRenderbuffer");
    if(!glFramebufferRenderbuffer)
    {
        return false;
    }

    glDrawBuffers = (PFNGLDRAWBUFFERSARBPROC)glXGetProcAddress((unsigned char*)"glDrawBuffers");
    if(!glDrawBuffers)
    {
        return false;
    }

    glDeleteRenderbuffers = (PFNGLDELETERENDERBUFFERSPROC)glXGetProcAddress((unsigned char*)"glDeleteRenderbuffers");
    if(!glDeleteRenderbuffers)
    {
        return false;
    }

    glBlendFuncSeparate = (PFNGLBLENDFUNCSEPARATEPROC)glXGetProcAddress((unsigned char*)"glBlendFuncSeparate");
    if(!glBlendFuncSeparate)
    {
        return false;
    }
    
    return true;
}


void OpenGLClass::MatrixRotationX(float* matrix, float angle)
{
    matrix[0]  = 1.0f;
    matrix[1]  = 0.0f;
    matrix[2]  = 0.0f;
    matrix[3]  = 0.0f;

    matrix[4]  = 0.0f;
    matrix[5]  = cosf(angle);
    matrix[6]  = sinf(angle);
    matrix[7]  = 0.0f;

    matrix[8]  = 0.0f;
    matrix[9]  = -sinf(angle);
    matrix[10] = cosf(angle);
    matrix[11] = 0.0f;

    matrix[12] = 0.0f;
    matrix[13] = 0.0f;
    matrix[14] = 0.0f;
    matrix[15] = 1.0f;

    return;
}


void OpenGLClass::MatrixRotationY(float* matrix, float angle)
{
    matrix[0]  = cosf(angle);
    matrix[1]  = 0.0f;
    matrix[2]  = -sinf(angle);
    matrix[3]  = 0.0f;

    matrix[4]  = 0.0f;
    matrix[5]  = 1.0f;
    matrix[6]  = 0.0f;
    matrix[7]  = 0.0f;

    matrix[8]  = sinf(angle);
    matrix[9]  = 0.0f;
    matrix[10] = cosf(angle);
    matrix[11] = 0.0f;

    matrix[12] = 0.0f;
    matrix[13] = 0.0f;
    matrix[14] = 0.0f;
    matrix[15] = 1.0f;

    return;
}


void OpenGLClass::MatrixRotationZ(float* matrix, float angle)
{
    matrix[0]  = cosf(angle);
    matrix[1]  = sinf(angle);
    matrix[2]  = 0.0f;
    matrix[3]  = 0.0f;

    matrix[4]  = -sinf(angle);
    matrix[5]  = cosf(angle);
    matrix[6]  = 0.0f;
    matrix[7]  = 0.0f;

    matrix[8]  = 0.0f;
    matrix[9]  = 0.0f;
    matrix[10] = 1.0f;
    matrix[11] = 0.0f;

    matrix[12] = 0.0f;
    matrix[13] = 0.0f;
    matrix[14] = 0.0f;
    matrix[15] = 1.0f;

    return;
}


void OpenGLClass::MatrixTranslation(float* matrix, float x, float y, float z)
{
    matrix[0]  = 1.0f;
    matrix[1]  = 0.0f;
    matrix[2]  = 0.0f;
    matrix[3]  = 0.0f;

    matrix[4]  = 0.0f;
    matrix[5]  = 1.0f;
    matrix[6]  = 0.0f;
    matrix[7]  = 0.0f;

    matrix[8]  = 0.0f;
    matrix[9]  = 0.0f;
    matrix[10] = 1.0f;
    matrix[11] = 0.0f;

    matrix[12] = x;
    matrix[13] = y;
    matrix[14] = z;
    matrix[15] = 1.0f;

    return;
}


void OpenGLClass::MatrixScale(float* matrix, float x, float y, float z)
{
    matrix[0]  = x;
    matrix[1]  = 0.0f;
    matrix[2]  = 0.0f;
    matrix[3]  = 0.0f;

    matrix[4]  = 0.0f;
    matrix[5]  = y;
    matrix[6]  = 0.0f;
    matrix[7]  = 0.0f;

    matrix[8]  = 0.0f;
    matrix[9]  = 0.0f;
    matrix[10] = z;
    matrix[11] = 0.0f;

    matrix[12] = 0.0f;
    matrix[13] = 0.0f;
    matrix[14] = 0.0f;
    matrix[15] = 1.0f;

    return;
}


void OpenGLClass::MatrixTranspose(float* result, float* matrix)
{
    result[0]  = matrix[0];
    result[1]  = matrix[4];
    result[2]  = matrix[8];
    result[3]  = matrix[12];

    result[4]  = matrix[1];
    result[5]  = matrix[5];
    result[6]  = matrix[9];
    result[7]  = matrix[13];

    result[8]  = matrix[2];
    result[9]  = matrix[6];
    result[10] = matrix[10];
    result[11] = matrix[14];

    result[12] = matrix[3];
    result[13] = matrix[7];
    result[14] = matrix[11];
    result[15] = matrix[15];

    return;
}


void OpenGLClass::MatrixMultiply(float* result, float* matrix1, float* matrix2)
{
    result[0]  = (matrix1[0] * matrix2[0]) + (matrix1[1] * matrix2[4]) + (matrix1[2] * matrix2[8]) + (matrix1[3] * matrix2[12]);
    result[1]  = (matrix1[0] * matrix2[1]) + (matrix1[1] * matrix2[5]) + (matrix1[2] * matrix2[9]) + (matrix1[3] * matrix2[13]);
    result[2]  = (matrix1[0] * matrix2[2]) + (matrix1[1] * matrix2[6]) + (matrix1[2] * matrix2[10]) + (matrix1[3] * matrix2[14]);
    result[3]  = (matrix1[0] * matrix2[3]) + (matrix1[1] * matrix2[7]) + (matrix1[2] * matrix2[11]) + (matrix1[3] * matrix2[15]);

    result[4]  = (matrix1[4] * matrix2[0]) + (matrix1[5] * matrix2[4]) + (matrix1[6] * matrix2[8]) + (matrix1[7] * matrix2[12]);
    result[5]  = (matrix1[4] * matrix2[1]) + (matrix1[5] * matrix2[5]) + (matrix1[6] * matrix2[9]) + (matrix1[7] * matrix2[13]);
    result[6]  = (matrix1[4] * matrix2[2]) + (matrix1[5] * matrix2[6]) + (matrix1[6] * matrix2[10]) + (matrix1[7] * matrix2[14]);
    result[7]  = (matrix1[4] * matrix2[3]) + (matrix1[5] * matrix2[7]) + (matrix1[6] * matrix2[11]) + (matrix1[7] * matrix2[15]);

    result[8]  = (matrix1[8] * matrix2[0]) + (matrix1[9] * matrix2[4]) + (matrix1[10] * matrix2[8]) + (matrix1[11] * matrix2[12]);
    result[9]  = (matrix1[8] * matrix2[1]) + (matrix1[9] * matrix2[5]) + (matrix1[10] * matrix2[9]) + (matrix1[11] * matrix2[13]);
    result[10] = (matrix1[8] * matrix2[2]) + (matrix1[9] * matrix2[6]) + (matrix1[10] * matrix2[10]) + (matrix1[11] * matrix2[14]);
    result[11] = (matrix1[8] * matrix2[3]) + (matrix1[9] * matrix2[7]) + (matrix1[10] * matrix2[11]) + (matrix1[11] * matrix2[15]);

    result[12] = (matrix1[12] * matrix2[0]) + (matrix1[13] * matrix2[4]) + (matrix1[14] * matrix2[8]) + (matrix1[15] * matrix2[12]);
    result[13] = (matrix1[12] * matrix2[1]) + (matrix1[13] * matrix2[5]) + (matrix1[14] * matrix2[9]) + (matrix1[15] * matrix2[13]);
    result[14] = (matrix1[12] * matrix2[2]) + (matrix1[13] * matrix2[6]) + (matrix1[14] * matrix2[10]) + (matrix1[15] * matrix2[14]);
    result[15] = (matrix1[12] * matrix2[3]) + (matrix1[13] * matrix2[7]) + (matrix1[14] * matrix2[11]) + (matrix1[15] * matrix2[15]);

    return;
}


void OpenGLClass::TurnZBufferOn()
{
    // Enable depth testing.
    glEnable(GL_DEPTH_TEST);

    return;
}


void OpenGLClass::TurnZBufferOff()
{
    // Disable depth testing.
    glDisable(GL_DEPTH_TEST);

    return;
}


void OpenGLClass::EnableAlphaBlending()
{
    // Enable alpha blending.
    glEnable(GL_BLEND);

    // Set the blending equation.
    glBlendFuncSeparate(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA, GL_ONE, GL_ZERO);

    return;
}


void OpenGLClass::DisableAlphaBlending()
{
    // Disable alpha blending.
    glDisable(GL_BLEND);

    return;
}


void OpenGLClass::SetBackBufferRenderTarget()
{
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
    return;
}


void OpenGLClass::ResetViewport()
{
    glViewport(0, 0, m_screenWidth, m_screenHeight);
    return;
}


void OpenGLClass::EnableClipping()
{
    // Enable clip plane 0.
    glEnable(GL_CLIP_DISTANCE0);

    return;
}


void OpenGLClass::DisableClipping()
{
    // Disable clip plane 0.
    glDisable(GL_CLIP_DISTANCE0);

    return;
}
