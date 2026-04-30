////////////////////////////////////////////////////////////////////////////////
// Filename: displayplaneclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _DISPLAYPLANECLASS_H_
#define _DISPLAYPLANECLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "openglclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class Name: DisplayPlaneClass
////////////////////////////////////////////////////////////////////////////////
class DisplayPlaneClass
{
private:
    struct VertexType
    {
        float x, y, z;
        float tu, tv;
    };

public:
    DisplayPlaneClass();
    DisplayPlaneClass(const DisplayPlaneClass&);
    ~DisplayPlaneClass();

    bool Initialize(OpenGLClass*, float, float);
    void Shutdown();
    void Render();

private:
    bool InitializeBuffers(float, float);
    void ShutdownBuffers();
    void RenderBuffers();

private:
    OpenGLClass* m_OpenGLPtr;
    int m_vertexCount, m_indexCount;
    unsigned int m_vertexArrayId, m_vertexBufferId, m_indexBufferId;
};

#endif

