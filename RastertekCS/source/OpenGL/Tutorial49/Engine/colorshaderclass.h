////////////////////////////////////////////////////////////////////////////////
// Filename: colorshaderclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _COLORSHADERCLASS_H_
#define _COLORSHADERCLASS_H_


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
// Class name: ColorShaderClass
////////////////////////////////////////////////////////////////////////////////
class ColorShaderClass
{
public:
    ColorShaderClass();
    ColorShaderClass(const ColorShaderClass&);
    ~ColorShaderClass();

    bool Initialize(OpenGLClass*);
    void Shutdown();

    bool SetShaderParameters(float*, float*, float*, float);

private:
    bool InitializeShader(char*, char*, char*, char*);
    void ShutdownShader();
    char* LoadShaderSourceFile(char*);
    void OutputShaderErrorMessage(unsigned int, char*);
    void OutputLinkerErrorMessage(unsigned int);

private:
    OpenGLClass* m_OpenGLPtr;
    unsigned int m_vertexShader;
    unsigned int m_hullShader;
    unsigned int m_domainShader;
    unsigned int m_fragmentShader;
    unsigned int m_shaderProgram;
};

#endif
