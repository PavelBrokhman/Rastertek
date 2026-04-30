///////////////////////////////////////////////////////////////////////////////
// Filename: fontclass.cpp
///////////////////////////////////////////////////////////////////////////////
#include "fontclass.h"


FontClass::FontClass()
{
    m_OpenGLPtr = 0;
    m_Font = 0;
    m_Texture = 0;
}


FontClass::FontClass(const FontClass& other)
{
}


FontClass::~FontClass()
{
}


bool FontClass::Initialize(OpenGLClass* OpenGL, int fontChoice)
{
    char fontFilename[128];
    char fontTextureFilename[128];
    bool result;


    // Store a pointer to the OpenGL object.
    m_OpenGLPtr = OpenGL;

    // Choose one of the available fonts, and default to the first font otherwise.
    switch(fontChoice)
    {
        case 0:
        {
            strcpy(fontFilename, "../Engine/data/font/font01.txt");
            strcpy(fontTextureFilename, "../Engine/data/font/font01.tga");
            m_fontHeight = 32.0f;
            m_spaceSize = 3;
            break;
        }
        default:
        {
            strcpy(fontFilename, "../Engine/data/font/font01.txt");
            strcpy(fontTextureFilename, "../Engine/data/font/font01.tga");
            m_fontHeight = 32.0f;
            m_spaceSize = 3;
            break;
        }
    }

    // Load in the text file containing the font data.
    result = LoadFontData(fontFilename);
    if(!result)
    {
        return false;
    }

    // Load the texture that has the font characters on it.
    result = LoadTexture(fontTextureFilename);
    if(!result)
    {
        return false;
    }

    return true;
}


void FontClass::Shutdown()
{
    // Release the font texture.
    ReleaseTexture();

    // Release the font data.
    ReleaseFontData();

    // Release the pointer to the OpenGL object.
    m_OpenGLPtr = 0;

    return;
}


bool FontClass::LoadFontData(char* filename)
{
    ifstream fin;
    int i;
    char temp;


    // Create the font spacing buffer.
    m_Font = new FontType[95];

    // Read in the font size and spacing between chars.
    fin.open(filename);
    if(fin.fail())
    {
        return false;
    }

    // Read in the 95 used ascii characters for text.
    for(i=0; i<95; i++)
    {
        fin.get(temp);
	while(temp != ' ')
	{
	    fin.get(temp);
	}
	fin.get(temp);
	while(temp != ' ')
	{
	    fin.get(temp);
	}

	fin >> m_Font[i].left;
	fin >> m_Font[i].right;
	fin >> m_Font[i].size;
    }

    // Close the file.
    fin.close();
    
    return true;
}


void FontClass::ReleaseFontData()
{
    // Release the font data array.
    if(m_Font)
    {
        delete [] m_Font;
	m_Font = 0;
    }

    return;
}


bool FontClass::LoadTexture(char* textureFilename)
{
    bool result;


    // Create and initialize the font texture object.
    m_Texture = new TextureClass;

    result = m_Texture->Initialize(m_OpenGLPtr, textureFilename, false);
    if(!result)
    {
        return false;
    }

    return true;
}


void FontClass::ReleaseTexture()
{
    // Release the texture object.
    if(m_Texture)
    {
        m_Texture->Shutdown();
	delete m_Texture;
	m_Texture = 0;
    }

    return;
}


void FontClass::SetTexture(unsigned int textureUnit)
{
    // Set the texture for the font.
    m_Texture->SetTexture(m_OpenGLPtr, textureUnit);

    return;
}


void FontClass::BuildVertexArray(void* vertices, char* sentence, float drawX, float drawY)
{
    VertexType* vertexPtr;
    int numLetters, index, i, letter;


    // Coerce the input vertices into a VertexType structure.
    vertexPtr = (VertexType*)vertices;

    // Get the number of letters in the sentence.
    numLetters = (int)strlen(sentence);

    // Initialize the index to the vertex array.
    index = 0;

    // Draw each letter onto a quad.
    for(i=0; i<numLetters; i++)
    {
        letter = ((int)sentence[i]) - 32;
	
	// If the letter is a space then just move over three pixels.
	if(letter == 0)
	{
	    drawX = drawX + (float)m_spaceSize;
	}
	else
	{
	    // First triangle in quad.
	  vertexPtr[index].x = drawX;  // Top left.
	  vertexPtr[index].y = drawY;
	  vertexPtr[index].z = 0.0f;
	  vertexPtr[index].tu = m_Font[letter].left;
	  vertexPtr[index].tv = 1.0f;
	  index++;

	  vertexPtr[index].x = drawX + m_Font[letter].size;  // Bottom right.
	  vertexPtr[index].y = drawY - m_fontHeight;
	  vertexPtr[index].z = 0.0f;
	  vertexPtr[index].tu = m_Font[letter].right;
	  vertexPtr[index].tv = 0.0f;
	  index++;

	  vertexPtr[index].x = drawX;  // Bottom left.
	  vertexPtr[index].y = drawY - m_fontHeight;
	  vertexPtr[index].z = 0.0f;
	  vertexPtr[index].tu = m_Font[letter].left;
	  vertexPtr[index].tv = 0.0f;
	  index++;

	  // Second triangle in quad.
	  vertexPtr[index].x = drawX;  // Top left.
	  vertexPtr[index].y = drawY;
	  vertexPtr[index].z = 0.0f;
	  vertexPtr[index].tu = m_Font[letter].left;
	  vertexPtr[index].tv = 1.0f;
	  index++;

	  vertexPtr[index].x = drawX + m_Font[letter].size;  // Top right.
	  vertexPtr[index].y = drawY;
	  vertexPtr[index].z = 0.0f;
	  vertexPtr[index].tu = m_Font[letter].right;
	  vertexPtr[index].tv = 1.0f;
	  index++;

	  vertexPtr[index].x = drawX + m_Font[letter].size;  // Bottom right.
	  vertexPtr[index].y = drawY - m_fontHeight;
	  vertexPtr[index].z = 0.0f;
	  vertexPtr[index].tu = m_Font[letter].right;
	  vertexPtr[index].tv = 0.0f;
	  index++;

	  // Update the x location for drawing by the size of the letter and one pixel.
	  drawX = drawX + m_Font[letter].size + 1.0f;
	}
    }
    
    return;
}


int FontClass::GetSentencePixelLength(char* sentence)
{
    int pixelLength, numLetters, i, letter;


    pixelLength = 0;
    numLetters = (int)strlen(sentence);

    for(i=0; i<numLetters; i++)
    {
        letter = ((int)sentence[i]) - 32;
      
        // If the letter is a space then count it as three pixels.
        if(letter == 0)
        {
	    pixelLength += m_spaceSize;
        }
        else
        {
	    pixelLength += (m_Font[letter].size + 1);
        }
    }

    return pixelLength;
}


int FontClass::GetFontHeight()
{
    return (int)m_fontHeight;
}
