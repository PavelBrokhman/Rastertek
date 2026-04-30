///////////////////////////////////////////////////////////////////////////////
// Filename: terrainclass.cpp
///////////////////////////////////////////////////////////////////////////////
#include "terrainclass.h"


TerrainClass::TerrainClass()
{
    m_heightMap = 0;
    m_terrainModel = 0;
    m_Texture = 0;
}


TerrainClass::TerrainClass(const TerrainClass& other)
{
}


TerrainClass::~TerrainClass()
{
}


bool TerrainClass::Initialize(OpenGLClass* OpenGL, char* setupFilename)
{
    char terrainFilename[256], textureFilename[256];
    float heightScale;
    bool result;


    // Get the terrain filename, dimensions, and so forth from the setup file.
    result = LoadSetupFile(setupFilename, terrainFilename, heightScale, textureFilename);
    if(!result)
    {
        return false;
    }

    // Initialize the terrain height map with the data from the bitmap file.
    result = LoadBitmapHeightMap(terrainFilename);
    if(!result)
    {
        return false;
    }

    // Setup the X and Z coordinates for the height map as well as scale the terrain height by the height scale value.
    SetTerrainCoordinates(heightScale);

    // Calculate the normals for the terrain data.
    CalculateNormals();

    // Now build the 3D model of the terrain.
    BuildTerrainModel();

    // We can now release the height map since it is no longer needed in memory once the 3D terrain model has been built.
    ReleaseHeightMap();

    // Initialize the vertex and index buffer that hold the geometry for the terrain.
    result = InitializeBuffers(OpenGL);
    if(!result)
    {
        return false;
    }

    // Release the terrain model now that the rendering buffers have been loaded.
    ReleaseTerrainModel();

    // Create and initialize the diffuse texture object.
    m_Texture = new TextureClass;

    result = m_Texture->Initialize(OpenGL, textureFilename, false);
    if(!result)
    {
        return false;
    }

    return true;
}


void TerrainClass::Shutdown(OpenGLClass* OpenGL)
{
    // Release the diffuse texture object.
    if(m_Texture)
    {
        m_Texture->Shutdown();
        delete m_Texture;
        m_Texture = 0;
    }

    // Release the vertex and index buffers.
    ShutdownBuffers(OpenGL);

    return;
}


bool TerrainClass::Render(OpenGLClass* OpenGL, ShaderManagerClass* ShaderManager, LightClass* Light, float* worldMatrix, float* viewMatrix, float* projectionMatrix)
{
    float diffuseLightColor[4], lightDirection[3];
    bool result, wireFrame;


    // Get the light properties.
    Light->GetDirection(lightDirection);
    Light->GetDiffuseColor(diffuseLightColor);

    // Set wireframe mode off.
    wireFrame = false;

    // Enable wireframe mode to see triangles composing the terrain clearly.
    if(wireFrame)
    {
        OpenGL->EnableWireframe();
    }

    // Set the terrain shader as the current shader program and set the matrices that it will use for rendering.
    result = ShaderManager->RenderTerrainShader(worldMatrix, viewMatrix, projectionMatrix, lightDirection, diffuseLightColor);
    if(!result)
    {
        return false;
    }

    // Set the diffuse texture for the terrain in the pixel shader texture unit 0.
    m_Texture->SetTexture(OpenGL, 0);

    // Put the vertex and index buffers on the graphics pipeline to prepare them for drawing.
    RenderBuffers(OpenGL);

    // Disable wireframe mode after rendering terrain.
    if(wireFrame)
    {
        OpenGL->DisableWireframe();
    }

    return true;
}


