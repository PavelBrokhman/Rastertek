////////////////////////////////////////////////////////////////////////////////
// Filename: shadermanagerclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _SHADERMANAGERCLASS_H_
#define _SHADERMANAGERCLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "fontshaderclass.h"
#include "terrainshaderclass.h"


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

    bool RenderFontShader(float*, float*, float*, float*);
    bool RenderTerrainShader(float*, float*, float*);

private:
    FontShaderClass* m_FontShader;
    TerrainShaderClass* m_TerrainShader;
};

#endif
