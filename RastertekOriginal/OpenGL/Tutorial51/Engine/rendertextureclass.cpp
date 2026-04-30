////////////////////////////////////////////////////////////////////////////////
// Filename: rendertextureclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "rendertextureclass.h"


RenderTextureClass::RenderTextureClass()
{
}


RenderTextureClass::RenderTextureClass(const RenderTextureClass& other)
{
}


RenderTextureClass::~RenderTextureClass()
{
}


bool RenderTextureClass::Initialize(OpenGLClass* OpenGL, int textureWidth, int textureHeight, float screenNear, float screenDepth, int format)
{
    int internalFormat, textureFormat, pixelDataType, filter;
    unsigned int drawBuffersArray[1];
    float fieldOfView, screenAspect;


    // Store the width and height of the render texture.
    m_textureWidth = textureWidth;
    m_textureHeight = textureHeight;

    // Set the texture format.
    switch(format)
    {
        case 0:
        {
            internalFormat = GL_RGBA;
            textureFormat = GL_RGBA;
            pixelDataType =  GL_UNSIGNED_BYTE;
            filter = GL_LINEAR;
            break;
        }
        case 1:
        {
            internalFormat = GL_R32F;
            textureFormat = GL_RED;
            pixelDataType = GL_FLOAT;
            filter = GL_NEAREST;
            break;
        }
        default:
        {
            internalFormat = GL_RGBA;
            textureFormat = GL_RGBA;
            pixelDataType =  GL_UNSIGNED_BYTE;
            filter = GL_LINEAR;
            break;
        }
    }

    // Generate an ID for the frame buffer and bind the frame buffer.
    OpenGL->glGenFramebuffers(1, &m_frameBufferId);
    OpenGL->glBindFramebuffer(GL_FRAMEBUFFER, m_frameBufferId);

    // Set the texture unit we are working with.
    OpenGL->glActiveTexture(GL_TEXTURE0 + 0);

    // Generate an ID for the texture and bind the 2D texture.
    glGenTextures(1, &m_textureID);
    glBindTexture(GL_TEXTURE_2D, m_textureID);

    // Create an empty texture with our desired format settings and no mipmapping.
    glTexImage2D(GL_TEXTURE_2D, 0, internalFormat, m_textureWidth, m_textureHeight, 0, textureFormat, pixelDataType, NULL);

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, filter);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, filter);

    // Now attach the texture that was just created to the frame buffer.
    OpenGL->glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, m_textureID, 0);

    // Next generate and ID for a depth buffer and bind the depth buffer.
    OpenGL->glGenRenderbuffers(1, &m_depthBufferId);
    OpenGL->glBindRenderbuffer(GL_RENDERBUFFER, m_depthBufferId);

    // Create the depth buffer.
    OpenGL->glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH_COMPONENT, m_textureWidth, m_textureHeight);

    // Attach the depth buffer to the frame buffer.
    OpenGL->glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, m_depthBufferId);

    // Now set the format for the pixel shader output when rendering with this frame buffer.
    drawBuffersArray[0] = GL_COLOR_ATTACHMENT0;
    OpenGL->glDrawBuffers(1, drawBuffersArray);

    // Now that we are done setting up the render texture frame buffer, we can switch back to the regular back buffer that is used for rendering.
    OpenGL->glBindFramebuffer(GL_FRAMEBUFFER, 0);

    // Setup the projection matrix for this render texture's dimensions.
    fieldOfView = 3.14159265358979323846f / 4.0f;
    screenAspect = (float)m_textureWidth / (float)m_textureHeight;
    OpenGL->BuildPerspectiveFovMatrix(m_projectionMatrix, fieldOfView, screenAspect, screenNear, screenDepth);

    // Create an orthographic projection matrix for this render texture's dimensions.
    OpenGL->BuildOrthoMatrix(m_orthoMatrix, (float)m_textureWidth, (float)m_textureHeight, screenNear, screenDepth);

    return true;
}


void RenderTextureClass::Shutdown(OpenGLClass* OpenGL)
{
    // Release the depth buffer.
    OpenGL->glDeleteRenderbuffers(1, &m_depthBufferId);

    // Release the texture.
    glDeleteTextures(1, &m_textureID);

    // Release the frame buffer.
    OpenGL->glDeleteFramebuffers(1, &m_frameBufferId);

    return;
}


void RenderTextureClass::SetRenderTarget(OpenGLClass* OpenGL)
{
    // Set the frame buffer (and its attached texture and depth buffer) as the render target.
    OpenGL->glBindFramebuffer(GL_FRAMEBUFFER, m_frameBufferId);

    // Set the viewport to be the correct dimensions for the texture dimensions used.
    glViewport(0, 0, m_textureWidth, m_textureHeight);

    return;
}


void RenderTextureClass::ClearRenderTarget(float red, float green, float blue, float alpha)
{
  // Clear the back buffer.
    glClearColor(red, green, blue, alpha);

    // Clear the depth buffer.
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    return;
}


void RenderTextureClass::SetTexture(OpenGLClass* OpenGL, unsigned int textureUnit)
{
    // Set the texture unit we are working with.
    OpenGL->glActiveTexture(GL_TEXTURE0 + textureUnit);

    // Bind the texture as a 2D texture.
    glBindTexture(GL_TEXTURE_2D, m_textureID);

    return;
}


int RenderTextureClass::GetTextureWidth()
{
    return m_textureWidth;
}


int RenderTextureClass::GetTextureHeight()
{
    return m_textureHeight;
}


void RenderTextureClass::GetProjectionMatrix(float* matrix)
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


void RenderTextureClass::GetOrthoMatrix(float* matrix)
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