bool TerrainClass::LoadSetupFile(char* filename, char* terrainFilename, float& heightScale, char* textureFilename)
{
    ifstream fin;
    char input;


    // Open the setup file.  If it could not open the file then exit.
    fin.open(filename);
    if(fin.fail())
    {
        return false;
    }

    // Read up to the terrain file name.
    fin.get(input);
    while(input != ':')
    {
        fin.get(input);
    }

    // Read in the terrain file name.
    fin >> terrainFilename;

    // Read up to the value of terrain height.
    fin.get(input);
    while(input != ':')
    {
        fin.get(input);
    }

    // Read in the terrain height.
    fin >> m_terrainHeight;

    // Read up to the value of terrain width.
    fin.get(input);
    while(input != ':')
    {
        fin.get(input);
    }

    // Read in the terrain width.
    fin >> m_terrainWidth;

    // Read up to the value of terrain height scaling.
    fin.get(input);
    while(input != ':')
    {
        fin.get(input);
    }

    // Read in the terrain height scaling.
    fin >> heightScale;

    // Read up to the texture file name.
    fin.get(input);
    while(input != ':')
    {
        fin.get(input);
    }

    // Read in the texture file name.
    fin >> textureFilename;

    // Close the setup file.
    fin.close();

    return true;
}


bool TerrainClass::LoadBitmapHeightMap(char* terrainFilename)
{
    FILE* filePtr;
    unsigned char* bitmapImage;
    unsigned char fileHeader[54];
    unsigned long count;
    int height, width, imageSize, i, j, k, index, error;
    unsigned char pixelHeight;


    // Start by creating the array structure to hold the height map data.
    m_heightMap = new HeightMapType[m_terrainWidth * m_terrainHeight];

    // Open the bitmap map file in binary.
    filePtr = fopen(terrainFilename, "rb");
    if(filePtr == NULL)
    {
        return false;
    }

    // Read in the bitmap file header which is 54 bytes.
    count = fread(fileHeader, sizeof(unsigned char), 54, filePtr);
    if(count != 54)
    {
        return false;
    }

    // Get the width and height integers from the unsigned char header data.
    height = (int)fileHeader[23];
    height <<= 8;
    height += (int)fileHeader[22];

    width = (int)fileHeader[19];
    width <<= 8;
    width += (int)fileHeader[18];

    // Make sure the height map dimensions are the same as the terrain dimensions for easy 1 to 1 mapping.
    if((height != m_terrainHeight) || (width != m_terrainWidth))
    {
        return false;
    }

    // Calculate the size of the bitmap image data.
    // Since we use non-divide by 2 dimensions (eg. 257x257) we need to add an extra byte to each line.
    imageSize = m_terrainHeight * ((m_terrainWidth * 3) + 1);

    // Allocate memory for the bitmap image data.
    bitmapImage = new unsigned char[imageSize];

    // Read in the bitmap image data.
    count = fread(bitmapImage, 1, imageSize, filePtr);
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

    // Initialize the position in the image data buffer.
    k=0;

    // Read the image data into the height map array.
    for(j=0; j<m_terrainHeight; j++)
    {
        for(i=0; i<m_terrainWidth; i++)
        {
            // Bitmaps are upside down so load bottom to top into the height map array.
            index = (m_terrainWidth * (m_terrainHeight - 1 - j)) + i;

            // Get the grey scale pixel value from the bitmap image data at this location.
            pixelHeight = bitmapImage[k];

            // Store the pixel value as the height at this point in the height map array.
            m_heightMap[index].y = (float)pixelHeight;

            // Increment the bitmap image data index.
            k+=3;
        }

        // Compensate for the extra byte at end of each line in non-divide by 2 bitmaps (eg. 257x257).
        k++;
    }

    // Release the bitmap image data now that the height map array has been loaded.
    delete [] bitmapImage;
    bitmapImage = 0;

    return true;
}


void TerrainClass::SetTerrainCoordinates(float heightScale)
{
    int i, j, index;


    // Loop through all the elements in the height map array and adjust their coordinates correctly.
    for(j=0; j<m_terrainHeight; j++)
    {
        for(i=0; i<m_terrainWidth; i++)
        {
            index = (m_terrainWidth * j) + i;

            // Set the X and Z coordinates.
            m_heightMap[index].x = (float)i;
            m_heightMap[index].z = -(float)j;

            // Move the terrain depth into the positive range.  For example from (0, -256) to (256, 0).
            m_heightMap[index].z += (float)(m_terrainHeight - 1);

            // Scale the height.
            m_heightMap[index].y /= heightScale;
        }
    }

    return;
}


