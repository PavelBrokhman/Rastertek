////////////////////////////////////////////////////////////////////////////////
// Filename: modelclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "modelclass.h"


ModelClass::ModelClass()
{
    m_OpenGLPtr = 0;
    m_Textures = 0;
    m_texture1Loaded = false;
    m_texture2Loaded = false;
    m_texture3Loaded = false;    
    m_model = 0;
}


ModelClass::ModelClass(const ModelClass& other)
{
}


ModelClass::~ModelClass()
{
}


bool ModelClass::Initialize(OpenGLClass* OpenGL, char* modelFilename, char* textureFilename1, bool wrap1, char* textureFilename2, bool wrap2,
			    char* textureFilename3, bool wrap3)
{
    bool result;


    // Store a pointer to the OpenGL object.
    m_OpenGLPtr = OpenGL;

    // Load in the model data.
    result = LoadModel(modelFilename);
    if(!result)
    {
        return false;
    }
    
    // Initialize the vertex and index buffer that hold the geometry for the triangle.
    result = InitializeBuffers();
    if(!result)
    {
      return false;
    }

    // Load the textures for this model.
    result = LoadTextures(textureFilename1, wrap1, textureFilename2, wrap2, textureFilename3, wrap3);
    if(!result)
    {
      return false;
    }

    return true;
}


void ModelClass::Shutdown()
{
    // Release the textures used for this model.
    ReleaseTextures();

    // Release the vertex and index buffers.
    ShutdownBuffers();

    // Release the model data.
    ReleaseModel();
    
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


bool ModelClass::InitializeBuffers()
{
    VertexType* vertices;
    unsigned int* indices;
    int i;
    

    // Create the vertex array.
    vertices = new VertexType[m_vertexCount];

    // Create the index array.
    indices = new unsigned int[m_indexCount];

    // Load the vertex array and index array with data.
    for(i=0; i<m_vertexCount; i++)
    {
        vertices[i].x  = m_model[i].x;
        vertices[i].y  = m_model[i].y;
        vertices[i].z  = m_model[i].z;
        vertices[i].tu = m_model[i].tu;
        vertices[i].tv = m_model[i].tv;
        vertices[i].nx = m_model[i].nx;
        vertices[i].ny = m_model[i].ny;
        vertices[i].nz = m_model[i].nz;

        indices[i] = i;
    }
    
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
    m_OpenGLPtr->glEnableVertexAttribArray(1);  // Texture coordinates.
    m_OpenGLPtr->glEnableVertexAttribArray(2);  // Normals
 
    // Specify the location and format of the position portion of the vertex buffer.
    m_OpenGLPtr->glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeof(VertexType), 0);

    // Specify the location and format of the texture coordinates portion of the vertex buffer.
    m_OpenGLPtr->glVertexAttribPointer(1, 2, GL_FLOAT, false, sizeof(VertexType), (unsigned char*)NULL + (3 * sizeof(float)));

    // Specify the location and format of the normal vector portion of the vertex buffer.
    m_OpenGLPtr->glVertexAttribPointer(2, 3, GL_FLOAT, false, sizeof(VertexType), (unsigned char*)NULL + (5 * sizeof(float)));
    
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

    // Render the vertex buffer using the index buffer.
    glDrawElements(GL_TRIANGLES, m_indexCount, GL_UNSIGNED_INT, 0);

    return;
}


bool ModelClass::LoadTextures(char* textureFilename1, bool wrap1, char* textureFilename2, bool wrap2, char* textureFilename3, bool wrap3)
{
    // Create and initialize the texture objects.
    m_Textures = new TextureClass[3];

    if(textureFilename1 != NULL)
    {
        m_texture1Loaded = m_Textures[0].Initialize(m_OpenGLPtr, textureFilename1, wrap1);
	if(!m_texture1Loaded)
	{
	    return false;
	}
    }

    if(textureFilename2 != NULL)
    {
        m_texture2Loaded = m_Textures[1].Initialize(m_OpenGLPtr, textureFilename2, wrap2);
	if(!m_texture2Loaded)
	{
	    return false;
	}
    }

    if(textureFilename3 != NULL)
    {
        m_texture3Loaded = m_Textures[2].Initialize(m_OpenGLPtr, textureFilename3, wrap3);
	if(!m_texture3Loaded)
	{
	    return false;
	}
    }
    
    return true;
}


void ModelClass::ReleaseTextures()
{
    // Release the texture objects.
    if(m_Textures)
    {
        m_Textures[0].Shutdown();
        m_Textures[1].Shutdown();
	m_Textures[2].Shutdown();

        delete [] m_Textures;
        m_Textures = 0;
    }

    return;
}


void ModelClass::SetTexture1(unsigned int textureUnit)
{
    // Set the first texture for the model.
    if(m_texture1Loaded)
    {
        m_Textures[0].SetTexture(m_OpenGLPtr, textureUnit);
    }
  
    return;
}


void ModelClass::SetTexture2(unsigned int textureUnit)
{
    // Set the second texture for the model.
    if(m_texture2Loaded)
    {
        m_Textures[1].SetTexture(m_OpenGLPtr, textureUnit);
    }
  
    return;
}


void ModelClass::SetTexture3(unsigned int textureUnit)
{
    // Set the third texture for the model.
    if(m_texture3Loaded)
    {
        m_Textures[2].SetTexture(m_OpenGLPtr, textureUnit);
    }
  
    return;
}


bool ModelClass::LoadModel(char* filename)
{
    ifstream fin;
    char input;
    int i;


    // Open the model file.
    fin.open(filename);

    // If it could not open the file then exit.
    if(fin.fail())
    {
        return false;
    }

    // Read up to the value of vertex count.
    fin.get(input);
    while(input != ':')
    {
        fin.get(input);
    }

    // Read in the vertex count.
    fin >> m_vertexCount;

    // Set the number of indices to be the same as the vertex count.
    m_indexCount = m_vertexCount;

    // Create the model using the vertex count that was read in.
    m_model = new ModelType[m_vertexCount];

    // Read up to the beginning of the data.
    fin.get(input);
    while(input != ':')
    {
        fin.get(input);
    }
    fin.get(input);
    fin.get(input);

    // Read in the vertex data.
    for(i=0; i<m_vertexCount; i++)
    {
        fin >> m_model[i].x >> m_model[i].y >> m_model[i].z;
        fin >> m_model[i].tu >> m_model[i].tv;
        fin >> m_model[i].nx >> m_model[i].ny >> m_model[i].nz;

        // Invert the V coordinate to match the OpenGL texture coordinate system.
        m_model[i].tv = 1.0f - m_model[i].tv;
    }

    // Close the model file.
    fin.close();

    return true;
}


void ModelClass::ReleaseModel()
{
    if(m_model)
    {
        delete [] m_model;
        m_model = 0;
    }

    return;
}
