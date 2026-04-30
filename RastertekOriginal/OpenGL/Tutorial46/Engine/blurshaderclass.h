////////////////////////////////////////////////////////////////////////////////
// Filename: blurshaderclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _BLURSHADERCLASS_H_
#define _BLURSHADERCLASS_H_


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
// Class name: BlurShaderClass
////////////////////////////////////////////////////////////////////////////////
class BlurShaderClass
{
public:
    BlurShaderClass();
    BlurShaderClass(const BlurShaderClass&);
    ~BlurShaderClass();

    bool Initialize(OpenGLClass*);
    void Shutdown();

    bool SetShaderParameters(float*, float*, float*, float, float, float);

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
