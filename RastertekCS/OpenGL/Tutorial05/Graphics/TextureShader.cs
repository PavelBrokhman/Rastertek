using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial05.Graphics;

public class TextureShader
{
    private uint _vertexShader;
    private uint _fragmentShader;
    private uint _shaderProgram;

    public bool Initialize(GL4 OpenGL)
    {
        // Set the location and names of the shader files.
        // Initialize the vertex and pixel shaders.
        return InitializeShader(OpenGL, "Shaders/Texture.vs", "Shaders/Texture.ps");
    }

    public void Shutdown(GL4 OpenGL)
    {
        ShutdownShader(OpenGL);
    }

    public unsafe bool SetShaderParameters(
        GL4 OpenGL,
        Matrix4X4<float> worldMatrix,
        Matrix4X4<float> viewMatrix,
        Matrix4X4<float> projectionMatrix
    )
    {
        var gl = OpenGL.Driver;

        // Transpose the matrices to prepare them for the shader.
        var tpWorld = GL4.MatrixTranspose(worldMatrix);
        var tpView = GL4.MatrixTranspose(viewMatrix);
        var tpProj = GL4.MatrixTranspose(projectionMatrix);

        // Install the shader program as part of the current rendering state.
        gl.UseProgram(_shaderProgram);

        // Set the world matrix in the vertex shader.
        int location = gl.GetUniformLocation(_shaderProgram, "worldMatrix");
        if (location == -1)
            global::System.Console.WriteLine("World matrix not set.");
        gl.UniformMatrix4(location, 1, false, (float*)&tpWorld);

        // Set the view matrix in the vertex shader.
        location = gl.GetUniformLocation(_shaderProgram, "viewMatrix");
        if (location == -1)
            global::System.Console.WriteLine("View matrix not set.");
        gl.UniformMatrix4(location, 1, false, (float*)&tpView);

        // Set the projection matrix in the vertex shader.
        location = gl.GetUniformLocation(_shaderProgram, "projectionMatrix");
        if (location == -1)
            global::System.Console.WriteLine("Projection matrix not set.");
        gl.UniformMatrix4(location, 1, false, (float*)&tpProj);

        // Set the texture in the pixel shader to use the data from the first texture unit.
        location = gl.GetUniformLocation(_shaderProgram, "shaderTexture");
        if (location == -1)
            global::System.Console.WriteLine("Shader texture not set.");
        gl.Uniform1(location, 0);

        return true;
    }

    private bool InitializeShader(GL4 OpenGL, string vsFilename, string psFilename)
    {
        var gl = OpenGL.Driver;

        // Load the vertex/fragment shader source files.
        string vsSource = File.ReadAllText(vsFilename);
        string psSource = File.ReadAllText(psFilename);

        // Create a vertex and fragment shader object.
        _vertexShader = gl.CreateShader(ShaderType.VertexShader);
        _fragmentShader = gl.CreateShader(ShaderType.FragmentShader);

        // Copy the shader source code strings into the vertex and fragment shader objects.
        gl.ShaderSource(_vertexShader, vsSource);
        gl.ShaderSource(_fragmentShader, psSource);

        // Compile the shaders.
        gl.CompileShader(_vertexShader);
        gl.CompileShader(_fragmentShader);

        // Check to see if the vertex shader compiled successfully.
        if (!CheckShaderCompile(gl, _vertexShader, vsFilename))
            return false;
        // Check to see if the fragment shader compiled successfully.
        if (!CheckShaderCompile(gl, _fragmentShader, psFilename))
            return false;

        // Create a shader program object.
        _shaderProgram = gl.CreateProgram();

        // Attach the vertex and fragment shader to the program object.
        gl.AttachShader(_shaderProgram, _vertexShader);
        gl.AttachShader(_shaderProgram, _fragmentShader);

        // Bind the shader input variables.
        gl.BindAttribLocation(_shaderProgram, 0, "inputPosition");
        gl.BindAttribLocation(_shaderProgram, 1, "inputTexCoord");

        // Link the shader program.
        gl.LinkProgram(_shaderProgram);

        // Check the status of the link.
        gl.GetProgram(_shaderProgram, ProgramPropertyARB.LinkStatus, out int linkStatus);
        if (linkStatus != 1)
        {
            string log = gl.GetProgramInfoLog(_shaderProgram);
            global::System.Console.WriteLine($"Ошибка линковки шейдеров: {log}");
            return false;
        }
        return true;
    }

    private void ShutdownShader(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;

        // Detach the vertex and fragment shaders from the program.
        gl.DetachShader(_shaderProgram, _vertexShader);
        gl.DetachShader(_shaderProgram, _fragmentShader);

        // Delete the vertex and fragment shaders.
        gl.DeleteShader(_vertexShader);
        gl.DeleteShader(_fragmentShader);

        // Delete the shader program.
        gl.DeleteProgram(_shaderProgram);
    }

    private static bool CheckShaderCompile(GL gl, uint shader, string filename)
    {
        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
        if (status != 1)
        {
            string log = gl.GetShaderInfoLog(shader);
            global::System.Console.WriteLine($"Ошибка компиляции {filename}: {log}");
            return false;
        }
        return true;
    }
}
