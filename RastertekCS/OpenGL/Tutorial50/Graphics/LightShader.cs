using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial50.Graphics;

public class LightShader
{
    private uint _vertexShader;
    private uint _fragmentShader;
    private uint _shaderProgram;

    public bool Initialize(GL4 OpenGL) => InitializeShader(OpenGL, "Shaders/Light.vs", "Shaders/Light.ps");

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

    public unsafe bool SetShaderParameters(GL4 OpenGL,
        Matrix4X4<float> worldMatrix, Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix,
        float[] lightDirection, int colorTextureUnit, int normalTextureUnit)
    {
        var gl = OpenGL.Gl;
        gl.UseProgram(_shaderProgram);

        int loc = gl.GetUniformLocation(_shaderProgram, "worldMatrix");
        if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&worldMatrix);

        loc = gl.GetUniformLocation(_shaderProgram, "viewMatrix");
        if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&viewMatrix);

        loc = gl.GetUniformLocation(_shaderProgram, "projectionMatrix");
        if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&projectionMatrix);

        loc = gl.GetUniformLocation(_shaderProgram, "colorTexture");
        if (loc == -1) return false;
        gl.Uniform1(loc, colorTextureUnit);

        loc = gl.GetUniformLocation(_shaderProgram, "normalTexture");
        if (loc == -1) return false;
        gl.Uniform1(loc, normalTextureUnit);

        loc = gl.GetUniformLocation(_shaderProgram, "lightDirection");
        if (loc == -1) return false;
        fixed (float* p = lightDirection)
            gl.Uniform3(loc, 1, p);

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
        gl.GetShader(_vertexShader, ShaderParameterName.CompileStatus, out int s);
        if (s != 1) { Console.WriteLine($"Compile {vsFilename}: {gl.GetShaderInfoLog(_vertexShader)}"); return false; }

        _fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(_fragmentShader, psSrc);
        gl.CompileShader(_fragmentShader);
        gl.GetShader(_fragmentShader, ShaderParameterName.CompileStatus, out s);
        if (s != 1) { Console.WriteLine($"Compile {psFilename}: {gl.GetShaderInfoLog(_fragmentShader)}"); return false; }

        _shaderProgram = gl.CreateProgram();
        gl.AttachShader(_shaderProgram, _vertexShader);
        gl.AttachShader(_shaderProgram, _fragmentShader);
        gl.BindAttribLocation(_shaderProgram, 0, "inputPosition");
        gl.BindAttribLocation(_shaderProgram, 1, "inputTexCoord");
        gl.LinkProgram(_shaderProgram);
        gl.GetProgram(_shaderProgram, ProgramPropertyARB.LinkStatus, out int ls);
        if (ls != 1) { Console.WriteLine($"Link: {gl.GetProgramInfoLog(_shaderProgram)}"); return false; }
        return true;
    }
}
