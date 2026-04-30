////////////////////////////////////////////////////////////////////////////////
// Filename: spriteclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "spriteclass.h"


SpriteClass::SpriteClass()
{
    m_OpenGLPtr = 0;
    m_Textures = 0;
}


SpriteClass::SpriteClass(const SpriteClass& other)
{
}


SpriteClass::~SpriteClass()
{
}


bool SpriteClass::Initialize(OpenGLClass* OpenGL, int screenWidth, int screenHeight, int renderX, int renderY, char* spriteFilename)
{
    bool result;


    // Store a pointer to the OpenGL object.
    m_OpenGLPtr = OpenGL;

    // Store the screen size.
    m_screenWidth = screenWidth;
    m_screenHeight = screenHeight;

    // Store where the bitmap should be rendered to.
    m_renderX = renderX;
    m_renderY = renderY;

    // Initialize the frame time for this sprite object.
    m_frameTime = 0.0f;

    // Initialize the vertex and index buffer that hold the geometry for the sprite bitmap.
    result = InitializeBuffers();
    if(!result)
    {
        return false;
    }

    // Load the textures for this sprite.
    result = LoadTextures(spriteFilename);
    if(!result)
    {
      return false;
    }

    return true;
}


void SpriteClass::Shutdown()
{
    // Release the texture used for this sprite.
    ReleaseTextures();

    // Release the vertex and index buffers.
    ShutdownBuffers();

    // Release the pointer to the OpenGL object.
    m_OpenGLPtr = 0;

    return;
}


void SpriteClass::Render()
{
    // Update the buffers if the position of the sprite has changed from its original position.
    UpdateBuffers();

    // Put the vertex and index buffers on the graphics pipeline to prepare them for drawing.
    RenderBuffers();

    return;
}


void SpriteClass::Update(float frameTime)
{
    // Increment the frame time each frame.
    m_frameTime += frameTime;

    // Check if the frame time has reached the cycle time.
    if(m_frameTime >= m_cycleTime)
    {
        // If it has then reset the frame time and cycle to the next sprite in the texture array.
        m_frameTime -= m_cycleTime;

        m_currentTexture++;

        // If we are at the last sprite texture then go back to the beginning of the texture array to the first texture again.
        if(m_currentTexture == m_textureCount)
        {
            m_currentTexture = 0;
        }
    }

    return;
}


bool SpriteClass::InitializeBuffers()
{
    VertexType* vertices;
    unsigned int* indices;
    int i;


    // Initialize the previous rendering position to negative one.
    m_prevPosX = -1;
    m_prevPosY = -1;

    // Set the number of vertices in the vertex array.
    m_vertexCount = 6;

    // Set the number of indices in the index array.
    m_indexCount = m_vertexCount;

    // Create the vertex array.
    vertices = new VertexType[m_vertexCount];

    // Create the index array.
    indices = new unsigned int[m_indexCount];

    // Initialize the vertex arrays to zeros at first.
    memset(vertices, 0, (sizeof(VertexType) * m_vertexCount));

    // Load the index array with data.
    for(i=0; i<m_indexCount; i++)
    {
        indices[i] = i;
    }

    // Allocate an OpenGL vertex array object.
    m_OpenGLPtr->glGenVertexArrays(1, &m_vertexArrayId);

    // Bind the vertex array object to store all the buffers and vertex attributes we create here.
    m_OpenGLPtr->glBindVertexArray(m_vertexArrayId);

    // Generate an ID for the vertex buffer.
    m_OpenGLPtr->glGenBuffers(1, &m_vertexBufferId);

    // Bind the vertex buffer and load the vertex data into the vertex buffer.  Set gpu hint to dynamic since it will change once in a while.
    m_OpenGLPtr->glBindBuffer(GL_ARRAY_BUFFER, m_vertexBufferId);
    m_OpenGLPtr->glBufferData(GL_ARRAY_BUFFER, m_vertexCount * sizeof(VertexType), vertices, GL_DYNAMIC_DRAW);

    // Enable the two vertex array attributes.
    m_OpenGLPtr->glEnableVertexAttribArray(0);  // Vertex position.
    m_OpenGLPtr->glEnableVertexAttribArray(1);  // Texture coordinates.

    // Specify the location and format of the position portion of the vertex buffer.
    m_OpenGLPtr->glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeof(VertexType), 0);

    // Specify the location and format of the texture coordinate portion of the vertex buffer.
    m_OpenGLPtr->glVertexAttribPointer(1, 2, GL_FLOAT, false, sizeof(VertexType), (unsigned char*)NULL + (3 * sizeof(float)));

    // Generate an ID for the index buffer.
    m_OpenGLPtr->glGenBuffers(1, &m_indexBufferId);

    // Bind the index buffer and load the index data into it.  Leave it static since the indices won't change, only the vertices.
    m_OpenGLPtr->glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, m_indexBufferId);
    m_OpenGLPtr->glBufferData(GL_ELEMENT_ARRAY_BUFFER, m_indexCount* sizeof(unsigned int), indices, GL_STATIC_DRAW);

    // Now that the buffers have been loaded we can release the array data.
    delete [] vertices;
    vertices = 0;

    delete [] indices;
    indices = 0;

    return true;
}


void SpriteClass::ShutdownBuffers()
{
    // Release the vertex array object.
    m_OpenGLPtr->glBindVertexArray(0);
    m_OpenGLPtr->glDeleteVertexArrays(1, &m_vertexArrayId);
    
    // Release the vertex buffer.
    m_OpenGLPtr->glBindBuffer(GL_ARRAY_BUFFER, 0);
    m_OpenGLPtr->glDeleteBuffers(1, &m_vertexBufferId);

    // Release the index buffer.
    m_OpenGLPtr->glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
    m_OpenGLPtr->glDeleteBuffers(1, &m_indexBufferId);

    return;
}


