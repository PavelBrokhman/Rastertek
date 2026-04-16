using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial15.Graphics;

public class FontShader
{
    private uint _vertexShader,
        _fragmentShader,
        _shaderProgram;

    public bool Initialize(GL4 OpenGL) =>
        InitializeShader(OpenGL, "Shaders/Font.vs", "Shaders/Font.ps");

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

    public bool SetShaderParameters(
        GL4 OpenGL,
        Matrix4X4<float> world,
        Matrix4X4<float> view,
        Matrix4X4<float> projection,
        int textureUnit,
        float[] pixelColor
    )
    {
        var gl = OpenGL.Driver;
        int loc;
        loc = gl.GetUniformLocation(_shaderProgram, "worldMatrix");
        if (loc == -1)
            return false;
        unsafe
        {
            gl.UniformMatrix4(loc, 1, false, (float*)&world);
        }
        loc = gl.GetUniformLocation(_shaderProgram, "viewMatrix");
        if (loc == -1)
            return false;
        unsafe
        {
            gl.UniformMatrix4(loc, 1, false, (float*)&view);
        }
        loc = gl.GetUniformLocation(_shaderProgram, "projectionMatrix");
        if (loc == -1)
            return false;
        unsafe
        {
            gl.UniformMatrix4(loc, 1, false, (float*)&projection);
        }
        loc = gl.GetUniformLocation(_shaderProgram, "shaderTexture");
        if (loc == -1)
            return false;
        gl.Uniform1(loc, textureUnit);
        loc = gl.GetUniformLocation(_shaderProgram, "pixelColor");
        if (loc == -1)
            return false;
        unsafe
        {
            fixed (float* p = pixelColor)
                gl.Uniform4(loc, 1, p);
        }
        return true;
    }

    private bool InitializeShader(GL4 OpenGL, string vs, string ps)
    {
        var gl = OpenGL.Driver;
        _vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(_vertexShader, File.ReadAllText(vs));
        gl.CompileShader(_vertexShader);
        gl.GetShader(_vertexShader, ShaderParameterName.CompileStatus, out int s1);
        if (s1 != 1)
        {
            global::System.Console.WriteLine($"Compile {vs}: {gl.GetShaderInfoLog(_vertexShader)}");
            return false;
        }

        _fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(_fragmentShader, File.ReadAllText(ps));
        gl.CompileShader(_fragmentShader);
        gl.GetShader(_fragmentShader, ShaderParameterName.CompileStatus, out int s2);
        if (s2 != 1)
        {
            global::System.Console.WriteLine(
                $"Compile {ps}: {gl.GetShaderInfoLog(_fragmentShader)}"
            );
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
            global::System.Console.WriteLine($"Link: {gl.GetProgramInfoLog(_shaderProgram)}");
            return false;
        }
        return true;
    }
}
