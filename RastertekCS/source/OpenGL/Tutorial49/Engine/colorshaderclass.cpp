////////////////////////////////////////////////////////////////////////////////
// Filename: colorshaderclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "colorshaderclass.h"


ColorShaderClass::ColorShaderClass()
{
    m_OpenGLPtr = 0;
}


ColorShaderClass::ColorShaderClass(const ColorShaderClass& other)
{
}


ColorShaderClass::~ColorShaderClass()
{
}


bool ColorShaderClass::Initialize(OpenGLClass* OpenGL)
{
    char vsFilename[128];
    char hsFilename[128];
    char dsFilename[128];
    char psFilename[128];
    bool result;


    // Store the pointer to the OpenGL object.
    m_OpenGLPtr = OpenGL;

    // Set the location and names of the shader files.
    strcpy(vsFilename, "../Engine/color.vs");
    strcpy(hsFilename, "../Engine/color.hs");
    strcpy(dsFilename, "../Engine/color.ds");
    strcpy(psFilename, "../Engine/color.ps");

    // Initialize the vertex, hull, domain, and fragment shaders.
    result = InitializeShader(vsFilename, hsFilename, dsFilename, psFilename);
    if(!result)
    {
        return false;
    }

    return true;
}


void ColorShaderClass::Shutdown()
{
    // Shutdown the shader.
    ShutdownShader();

    // Release the pointer to the OpenGL object.
    m_OpenGLPtr = 0;

    return;
}


bool ColorShaderClass::InitializeShader(char* vsFilename, char* hsFilename, char* dsFilename, char* fsFilename)
{
    const char* vertexShaderBuffer;
    const char* hullShaderBuffer;
    const char* domainShaderBuffer;
    const char* fragmentShaderBuffer;
    int status;


    // Load the vertex shader source file into a text buffer.
    vertexShaderBuffer = LoadShaderSourceFile(vsFilename);
    if(!vertexShaderBuffer)
    {
        return false;
    }

    // Load the hull (tessellation control) shader source file into a text buffer.
    hullShaderBuffer = LoadShaderSourceFile(hsFilename);
    if(!hullShaderBuffer)
    {
        return false;
    }

    // Load the domain (tessellation evaluation) shader source file into a text buffer.
    domainShaderBuffer = LoadShaderSourceFile(dsFilename);
    if(!domainShaderBuffer)
    {
        return false;
    }

    // Load the fragment shader source file into a text buffer.
    fragmentShaderBuffer = LoadShaderSourceFile(fsFilename);
    if(!fragmentShaderBuffer)
    {
        return false;
    }

    // Create the vertex, hull, domain, and fragment shader objects.
    m_vertexShader = m_OpenGLPtr->glCreateShader(GL_VERTEX_SHADER);
    m_hullShader = m_OpenGLPtr->glCreateShader(GL_TESS_CONTROL_SHADER);
    m_domainShader = m_OpenGLPtr->glCreateShader(GL_TESS_EVALUATION_SHADER);
    m_fragmentShader = m_OpenGLPtr->glCreateShader(GL_FRAGMENT_SHADER);

    // Copy the shader source code strings into the vertex, hull, domain, and fragment shader objects.
    m_OpenGLPtr->glShaderSource(m_vertexShader, 1, &vertexShaderBuffer, NULL);
    m_OpenGLPtr->glShaderSource(m_hullShader, 1, &hullShaderBuffer, NULL);
    m_OpenGLPtr->glShaderSource(m_domainShader, 1, &domainShaderBuffer, NULL);
    m_OpenGLPtr->glShaderSource(m_fragmentShader, 1, &fragmentShaderBuffer, NULL);

    // Release the shader source buffers.
    delete [] vertexShaderBuffer;
    vertexShaderBuffer = 0;

    delete [] hullShaderBuffer;
    hullShaderBuffer = 0;

    delete [] domainShaderBuffer;
    domainShaderBuffer = 0;

    delete [] fragmentShaderBuffer;
    fragmentShaderBuffer = 0;

    // Compile the shaders.
    m_OpenGLPtr->glCompileShader(m_vertexShader);
    m_OpenGLPtr->glCompileShader(m_hullShader);
    m_OpenGLPtr->glCompileShader(m_domainShader);
    m_OpenGLPtr->glCompileShader(m_fragmentShader);

    // Check to see if the vertex shader compiled successfully.
    m_OpenGLPtr->glGetShaderiv(m_vertexShader, GL_COMPILE_STATUS, &status);
    if(status != 1)
    {
        OutputShaderErrorMessage(m_vertexShader, vsFilename);
        return false;
    }

    // Check to see if the hull shader compiled successfully.
    m_OpenGLPtr->glGetShaderiv(m_hullShader, GL_COMPILE_STATUS, &status);
    if(status != 1)
    {
        OutputShaderErrorMessage(m_hullShader, hsFilename);
        return false;
    }

    // Check to see if the domain shader compiled successfully.
    m_OpenGLPtr->glGetShaderiv(m_domainShader, GL_COMPILE_STATUS, &status);
    if(status != 1)
    {
        OutputShaderErrorMessage(m_domainShader, dsFilename);
        return false;
    }

    // Check to see if the fragment shader compiled successfully.
    m_OpenGLPtr->glGetShaderiv(m_fragmentShader, GL_COMPILE_STATUS, &status);
    if(status != 1)
    {
        OutputShaderErrorMessage(m_fragmentShader, fsFilename);
        return false;
    }

    // Create a shader program object.
    m_shaderProgram = m_OpenGLPtr->glCreateProgram();

    // Attach the vertex, hull, domain, and fragment shaders to the program object.
    m_OpenGLPtr->glAttachShader(m_shaderProgram, m_vertexShader);
    m_OpenGLPtr->glAttachShader(m_shaderProgram, m_hullShader);
    m_OpenGLPtr->glAttachShader(m_shaderProgram, m_domainShader);
    m_OpenGLPtr->glAttachShader(m_shaderProgram, m_fragmentShader);

    // Bind the shader input variables.
    m_OpenGLPtr->glBindAttribLocation(m_shaderProgram, 0, "inputPosition");
    m_OpenGLPtr->glBindAttribLocation(m_shaderProgram, 1, "inputColor");

    // Link the shader program.
    m_OpenGLPtr->glLinkProgram(m_shaderProgram);

    // Check the status of the link.
    m_OpenGLPtr->glGetProgramiv(m_shaderProgram, GL_LINK_STATUS, &status);
    if(status != 1)
    {
        OutputLinkerErrorMessage(m_shaderProgram);
        return false;
    }

    return true;
}


