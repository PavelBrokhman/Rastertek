using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial23.Graphics;

public class LightShader
{
    private uint _vertexShader,
        _fragmentShader,
        _shaderProgram;

    public bool Initialize(GL4 OpenGL) =>
        InitializeShader(OpenGL, "Shaders/Light.vs", "Shaders/Light.ps");

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        gl.DetachShader(_shaderProgram, _vertexShader);
        gl.DetachShader(_shaderProgram, _fragmentShader);
        gl.DeleteShader(_vertexShader);
        gl.DeleteShader(_fragmentShader);
        gl.DeleteProgram(_shaderProgram);
    }

    public void SetShader(GL4 OpenGL) => OpenGL.Driver.UseProgram(_shaderProgram);

    public unsafe bool SetShaderParameters(
        GL4 OpenGL,
        Matrix4X4<float> world,
        Matrix4X4<float> view,
        Matrix4X4<float> projection,
        int textureUnit,
        float[] lightDirection,
        float[] diffuseLightColor
    )
    {
        var gl = OpenGL.Driver;
        int loc;

        gl.UseProgram(_shaderProgram);

        loc = gl.GetUniformLocation(_shaderProgram, "worldMatrix");
        if (loc == -1)
            return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&world);

        loc = gl.GetUniformLocation(_shaderProgram, "viewMatrix");
        if (loc == -1)
            return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&view);

        loc = gl.GetUniformLocation(_shaderProgram, "projectionMatrix");
        if (loc == -1)
            return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&projection);

        loc = gl.GetUniformLocation(_shaderProgram, "shaderTexture");
        if (loc == -1)
            return false;
        gl.Uniform1(loc, textureUnit);

        loc = gl.GetUniformLocation(_shaderProgram, "lightDirection");
        if (loc == -1)
            return false;
        fixed (float* p = lightDirection)
            gl.Uniform3(loc, 1, p);

        loc = gl.GetUniformLocation(_shaderProgram, "diffuseLightColor");
        if (loc == -1)
            return false;
        fixed (float* p = diffuseLightColor)
            gl.Uniform4(loc, 1, p);

        return true;
    }

    private bool InitializeShader(GL4 OpenGL, string vsFilename, string psFilename)
    {
        var gl = OpenGL.Driver;
        string vsSrc = File.ReadAllText(vsFilename);
        string psSrc = File.ReadAllText(psFilename);

        _vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(_vertexShader, vsSrc);
        gl.CompileShader(_vertexShader);
        gl.GetShader(_vertexShader, ShaderParameterName.CompileStatus, out int s1);
        if (s1 != 1)
        {
            Console.WriteLine($"Compile {vsFilename}: {gl.GetShaderInfoLog(_vertexShader)}");
            return false;
        }

        _fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(_fragmentShader, psSrc);
        gl.CompileShader(_fragmentShader);
        gl.GetShader(_fragmentShader, ShaderParameterName.CompileStatus, out int s2);
        if (s2 != 1)
        {
            Console.WriteLine($"Compile {psFilename}: {gl.GetShaderInfoLog(_fragmentShader)}");
            return false;
        }

        _shaderProgram = gl.CreateProgram();
        gl.AttachShader(_shaderProgram, _vertexShader);
        gl.AttachShader(_shaderProgram, _fragmentShader);
        gl.BindAttribLocation(_shaderProgram, 0, "inputPosition");
        gl.BindAttribLocation(_shaderProgram, 1, "inputTexCoord");
        gl.BindAttribLocation(_shaderProgram, 2, "inputNormal");
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
