////////////////////////////////////////////////////////////////////////////////
// Filename: textureclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "textureclass.h"


TextureClass::TextureClass()
{
    m_loaded = false;
}


TextureClass::TextureClass(const TextureClass& other)
{
}


TextureClass::~TextureClass()
{
}


bool TextureClass::Initialize(OpenGLClass* OpenGL, char* filename, bool wrap)
{
    bool result;


    // Load the 32 bit texture from file.
    result = LoadTarga32Bit(OpenGL, filename, wrap);
    if(!result)
    {
        return false;
    }

    // Set that the texture is now loaded.
    m_loaded = true;

    return true;
}


void TextureClass::Shutdown()
{
    // If the texture was loaded then make sure to release it on shutdown.
    if(m_loaded)
    {
        glDeleteTextures(1, &m_textureID);
        m_loaded = false;
    }

    return;
}


bool TextureClass::LoadTarga32Bit(OpenGLClass* OpenGL, char* filename, bool wrap)
{
    TargaHeader targaFileHeader;
    FILE* filePtr;
    int bpp, error, index, i, j;
    unsigned long count, imageSize;
    unsigned char* targaData;
    unsigned char* targaImage;


    // Open the targa file for reading in binary.
    filePtr = fopen(filename, "rb");
    if(filePtr == NULL)
    {
        return false;
    }

    // Read in the file header.
    count = fread(&targaFileHeader, sizeof(TargaHeader), 1, filePtr);
    if(count != 1)
    {
        return false;
    }

    // Get the important information from the header.
    m_width = (int)targaFileHeader.width;
    m_height = (int)targaFileHeader.height;
    bpp = (int)targaFileHeader.bpp;

    // Check that it is 32 bit and not 24 bit.
    if(bpp != 32)
    {
        return false;
    }

    // Calculate the size of the 32 bit image data.
    imageSize = m_width * m_height * 4;

    // Allocate memory for the targa image data.
    targaImage = new unsigned char[imageSize];

    // Read in the targa image data.
    count = fread(targaImage, 1, imageSize, filePtr);
    if(count != imageSize)
    {
        return false;
    }

    // Close the file.
    error = fclose(filePtr);
    if(error != 0)
    {
        return false;
    }

    // Allocate memory for the targa destination data.
    targaData = new unsigned char[imageSize];

    // Initialize the index into the targa destination data array.
    index = 0;

    // Now copy the targa image data into the targa destination array in the correct order since the targa format is not stored in the RGBA order.
    for(j=0; j<m_height; j++)
    {
        for(i=0; i<m_width; i++)
        {
            targaData[index + 0] = targaImage[index + 2];  // Red.
            targaData[index + 1] = targaImage[index + 1];  // Green.
            targaData[index + 2] = targaImage[index + 0];  // Blue
            targaData[index + 3] = targaImage[index + 3];  // Alpha

            // Increment the indexes into the targa data.
            index += 4;
        }
    }

    // Release the targa image data now that it was copied into the destination array.
    delete [] targaImage;
    targaImage = 0;

    // Create the empty texture and generate an ID to reference it.
    glGenTextures(1, &m_textureID);

    // Set the texture unit (zero) that will be used while initially loading the data.
    OpenGL->glActiveTexture(GL_TEXTURE0 + 0);

    // Bind the texture as a 2D texture.
    glBindTexture(GL_TEXTURE_2D, m_textureID);

    // Copy the targa image data into the opengl texture.
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, m_width, m_height, 0, GL_RGBA, GL_UNSIGNED_BYTE, targaData);

    // Set the texture color to either wrap around or clamp to the edge.
    if(wrap)
    {
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
    }
    else
    {
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    }

    // Set the texture filtering.
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);

    // Generate mipmaps for the texture.
    OpenGL->glGenerateMipmap(GL_TEXTURE_2D);

    // Release the targa image data.
    delete [] targaData;
    targaData = 0;

    return true;
}


void TextureClass::SetTexture(OpenGLClass* OpenGL, unsigned int textureUnit)
{
    if(m_loaded)
    {
        // Set the texture unit we are working with.
        OpenGL->glActiveTexture(GL_TEXTURE0 + textureUnit);

        // Now use the texture ID to bind the texture to the texture unit.
        glBindTexture(GL_TEXTURE_2D, m_textureID);
    }

    return;
}


int TextureClass::GetWidth()
{
    return m_width;
}


int TextureClass::GetHeight()
{
    return m_height;
}
