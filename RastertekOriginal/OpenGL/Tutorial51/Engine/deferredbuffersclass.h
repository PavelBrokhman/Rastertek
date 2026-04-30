////////////////////////////////////////////////////////////////////////////////
// Filename: deferredbuffersclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _DEFERREDBUFFERSCLASS_H_
#define _DEFERREDBUFFERSCLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "openglclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: DeferredBuffersClass
////////////////////////////////////////////////////////////////////////////////
class DeferredBuffersClass
{
public:
    DeferredBuffersClass();
    DeferredBuffersClass(const DeferredBuffersClass&);
    ~DeferredBuffersClass();

    bool Initialize(OpenGLClass*, int, int, float, float);
    void Shutdown(OpenGLClass*);

    void SetRenderTarget(OpenGLClass*);
    void ClearRenderTargets(float, float, float, float);

    void SetShaderResourcePositions(OpenGLClass*, unsigned int);
    void SetShaderResourceNormals(OpenGLClass*, unsigned int);
    void SetShaderResourceColors(OpenGLClass*, unsigned int);

private:
    int m_textureWidth, m_textureHeight;
    unsigned int m_frameBufferId, m_depthBufferId;
    unsigned int m_textureIDArray[3];
};

#endif
