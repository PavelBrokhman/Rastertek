////////////////////////////////////////////////////////////////////////////////
// Filename: modelclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _MODELCLASS_H_
#define _MODELCLASS_H_


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
// Class name: ModelClass
////////////////////////////////////////////////////////////////////////////////
class ModelClass
{
private:
	struct VertexType
	{
		float x, y, z;
		float tu, tv;
		float nx, ny, nz;
	};

	struct ModelType
	{
		float x, y, z;
		float tu, tv;
		float nx, ny, nz;
	};

public:
	ModelClass();
	ModelClass(const ModelClass&);
	~ModelClass();

	bool Initialize(OpenGLClass*, char*, char*, unsigned int, bool);
	void Shutdown(OpenGLClass*);
	void Render(OpenGLClass*);

private:
	bool InitializeBuffers(OpenGLClass*);
	void ShutdownBuffers(OpenGLClass*);
	void RenderBuffers(OpenGLClass*);

	bool LoadTexture(OpenGLClass*, char*, unsigned int, bool);
	void ReleaseTexture();

	bool LoadModel(char*);
	void ReleaseModel();

private:
	int m_vertexCount, m_indexCount;
	unsigned int m_vertexArrayId, m_vertexBufferId, m_indexBufferId;
	TextureClass* m_Texture;
	ModelType* m_model;
};

#endif