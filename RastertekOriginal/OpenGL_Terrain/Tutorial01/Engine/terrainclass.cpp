///////////////////////////////////////////////////////////////////////////////
// Filename: terrainclass.cpp
///////////////////////////////////////////////////////////////////////////////
#include "terrainclass.h"


TerrainClass::TerrainClass()
{
}


TerrainClass::TerrainClass(const TerrainClass& other)
{
}


TerrainClass::~TerrainClass()
{
}


bool TerrainClass::Initialize(OpenGLClass* OpenGL)
{
    bool result;


    // Initialize the vertex and index buffer that hold the geometry for the terrain.
    result = InitializeBuffers(OpenGL);
    if(!result)
    {
        return false;
    }

    return true;
}


void TerrainClass::Shutdown(OpenGLClass* OpenGL)
{
    // Release the vertex and index buffers.
    ShutdownBuffers(OpenGL);

    return;
}


bool TerrainClass::Render(OpenGLClass* OpenGL, ShaderManagerClass* ShaderManager, float* worldMatrix, float* viewMatrix, float* projectionMatrix)
{
    bool result;

  
    // Set the terrain shader as the current shader program and set the matrices that it will use for rendering.
    result = ShaderManager->RenderTerrainShader(worldMatrix, viewMatrix, projectionMatrix);
    if(!result)
    {
        return false;
    }

    // Put the vertex and index buffers on the graphics pipeline to prepare them for drawing.
    RenderBuffers(OpenGL);

    return true;
}


bool TerrainClass::InitializeBuffers(OpenGLClass* OpenGL)
{
    VertexType* vertices;
    unsigned int* indices;
    int terrainHeight, terrainWidth, index, i, j;
    float red, green, blue;


    // Set the height and width of the terrain grid.
    terrainHeight = 256;
    terrainWidth = 256;

    // Set the color of the terrain grid.
    red = 0.0f;
    green = 0.8f;
    blue = 1.0f;

    // Calculate the number of vertices in the terrain.
    m_vertexCount = (terrainHeight - 1) * (terrainWidth - 1) * 8;

    // Set the index count to the same as the vertex count.
    m_indexCount = m_vertexCount;

    // Create the vertex array.
    vertices = new VertexType[m_vertexCount];

    // Create the index array.
    indices = new unsigned int[m_indexCount];

    // Initialize the index into the vertex and index arrays.
    index = 0;

    // Load the vertex array and index array with data.
    for(j=0; j<(terrainHeight-1); j++)
    {
        for(i=0; i<(terrainWidth-1); i++)
        {
            // Line 1 - Upper left.
            vertices[index].x = (float)i;
            vertices[index].y = 0.0f;
            vertices[index].z = (float)(j+1);
            vertices[index].r = red;
            vertices[index].g = green;
            vertices[index].b = blue;
            indices[index] = index;
            index++;

            // Line 1 - Upper right.
            vertices[index].x = (float)(i+1);
            vertices[index].y = 0.0f;
            vertices[index].z = (float)(j+1);
            vertices[index].r = red;
            vertices[index].g = green;
            vertices[index].b = blue;
            indices[index] = index;
            index++;

            // Line 2 - Upper right.
            vertices[index].x = (float)(i+1);
            vertices[index].y = 0.0f;
            vertices[index].z = (float)(j+1);
            vertices[index].r = red;
            vertices[index].g = green;
            vertices[index].b = blue;
            indices[index] = index;
            index++;

            // Line 2 - Bottom right.
            vertices[index].x = (float)(i+1);
            vertices[index].y = 0.0f;
            vertices[index].z = (float)j;
            vertices[index].r = red;
            vertices[index].g = green;
            vertices[index].b = blue;
            indices[index] = index;
            index++;

            // Line 3 - Bottom right.
            vertices[index].x = (float)(i+1);
            vertices[index].y = 0.0f;
            vertices[index].z = (float)j;
            vertices[index].r = red;
            vertices[index].g = green;
            vertices[index].b = blue;
            indices[index] = index;
            index++;

            // Line 3 - Bottom left.
            vertices[index].x = (float)i;
            vertices[index].y = 0.0f;
            vertices[index].z = (float)j;
            vertices[index].r = red;
            vertices[index].g = green;
            vertices[index].b = blue;
            indices[index] = index;
            index++;

            // Line 4 - Bottom left.
            vertices[index].x = (float)i;
            vertices[index].y = 0.0f;
            vertices[index].z = (float)j;
            vertices[index].r = red;
            vertices[index].g = green;
            vertices[index].b = blue;
            indices[index] = index;
            index++;

            // Line 4 - Bottom left.
            vertices[index].x = (float)i;
            vertices[index].y = 0.0f;
            vertices[index].z = (float)(j+1);
            vertices[index].r = red;
            vertices[index].g = green;
            vertices[index].b = blue;
            indices[index] = index;
            index++;
        }
    }

    // Allocate an OpenGL vertex array object.
    OpenGL->glGenVertexArrays(1, &m_vertexArrayId);

    // Bind the vertex array object to store all the buffers and vertex attributes we create here.
    OpenGL->glBindVertexArray(m_vertexArrayId);

    // Generate an ID for the vertex buffer.
    OpenGL->glGenBuffers(1, &m_vertexBufferId);

    // Bind the vertex buffer and load the vertex (position and color) data into the vertex buffer.
    OpenGL->glBindBuffer(GL_ARRAY_BUFFER, m_vertexBufferId);
    OpenGL->glBufferData(GL_ARRAY_BUFFER, m_vertexCount * sizeof(VertexType), vertices, GL_STATIC_DRAW);

    // Enable the two vertex array attributes.
    OpenGL->glEnableVertexAttribArray(0);  // Vertex position.
    OpenGL->glEnableVertexAttribArray(1);  // Vertex color.

    // Specify the location and format of the position portion of the vertex buffer.
    OpenGL->glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeof(VertexType), 0);

    // Specify the location and format of the color portion of the vertex buffer.
    OpenGL->glVertexAttribPointer(1, 3, GL_FLOAT, false, sizeof(VertexType), (unsigned char*)NULL + (3 * sizeof(float)));

    // Generate an ID for the index buffer.
    OpenGL->glGenBuffers(1, &m_indexBufferId);

    // Bind the index buffer and load the index data into it.
    OpenGL->glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, m_indexBufferId);
    OpenGL->glBufferData(GL_ELEMENT_ARRAY_BUFFER, m_indexCount* sizeof(unsigned int), indices, GL_STATIC_DRAW);

    // Now that the buffers have been loaded we can release the array data.
    delete [] vertices;
    vertices = 0;

    delete [] indices;
    indices = 0;

    return true;
}


void TerrainClass::ShutdownBuffers(OpenGLClass* OpenGL)
{
    // Release the vertex array object.
    OpenGL->glBindVertexArray(0);
    OpenGL->glDeleteVertexArrays(1, &m_vertexArrayId);

    // Release the vertex buffer.
    OpenGL->glBindBuffer(GL_ARRAY_BUFFER, 0);
    OpenGL->glDeleteBuffers(1, &m_vertexBufferId);

    // Release the index buffer.
    OpenGL->glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
    OpenGL->glDeleteBuffers(1, &m_indexBufferId);

    return;
}


void TerrainClass::RenderBuffers(OpenGLClass* OpenGL)
{
    // Bind the vertex array object that stored all the information about the vertex and index buffers.
    OpenGL->glBindVertexArray(m_vertexArrayId);

    // Render the vertex buffer as lines using the index buffer.
    glDrawElements(GL_LINES, m_indexCount, GL_UNSIGNED_INT, 0);

    return;
}
