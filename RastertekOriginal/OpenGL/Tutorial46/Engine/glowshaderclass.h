////////////////////////////////////////////////////////////////////////////////
// Filename: glowshaderclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _GLOWSHADERCLASS_H_
#define _GLOWSHADERCLASS_H_


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
// Class name: GlowShaderClass
////////////////////////////////////////////////////////////////////////////////
class GlowShaderClass
{
public:
    GlowShaderClass();
    GlowShaderClass(const GlowShaderClass&);
    ~GlowShaderClass();

    bool Initialize(OpenGLClass*);
    void Shutdown();

    bool SetShaderParameters(float*, float*, float*, float);

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
