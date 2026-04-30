////////////////////////////////////////////////////////////////////////////////
// Filename: textureclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _TEXTURECLASS_H_
#define _TEXTURECLASS_H_


//////////////
// INCLUDES //
//////////////
#include <stdio.h>


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "openglclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: TextureClass
////////////////////////////////////////////////////////////////////////////////
class TextureClass
{
private:
    struct TargaHeader
    {
        unsigned char data1[12];
        unsigned short width;
        unsigned short height;
        unsigned char bpp;
        unsigned char data2;
    };

public:
    TextureClass();
    TextureClass(const TextureClass&);
    ~TextureClass();

    bool Initialize(OpenGLClass*, char*, bool);
    void Shutdown();

    void SetTexture(OpenGLClass*, unsigned int);

    int GetWidth();
    int GetHeight();

private:
    bool LoadTarga32Bit(OpenGLClass*, char*, bool);

private:
    unsigned int m_textureID;
    int m_width, m_height;
    bool m_loaded;
};

#endif
