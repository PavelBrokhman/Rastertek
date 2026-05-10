using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial36.Graphics;

public class BlurShader
{
    private uint _vertexShader,
        _fragmentShader,
        _shaderProgram;

    public bool Initialize(GL4 OpenGL) =>
        InitializeShader(OpenGL, "Shaders/Blur.vs", "Shaders/Blur.ps");

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        gl.DetachShader(_shaderProgram, _vertexShader);
        gl.DetachShader(_shaderProgram, _fragmentShader);
        gl.DeleteShader(_vertexShader);
        gl.DeleteShader(_fragmentShader);
        gl.DeleteProgram(_shaderProgram);
    }

    public unsafe bool SetShaderParameters(
        GL4 OpenGL,
        Matrix4X4<float> worldMatrix,
        Matrix4X4<float> viewMatrix,
        Matrix4X4<float> projectionMatrix,
        float screenWidth,
        float screenHeight,
        float blurType
    )
    {
        var gl = OpenGL.Driver;
        gl.UseProgram(_shaderProgram);
        int loc = gl.GetUniformLocation(_shaderProgram, "worldMatrix");
        if (loc == -1)
            return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&worldMatrix);
        loc = gl.GetUniformLocation(_shaderProgram, "viewMatrix");
        if (loc == -1)
            return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&viewMatrix);
        loc = gl.GetUniformLocation(_shaderProgram, "projectionMatrix");
        if (loc == -1)
            return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&projectionMatrix);
        loc = gl.GetUniformLocation(_shaderProgram, "shaderTexture");
        if (loc == -1)
            return false;
        gl.Uniform1(loc, 0);
        loc = gl.GetUniformLocation(_shaderProgram, "screenWidth");
        if (loc == -1)
            return false;
        gl.Uniform1(loc, screenWidth);
        loc = gl.GetUniformLocation(_shaderProgram, "screenHeight");
        if (loc == -1)
            return false;
        gl.Uniform1(loc, screenHeight);
        loc = gl.GetUniformLocation(_shaderProgram, "blurType");
        if (loc == -1)
            return false;
        gl.Uniform1(loc, blurType);
        return true;
    }

    private bool InitializeShader(GL4 OpenGL, string vertexShaderFile, string pixelShaderFile)
    {
        var gl = OpenGL.Driver;
        _vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(_vertexShader, File.ReadAllText(vertexShaderFile));
        gl.CompileShader(_vertexShader);
        gl.GetShader(_vertexShader, ShaderParameterName.CompileStatus, out int s);
        if (s != 1)
        {
            Console.WriteLine($"VS: {gl.GetShaderInfoLog(_vertexShader)}");
            return false;
        }
        _fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(_fragmentShader, File.ReadAllText(pixelShaderFile));
        gl.CompileShader(_fragmentShader);
        gl.GetShader(_fragmentShader, ShaderParameterName.CompileStatus, out s);
        if (s != 1)
        {
            Console.WriteLine($"PS: {gl.GetShaderInfoLog(_fragmentShader)}");
            return false;
        }
        _shaderProgram = gl.CreateProgram();
        gl.AttachShader(_shaderProgram, _vertexShader);
        gl.AttachShader(_shaderProgram, _fragmentShader);
        gl.BindAttribLocation(_shaderProgram, 0, "inputPosition");
        gl.BindAttribLocation(_shaderProgram, 1, "inputTexCoord");
        gl.LinkProgram(_shaderProgram);
        gl.GetProgram(_shaderProgram, ProgramPropertyARB.LinkStatus, out int ls);
        if (ls != 1)
        {
            Console.WriteLine($"Link: {gl.GetProgramInfoLog(_shaderProgram)}");
            return false;
        }
        return true;
    }
}
