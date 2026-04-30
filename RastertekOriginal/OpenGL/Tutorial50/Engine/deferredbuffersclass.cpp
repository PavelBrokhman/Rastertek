////////////////////////////////////////////////////////////////////////////////
// Filename: deferredbuffersclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "deferredbuffersclass.h"


DeferredBuffersClass::DeferredBuffersClass()
{
}


DeferredBuffersClass::DeferredBuffersClass(const DeferredBuffersClass& other)
{
}


DeferredBuffersClass::~DeferredBuffersClass()
{
}


bool DeferredBuffersClass::Initialize(OpenGLClass* OpenGL, int textureWidth, int textureHeight, float screenNear, float screenDepth)
{
    unsigned int drawBuffersArray[2];


    // Store the width and height of the render texture.
    m_textureWidth = textureWidth;
    m_textureHeight = textureHeight;

    // Generate an ID for the frame buffer and bind the frame buffer.
    OpenGL->glGenFramebuffers(1, &m_frameBufferId);
    OpenGL->glBindFramebuffer(GL_FRAMEBUFFER, m_frameBufferId);

    // Create the depth buffer and attach it to the frame buffer using GL_DEPTH_ATTACHMENT.
    OpenGL->glGenRenderbuffers(1, &m_depthBufferId);
    OpenGL->glBindRenderbuffer(GL_RENDERBUFFER, m_depthBufferId);
    OpenGL->glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH_COMPONENT, m_textureWidth, m_textureHeight);
    OpenGL->glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, m_depthBufferId);

    // Create the color texture and attach it to the frame buffer using GL_COLOR_ATTACHMENT0.
    OpenGL->glActiveTexture(GL_TEXTURE0 + 0);
    glGenTextures(1, &m_textureIDArray[0]);
    glBindTexture(GL_TEXTURE_2D, m_textureIDArray[0]);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, m_textureWidth, m_textureHeight, 0, GL_RGBA, GL_UNSIGNED_BYTE, NULL);  // Color - GL_RGBA
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);  // Requires GL_NEAREST.
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);  // Requires GL_NEAREST.
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    OpenGL->glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, m_textureIDArray[0], 0);

    // Create the normal texture and attach it to the frame buffer using GL_COLOR_ATTACHMENT1.  Normals will require GL_RGBA32F format for third internalformat input.
    OpenGL->glActiveTexture(GL_TEXTURE0 + 0);
    glGenTextures(1, &m_textureIDArray[1]);
    glBindTexture(GL_TEXTURE_2D, m_textureIDArray[1]);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA32F, m_textureWidth, m_textureHeight, 0, GL_RGBA, GL_FLOAT, NULL);  // Normals - GL_RGBA32F
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);  // Requires GL_NEAREST.
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);  // Requires GL_NEAREST.
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    OpenGL->glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT1, GL_TEXTURE_2D, m_textureIDArray[1], 0);

    // Now set the format for the pixel shader output when rendering with this frame buffer.
    drawBuffersArray[0] = GL_COLOR_ATTACHMENT0;
    drawBuffersArray[1] = GL_COLOR_ATTACHMENT1;
    OpenGL->glDrawBuffers(2, drawBuffersArray);

    // Now that we are done setting up the render texture frame buffer, we can switch back to the regular back buffer that is used for rendering.
    OpenGL->glBindFramebuffer(GL_FRAMEBUFFER, 0);

    return true;
}


void DeferredBuffersClass::Shutdown(OpenGLClass* OpenGL)
{
    // Release the depth buffer.
    OpenGL->glDeleteRenderbuffers(1, &m_depthBufferId);

    // Release the textures.
    glDeleteTextures(1, &m_textureIDArray[0]);
    glDeleteTextures(1, &m_textureIDArray[1]);

    // Release the frame buffer.
    OpenGL->glDeleteFramebuffers(1, &m_frameBufferId);

    return;
}


void DeferredBuffersClass::SetRenderTarget(OpenGLClass* OpenGL)
{
    // Set the frame buffer (and its attached textures and depth buffer) as the render target.
    OpenGL->glBindFramebuffer(GL_FRAMEBUFFER, m_frameBufferId);

    // Set the viewport to be the correct dimensions for the texture dimensions used.
    glViewport(0, 0, m_textureWidth, m_textureHeight);

    return;
}


void DeferredBuffersClass::ClearRenderTargets(float red, float green, float blue, float alpha)
{
    // Clear the back buffer.
    glClearColor(red, green, blue, alpha);

    // Clear the depth buffer.
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    return;
}


void DeferredBuffersClass::SetTexture(OpenGLClass* OpenGL, unsigned int textureUnit, int view)
{
    // Set the texture unit we are working with.
    OpenGL->glActiveTexture(GL_TEXTURE0 + textureUnit);

    // Bind the texture as a 2D texture.
    glBindTexture(GL_TEXTURE_2D, m_textureIDArray[view]);

    return;
}
