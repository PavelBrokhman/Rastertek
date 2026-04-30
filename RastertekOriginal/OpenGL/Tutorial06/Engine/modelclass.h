////////////////////////////////////////////////////////////////////////////////
// Filename: modelclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _MODELCLASS_H_
#define _MODELCLASS_H_


//////////////
// INCLUDES //
//////////////
#include <fstream>
using namespace std;


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "textureclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class Name: ModelClass
////////////////////////////////////////////////////////////////////////////////
class ModelClass
{
private:
    struct VertexType
    {
        float x, y, z;
        float tu, tv;
        float nx, ny, nz;
    };

public:
    ModelClass();
    ModelClass(const ModelClass&);
    ~ModelClass();

    bool Initialize(OpenGLClass*, char*, bool);
    void Shutdown();
    void Render();

    void SetTexture(unsigned int);
  
private:
    bool InitializeBuffers();
    void ShutdownBuffers();
    void RenderBuffers();

    bool LoadTexture(char*, bool);
    void ReleaseTexture();

private:
    OpenGLClass* m_OpenGLPtr;
    int m_vertexCount, m_indexCount;
    unsigned int m_vertexArrayId, m_vertexBufferId, m_indexBufferId;
    TextureClass* m_Texture;
};

#endif

