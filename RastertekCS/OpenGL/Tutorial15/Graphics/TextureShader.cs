using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial15.Graphics;

public class TextureShader
{
    private uint _vertexShader;
    private uint _fragmentShader;
    private uint _shaderProgram;

    public bool Initialize(GL4 OpenGL) =>
        InitializeShader(OpenGL, "Shaders/Texture.vs", "Shaders/Texture.ps");

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
        Matrix4X4<float> projectionMatrix,
        int textureUnit
    )
    {
        var gl = OpenGL.Gl;
        int loc;

        loc = gl.GetUniformLocation(_shaderProgram, "worldMatrix");
        if (loc == -1)
            return false;
        unsafe
        {
            gl.UniformMatrix4(loc, 1, false, (float*)&worldMatrix);
        }
        loc = gl.GetUniformLocation(_shaderProgram, "viewMatrix");
        if (loc == -1)
            return false;
        unsafe
        {
            gl.UniformMatrix4(loc, 1, false, (float*)&viewMatrix);
        }
        loc = gl.GetUniformLocation(_shaderProgram, "projectionMatrix");
        if (loc == -1)
            return false;
        unsafe
        {
            gl.UniformMatrix4(loc, 1, false, (float*)&projectionMatrix);
        }
        loc = gl.GetUniformLocation(_shaderProgram, "shaderTexture");
        if (loc == -1)
            return false;
        gl.Uniform1(loc, textureUnit);
        return true;
    }

    private bool InitializeShader(GL4 OpenGL, string vsFilename, string psFilename)
    {
        var gl = OpenGL.Gl;
        string vsSrc = File.ReadAllText(vsFilename);
        string psSrc = File.ReadAllText(psFilename);

        _vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(_vertexShader, vsSrc);
        gl.CompileShader(_vertexShader);
        if (!CheckShaderCompile(gl, _vertexShader, vsFilename))
            return false;

        _fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(_fragmentShader, psSrc);
        gl.CompileShader(_fragmentShader);
        if (!CheckShaderCompile(gl, _fragmentShader, psFilename))
            return false;

        _shaderProgram = gl.CreateProgram();
        gl.AttachShader(_shaderProgram, _vertexShader);
        gl.AttachShader(_shaderProgram, _fragmentShader);
        gl.BindAttribLocation(_shaderProgram, 0, "inputPosition");
        gl.BindAttribLocation(_shaderProgram, 1, "inputTexCoord");
        gl.LinkProgram(_shaderProgram);
        gl.GetProgram(_shaderProgram, ProgramPropertyARB.LinkStatus, out int ls);
        if (ls != 1)
        {
            global::System.Console.WriteLine($"Link: {gl.GetProgramInfoLog(_shaderProgram)}");
            return false;
        }
        return true;
    }

    private static bool CheckShaderCompile(GL gl, uint shader, string filename)
    {
        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int s);
        if (s != 1)
        {
            global::System.Console.WriteLine($"Compile {filename}: {gl.GetShaderInfoLog(shader)}");
            return false;
        }
        return true;
    }
}