void TerrainClass::CalculateNormals()
{
    int i, j, index1, index2, index3, index;
    float vertex1[3], vertex2[3], vertex3[3], vector1[3], vector2[3], sum[3], length;
    VectorType* normals;


    // Create a temporary array to hold the face normal vectors.
    normals = new VectorType[(m_terrainHeight-1) * (m_terrainWidth-1)];

    // Go through all the faces in the mesh and calculate their normals.
    for(j=0; j<(m_terrainHeight-1); j++)
    {
        for(i=0; i<(m_terrainWidth-1); i++)
        {
            index1 = ((j+1) * m_terrainWidth) + i;      // Bottom left vertex.
            index2 = ((j+1) * m_terrainWidth) + (i+1);  // Bottom right vertex.
            index3 = (j * m_terrainWidth) + i;          // Upper left vertex.

            // Get three vertices from the face.
            vertex1[0] = m_heightMap[index1].x;
            vertex1[1] = m_heightMap[index1].y;
            vertex1[2] = m_heightMap[index1].z;

            vertex2[0] = m_heightMap[index2].x;
            vertex2[1] = m_heightMap[index2].y;
            vertex2[2] = m_heightMap[index2].z;

            vertex3[0] = m_heightMap[index3].x;
            vertex3[1] = m_heightMap[index3].y;
            vertex3[2] = m_heightMap[index3].z;

            // Calculate the two vectors for this face.
            vector1[0] = vertex1[0] - vertex3[0];
            vector1[1] = vertex1[1] - vertex3[1];
            vector1[2] = vertex1[2] - vertex3[2];
            vector2[0] = vertex3[0] - vertex2[0];
            vector2[1] = vertex3[1] - vertex2[1];
            vector2[2] = vertex3[2] - vertex2[2];

            index = (j * (m_terrainWidth - 1)) + i;

            // Calculate the cross product of those two vectors to get the un-normalized value for this face normal.
            normals[index].x = (vector1[1] * vector2[2]) - (vector1[2] * vector2[1]);
            normals[index].y = (vector1[2] * vector2[0]) - (vector1[0] * vector2[2]);
            normals[index].z = (vector1[0] * vector2[1]) - (vector1[1] * vector2[0]);

            // Calculate the length.
            length = (float)sqrt((normals[index].x * normals[index].x) + (normals[index].y * normals[index].y) +
                     (normals[index].z * normals[index].z));

            // Normalize the final value for this face using the length.
            normals[index].x = (normals[index].x / length);
            normals[index].y = (normals[index].y / length);
            normals[index].z = (normals[index].z / length);
        }
    }

    // Now go through all the vertices and take a sum of the face normals that touch this vertex.
    for(j=0; j<m_terrainHeight; j++)
    {
        for(i=0; i<m_terrainWidth; i++)
        {
            // Initialize the sum.
            sum[0] = 0.0f;
            sum[1] = 0.0f;
            sum[2] = 0.0f;

            // Bottom left face.
            if(((i-1) >= 0) && ((j-1) >= 0))
            {
                index = ((j-1) * (m_terrainWidth-1)) + (i-1);

                sum[0] += normals[index].x;
                sum[1] += normals[index].y;
                sum[2] += normals[index].z;
            }

            // Bottom right face.
            if((i<(m_terrainWidth-1)) && ((j-1) >= 0))
            {
                index = ((j - 1) * (m_terrainWidth - 1)) + i;

                sum[0] += normals[index].x;
                sum[1] += normals[index].y;
                sum[2] += normals[index].z;
            }

            // Upper left face.
            if(((i-1) >= 0) && (j<(m_terrainHeight-1)))
            {
                index = (j * (m_terrainWidth-1)) + (i-1);

                sum[0] += normals[index].x;
                sum[1] += normals[index].y;
                sum[2] += normals[index].z;
            }

            // Upper right face.
            if((i < (m_terrainWidth-1)) && (j < (m_terrainHeight-1)))
            {
                index = (j * (m_terrainWidth-1)) + i;

                sum[0] += normals[index].x;
                sum[1] += normals[index].y;
                sum[2] += normals[index].z;
            }

            // Calculate the length of this normal.
            length = (float)sqrt((sum[0] * sum[0]) + (sum[1] * sum[1]) + (sum[2] * sum[2]));

            // Get an index to the vertex location in the height map array.
            index = (j * m_terrainWidth) + i;

            // Normalize the final shared normal for this vertex and store it in the height map array.
            m_heightMap[index].nx = (sum[0] / length);
            m_heightMap[index].ny = (sum[1] / length);
            m_heightMap[index].nz = (sum[2] / length);
        }
    }

    // Release the temporary normals.
    delete [] normals;
    normals = 0;

    return;
}


