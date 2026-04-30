////////////////////////////////////////////////////////////////////////////////
// Filename: parallaxscrollclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "parallaxscrollclass.h"


ParallaxScrollClass::ParallaxScrollClass()
{
    m_TextureArray = 0;
    m_ParallaxArray = 0;
}


ParallaxScrollClass::ParallaxScrollClass(const ParallaxScrollClass& other)
{
}


ParallaxScrollClass::~ParallaxScrollClass()
{
}


bool ParallaxScrollClass::Initialize(OpenGLClass* OpenGL, char* configFilename)
{
    ifstream fin;
    char textureFilename[256];
    int i, j;
    char input;
    float scrollSpeed;
    bool result;


    // Open the file.
    fin.open(configFilename);
    if(!fin.good())
    {
        return false;
    }

    // Read the number of textures in the file.
    fin.get(input);
    while(input != ':')
    {
        fin.get(input);
    }
    fin >> m_textureCount;
    
    // Read end of line.
    fin.get(input);
    fin.get(input);

    // Boundary check.
    if((m_textureCount < 1) || (m_textureCount > 64))
    {
        return false;
    }

    // Create and initialize the texture object array.
    m_TextureArray = new TextureClass[m_textureCount];

    // Create and initialize the parallax struct array.
    m_ParallaxArray = new ParallaxArrayType[m_textureCount];

    // Loop through each line containing a texture filename and a scroll speed.
    for(i=0; i<m_textureCount; i++)
    {
        j=0;
	fin.get(input);

	// Read in the texture file name.
	while(input != ' ')
	{
	    textureFilename[j] = input;
	    j++;
	    fin.get(input);
	}
	textureFilename[j] = '\0';

	// Read in the scroll speed.
	fin >> scrollSpeed;

	// Read end of line.
	fin.get(input);
	fin.get(input);

	// Load the texture into the texture array.
	result = m_TextureArray[i].Initialize(OpenGL, textureFilename, true);
	if(!result)
	{
	    return false;
	}
	
	// Store the scroll speed and initialize the translation of the texture at the start.
	m_ParallaxArray[i].scrollSpeed = scrollSpeed;
	m_ParallaxArray[i].textureTranslation = 0.0f;
    }

    // Close the file.
    fin.close();

    return true;
}


void ParallaxScrollClass::Shutdown()
{
    int i;


    // Release the parallax struct array.
    if(m_ParallaxArray)
    {
        delete [] m_ParallaxArray;
	m_ParallaxArray = 0;
    }

    // Release the texture object array.
    if(m_TextureArray)
    {
        for(i=0; i<m_textureCount; i++)
	{
	    m_TextureArray[i].Shutdown();
	}

        delete [] m_TextureArray;
        m_TextureArray = 0;
    }

    return;
}


void ParallaxScrollClass::Frame(float frameTime)
{
    int i;


    // Loop through each of the elements in the parallax struct array.
    for(i=0; i<m_textureCount; i++)
    {
        // Increment the texture translation by the frame time each frame.
        m_ParallaxArray[i].textureTranslation += frameTime * m_ParallaxArray[i].scrollSpeed;
	if(m_ParallaxArray[i].textureTranslation > 1.0f)
	{
	    m_ParallaxArray[i].textureTranslation -= 1.0f;
	}
    }

    return;
}


int ParallaxScrollClass::GetTextureCount()
{
    return m_textureCount;
}


void ParallaxScrollClass::SetTexture(OpenGLClass* OpenGL, int index, unsigned int textureUnit)
{
    m_TextureArray[index].SetTexture(OpenGL, textureUnit);
    return;
}


float ParallaxScrollClass::GetTranslation(int index)
{
    return m_ParallaxArray[index].textureTranslation;
}
