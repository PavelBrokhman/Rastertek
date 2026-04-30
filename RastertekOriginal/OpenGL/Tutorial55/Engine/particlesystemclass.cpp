////////////////////////////////////////////////////////////////////////////////
// Filename: particlesystemclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "particlesystemclass.h"


ParticleSystemClass::ParticleSystemClass()
{
    m_OpenGLPtr = 0;
    m_particleList = 0;
    m_vertices = 0;
    m_Texture = 0;
}


ParticleSystemClass::ParticleSystemClass(const ParticleSystemClass& other)
{
}


ParticleSystemClass::~ParticleSystemClass()
{
    // Release the pointer to the OpenGL object.
    m_OpenGLPtr = 0;
}


bool ParticleSystemClass::Initialize(OpenGLClass* OpenGL, char* configFilename)
{
    bool result;


    // Store a pointer to the OpenGL object.
    m_OpenGLPtr = OpenGL;

    // Keep a copy of the config file name for loading the particle configuration, and also for mid-app reloading.
    strcpy(m_configFilename, configFilename);

    // Load the particle configuration file to set all the particle parameters for rendering.
    result = LoadParticleConfiguration();
    if(!result)
    {
        return false;
    }

    // Initialize the particle system.
    InitializeParticleSystem();

    // Create the buffers that will be used to render the particles with.
    InitializeBuffers();

    // Load the texture that is used for the particles.
    result = LoadTexture();
    if(!result)
    {
        return false;
    }

    return true;
}


void ParticleSystemClass::Shutdown()
{
    // Release the texture used for the particles.
    ReleaseTexture();
    
    // Release the buffers.
    ShutdownBuffers();

    // Release the particle system.
    ShutdownParticleSystem();

    return;
}


void ParticleSystemClass::Frame(float frameTime)
{
    // Release old particles.
    KillParticles();

    // Emit new particles.
    EmitParticles(frameTime);

    // Update the position of the particles.
    UpdateParticles(frameTime);

    // Update the dynamic vertex buffer with the new position of each particle.
    UpdateBuffers();

    return;
}


void ParticleSystemClass::Render()
{
    // Set the texture for the particles.
    m_Texture->SetTexture(m_OpenGLPtr, 0);

    // Put the vertex and index buffers on the graphics pipeline to prepare them for drawing.
    RenderBuffers();

    return;
}


bool ParticleSystemClass::LoadParticleConfiguration()
{
    ifstream fin;
    int i;
    char input;


    // Open the particle configuration file.
    fin.open(m_configFilename);
    if(fin.fail())
    {
        return false;
    }

    // Read up to the value of the particle count and read it in.
    fin.get(input);
    while(input != ':')
    { 
        fin.get(input); 
    }
    fin >> m_maxParticles;

    // Read up to the value of the particle per second and read it in.
    fin.get(input);
    while(input != ':')
    { 
        fin.get(input); 
    }
    fin >> m_particlesPerSecond;

    // Read up to the value of the particle size and read it in.
    fin.get(input);
    while(input != ':')
    { 
        fin.get(input); 
    }
    fin >> m_particleSize;

    // Read up to the value of the particle life time and read it in.
    fin.get(input);
    while(input != ':')
    { 
        fin.get(input); 
    }
    fin >> m_particleLifeTime;

    // Read up to the filename of the first texture and read it in.
    fin.get(input);
    while(input != ':')
    { 
        fin.get(input); 
    }
    fin.get(input); 

    i=0;
    fin.get(input);
    while(input != '\n')
    {
        m_textureFilename[i] = input;
        i++;
        fin.get(input);
    }
    m_textureFilename[i-1] = '\0';
    
    // Close the file.
    fin.close();

    return true;
}


void ParticleSystemClass::InitializeParticleSystem()
{
    int i;


    // Create the particle list.
    m_particleList = new ParticleType[m_maxParticles];

    // Initialize the particle list.
    for(i=0; i<m_maxParticles; i++)
    {
        m_particleList[i].active = false;
    }

    // Clear the initial accumulated time for the particle per second emission rate.
    m_accumulatedTime = 0.0f;

    // Initialize the current particle count to zero since none are emitted yet.
    m_currentParticleCount = 0;

    return;
}


void ParticleSystemClass::ShutdownParticleSystem()
{
    // Release the particle list.
    if(m_particleList)
    {
        delete [] m_particleList;
	m_particleList = 0;
    }

    return;
}


