////////////////////////////////////////////////////////////////////////////////
// Filename: modelclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "modelclass.h"


ModelClass::ModelClass()
{
    m_OpenGLPtr = 0;
}


ModelClass::ModelClass(const ModelClass& other)
{
}


ModelClass::~ModelClass()
{
}


bool ModelClass::Initialize(OpenGLClass* OpenGL)
{
    bool result;


    // Store a pointer to the OpenGL object.
    m_OpenGLPtr = OpenGL;

    // Initialize the vertex and index buffer that hold the geometry for the triangle.
    result = InitializeBuffers();
    if(!result)
    {
        return false;
    }

    return true;
}


void ModelClass::Shutdown()
{
    // Release the vertex and index buffers.
    ShutdownBuffers();

    // Release the pointer to the OpenGL object.
    m_OpenGLPtr = 0;

    return;
}


void ModelClass::Render()
{
    // Put the vertex and index buffers on the graphics pipeline to prepare them for drawing.
    RenderBuffers();

    return;
}


int ModelClass::GetIndexCount()
{
    return m_indexCount;
}


bool ModelClass::InitializeBuffers()
{
    VertexType* vertices;
    unsigned int* indices;


    // Set the number of vertices in the vertex array.
    m_vertexCount = 3;

    // Set the number of indices in the index array.
    m_indexCount = 3;

    // Create the vertex array.
    vertices = new VertexType[m_vertexCount];

    // Create the index array.
    indices = new unsigned int[m_indexCount];

    // Load the vertex array with data.

    // Bottom left.
    vertices[0].x = -1.0f;  // Position.
    vertices[0].y = -1.0f;
    vertices[0].z =  0.0f;

    vertices[0].r = 0.0f;  // Color.
    vertices[0].g = 1.0f;
    vertices[0].b = 0.0f;

    // Top middle.
    vertices[1].x = 0.0f;  // Position.
    vertices[1].y = 1.0f;
    vertices[1].z = 0.0f;

    vertices[1].r = 0.0f;  // Color.
    vertices[1].g = 1.0f;
    vertices[1].b = 0.0f;

    // Bottom right.
    vertices[2].x =  1.0f;  // Position.
    vertices[2].y = -1.0f;
    vertices[2].z =  0.0f;

    vertices[2].r = 0.0f;  // Color.
    vertices[2].g = 1.0f;
    vertices[2].b = 0.0f;

    // Load the index array with data.
    indices[0] = 0;  // Bottom left.
    indices[1] = 1;  // Top middle.
    indices[2] = 2;  // Bottom right.

    // Allocate an OpenGL vertex array object.
    m_OpenGLPtr->glGenVertexArrays(1, &m_vertexArrayId);

    // Bind the vertex array object to store all the buffers and vertex attributes we create here.
    m_OpenGLPtr->glBindVertexArray(m_vertexArrayId);

    // Generate an ID for the vertex buffer.
    m_OpenGLPtr->glGenBuffers(1, &m_vertexBufferId);

    // Bind the vertex buffer and load the vertex (position and color) data into the vertex buffer.
    m_OpenGLPtr->glBindBuffer(GL_ARRAY_BUFFER, m_vertexBufferId);
    m_OpenGLPtr->glBufferData(GL_ARRAY_BUFFER, m_vertexCount * sizeof(VertexType), vertices, GL_STATIC_DRAW);

    // Enable the two vertex array attributes.
    m_OpenGLPtr->glEnableVertexAttribArray(0);  // Vertex position.
    m_OpenGLPtr->glEnableVertexAttribArray(1);  // Vertex color.

    // Specify the location and format of the position portion of the vertex buffer.
    m_OpenGLPtr->glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeof(VertexType), 0);

    // Specify the location and format of the color portion of the vertex buffer.
    m_OpenGLPtr->glVertexAttribPointer(1, 3, GL_FLOAT, false, sizeof(VertexType), (unsigned char*)NULL + (3 * sizeof(float)));

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


void ModelClass::ShutdownBuffers()
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


void ModelClass::RenderBuffers()
{
    // Bind the vertex array object that stored all the information about the vertex and index buffers.
    m_OpenGLPtr->glBindVertexArray(m_vertexArrayId);

    // Set the number of control points per patch.  Three control points form the triangle patch that will be tessellated.
    m_OpenGLPtr->glPatchParameteri(GL_PATCH_VERTICES, 3);

    // Render the patch using the index buffer.  Each patch is dispatched to the tessellator and then drawn as triangles.
    glDrawElements(GL_PATCHES, m_indexCount, GL_UNSIGNED_INT, 0);

    return;
}
