////////////////////////////////////////////////////////////////////////////////
// Filename: softshadowshaderclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "softshadowshaderclass.h"


SoftShadowShaderClass::SoftShadowShaderClass()
{
    m_OpenGLPtr = 0;
}


SoftShadowShaderClass::SoftShadowShaderClass(const SoftShadowShaderClass& other)
{
}


SoftShadowShaderClass::~SoftShadowShaderClass()
{
}


bool SoftShadowShaderClass::Initialize(OpenGLClass* OpenGL)
{
    char vsFilename[128];
    char psFilename[128];
    bool result;


    // Store the pointer to the OpenGL object.
    m_OpenGLPtr = OpenGL;

    // Set the location and names of the shader files.
    strcpy(vsFilename, "../Engine/softshadow.vs");
    strcpy(psFilename, "../Engine/softshadow.ps");

    // Initialize the vertex and pixel shaders.
    result = InitializeShader(vsFilename, psFilename);
    if(!result)
    {
        return false;
    }

    return true;
}


void SoftShadowShaderClass::Shutdown()
{
    // Shutdown the shader.
    ShutdownShader();

    // Release the pointer to the OpenGL object.
    m_OpenGLPtr = 0;

    return;
}


bool SoftShadowShaderClass::InitializeShader(char* vsFilename, char* fsFilename)
{
    const char* vertexShaderBuffer;
    const char* fragmentShaderBuffer;
    int status;


    // Load the vertex shader source file into a text buffer.
    vertexShaderBuffer = LoadShaderSourceFile(vsFilename);
    if(!vertexShaderBuffer)
    {
        return false;
    }

    // Load the fragment shader source file into a text buffer.
    fragmentShaderBuffer = LoadShaderSourceFile(fsFilename);
    if(!fragmentShaderBuffer)
    {
        return false;
    }

    // Create a vertex and fragment shader object.
    m_vertexShader = m_OpenGLPtr->glCreateShader(GL_VERTEX_SHADER);
    m_fragmentShader = m_OpenGLPtr->glCreateShader(GL_FRAGMENT_SHADER);

    // Copy the shader source code strings into the vertex and fragment shader objects.
    m_OpenGLPtr->glShaderSource(m_vertexShader, 1, &vertexShaderBuffer, NULL);
    m_OpenGLPtr->glShaderSource(m_fragmentShader, 1, &fragmentShaderBuffer, NULL);

    // Release the vertex and fragment shader buffers.
    delete [] vertexShaderBuffer;
    vertexShaderBuffer = 0;

    delete [] fragmentShaderBuffer;
    fragmentShaderBuffer = 0;

    // Compile the shaders.
    m_OpenGLPtr->glCompileShader(m_vertexShader);
    m_OpenGLPtr->glCompileShader(m_fragmentShader);

    // Check to see if the vertex shader compiled successfully.
    m_OpenGLPtr->glGetShaderiv(m_vertexShader, GL_COMPILE_STATUS, &status);
    if(status != 1)
    {
        // If it did not compile then write the syntax error message out to a text file for review.
        OutputShaderErrorMessage(m_vertexShader, vsFilename);
        return false;
    }

    // Check to see if the fragment shader compiled successfully.
    m_OpenGLPtr->glGetShaderiv(m_fragmentShader, GL_COMPILE_STATUS, &status);
    if(status != 1)
    {
        // If it did not compile then write the syntax error message out to a text file for review.
        OutputShaderErrorMessage(m_fragmentShader, fsFilename);
        return false;
    }

    // Create a shader program object.
    m_shaderProgram = m_OpenGLPtr->glCreateProgram();

    // Attach the vertex and fragment shader to the program object.
    m_OpenGLPtr->glAttachShader(m_shaderProgram, m_vertexShader);
    m_OpenGLPtr->glAttachShader(m_shaderProgram, m_fragmentShader);

    // Bind the shader input variables.
    m_OpenGLPtr->glBindAttribLocation(m_shaderProgram, 0, "inputPosition");
    m_OpenGLPtr->glBindAttribLocation(m_shaderProgram, 1, "inputTexCoord");
    m_OpenGLPtr->glBindAttribLocation(m_shaderProgram, 2, "inputNormal");

    // Link the shader program.
    m_OpenGLPtr->glLinkProgram(m_shaderProgram);

    // Check the status of the link.
    m_OpenGLPtr->glGetProgramiv(m_shaderProgram, GL_LINK_STATUS, &status);
    if(status != 1)
    {
        // If it did not link then write the syntax error message out to a text file for review.
        OutputLinkerErrorMessage(m_shaderProgram);
        return false;
    }

    return true;
}


