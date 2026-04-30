////////////////////////////////////////////////////////////////////////////////
// Filename: modelclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _MODELCLASS_H_
#define _MODELCLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "openglclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: ModelClass
////////////////////////////////////////////////////////////////////////////////
class ModelClass
{
private:
	struct VertexType
	{
		float x, y, z;
		float r, g, b;
	};

public:
	ModelClass();
	ModelClass(const ModelClass&);
	~ModelClass();

	bool Initialize(OpenGLClass*);
	void Shutdown(OpenGLClass*);
	void Render(OpenGLClass*);

private:
	bool InitializeBuffers(OpenGLClass*);
	void ShutdownBuffers(OpenGLClass*);
	void RenderBuffers(OpenGLClass*);

private:
	int m_vertexCount, m_indexCount;
	unsigned int m_vertexArrayId, m_vertexBufferId, m_indexBufferId;
};

#endif