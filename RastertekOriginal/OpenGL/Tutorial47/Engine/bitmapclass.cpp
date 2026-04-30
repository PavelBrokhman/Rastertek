////////////////////////////////////////////////////////////////////////////////
// Filename: bitmapclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "bitmapclass.h"


BitmapClass::BitmapClass()
{
    m_OpenGLPtr = 0;
    m_Texture = 0;
}


BitmapClass::BitmapClass(const BitmapClass& other)
{
}


BitmapClass::~BitmapClass()
{
}


bool BitmapClass::Initialize(OpenGLClass* OpenGL, int screenWidth, int screenHeight, char* textureFilename, int renderX, int renderY)
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

    // Initialize the vertex and index buffer that hold the geometry for the bitmap.
    result = InitializeBuffers();
    if(!result)
    {
        return false;
    }

    // Load the texture for this model.
    result = LoadTexture(textureFilename);
    if(!result)
    {
        return false;
    }

    return true;
}


void BitmapClass::Shutdown()
{
    // Release the texture used for this model.
    ReleaseTexture();

    // Release the vertex and index buffers.
    ShutdownBuffers();

    // Release the pointer to the OpenGL object.
    m_OpenGLPtr = 0;

    return;
}


void BitmapClass::Render()
{
    // Update the buffers if the position of the bitmap has changed from its original position.
    UpdateBuffers();

    // Put the vertex and index buffers on the graphics pipeline to prepare them for drawing.
    RenderBuffers();

    return;
}


bool BitmapClass::InitializeBuffers()
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


void BitmapClass::ShutdownBuffers()
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


bool BitmapClass::UpdateBuffers()
{
    VertexType* vertices;
    void* dataPtr;
    float left, right, top, bottom;


    // If the position we are rendering this bitmap to hasn't changed then don't update the vertex buffer.
    if((m_prevPosX == m_renderX) && (m_prevPosY == m_renderY))
    {
        return true;
    }

    // If the rendering location has changed then store the new position and update the vertex buffer.
    m_prevPosX = m_renderX;
    m_prevPosY = m_renderY;

    // Create the vertex array.
    vertices = new VertexType[m_vertexCount];

    // Calculate the screen coordinates of the left side of the bitmap.
    left = (float)((m_screenWidth / 2) * -1) + (float)m_renderX;

    // Calculate the screen coordinates of the right side of the bitmap.
    right = left + (float)m_bitmapWidth;

    // Calculate the screen coordinates of the top of the bitmap.
    top = (float)(m_screenHeight / 2) - (float)m_renderY;

    // Calculate the screen coordinates of the bottom of the bitmap.
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


void BitmapClass::RenderBuffers()
{
    // Bind the vertex array object that stored all the information about the vertex and index buffers.
    m_OpenGLPtr->glBindVertexArray(m_vertexArrayId);

    // Render the vertex buffer using the index buffer.
    glDrawElements(GL_TRIANGLES, m_indexCount, GL_UNSIGNED_INT, 0);

    return;
}


bool BitmapClass::LoadTexture(char* textureFilename)
{
    bool result;


    // Create and initialize the texture object.
    m_Texture = new TextureClass;

    result = m_Texture->Initialize(m_OpenGLPtr, textureFilename, false);
    if(!result)
    {
        return false;
    }

    // Get the dimensions of the texture and use that as the dimensions of the 2D bitmap image.
    m_bitmapWidth = m_Texture->GetWidth();
    m_bitmapHeight = m_Texture->GetHeight();

    return true;
}


void BitmapClass::ReleaseTexture()
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


void BitmapClass::SetTexture(unsigned int textureUnit)
{
    // Set the texture for the model.
    m_Texture->SetTexture(m_OpenGLPtr, textureUnit);

    return;
}


void BitmapClass::SetRenderLocation(int x, int y)
{
    m_renderX = x;
    m_renderY = y;
    return;
}