bool SpriteClass::UpdateBuffers()
{
    VertexType* vertices;
    void* dataPtr;
    float left, right, top, bottom;


    // If the position we are rendering this sprite to hasn't changed then don't update the vertex buffer.
    if((m_prevPosX == m_renderX) && (m_prevPosY == m_renderY))
    {
        return true;
    }

    // If the rendering location has changed then store the new position and update the vertex buffer.
    m_prevPosX = m_renderX;
    m_prevPosY = m_renderY;

    // Create the vertex array.
    vertices = new VertexType[m_vertexCount];

    // Calculate the screen coordinates of the left side of the sprite.
    left = (float)((m_screenWidth / 2) * -1) + (float)m_renderX;

    // Calculate the screen coordinates of the right side of the sprite.
    right = left + (float)m_bitmapWidth;

    // Calculate the screen coordinates of the top of the sprite.
    top = (float)(m_screenHeight / 2) - (float)m_renderY;

    // Calculate the screen coordinates of the bottom of the sprite.
    bottom = top - (float)m_bitmapHeight;

    // Load the vertex array with data.

    // First triangle.
    vertices[0].x = left;  // Top left.
    vertices[0].y = top;
    vertices[0].z =  0.0f;
    vertices[0].tu = 0.0f;
    vertices[0].tv = 1.0f;

    vertices[1].x = right;  // Bottom right.
    vertices[1].y = bottom;
    vertices[1].z =  0.0f;
    vertices[1].tu = 1.0f;
    vertices[1].tv = 0.0f;

    vertices[2].x = left;  // Bottom left.
    vertices[2].y = bottom;
    vertices[2].z =  0.0f;
    vertices[2].tu = 0.0f;
    vertices[2].tv = 0.0f;

    // Second triangle.
    vertices[3].x = left;  // Top left.
    vertices[3].y = top;
    vertices[3].z =  0.0f;
    vertices[3].tu = 0.0f;
    vertices[3].tv = 1.0f;

    vertices[4].x = right;  // Top right.
    vertices[4].y = top;
    vertices[4].z =  0.0f;
    vertices[4].tu = 1.0f;
    vertices[4].tv = 1.0f;

    vertices[5].x = right;  // Bottom right.
    vertices[5].y = bottom;
    vertices[5].z =  0.0f;
    vertices[5].tu = 1.0f;
    vertices[5].tv = 0.0f;

    // Bind the vertex buffer.
    m_OpenGLPtr->glBindBuffer(GL_ARRAY_BUFFER, m_vertexBufferId);

    // Get a pointer to the buffer's actual location in memory.
    dataPtr = m_OpenGLPtr->glMapBuffer(GL_ARRAY_BUFFER, GL_WRITE_ONLY);

    // Copy the vertex data into memory.
    memcpy(dataPtr, vertices, m_vertexCount * sizeof(VertexType));

    // Unlock the vertex buffer.
    m_OpenGLPtr->glUnmapBuffer(GL_ARRAY_BUFFER);

    // Now that the vertex buffer has been loaded we can release the array data.
    delete [] vertices;
    vertices = 0;

    return true;
}


void SpriteClass::RenderBuffers()
{
    // Bind the vertex array object that stored all the information about the vertex and index buffers.
    m_OpenGLPtr->glBindVertexArray(m_vertexArrayId);

    // Render the vertex buffer using the index buffer.
    glDrawElements(GL_TRIANGLES, m_indexCount, GL_UNSIGNED_INT, 0);

    return;
}


bool SpriteClass::LoadTextures(char* filename)
{
    char textureFilename[128];
    ifstream fin;
    int i, j, cycleTime;
    char input;
    bool result;


    // Open the sprite info data file.
    fin.open(filename);
    if(fin.fail())
    {
        return false;
    }

    // Read in the number of textures.
    fin >> m_textureCount;

    // Create and initialize the texture array with the texture count from the file.
    m_Textures = new TextureClass[m_textureCount];

    // Read to start of next line.
    fin.get(input);

    // Read in each texture file name.
    for(i=0; i<m_textureCount; i++)
    {
        j=0;
        fin.get(input);
        while(input != '\n')
        {
            textureFilename[j] = input;
            j++;
            fin.get(input);
        }
        textureFilename[j] = '\0';

        // Once you have the filename then load the texture in the texture array.
        result = m_Textures[i].Initialize(m_OpenGLPtr, textureFilename, false);
        if(!result)
        {
            return false;
        }
    }

    // Read in the cycle time.
    fin >> cycleTime;

    // Convert integer millisecond cycle time to floating point as seconds.
    m_cycleTime = (float)cycleTime * 0.001f;
    
    // Close the file.
    fin.close();

    // Get the dimensions of the first texture and use that as the dimensions of the 2D sprite images.
    m_bitmapWidth = m_Textures[0].GetWidth();
    m_bitmapHeight = m_Textures[0].GetHeight();

    // Set the starting texture in the cycle to be the first one in the list.
    m_currentTexture = 0;

    return true;
}


void SpriteClass::ReleaseTextures()
{
    int i;


    // Release the texture objects.
    if(m_Textures)
    {
        for(i=0; i<m_textureCount; i++)
        {
            m_Textures[i].Shutdown();
        }

	delete [] m_Textures;
	m_Textures = 0;
    }

    return;
}


void SpriteClass::SetTexture(unsigned int textureUnit)
{
    // Set the current texture in the sprite cycle.
    m_Textures[m_currentTexture].SetTexture(m_OpenGLPtr, textureUnit);

    return;
}

void SpriteClass::SetRenderLocation(int x, int y)
{
    m_renderX = x;
    m_renderY = y;
    return;
}