void ParticleSystemClass::EmitParticles(float frameTime)
{
    float centerX, centerY, radius, positionX, positionY, positionZ, scroll1X, scroll1Y;
    float emitterOrigin[3];
    int index, i, j;
    bool emitParticle, found;
    static float angle = 0.0f;


    // Set the center of the circle.
    centerX = 0.0f;
    centerY = 0.0f;

    // Set the radius of the circle.
    radius = 1.0f;

    // Update the angle each frame to move any generated particle origin position along the circumference of the circle each frame.
    angle += frameTime * 2.0f;

    // Calculate the origin that the particle should be emitted on the circle's circumference.
    emitterOrigin[0] = centerX + radius * sin(angle);
    emitterOrigin[1] = centerY + radius * cos(angle);
    emitterOrigin[2] = 0.0f;

    // Increment the accumulated time that is used to determine when to emit a particle next.
    m_accumulatedTime += frameTime;

    // Set emit particle to false for now.
    emitParticle = false;
	
    // Check if it is time to emit a new particle or not.
    if(m_accumulatedTime > (1.0f / m_particlesPerSecond))
    {
        m_accumulatedTime = 0.0f;
        emitParticle = true;
    }

    // If there are particles to emit then emit one per frame.
    if((emitParticle == true) && (m_currentParticleCount < (m_maxParticles - 1)))
    {
        m_currentParticleCount++;

        positionX = emitterOrigin[0];
        positionY = emitterOrigin[1];
        positionZ = emitterOrigin[2];

        // Create a random X scrolling positive value.
        scroll1X = (((float)rand() - (float)rand())/RAND_MAX);
        if(scroll1X < 0.0f)
        {
            scroll1X *= -1.0f;
        }

        // Set the Y scroll to the same value.
        scroll1Y = scroll1X;

        // Now since the particles need to be rendered from back to front for blending we have to sort the particle array.
        // We will sort using Z depth so we need to find where in the list the particle should be inserted.
        index = 0;
        found = false;
        while(!found)
        {
            if((m_particleList[index].active == false) || (m_particleList[index].positionZ < positionZ))
            {
                found = true;
            }
            else
            {
                index++;
            }
        }

        // Now that we know the location to insert into we need to copy the array over by one position from the index to make room for the new particle.
        i = m_currentParticleCount;
        j = i - 1;

        while(i != index)
        {
            CopyParticle(i, j);
            i--;
            j--;
        }

        // Now insert the newly emitted particle into the particle array in the correct depth order.
        m_particleList[index].positionX = positionX;
        m_particleList[index].positionY = positionY;
        m_particleList[index].positionZ = positionZ;
        m_particleList[index].active    = true;
        m_particleList[index].lifeTime  = m_particleLifeTime;
        m_particleList[index].scroll1X  = scroll1X;
        m_particleList[index].scroll1Y  = scroll1Y;
    }

    return;
}


void ParticleSystemClass::UpdateParticles(float frameTime)
{
    int i;


    // Each frame we update all the particles using the frame time.
    for(i=0; i<m_currentParticleCount; i++)
    {
        // Negate the life time of the particle each frame.
        m_particleList[i].lifeTime = m_particleList[i].lifeTime - frameTime;

        // Update the scrolling position of each particle each frame.
        m_particleList[i].scroll1X = m_particleList[i].scroll1X + (frameTime * 0.5f);
        if(m_particleList[i].scroll1X > 1.0f)
        {
            m_particleList[i].scroll1X -= 1.0f;
        }

        m_particleList[i].scroll1Y = m_particleList[i].scroll1Y + (frameTime * 0.5f);
        if(m_particleList[i].scroll1Y > 1.0f)
        {
            m_particleList[i].scroll1Y -= 1.0f;
        }
    }

    return;
}


void ParticleSystemClass::KillParticles()
{
    int i, j;


    // Kill all the particles that have a life time that is now zero.
    for(i=0; i<m_maxParticles; i++)
    {
        if((m_particleList[i].active == true) && (m_particleList[i].lifeTime <= 0.0f))
        {
            m_particleList[i].active = false;
            m_currentParticleCount--;

            // Now shift all the live particles back up the array to erase the destroyed particle and keep the array sorted correctly.
            for(j=i; j<m_maxParticles-1; j++)
            {
                CopyParticle(j, j+1);
            }
        }
    }

    return;
}


