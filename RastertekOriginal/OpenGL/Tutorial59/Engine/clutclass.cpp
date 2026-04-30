////////////////////////////////////////////////////////////////////////////////
// Filename: clutclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "clutclass.h"


ClutClass::ClutClass()
{
}


ClutClass::ClutClass(const ClutClass& other)
{
}


ClutClass::~ClutClass()
{
}


bool ClutClass::Initialize(OpenGLClass* OpenGL)
{
    char filename[256];
    bool result;


    // Set the filename of the CLUT data file.
    strcpy(filename, "../Engine/data/clut.dat");

    // Create the 3D texture from the CLUT file data.
	result = Load3DTexture(OpenGL, filename);
	if(!result)
	{
		return false;
	}

    return true;
}


void ClutClass::Shutdown(OpenGLClass* OpenGL)
{
    // Release the 3D texture.
    Release3DTexture(OpenGL);

    return;
}


bool ClutClass::Load3DTexture(OpenGLClass* OpenGL, char* filename)
{
    FILE* filePtr;
    unsigned char* buffer;
    unsigned long count, bufferSize;
    int cubeSize, error, textureWidth, textureHeight, textureDepth;


    // Open the CLUT file for reading in binary.
    filePtr = fopen(filename, "rb");
    if(filePtr == NULL)
    {
        return false;
    }

    // Read in the cube size.
	count = fread(&cubeSize, sizeof(int), 1, filePtr);
	if(count != 1)
	{
		return false;
	}

	// Set the size of the buffer using the cube size.
    bufferSize = cubeSize * cubeSize * cubeSize * 3;

    // Create the buffer.
    buffer = new unsigned char[bufferSize];

    // Read the CLUT data into the buffer.
    count = fread(buffer, sizeof(unsigned char), bufferSize, filePtr);
	if(count != bufferSize)
	{
		return false;
	}

    // Close the file.
    error = fclose(filePtr);
    if(error != 0)
    {
        return false;
    }

    // Set the 3D texture size.
    textureWidth = cubeSize;
    textureHeight = cubeSize;
    textureDepth = cubeSize;

    // Enable 3D textures.
    OpenGL->Enable3DTextures();

    // Set the active texture unit in which to store the data.
    OpenGL->glActiveTexture(GL_TEXTURE0 + 0);

    // Generate an ID for the texture.
    glGenTextures(1, &m_textureID);

    // Bind the texture as a 3D texture.
    glBindTexture(GL_TEXTURE_3D, m_textureID);

    // Load the CLUT data into the texture unit.
    glTexImage3D(GL_TEXTURE_3D, 0, GL_RGB8, textureWidth, textureHeight, textureDepth, 0, GL_RGB, GL_UNSIGNED_BYTE, buffer);

    // Set the texture to clamp to the edge.
    glTexParameteri(GL_TEXTURE_3D, GL_TEXTURE_WRAP_R, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_3D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_3D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

    // Set the texture filtering to linear interpoliation for CLUT textures.
    glTexParameteri(GL_TEXTURE_3D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_3D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);

    // 3D textures require mipmaps as well, basically lower quality 3D pyramids.
    OpenGL->glGenerateMipmap(GL_TEXTURE_3D);

    // Release the buffer.
    delete [] buffer;
    buffer = 0;

    return true;
}


void ClutClass::Release3DTexture(OpenGLClass* OpenGL)
{
    // If the texture was loaded then make sure to release it on shutdown.
    glDeleteTextures(1, &m_textureID);

    // Disable 3D textures.
    OpenGL->Disable3DTextures();

    return;
}


void ClutClass::SetTexture(OpenGLClass* OpenGL, unsigned int textureUnit)
{
    // Set the texture unit we are working with.
    OpenGL->glActiveTexture(GL_TEXTURE0 + textureUnit);

    // Bind the CLUT data as a 3D texture.
    glBindTexture(GL_TEXTURE_3D, m_textureID);

    return;
}
