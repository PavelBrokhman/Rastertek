////////////////////////////////////////////////////////////////////////////////
// Filename: rendertextureclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _RENDERTEXTURECLASS_H_
#define _RENDERTEXTURECLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "openglclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: RenderTextureClass
////////////////////////////////////////////////////////////////////////////////
class RenderTextureClass
{
public:
    RenderTextureClass();
    RenderTextureClass(const RenderTextureClass&);
    ~RenderTextureClass();

    bool Initialize(OpenGLClass*, int, int, float, float, int);
    void Shutdown();

    void SetRenderTarget();
    void ClearRenderTarget(float, float, float, float);
    void SetTexture(unsigned int);

    int GetTextureWidth();
    int GetTextureHeight();

    void GetProjectionMatrix(float*);
    void GetOrthoMatrix(float*);

private:
    OpenGLClass* m_OpenGLPtr;
    int m_textureWidth, m_textureHeight;
    unsigned int m_frameBufferId, m_textureID, m_depthBufferId;
    float m_projectionMatrix[16], m_orthoMatrix[16];
};

#endif