void ParticleSystemClass::CopyParticle(int dst, int src)
{
    m_particleList[dst].positionX = m_particleList[src].positionX;
    m_particleList[dst].positionY = m_particleList[src].positionY;
    m_particleList[dst].positionZ = m_particleList[src].positionZ;
    m_particleList[dst].active    = m_particleList[src].active;
    m_particleList[dst].lifeTime  = m_particleList[src].lifeTime;
    m_particleList[dst].scroll1X  = m_particleList[src].scroll1X;
    m_particleList[dst].scroll1Y  = m_particleList[src].scroll1Y;
    return;
}


void ParticleSystemClass::InitializeBuffers()
{
    unsigned int* indices;
    int i;


    // Set the maximum number of vertices in the vertex array.
    m_vertexCount = m_maxParticles * 6;
    
    // Set the maximum number of indices in the index array.
    m_indexCount = m_vertexCount;

    // Create the vertex array.
    m_vertices = new VertexType[m_vertexCount];

    // Create the index array.
    indices = new unsigned int[m_indexCount];

    // Initialize vertex array to zeros at first.
    memset(m_vertices, 0, (sizeof(VertexType) * m_vertexCount));

    // Initialize the index array.
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
    m_OpenGLPtr->glBufferData(GL_ARRAY_BUFFER, m_vertexCount * sizeof(VertexType), m_vertices, GL_DYNAMIC_DRAW);

    // Enable the three vertex array attributes.
    m_OpenGLPtr->glEnableVertexAttribArray(0);  // Vertex position.
    m_OpenGLPtr->glEnableVertexAttribArray(1);  // Texture coordinates.
    m_OpenGLPtr->glEnableVertexAttribArray(2);  // Data1

    // Specify the location and format of the position portion of the vertex buffer.
    m_OpenGLPtr->glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeof(VertexType), 0);

    // Specify the location and format of the texture coordinates portion of the vertex buffer.
    m_OpenGLPtr->glVertexAttribPointer(1, 2, GL_FLOAT, false, sizeof(VertexType), (unsigned char*)NULL + (3 * sizeof(float)));

    // Specify the location and format of the data1 portion of the vertex buffer.
    m_OpenGLPtr->glVertexAttribPointer(2, 3, GL_FLOAT, false, sizeof(VertexType), (unsigned char*)NULL + (5 * sizeof(float)));

    // Generate an ID for the index buffer.
    m_OpenGLPtr->glGenBuffers(1, &m_indexBufferId);

    // Bind the index buffer and load the index data into it.
    m_OpenGLPtr->glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, m_indexBufferId);
    m_OpenGLPtr->glBufferData(GL_ELEMENT_ARRAY_BUFFER, m_indexCount* sizeof(unsigned int), indices, GL_STATIC_DRAW);

    // Now that the buffers have been loaded we can release the array data.
    delete [] indices;
    indices = 0;

    return;
}


void ParticleSystemClass::ShutdownBuffers()
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

    // Release the vertices.
    if(m_vertices)
    {
        delete [] m_vertices;
        m_vertices = 0;
    }

    return;
}


void ParticleSystemClass::RenderBuffers()
{
    // Bind the vertex array object that stored all the information about the vertex and index buffers.
    m_OpenGLPtr->glBindVertexArray(m_vertexArrayId);

    // Render the vertex buffer using the index buffer.
    glDrawElements(GL_TRIANGLES, m_indexCount, GL_UNSIGNED_INT, 0);

    return;
}


