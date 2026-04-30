////////////////////////////////////////////////////////////////////////////////
// Filename: particlesystemclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _PARTICLESYSTEMCLASS_H_
#define _PARTICLESYSTEMCLASS_H_


//////////////
// INCLUDES //
//////////////
#include <fstream>
using namespace std;


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "textureclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class Name: ParticleSystemClass
////////////////////////////////////////////////////////////////////////////////
class ParticleSystemClass
{
private:
    struct VertexType
    {
        float x, y, z;
        float tu, tv;
        float lifeTime, scroll1X, scroll1Y;
    };

    struct ParticleType
    {
        float positionX, positionY, positionZ;
        bool active;
        float lifeTime;
        float scroll1X, scroll1Y;
    };

public:
    ParticleSystemClass();
    ParticleSystemClass(const ParticleSystemClass&);
    ~ParticleSystemClass();

    bool Initialize(OpenGLClass*, char*);
    void Shutdown();
    void Frame(float);
    void Render();

    bool Reload();
  
private:
    bool LoadParticleConfiguration();

    void InitializeParticleSystem();
    void ShutdownParticleSystem();

    void EmitParticles(float);
    void UpdateParticles(float);
    void KillParticles();
    void CopyParticle(int, int);

    void InitializeBuffers();
    void ShutdownBuffers();
    void RenderBuffers();
    void UpdateBuffers();
  
    bool LoadTexture();
    void ReleaseTexture();

private:
    OpenGLClass* m_OpenGLPtr;
    ParticleType* m_particleList;
    VertexType* m_vertices;
    TextureClass* m_Texture;
    unsigned int m_vertexArrayId, m_vertexBufferId, m_indexBufferId;
    int m_vertexCount, m_indexCount;

    char m_configFilename[256];
    int m_maxParticles;
    float m_particlesPerSecond;
    float m_particleSize;
    float m_particleLifeTime;
    char m_textureFilename[256];
    float m_accumulatedTime;
    int m_currentParticleCount;
};

#endif

