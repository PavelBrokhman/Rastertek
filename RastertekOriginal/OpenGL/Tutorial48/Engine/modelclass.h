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
    };

    struct InstanceType
    {
      float x, y, z;
    };
  
public:
    ModelClass();
    ModelClass(const ModelClass&);
    ~ModelClass();

    bool Initialize(OpenGLClass*, char*, bool);
    void Shutdown();
    void Render();

    void SetTexture1(unsigned int);
  
private:
    bool InitializeBuffers();
    void ShutdownBuffers();
    void RenderBuffers();

    bool LoadTexture(char*, bool);
    void ReleaseTexture();

private:
    OpenGLClass* m_OpenGLPtr;
    int m_vertexCount, m_instanceCount;
    unsigned int m_vertexArrayId, m_vertexBufferId, m_instanceBufferId;
    TextureClass* m_Texture;
    bool m_texture1Loaded;
};

#endif

