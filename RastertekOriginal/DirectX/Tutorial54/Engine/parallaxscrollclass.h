////////////////////////////////////////////////////////////////////////////////
// Filename: parallaxscrollclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _PARALLAXSCROLLCLASS_H_
#define _PARALLAXSCROLLCLASS_H_


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
// Class name: ParallaxScrollClass
////////////////////////////////////////////////////////////////////////////////
class ParallaxScrollClass
{
private:
	struct ParallaxArrayType
    {
        float scrollSpeed;
		float textureTranslation;
    };

public:
	ParallaxScrollClass();
	ParallaxScrollClass(const ParallaxScrollClass&);
	~ParallaxScrollClass();

	bool Initialize(ID3D11Device*, ID3D11DeviceContext*, char*);
	void Shutdown();
	void Frame(float);

	int GetTextureCount();
	ID3D11ShaderResourceView* GetTexture(int);
	float GetTranslation(int);

private:
	TextureClass* m_TextureArray;
	ParallaxArrayType* m_ParallaxArray;
	int m_textureCount;
};

#endif