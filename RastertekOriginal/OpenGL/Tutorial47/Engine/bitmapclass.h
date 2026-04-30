////////////////////////////////////////////////////////////////////////////////
// Filename: bitmapclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _BITMAPCLASS_H_
#define _BITMAPCLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "textureclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class Name: BitmapClass
////////////////////////////////////////////////////////////////////////////////
class BitmapClass
{
private:
    struct VertexType
    {
        float x, y, z;
        float tu, tv;
    };

public:
    BitmapClass();
    BitmapClass(const BitmapClass&);
    ~BitmapClass();

    bool Initialize(OpenGLClass*, int, int, char*, int, int);
    void Shutdown();
    void Render();

    void SetTexture(unsigned int);
    void SetRenderLocation(int, int);

private:
    bool InitializeBuffers();
    void ShutdownBuffers();
    bool UpdateBuffers();
    void RenderBuffers();

    bool LoadTexture(char*);
    void ReleaseTexture();

private:
    OpenGLClass* m_OpenGLPtr;
    int m_vertexCount, m_indexCount, m_screenWidth, m_screenHeight, m_bitmapWidth, m_bitmapHeight, m_renderX, m_renderY, m_prevPosX, m_prevPosY;
    unsigned int m_vertexArrayId, m_vertexBufferId, m_indexBufferId;
    TextureClass* m_Texture;
};

#endif

