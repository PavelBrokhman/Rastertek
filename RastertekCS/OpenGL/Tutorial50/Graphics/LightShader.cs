using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial50.Graphics;

public class LightShader
{
    private uint _vertexshader;
    private uint _fragmentshader;
    private uint _shaderprogram;

    public bool Initialize(GL4 OpenGL) => InitializeShader(OpenGL, "Shaders/Light.vs", "Shaders/Light.ps");

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DetachShader(_shaderprogram, _vertexshader);
        gl.DetachShader(_shaderprogram, _fragmentshader);
        gl.DeleteShader(_vertexshader);
        gl.DeleteShader(_fragmentshader);
        gl.DeleteProgram(_shaderprogram);
    }

    public void SetShader(GL4 OpenGL) => OpenGL.Gl.UseProgram(_shaderprogram);

    public unsafe bool SetShaderParameters(GL4 OpenGL,
        Matrix4X4<float> worldMatrix, Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix,
        float[] lightDirection, int colorTextureUnit, int normalTextureUnit)
    {
        var gl = OpenGL.Gl;
        gl.UseProgram(_shaderprogram);

        int loc = gl.GetUniformLocation(_shaderprogram, "worldMatrix");
        if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&worldMatrix);

        loc = gl.GetUniformLocation(_shaderprogram, "viewMatrix");
        if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&viewMatrix);

        loc = gl.GetUniformLocation(_shaderprogram, "projectionMatrix");
        if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&projectionMatrix);

        loc = gl.GetUniformLocation(_shaderprogram, "colorTexture");
        if (loc == -1) return false;
        gl.Uniform1(loc, colorTextureUnit);

        loc = gl.GetUniformLocation(_shaderprogram, "normalTexture");
        if (loc == -1) return false;
        gl.Uniform1(loc, normalTextureUnit);

        loc = gl.GetUniformLocation(_shaderprogram, "lightDirection");
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

        _vertexshader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(_vertexshader, vsSrc);
        gl.CompileShader(_vertexshader);
        gl.GetShader(_vertexshader, ShaderParameterName.CompileStatus, out int s);
        if (s != 1) { Console.WriteLine($"Compile {vsFilename}: {gl.GetShaderInfoLog(_vertexshader)}"); return false; }

        _fragmentshader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(_fragmentshader, psSrc);
        gl.CompileShader(_fragmentshader);
        gl.GetShader(_fragmentshader, ShaderParameterName.CompileStatus, out s);
        if (s != 1) { Console.WriteLine($"Compile {psFilename}: {gl.GetShaderInfoLog(_fragmentshader)}"); return false; }

        _shaderprogram = gl.CreateProgram();
        gl.AttachShader(_shaderprogram, _vertexshader);
        gl.AttachShader(_shaderprogram, _fragmentshader);
        gl.BindAttribLocation(_shaderprogram, 0, "inputPosition");
        gl.BindAttribLocation(_shaderprogram, 1, "inputTexCoord");
        gl.LinkProgram(_shaderprogram);
        gl.GetProgram(_shaderprogram, ProgramPropertyARB.LinkStatus, out int ls);
        if (ls != 1) { Console.WriteLine($"Link: {gl.GetProgramInfoLog(_shaderprogram)}"); return false; }
        return true;
    }
}