void SoftShadowShaderClass::ShutdownShader()
{
    // Detach the vertex and fragment shaders from the program.
    m_OpenGLPtr->glDetachShader(m_shaderProgram, m_vertexShader);
    m_OpenGLPtr->glDetachShader(m_shaderProgram, m_fragmentShader);

    // Delete the vertex and fragment shaders.
    m_OpenGLPtr->glDeleteShader(m_vertexShader);
    m_OpenGLPtr->glDeleteShader(m_fragmentShader);

    // Delete the shader program.
    m_OpenGLPtr->glDeleteProgram(m_shaderProgram);

    return;
}


char* SoftShadowShaderClass::LoadShaderSourceFile(char* filename)
{
    FILE* filePtr;
    char* buffer;
    long fileSize, count;
    int error;


    // Open the shader file for reading in text modee.
    filePtr = fopen(filename, "r");
    if(filePtr == NULL)
    {
        return 0;
    }

    // Go to the end of the file and get the size of the file.
    fseek(filePtr, 0, SEEK_END);
    fileSize = ftell(filePtr);

    // Initialize the buffer to read the shader source file into, adding 1 for an extra null terminator.
    buffer = new char[fileSize + 1];

    // Return the file pointer back to the beginning of the file.
    fseek(filePtr, 0, SEEK_SET);

    // Read the shader text file into the buffer.
    count = fread(buffer, 1, fileSize, filePtr);
    if(count != fileSize)
    {
        return 0;
    }

    // Close the file.
    error = fclose(filePtr);
    if(error != 0)
    {
        return 0;
    }

    // Null terminate the buffer.
    buffer[fileSize] = '\0';

    return buffer;
}


void SoftShadowShaderClass::OutputShaderErrorMessage(unsigned int shaderId, char* shaderFilename)
{
    long count;
    int logSize, error;
    char* infoLog;
    FILE* filePtr;


    // Get the size of the string containing the information log for the failed shader compilation message.
    m_OpenGLPtr->glGetShaderiv(shaderId, GL_INFO_LOG_LENGTH, &logSize);

    // Increment the size by one to handle also the null terminator.
    logSize++;

    // Create a char buffer to hold the info log.
    infoLog = new char[logSize];

    // Now retrieve the info log.
    m_OpenGLPtr->glGetShaderInfoLog(shaderId, logSize, NULL, infoLog);

    // Open a text file to write the error message to.
    filePtr = fopen("shader-error.txt", "w");
    if(filePtr == NULL)
    {
        cout << "Error opening shader error message output file." << endl;
	return;
    }

    // Write out the error message.
    count = fwrite(infoLog, sizeof(char), logSize, filePtr);
    if(count != logSize)
    {
        cout << "Error writing shader error message output file." << endl;
	return;
    }

    // Close the file.
    error = fclose(filePtr);
    if(error != 0)
    {
        cout << "Error closing shader error message output file." << endl;
	return;
    }

    // Notify the user to check the text file for compile errors.
    cout << "Error compiling shader.  Check shader-error.txt for error message.  Shader filename: " << shaderFilename << endl;

    return;
}


