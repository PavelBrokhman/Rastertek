using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial23.Graphics;

public class FontShader
{
    private uint _vertexShader,
        _fragmentShader,
        _shaderProgram;

    public bool Initialize(GL4 OpenGL) =>
        InitializeShader(OpenGL, "Shaders/Font.vs", "Shaders/Font.ps");

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

    public unsafe bool SetShaderParameters(
        GL4 OpenGL,
        Matrix4X4<float> world,
        Matrix4X4<float> view,
        Matrix4X4<float> projection,
        int textureUnit,
        float[] pixelColor
    )
    {
        var gl = OpenGL.Gl;
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

        loc = gl.GetUniformLocation(_shaderProgram, "pixelColor");
        if (loc == -1)
            return false;
        fixed (float* p = pixelColor)
            gl.Uniform4(loc, 1, p);

        return true;
    }

    private bool InitializeShader(GL4 OpenGL, string vsFilename, string psFilename)
    {
        var gl = OpenGL.Gl;
        _vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(_vertexShader, File.ReadAllText(vsFilename));
        gl.CompileShader(_vertexShader);
        gl.GetShader(_vertexShader, ShaderParameterName.CompileStatus, out int s1);
        if (s1 != 1)
        {
            Console.WriteLine($"Compile {vsFilename}: {gl.GetShaderInfoLog(_vertexShader)}");
            return false;
        }

        _fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(_fragmentShader, File.ReadAllText(psFilename));
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
