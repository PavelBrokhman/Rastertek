using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial32.Graphics;

public class GlassShader
{
    private uint _vertexShader,
        _fragmentShader,
        _shaderProgram;

    public bool Initialize(GL4 gl) => Init(gl, "Shaders/glass.vs", "Shaders/glass.ps");

    public void Shutdown(GL4 gl)
    {
        var glApi = gl.Driver;
        glApi.DetachShader(_shaderProgram, _vertexShader);
        glApi.DetachShader(_shaderProgram, _fragmentShader);
        glApi.DeleteShader(_vertexShader);
        glApi.DeleteShader(_fragmentShader);
        glApi.DeleteProgram(_shaderProgram);
    }

    public unsafe bool SetShaderParameters(
        GL4 gl,
        Matrix4X4<float> worldMatrix,
        Matrix4X4<float> viewMatrix,
        Matrix4X4<float> projectionMatrix,
        float refractionScale
    )
    {
        var glApi = gl.Driver;
        glApi.UseProgram(_shaderProgram);
        int loc;
        loc = glApi.GetUniformLocation(_shaderProgram, "worldMatrix");
        if (loc >= 0)
            glApi.UniformMatrix4(loc, 1, false, (float*)&worldMatrix);
        loc = glApi.GetUniformLocation(_shaderProgram, "viewMatrix");
        if (loc >= 0)
            glApi.UniformMatrix4(loc, 1, false, (float*)&viewMatrix);
        loc = glApi.GetUniformLocation(_shaderProgram, "projectionMatrix");
        if (loc >= 0)
            glApi.UniformMatrix4(loc, 1, false, (float*)&projectionMatrix);
        loc = glApi.GetUniformLocation(_shaderProgram, "colorTexture");
        if (loc >= 0)
            glApi.Uniform1(loc, 0);
        loc = glApi.GetUniformLocation(_shaderProgram, "normalTexture");
        if (loc >= 0)
            glApi.Uniform1(loc, 1);
        loc = glApi.GetUniformLocation(_shaderProgram, "refractionTexture");
        if (loc >= 0)
            glApi.Uniform1(loc, 2);
        loc = glApi.GetUniformLocation(_shaderProgram, "refractionScale");
        if (loc >= 0)
            glApi.Uniform1(loc, refractionScale);
        return true;
    }

    bool Init(GL4 gl, string vertexShaderFile, string pixelShaderFile)
    {
        var glApi = gl.Driver;
        _vertexShader = glApi.CreateShader(ShaderType.VertexShader);
        glApi.ShaderSource(_vertexShader, File.ReadAllText(vertexShaderFile));
        glApi.CompileShader(_vertexShader);
        glApi.GetShader(_vertexShader, ShaderParameterName.CompileStatus, out int s);
        if (s != 1)
        {
            Console.WriteLine($"VS: {glApi.GetShaderInfoLog(_vertexShader)}");
            return false;
        }
        _fragmentShader = glApi.CreateShader(ShaderType.FragmentShader);
        glApi.ShaderSource(_fragmentShader, File.ReadAllText(pixelShaderFile));
        glApi.CompileShader(_fragmentShader);
        glApi.GetShader(_fragmentShader, ShaderParameterName.CompileStatus, out s);
        if (s != 1)
        {
            Console.WriteLine($"PS: {glApi.GetShaderInfoLog(_fragmentShader)}");
            return false;
        }
        _shaderProgram = glApi.CreateProgram();
        glApi.AttachShader(_shaderProgram, _vertexShader);
        glApi.AttachShader(_shaderProgram, _fragmentShader);
        glApi.BindAttribLocation(_shaderProgram, 0, "inputPosition");
        glApi.BindAttribLocation(_shaderProgram, 1, "inputTexCoord");
        glApi.BindAttribLocation(_shaderProgram, 2, "inputNormal");
        glApi.LinkProgram(_shaderProgram);
        glApi.GetProgram(_shaderProgram, ProgramPropertyARB.LinkStatus, out int ls);
        if (ls != 1)
        {
            Console.WriteLine($"Link: {glApi.GetProgramInfoLog(_shaderProgram)}");
            return false;
        }
        return true;
    }
}
