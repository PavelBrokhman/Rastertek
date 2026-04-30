////////////////////////////////////////////////////////////////////////////////
// Filename: fontclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _FONTCLASS_H_
#define _FONTCLASS_H_


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
// Class name: FontClass
////////////////////////////////////////////////////////////////////////////////
class FontClass
{
private:
	struct FontType
	{
		float left, right;
		int size;
	};

	struct VertexType
	{
        float x, y, z;
        float tu, tv;
	};

public:
	FontClass();
	FontClass(const FontClass&);
	~FontClass();

	bool Initialize(OpenGLClass*, int);
	void Shutdown();

	void BuildVertexArray(void*, char*, float, float);
	int GetSentencePixelLength(char*);
	int GetFontHeight();

	void SetTexture(unsigned int);

private:
	bool LoadFontData(char*);
	void ReleaseFontData();
	bool LoadTexture(char*);
	void ReleaseTexture();

private:
    OpenGLClass* m_OpenGLPtr;
	FontType* m_Font;
	TextureClass* m_Texture;
	float m_fontHeight;
	int m_spaceSize;
};

#endif
