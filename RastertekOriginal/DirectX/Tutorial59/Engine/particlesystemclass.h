////////////////////////////////////////////////////////////////////////////////
// Filename: particlesystemclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _PARTICLESYSTEMCLASS_H_
#define _PARTICLESYSTEMCLASS_H_


//////////////
// INCLUDES //
//////////////
#include <d3d11.h>
#include <directxmath.h>
#include <fstream>
using namespace DirectX;
using namespace std;


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "textureclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: ParticleSystemClass
////////////////////////////////////////////////////////////////////////////////
class ParticleSystemClass
{
private:
	struct VertexType
	{
		XMFLOAT3 position;
		XMFLOAT2 texture;
		XMFLOAT4 data1;
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

	bool Initialize(ID3D11Device*, ID3D11DeviceContext*, char*);
	void Shutdown();
	bool Frame(float, ID3D11DeviceContext*);
	void Render(ID3D11DeviceContext*);

	ID3D11ShaderResourceView* GetTexture();

	int GetIndexCount();

	bool Reload(ID3D11Device*, ID3D11DeviceContext*);

private:
	bool LoadParticleConfiguration();

	bool InitializeParticleSystem();
	void ShutdownParticleSystem();

	void EmitParticles(float);
	void UpdateParticles(float);
	void KillParticles();
	void CopyParticle(int, int);

	bool InitializeBuffers(ID3D11Device*);
	void ShutdownBuffers();
	void RenderBuffers(ID3D11DeviceContext*);
	bool UpdateBuffers(ID3D11DeviceContext*);

	bool LoadTexture(ID3D11Device*, ID3D11DeviceContext*);
	void ReleaseTexture();

private:
	ID3D11Buffer* m_vertexBuffer, * m_indexBuffer;
	ParticleType* m_particleList;
	VertexType* m_vertices;
	TextureClass* m_Texture;
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