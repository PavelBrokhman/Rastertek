////////////////////////////////////////////////////////////////////////////////
// Filename: textclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _TEXTCLASS_H_
#define _TEXTCLASS_H_


//////////////
// INCLUDES //
//////////////
#include "fontclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: TextClass
////////////////////////////////////////////////////////////////////////////////
class TextClass
{
private:
	struct VertexType
	{
		float x, y, z;
        float tu, tv;
	};

public:
	TextClass();
	TextClass(const TextClass&);
	~TextClass();

	bool Initialize(OpenGLClass*, int, int, int, FontClass*, char*, int, int, float, float, float);
	void Shutdown();
	void Render();

	bool UpdateText(FontClass*, char*, int, int, float, float, float);
    void GetPixelColor(float*);

private:
	bool InitializeBuffers(FontClass*, char*, int, int, float, float, float);
	void ShutdownBuffers();
	void RenderBuffers();

private:
    OpenGLClass* m_OpenGLPtr;
    int m_screenWidth, m_screenHeight, m_maxLength, m_vertexCount, m_indexCount;
    unsigned int m_vertexArrayId, m_vertexBufferId, m_indexBufferId;
    float m_pixelColor[4];
};

#endif