void TerrainClass::BuildTerrainModel()
{
    int vertexCount, i, j, index, index1, index2, index3, index4;


    // Calculate the number of vertices in the 3D terrain model.
    vertexCount = (m_terrainHeight - 1) * (m_terrainWidth - 1) * 6;

    // Create the 3D terrain model array.
    m_terrainModel = new ModelType[vertexCount];

    // Initialize the index into the height map array.
    index = 0;

    // Load the 3D terrain model with the height map terrain data.
    // We will be creating 2 triangles for each of the four points in a quad.
    for(j=0; j<(m_terrainHeight-1); j++)
    {
        for(i=0; i<(m_terrainWidth-1); i++)
        {
            // Get the indexes to the four points of the quad.
            index1 = (m_terrainWidth * j) + i;          // Upper left.
            index2 = (m_terrainWidth * j) + (i+1);      // Upper right.
            index3 = (m_terrainWidth * (j+1)) + i;      // Bottom left.
            index4 = (m_terrainWidth * (j+1)) + (i+1);  // Bottom right.

            // Now create two triangles for that quad.
            // Triangle 1 - Upper left.
            m_terrainModel[index].x = m_heightMap[index1].x;
            m_terrainModel[index].y = m_heightMap[index1].y;
            m_terrainModel[index].z = m_heightMap[index1].z;
            m_terrainModel[index].tu = 0.0f;
            m_terrainModel[index].tv = 1.0f;
            m_terrainModel[index].nx = m_heightMap[index1].nx;
            m_terrainModel[index].ny = m_heightMap[index1].ny;
            m_terrainModel[index].nz = m_heightMap[index1].nz;
            index++;

            // Triangle 1 - Upper right.
            m_terrainModel[index].x = m_heightMap[index2].x;
            m_terrainModel[index].y = m_heightMap[index2].y;
            m_terrainModel[index].z = m_heightMap[index2].z;
            m_terrainModel[index].tu = 1.0f;
            m_terrainModel[index].tv = 1.0f;
            m_terrainModel[index].nx = m_heightMap[index2].nx;
            m_terrainModel[index].ny = m_heightMap[index2].ny;
            m_terrainModel[index].nz = m_heightMap[index2].nz;
            index++;

            // Triangle 1 - Bottom left.
            m_terrainModel[index].x = m_heightMap[index3].x;
            m_terrainModel[index].y = m_heightMap[index3].y;
            m_terrainModel[index].z = m_heightMap[index3].z;
            m_terrainModel[index].tu = 0.0f;
            m_terrainModel[index].tv = 0.0f;
            m_terrainModel[index].nx = m_heightMap[index3].nx;
            m_terrainModel[index].ny = m_heightMap[index3].ny;
            m_terrainModel[index].nz = m_heightMap[index3].nz;
            index++;

            // Triangle 2 - Bottom left.
            m_terrainModel[index].x = m_heightMap[index3].x;
            m_terrainModel[index].y = m_heightMap[index3].y;
            m_terrainModel[index].z = m_heightMap[index3].z;
            m_terrainModel[index].tu = 0.0f;
            m_terrainModel[index].tv = 0.0f;
            m_terrainModel[index].nx = m_heightMap[index3].nx;
            m_terrainModel[index].ny = m_heightMap[index3].ny;
            m_terrainModel[index].nz = m_heightMap[index3].nz;
            index++;

            // Triangle 2 - Upper right.
            m_terrainModel[index].x = m_heightMap[index2].x;
            m_terrainModel[index].y = m_heightMap[index2].y;
            m_terrainModel[index].z = m_heightMap[index2].z;
            m_terrainModel[index].tu = 1.0f;
            m_terrainModel[index].tv = 1.0f;
            m_terrainModel[index].nx = m_heightMap[index2].nx;
            m_terrainModel[index].ny = m_heightMap[index2].ny;
            m_terrainModel[index].nz = m_heightMap[index2].nz;
            index++;

            // Triangle 2 - Bottom right.
            m_terrainModel[index].x = m_heightMap[index4].x;
            m_terrainModel[index].y = m_heightMap[index4].y;
            m_terrainModel[index].z = m_heightMap[index4].z;
            m_terrainModel[index].tu = 1.0f;
            m_terrainModel[index].tv = 0.0f;
            m_terrainModel[index].nx = m_heightMap[index4].nx;
            m_terrainModel[index].ny = m_heightMap[index4].ny;
            m_terrainModel[index].nz = m_heightMap[index4].nz;
            index++;
        }
    }

    return;
}