void ParticleSystemClass::UpdateBuffers()
{
    int index, i;
    float lifeTime, scroll1X, scroll1Y;
    void* dataPtr;


    // Initialize vertex array to zeros at first.
    memset(m_vertices, 0, (sizeof(VertexType) * m_vertexCount));
    
    // Now build the vertex array from the particle list array.  Each particle is a quad made out of two triangles.
    index = 0;

    for(i=0; i<m_currentParticleCount; i++)
    {
        // Get the life time and scroll for the current particle.  This will be set in the data1 portion of the vertex.
        lifeTime = m_particleList[i].lifeTime / m_particleLifeTime;
        scroll1X = m_particleList[i].scroll1X;
        scroll1Y = m_particleList[i].scroll1Y;
	
        // Bottom left.
	m_vertices[index].x = m_particleList[i].positionX - m_particleSize;
	m_vertices[index].y = m_particleList[i].positionY - m_particleSize;
	m_vertices[index].z = m_particleList[i].positionZ;
	m_vertices[index].tu = 0.0f;
	m_vertices[index].tv = 0.0f;
	m_vertices[index].lifeTime = lifeTime;
	m_vertices[index].scroll1X = scroll1X;
	m_vertices[index].scroll1Y = scroll1Y;
	index++;
      
	// Top left.
	m_vertices[index].x = m_particleList[i].positionX - m_particleSize;
	m_vertices[index].y = m_particleList[i].positionY + m_particleSize;
	m_vertices[index].z = m_particleList[i].positionZ;
	m_vertices[index].tu = 0.0f;
	m_vertices[index].tv = 1.0f;
	m_vertices[index].lifeTime = lifeTime;
	m_vertices[index].scroll1X = scroll1X;
	m_vertices[index].scroll1Y = scroll1Y;
	index++;
      
	// Bottom right.
	m_vertices[index].x = m_particleList[i].positionX + m_particleSize;
	m_vertices[index].y = m_particleList[i].positionY - m_particleSize;
	m_vertices[index].z = m_particleList[i].positionZ;
	m_vertices[index].tu = 1.0f;
	m_vertices[index].tv = 0.0f;
	m_vertices[index].lifeTime = lifeTime;
	m_vertices[index].scroll1X = scroll1X;
	m_vertices[index].scroll1Y = scroll1Y;
	index++;
      
	// Bottom right.
	m_vertices[index].x = m_particleList[i].positionX + m_particleSize;
	m_vertices[index].y = m_particleList[i].positionY - m_particleSize;
	m_vertices[index].z = m_particleList[i].positionZ;
	m_vertices[index].tu = 1.0f;
	m_vertices[index].tv = 0.0f;
	m_vertices[index].lifeTime = lifeTime;
	m_vertices[index].scroll1X = scroll1X;
	m_vertices[index].scroll1Y = scroll1Y;
	index++;
      
	// Top left.
	m_vertices[index].x = m_particleList[i].positionX - m_particleSize;
	m_vertices[index].y = m_particleList[i].positionY + m_particleSize;
	m_vertices[index].z = m_particleList[i].positionZ;
	m_vertices[index].tu = 0.0f;
	m_vertices[index].tv = 1.0f;
	m_vertices[index].lifeTime = lifeTime;
	m_vertices[index].scroll1X = scroll1X;
	m_vertices[index].scroll1Y = scroll1Y;
	index++;
      
	// Top right.
	m_vertices[index].x = m_particleList[i].positionX + m_particleSize;
	m_vertices[index].y = m_particleList[i].positionY + m_particleSize;
	m_vertices[index].z = m_particleList[i].positionZ;
	m_vertices[index].tu = 1.0f;
	m_vertices[index].tv = 1.0f;
	m_vertices[index].lifeTime = lifeTime;
	m_vertices[index].scroll1X = scroll1X;
	m_vertices[index].scroll1Y = scroll1Y;
	index++;
    }

    // Bind the vertex buffer.
    m_OpenGLPtr->glBindBuffer(GL_ARRAY_BUFFER, m_vertexBufferId);

    // Get a pointer to the buffer's actual location in memory.
    dataPtr = m_OpenGLPtr->glMapBuffer(GL_ARRAY_BUFFER, GL_WRITE_ONLY);

    // Copy the vertex data into memory.
    memcpy(dataPtr, m_vertices, (sizeof(VertexType) * m_vertexCount));

    // Unlock the vertex buffer.
    m_OpenGLPtr->glUnmapBuffer(GL_ARRAY_BUFFER);

    return;
}


bool ParticleSystemClass::LoadTexture()
{
    bool result;


    // Create and initialize the texture object.
    m_Texture = new TextureClass;

    result = m_Texture->Initialize(m_OpenGLPtr, m_textureFilename, true);
    if(!result)
    {
        return false;
    }

    return true;
}


void ParticleSystemClass::ReleaseTexture()
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


bool ParticleSystemClass::Reload()
{
    bool result;


    // Release all of the data.
    Shutdown();

    // Reload all of the data.
    result = LoadParticleConfiguration();
    if(!result)
    {
        return false;
    }

    InitializeParticleSystem();

    InitializeBuffers();

    result = LoadTexture();
    if(!result)
    {
        return false;
    }

    return true;
}
