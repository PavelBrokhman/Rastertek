////////////////////////////////////////////////////////////////////////////////
// Filename: transparentdepthshaderclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _TRANSPARENTDEPTHSHADERCLASS_H_
#define _TRANSPARENTDEPTHSHADERCLASS_H_


//////////////
// INCLUDES //
//////////////
#include <iostream>
using namespace std;


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "openglclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: TransparentDepthShaderClass
////////////////////////////////////////////////////////////////////////////////
class TransparentDepthShaderClass
{
public:
    TransparentDepthShaderClass();
    TransparentDepthShaderClass(const TransparentDepthShaderClass&);
    ~TransparentDepthShaderClass();

    bool Initialize(OpenGLClass*);
    void Shutdown();

    bool SetShaderParameters(float*, float*, float*);

private:
    bool InitializeShader(char*, char*);
    void ShutdownShader();
    char* LoadShaderSourceFile(char*);
    void OutputShaderErrorMessage(unsigned int, char*);
    void OutputLinkerErrorMessage(unsigned int);

private:
    OpenGLClass* m_OpenGLPtr;
    unsigned int m_vertexShader;
    unsigned int m_fragmentShader;
    unsigned int m_shaderProgram;
};

#endif
