////////////////////////////////////////////////////////////////////////////////
// Filename: modelclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "modelclass.h"


ModelClass::ModelClass()
{
    m_OpenGLPtr = 0;
    m_Texture = 0;
    m_texture1Loaded = false;
}


ModelClass::ModelClass(const ModelClass& other)
{
}


ModelClass::~ModelClass()
{
}


bool ModelClass::Initialize(OpenGLClass* OpenGL, char* textureFilename1, bool wrap1)
{
    bool result;


    // Store a pointer to the OpenGL object.
    m_OpenGLPtr = OpenGL;

    // Initialize the vertex and instance buffers.
    result = InitializeBuffers();
    if(!result)
    {
      return false;
    }

    // Load the texture for this model.
    result = LoadTexture(textureFilename1, wrap1);
    if(!result)
    {
      return false;
    }

    return true;
}


void ModelClass::Shutdown()
{
    // Release the texture used for this model.
    ReleaseTexture();

    // Release the vertex and instance buffers.
    ShutdownBuffers();

    // Release the pointer to the OpenGL object.
    m_OpenGLPtr = 0;

    return;
}


void ModelClass::Render()
{
    // Put the vertex and instance buffers on the graphics pipeline to prepare them for drawing.
    RenderBuffers();

    return;
}


bool ModelClass::InitializeBuffers()
{
    VertexType* vertices;
    InstanceType* instances;


    // Set the number of vertices in the vertex array.
    m_vertexCount = 3;

    // Create the vertex array.
    vertices = new VertexType[m_vertexCount];

    // Bottom left.
    vertices[0].x = -1.0f;  // Position.
    vertices[0].y = -1.0f;
    vertices[0].z =  0.0f;
    vertices[0].tu = 0.0f;  // Texture
    vertices[0].tv = 0.0f;

    // Top middle.
    vertices[1].x =  0.0f;  // Position.
    vertices[1].y =  1.0f;
    vertices[1].z =  0.0f;
    vertices[1].tu = 0.5f;  // Texture
    vertices[1].tv = 1.0f;

    // Bottom right.
    vertices[2].x =  1.0f;  // Position.
    vertices[2].y = -1.0f;
    vertices[2].z =  0.0f;
    vertices[2].tu = 1.0f;  // Texture
    vertices[2].tv = 0.0f;

    // Set the number of instances in the array.
    m_instanceCount = 4;

    // Create the instance array.
    instances = new InstanceType[m_instanceCount];

    // Load the instance array with data.
    instances[0].x = -1.5f;
    instances[0].y = -1.5f;
    instances[0].z =  5.0f;
    
    instances[1].x = -1.5f;
    instances[1].y =  1.5f;
    instances[1].z =  5.0f;
    
    instances[2].x =  1.5f;
    instances[2].y = -1.5f;
    instances[2].z =  5.0f;
    
    instances[3].x = 1.5f;
    instances[3].y = 1.5f;
    instances[3].z = 5.0f;

    // Allocate an OpenGL vertex array object.
    m_OpenGLPtr->glGenVertexArrays(1, &m_vertexArrayId);

    // Bind the vertex array object to store all the buffers and vertex attributes we create here.
    m_OpenGLPtr->glBindVertexArray(m_vertexArrayId);

    // Generate an ID for the vertex buffer.
    m_OpenGLPtr->glGenBuffers(1, &m_vertexBufferId);

    // Bind the vertex buffer and load the vertex (position and texture coords) data into the vertex buffer.
    m_OpenGLPtr->glBindBuffer(GL_ARRAY_BUFFER, m_vertexBufferId);
    m_OpenGLPtr->glBufferData(GL_ARRAY_BUFFER, m_vertexCount * sizeof(VertexType), vertices, GL_STATIC_DRAW);

    // Enable the two vertex array attributes.
    m_OpenGLPtr->glEnableVertexAttribArray(0);  // Vertex position.
    m_OpenGLPtr->glEnableVertexAttribArray(1);  // Texture coordinates.
    
    // Specify the location and format of the position portion of the vertex buffer.
    m_OpenGLPtr->glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeof(VertexType), 0);

    // Specify the location and format of the texture coordinates portion of the vertex buffer.
    m_OpenGLPtr->glVertexAttribPointer(1, 2, GL_FLOAT, false, sizeof(VertexType), (unsigned char*)NULL + (3 * sizeof(float)));

    // Generate an ID for the instance buffer.
    m_OpenGLPtr->glGenBuffers(1, &m_instanceBufferId);

    // Bind the instance buffer and load the instance data into it.
    m_OpenGLPtr->glBindBuffer(GL_ARRAY_BUFFER, m_instanceBufferId);
    m_OpenGLPtr->glBufferData(GL_ARRAY_BUFFER, m_instanceCount* sizeof(InstanceType), instances, GL_STATIC_DRAW);

    // Enable the instance array attribute.
    m_OpenGLPtr->glEnableVertexAttribArray(2);  // Instanced position.
    
    // Specify the location and format of the instanced position portion of the instance buffer.
    m_OpenGLPtr->glVertexAttribPointer(2, 3, GL_FLOAT, false, sizeof(InstanceType), 0);
    
    // Set the instance data step rate to one instance element per update.
    m_OpenGLPtr->glVertexAttribDivisor(2, 1);
    
    // Now that the buffers have been loaded we can release the array data.
    delete [] vertices;
    vertices = 0;

    delete [] instances;
    instances = 0;

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

    // Release the instance buffer.
    m_OpenGLPtr->glBindBuffer(GL_ARRAY_BUFFER, 0);
    m_OpenGLPtr->glDeleteBuffers(1, &m_instanceBufferId);

    return;
}


void ModelClass::RenderBuffers()
{
    // Bind the vertex array object that stored all the information about the vertex and instance buffers.
    m_OpenGLPtr->glBindVertexArray(m_vertexArrayId);

    // Render the vertex buffer using the instance buffer.
    m_OpenGLPtr->glDrawArraysInstanced(GL_TRIANGLES, 0, 3, m_instanceCount);

    return;
}


bool ModelClass::LoadTexture(char* textureFilename1, bool wrap1)
{
    if(textureFilename1 != NULL)
    {
        // Create and initialize the texture object.
        m_Texture = new TextureClass;

	m_texture1Loaded = m_Texture->Initialize(m_OpenGLPtr, textureFilename1, wrap1);
	if(!m_texture1Loaded)
	{
	    return false;
	}
    }

    return true;
}


void ModelClass::ReleaseTexture()
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


void ModelClass::SetTexture1(unsigned int textureUnit)
{
    // Set the first texture for the model.
    if(m_texture1Loaded)
    {
        m_Texture->SetTexture(m_OpenGLPtr, textureUnit);
    }
  
    return;
}
