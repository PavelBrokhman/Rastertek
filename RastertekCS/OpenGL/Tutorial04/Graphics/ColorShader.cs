using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial04.Graphics;

public class ColorShader
{
    private uint _vertexShader;
    private uint _fragmentShader;
    private uint _shaderProgram;

    public bool Initialize(GL4 OpenGL)
    {
        return InitializeShader(OpenGL, "Shaders/Color.vs", "Shaders/Color.ps");
    }

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DetachShader(_shaderProgram, _vertexShader);
        gl.DetachShader(_shaderProgram, _fragmentShader);
        gl.DeleteShader(_vertexShader);
        gl.DeleteShader(_fragmentShader);
        gl.DeleteProgram(_shaderProgram);
    }

    public void SetShader(GL4 OpenGL) => OpenGL.Gl.UseProgram(_shaderProgram);

    public bool SetShaderParameters(
        GL4 OpenGL,
        Matrix4X4<float> worldMatrix,
        Matrix4X4<float> viewMatrix,
        Matrix4X4<float> projectionMatrix
    )
    {
        var gl = OpenGL.Gl;

        int location = gl.GetUniformLocation(_shaderProgram, "worldMatrix");
        if (location == -1)
            return false;
        unsafe
        {
            gl.UniformMatrix4(location, 1, false, (float*)&worldMatrix);
        }

        location = gl.GetUniformLocation(_shaderProgram, "viewMatrix");
        if (location == -1)
            return false;
        unsafe
        {
            gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
        }

        location = gl.GetUniformLocation(_shaderProgram, "projectionMatrix");
        if (location == -1)
            return false;
        unsafe
        {
            gl.UniformMatrix4(location, 1, false, (float*)&projectionMatrix);
        }

        return true;
    }

    private bool InitializeShader(GL4 OpenGL, string vsFilename, string psFilename)
    {
        var gl = OpenGL.Gl;

        string vsSource = File.ReadAllText(vsFilename);
        string psSource = File.ReadAllText(psFilename);

        _vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(_vertexShader, vsSource);
        gl.CompileShader(_vertexShader);
        if (!CheckShaderCompile(gl, _vertexShader, vsFilename))
            return false;

        _fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(_fragmentShader, psSource);
        gl.CompileShader(_fragmentShader);
        if (!CheckShaderCompile(gl, _fragmentShader, psFilename))
            return false;

        _shaderProgram = gl.CreateProgram();
        gl.AttachShader(_shaderProgram, _vertexShader);
        gl.AttachShader(_shaderProgram, _fragmentShader);

        gl.BindAttribLocation(_shaderProgram, 0, "inputPosition");
        gl.BindAttribLocation(_shaderProgram, 1, "inputColor");

        gl.LinkProgram(_shaderProgram);
        gl.GetProgram(_shaderProgram, ProgramPropertyARB.LinkStatus, out int linkStatus);
        if (linkStatus != 1)
        {
            string log = gl.GetProgramInfoLog(_shaderProgram);
            global::System.Console.WriteLine($"Ошибка линковки шейдеров: {log}");
            return false;
        }
        return true;
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