void TerrainClass::ReleaseHeightMap()
{
    // Release the height map array.
    if(m_heightMap)
    {
        delete [] m_heightMap;
        m_heightMap = 0;
    }

    return;
}


void TerrainClass::ReleaseTerrainModel()
{
    // Release the terrain model data.
    if(m_terrainModel)
    {
        delete [] m_terrainModel;
        m_terrainModel = 0;
    }

    return;
}


bool TerrainClass::InitializeBuffers(OpenGLClass* OpenGL)
{
    VertexType* vertices;
    unsigned int* indices;
    int i;


    // Calculate the number of vertices in the terrain.
    m_vertexCount = (m_terrainHeight - 1) * (m_terrainWidth - 1) * 6;

    // Set the index count to the same as the vertex count.
    m_indexCount = m_vertexCount;

    // Create the vertex array.
    vertices = new VertexType[m_vertexCount];

    // Create the index array.
    indices = new unsigned int[m_indexCount];

    // Load the vertex array and index array with 3D terrain model data.
    for(i=0; i<m_vertexCount; i++)
    {
        vertices[i].x = m_terrainModel[i].x;
        vertices[i].y = m_terrainModel[i].y;
        vertices[i].z = m_terrainModel[i].z;
        vertices[i].tu = m_terrainModel[i].tu;
        vertices[i].tv = m_terrainModel[i].tv;
        vertices[i].nx = m_terrainModel[i].nx;
        vertices[i].ny = m_terrainModel[i].ny;
        vertices[i].nz = m_terrainModel[i].nz;
        indices[i] = i;
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
    OpenGL->glEnableVertexAttribArray(1);  // Texture coordinates.
    OpenGL->glEnableVertexAttribArray(2);  // Normals.

    // Specify the location and format of the position portion of the vertex buffer.
    OpenGL->glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeof(VertexType), 0);

    // Specify the location and format of the texture portion of the vertex buffer.
    OpenGL->glVertexAttribPointer(1, 2, GL_FLOAT, false, sizeof(VertexType), (unsigned char*)NULL + (3 * sizeof(float)));

    // Specify the location and format of the normal vector portion of the vertex buffer.
    OpenGL->glVertexAttribPointer(2, 3, GL_FLOAT, false, sizeof(VertexType), (unsigned char*)NULL + (5 * sizeof(float)));

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

    // Render the vertex buffer as triangles using the index buffer.
    glDrawElements(GL_TRIANGLES, m_indexCount, GL_UNSIGNED_INT, 0);

    return;
}
