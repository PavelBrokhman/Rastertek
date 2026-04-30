////////////////////////////////////////////////////////////////////////////////
// Filename: displayplaneclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "displayplaneclass.h"


DisplayPlaneClass::DisplayPlaneClass()
{
    m_OpenGLPtr = 0;
}


DisplayPlaneClass::DisplayPlaneClass(const DisplayPlaneClass& other)
{
}


DisplayPlaneClass::~DisplayPlaneClass()
{
}


bool DisplayPlaneClass::Initialize(OpenGLClass* OpenGL, float width, float height)
{
    bool result;


    // Store a pointer to the OpenGL object.
    m_OpenGLPtr = OpenGL;

    // Initialize the vertex and index buffer that hold the geometry for the display plane.
    result = InitializeBuffers(width, height);
    if(!result)
    {
        return false;
    }

    return true;
}


void DisplayPlaneClass::Shutdown()
{
    // Release the vertex and index buffers.
    ShutdownBuffers();

    // Release the pointer to the OpenGL object.
    m_OpenGLPtr = 0;

    return;
}


void DisplayPlaneClass::Render()
{
    // Put the vertex and index buffers on the graphics pipeline to prepare them for drawing.
    RenderBuffers();

    return;
}


bool DisplayPlaneClass::InitializeBuffers(float width, float height)
{
    VertexType* vertices;
    unsigned int* indices;
    int i;


    // Set the number of vertices in the vertex array.
    m_vertexCount = 6;

    // Set the number of indices in the index array.
    m_indexCount = m_vertexCount;

    // Create the vertex array.
    vertices = new VertexType[m_vertexCount];

    // Create the index array.
    indices = new unsigned int[m_indexCount];

    // Load the vertex array with data.

    // First triangle.
    vertices[0].x = -width;  // Top left.
    vertices[0].y = height;
    vertices[0].z =  0.0f;
    vertices[0].tu = 0.0f;
    vertices[0].tv = 1.0f;

    vertices[1].x = width;  // Bottom right.
    vertices[1].y = -height;
    vertices[1].z =  0.0f;
    vertices[1].tu = 1.0f;
    vertices[1].tv = 0.0f;

    vertices[2].x = -width;  // Bottom left.
    vertices[2].y = -height;
    vertices[2].z =  0.0f;
    vertices[2].tu = 0.0f;
    vertices[2].tv = 0.0f;

    // Second triangle.
    vertices[3].x = -width;  // Top left.
    vertices[3].y = height;
    vertices[3].z =  0.0f;
    vertices[3].tu = 0.0f;
    vertices[3].tv = 1.0f;

    vertices[4].x = width;  // Top right.
    vertices[4].y = height;
    vertices[4].z =  0.0f;
    vertices[4].tu = 1.0f;
    vertices[4].tv = 1.0f;

    vertices[5].x = width;  // Bottom right.
    vertices[5].y = -height;
    vertices[5].z =  0.0f;
    vertices[5].tu = 1.0f;
    vertices[5].tv = 0.0f;

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

    // Bind the vertex buffer and load the vertex data into the vertex buffer.
    m_OpenGLPtr->glBindBuffer(GL_ARRAY_BUFFER, m_vertexBufferId);
    m_OpenGLPtr->glBufferData(GL_ARRAY_BUFFER, m_vertexCount * sizeof(VertexType), vertices, GL_STATIC_DRAW);

    // Enable the two vertex array attributes.
    m_OpenGLPtr->glEnableVertexAttribArray(0);  // Vertex position.
    m_OpenGLPtr->glEnableVertexAttribArray(1);  // Texture coordinates.

    // Specify the location and format of the position portion of the vertex buffer.
    m_OpenGLPtr->glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeof(VertexType), 0);

    // Specify the location and format of the texture coordinates portion of the vertex buffer.
    m_OpenGLPtr->glVertexAttribPointer(1, 2, GL_FLOAT, false, sizeof(VertexType), (unsigned char*)NULL + (3 * sizeof(float)));

    // Generate an ID for the index buffer.
    m_OpenGLPtr->glGenBuffers(1, &m_indexBufferId);

    // Bind the index buffer and load the index data into it.
    m_OpenGLPtr->glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, m_indexBufferId);
    m_OpenGLPtr->glBufferData(GL_ELEMENT_ARRAY_BUFFER, m_indexCount* sizeof(unsigned int), indices, GL_STATIC_DRAW);

    // Now that the buffers have been loaded we can release the array data.
    delete [] vertices;
    vertices = 0;

    delete [] indices;
    indices = 0;

    return true;
}


void DisplayPlaneClass::ShutdownBuffers()
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


void DisplayPlaneClass::RenderBuffers()
{
    // Bind the vertex array object that stored all the information about the vertex and index buffers.
    m_OpenGLPtr->glBindVertexArray(m_vertexArrayId);

    // Render the vertex buffer using the index buffer.
    glDrawElements(GL_TRIANGLES, m_indexCount, GL_UNSIGNED_INT, 0);

    return;
}
