////////////////////////////////////////////////////////////////////////////////
// Filename: shadermanagerclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _SHADERMANAGERCLASS_H_
#define _SHADERMANAGERCLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "textureshaderclass.h"
#include "lightshaderclass.h"
#include "normalmapshaderclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: ShaderManagerClass
////////////////////////////////////////////////////////////////////////////////
class ShaderManagerClass
{
public:
    ShaderManagerClass();
    ShaderManagerClass(const ShaderManagerClass&);
    ~ShaderManagerClass();

    bool Initialize(OpenGLClass*);
    void Shutdown();

    bool RenderTextureShader(float*, float*, float*);
    bool RenderLightShader(float*, float*, float*, float*, float*);
    bool RenderNormalMapShader(float*, float*, float*, float*, float*);

private:
    TextureShaderClass* m_TextureShader;
    LightShaderClass* m_LightShader;
    NormalMapShaderClass* m_NormalMapShader;
};

#endif
