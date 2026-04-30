////////////////////////////////////////////////////////////////////////////////
// Filename: terrainclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _TERRAINCLASS_H_
#define _TERRAINCLASS_H_


//////////////
// INCLUDES //
//////////////
#include <fstream>
using namespace std;


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "openglclass.h"
#include "shadermanagerclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: TerrainClass
////////////////////////////////////////////////////////////////////////////////
class TerrainClass
{
private:
    struct VertexType
    {
        float x, y, z;
        float r, g, b;
    };

    struct HeightMapType
    {
        float x, y, z;
    };

    struct ModelType
    {
        float x, y, z;
    };

public:
    TerrainClass();
    TerrainClass(const TerrainClass&);
    ~TerrainClass();

    bool Initialize(OpenGLClass*, char*);
    void Shutdown(OpenGLClass*);
    bool Render(OpenGLClass*, ShaderManagerClass*, float*, float*, float*);

private:
    bool LoadSetupFile(char*, char*, float&);
    bool LoadBitmapHeightMap(char*);
    void SetTerrainCoordinates(float);
    void BuildTerrainModel();
    void ReleaseHeightMap();
    void ReleaseTerrainModel();

    bool InitializeBuffers(OpenGLClass*);
    void ShutdownBuffers(OpenGLClass*);
    void RenderBuffers(OpenGLClass*);

private:
    int m_vertexCount, m_indexCount;
    unsigned int m_vertexArrayId, m_vertexBufferId, m_indexBufferId;
    int m_terrainHeight, m_terrainWidth;
    HeightMapType* m_heightMap;
    ModelType* m_terrainModel;
};

#endif