void ColorShaderClass::ShutdownShader()
{
    // Detach the shaders from the program.
    m_OpenGLPtr->glDetachShader(m_shaderProgram, m_vertexShader);
    m_OpenGLPtr->glDetachShader(m_shaderProgram, m_hullShader);
    m_OpenGLPtr->glDetachShader(m_shaderProgram, m_domainShader);
    m_OpenGLPtr->glDetachShader(m_shaderProgram, m_fragmentShader);

    // Delete the shaders.
    m_OpenGLPtr->glDeleteShader(m_vertexShader);
    m_OpenGLPtr->glDeleteShader(m_hullShader);
    m_OpenGLPtr->glDeleteShader(m_domainShader);
    m_OpenGLPtr->glDeleteShader(m_fragmentShader);

    // Delete the shader program.
    m_OpenGLPtr->glDeleteProgram(m_shaderProgram);

    return;
}


char* ColorShaderClass::LoadShaderSourceFile(char* filename)
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


void ColorShaderClass::OutputShaderErrorMessage(unsigned int shaderId, char* shaderFilename)
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


void ColorShaderClass::OutputLinkerErrorMessage(unsigned int programId)
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


bool ColorShaderClass::SetShaderParameters(float* worldMatrix, float* viewMatrix, float* projectionMatrix, float tessellationAmount)
{
    float tpWorldMatrix[16], tpViewMatrix[16], tpProjectionMatrix[16];
    int location;


    // Transpose the matrices to prepare them for the shader.
    m_OpenGLPtr->MatrixTranspose(tpWorldMatrix, worldMatrix);
    m_OpenGLPtr->MatrixTranspose(tpViewMatrix, viewMatrix);
    m_OpenGLPtr->MatrixTranspose(tpProjectionMatrix, projectionMatrix);

    // Install the shader program as part of the current rendering state.
    m_OpenGLPtr->glUseProgram(m_shaderProgram);

    // Set the world matrix in the domain shader.
    location = m_OpenGLPtr->glGetUniformLocation(m_shaderProgram, "worldMatrix");
    if(location == -1)
    {
        cout << "World matrix is not set." << endl;
    }
    m_OpenGLPtr->glUniformMatrix4fv(location, 1, false, tpWorldMatrix);

    // Set the view matrix in the domain shader.
    location = m_OpenGLPtr->glGetUniformLocation(m_shaderProgram, "viewMatrix");
    if(location == -1)
    {
        cout << "View matrix is not set." << endl;
    }
    m_OpenGLPtr->glUniformMatrix4fv(location, 1, false, tpViewMatrix);

    // Set the projection matrix in the domain shader.
    location = m_OpenGLPtr->glGetUniformLocation(m_shaderProgram, "projectionMatrix");
    if(location == -1)
    {
        cout << "Projection matrix is not set." << endl;
    }
    m_OpenGLPtr->glUniformMatrix4fv(location, 1, false, tpProjectionMatrix);

    // Set the tessellation amount in the hull (tessellation control) shader.
    location = m_OpenGLPtr->glGetUniformLocation(m_shaderProgram, "tessellationAmount");
    if(location == -1)
    {
        cout << "Tessellation amount is not set." << endl;
    }
    m_OpenGLPtr->glUniform1f(location, tessellationAmount);

    return true;
}