void SoftShadowShaderClass::OutputLinkerErrorMessage(unsigned int programId)
{
    long count;
    FILE* filePtr;
    int logSize, error;
    char* infoLog;


    // Get the size of the string containing the information log for the failed shader compilation message.
    m_OpenGLPtr->glGetProgramiv(programId, GL_INFO_LOG_LENGTH, &logSize);

    // Increment the size by one to handle also the null terminator.
    logSize++;

    // Create a char buffer to hold the info log.
    infoLog = new char[logSize];

    // Now retrieve the info log.
    m_OpenGLPtr->glGetProgramInfoLog(programId, logSize, NULL, infoLog);

    // Open a file to write the error message to.
    filePtr = fopen("linker-error.txt", "w");
    if(filePtr == NULL)
    {
        cout << "Error opening linker error message output file." << endl;
	return;
    }

    // Write out the error message.
    count = fwrite(infoLog, sizeof(char), logSize, filePtr);
    if(count != logSize)
    {
        cout << "Error writing linker error message output file." << endl;
	return;
    }

    // Close the file.
    error = fclose(filePtr);
    if(error != 0)
    {
        cout << "Error closing linker error message output file." << endl;
	return;
    }

    // Pop a message up on the screen to notify the user to check the text file for linker errors.
    cout << "Error linking shader program.  Check linker-error.txt for message." << endl;

    return;
}


bool SoftShadowShaderClass::SetShaderParameters(float* worldMatrix, float* viewMatrix, float* projectionMatrix, float* lightPosition, float* ambientColor, float* diffuseColor)
{
    float tpWorldMatrix[16], tpViewMatrix[16], tpProjectionMatrix[16];
    int location;


    // Transpose the matrices to prepare them for the shader.
    m_OpenGLPtr->MatrixTranspose(tpWorldMatrix, worldMatrix);
    m_OpenGLPtr->MatrixTranspose(tpViewMatrix, viewMatrix);
    m_OpenGLPtr->MatrixTranspose(tpProjectionMatrix, projectionMatrix);

    // Install the shader program as part of the current rendering state.
    m_OpenGLPtr->glUseProgram(m_shaderProgram);

    // Set the world matrix in the vertex shader.
    location = m_OpenGLPtr->glGetUniformLocation(m_shaderProgram, "worldMatrix");
    if(location == -1)
    {
        cout << "World matrix not set." << endl;
    }
    m_OpenGLPtr->glUniformMatrix4fv(location, 1, false, tpWorldMatrix);

    // Set the view matrix in the vertex shader.
    location = m_OpenGLPtr->glGetUniformLocation(m_shaderProgram, "viewMatrix");
    if(location == -1)
    {
        cout << "View matrix not set." << endl;
    }
    m_OpenGLPtr->glUniformMatrix4fv(location, 1, false, tpViewMatrix);

    // Set the projection matrix in the vertex shader.
    location = m_OpenGLPtr->glGetUniformLocation(m_shaderProgram, "projectionMatrix");
    if(location == -1)
    {
        cout << "Projection matrix not set." << endl;
    }
    m_OpenGLPtr->glUniformMatrix4fv(location, 1, false, tpProjectionMatrix);

    // Set the light position in the vertex shader.
    location = m_OpenGLPtr->glGetUniformLocation(m_shaderProgram, "lightPosition");
    if(location == -1)
    {
        cout << "Light position not set." << endl;
    }
    m_OpenGLPtr->glUniform3fv(location, 1, lightPosition);

    // Set the texture in the pixel shader to use the data from the first texture unit.
    location = m_OpenGLPtr->glGetUniformLocation(m_shaderProgram, "shaderTexture");
    if(location == -1)
    {
        cout << "Shader texture not set." << endl;
    }
    m_OpenGLPtr->glUniform1i(location, 0);

    // Set the shadow texture in the pixel shader to use the data from the second texture unit.
    location = m_OpenGLPtr->glGetUniformLocation(m_shaderProgram, "shadowTexture");
    if(location == -1)
    {
        cout << "Shadow texture not set." << endl;
    }
    m_OpenGLPtr->glUniform1i(location, 1);

    // Set the ambient light in the pixel shader.
    location = m_OpenGLPtr->glGetUniformLocation(m_shaderProgram, "ambientColor");
    if(location == -1)
    {
        cout << "Ambient color not set." << endl;
    }
    m_OpenGLPtr->glUniform4fv(location, 1, ambientColor);

    // Set the diffuse light color in the pixel shader.
    location = m_OpenGLPtr->glGetUniformLocation(m_shaderProgram, "diffuseColor");
    if(location == -1)
    {
        cout << "Diffuse color not set." << endl;
    }
    m_OpenGLPtr->glUniform4fv(location, 1, diffuseColor);

    return true;
}
