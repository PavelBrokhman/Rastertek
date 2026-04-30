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
#include "shadermanagerclass.h"
#include "textureclass.h"
#include "lightclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: TerrainClass
////////////////////////////////////////////////////////////////////////////////
class TerrainClass
{
private:
    struct VertexType
    {
        float x, y, z;
        float tu, tv;
        float nx, ny, nz;
        float r, g, b;
    };

    struct HeightMapType
    {
        float x, y, z;
        float nx, ny, nz;
        float r, g, b;
    };

    struct ModelType
    {
        float x, y, z;
        float tu, tv;
        float nx, ny, nz;
        float r, g, b;
    };

    struct VectorType
    {
        float x, y, z;
    };

    struct TargaHeader
    {
        unsigned char data1[12];
        unsigned short width;
        unsigned short height;
        unsigned char bpp;
        unsigned char data2;
    };
  
public:
    TerrainClass();
    TerrainClass(const TerrainClass&);
    ~TerrainClass();

    bool Initialize(OpenGLClass*, char*);
    void Shutdown(OpenGLClass*);
    bool Render(OpenGLClass*, ShaderManagerClass*, LightClass*, float*, float*, float*);

private:
    bool LoadSetupFile(char*, char*, float&, char*, char*);
    bool LoadBitmapHeightMap(char*);
    void SetTerrainCoordinates(float);
    void CalculateNormals();
    bool LoadColorMap(char*);
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
    TextureClass* m_Texture;
};

#endif
