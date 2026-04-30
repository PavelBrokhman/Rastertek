////////////////////////////////////////////////////////////////////////////////
// Filename: ssaoshaderclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _SSAOSHADERCLASS_H_
#define _SSAOSHADERCLASS_H_


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
// Class name: SsaoShaderClass
////////////////////////////////////////////////////////////////////////////////
class SsaoShaderClass
{
public:
	SsaoShaderClass();
	SsaoShaderClass(const SsaoShaderClass&);
	~SsaoShaderClass();

	bool Initialize(OpenGLClass*);
	void Shutdown();

	bool SetShaderParameters(float*, float*, float*, float, float, float, float, float, float, float);

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
