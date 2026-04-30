////////////////////////////////////////////////////////////////////////////////
// Filename: orthowindowclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _ORTHOWINDOWCLASS_H_
#define _ORTHOWINDOWCLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "openglclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class Name: OrthoWindowClass
////////////////////////////////////////////////////////////////////////////////
class OrthoWindowClass
{
private:
    struct VertexType
    {
        float x, y, z;
        float tu, tv;
    };

public:
    OrthoWindowClass();
    OrthoWindowClass(const OrthoWindowClass&);
    ~OrthoWindowClass();

    bool Initialize(OpenGLClass*, int, int);
    void Shutdown();
    void Render();

private:
    bool InitializeBuffers(int, int);
    void ShutdownBuffers();
    void RenderBuffers();

private:
    OpenGLClass* m_OpenGLPtr;
    int m_vertexCount, m_indexCount;
    unsigned int m_vertexArrayId, m_vertexBufferId, m_indexBufferId;
};

#endif

