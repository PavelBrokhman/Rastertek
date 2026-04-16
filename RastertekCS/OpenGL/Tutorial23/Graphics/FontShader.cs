using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial23.Graphics;

public class FontShader
{
    private uint m_vertexShader,
        m_fragmentShader,
        m_shaderProgram;

    public bool Initialize(GL4 OpenGL) =>
        InitializeShader(OpenGL, "Shaders/Font.vs", "Shaders/Font.ps");

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DetachShader(m_shaderProgram, m_vertexShader);
        gl.DetachShader(m_shaderProgram, m_fragmentShader);
        gl.DeleteShader(m_vertexShader);
        gl.DeleteShader(m_fragmentShader);
        gl.DeleteProgram(m_shaderProgram);
    }

    public void SetShader(GL4 OpenGL) => OpenGL.Gl.UseProgram(m_shaderProgram);

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

        gl.UseProgram(m_shaderProgram);

        loc = gl.GetUniformLocation(m_shaderProgram, "worldMatrix");
        if (loc == -1)
            return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&world);

        loc = gl.GetUniformLocation(m_shaderProgram, "viewMatrix");
        if (loc == -1)
            return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&view);

        loc = gl.GetUniformLocation(m_shaderProgram, "projectionMatrix");
        if (loc == -1)
            return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&projection);

        loc = gl.GetUniformLocation(m_shaderProgram, "shaderTexture");
        if (loc == -1)
            return false;
        gl.Uniform1(loc, textureUnit);

        loc = gl.GetUniformLocation(m_shaderProgram, "pixelColor");
        if (loc == -1)
            return false;
        fixed (float* p = pixelColor)
            gl.Uniform4(loc, 1, p);

        return true;
    }

    private bool InitializeShader(GL4 OpenGL, string vsFilename, string psFilename)
    {
        var gl = OpenGL.Gl;
        m_vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(m_vertexShader, File.ReadAllText(vsFilename));
        gl.CompileShader(m_vertexShader);
        gl.GetShader(m_vertexShader, ShaderParameterName.CompileStatus, out int s1);
        if (s1 != 1)
        {
            Console.WriteLine($"Compile {vsFilename}: {gl.GetShaderInfoLog(m_vertexShader)}");
            return false;
        }

        m_fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(m_fragmentShader, File.ReadAllText(psFilename));
        gl.CompileShader(m_fragmentShader);
        gl.GetShader(m_fragmentShader, ShaderParameterName.CompileStatus, out int s2);
        if (s2 != 1)
        {
            Console.WriteLine($"Compile {psFilename}: {gl.GetShaderInfoLog(m_fragmentShader)}");
            return false;
        }

        m_shaderProgram = gl.CreateProgram();
        gl.AttachShader(m_shaderProgram, m_vertexShader);
        gl.AttachShader(m_shaderProgram, m_fragmentShader);
        gl.BindAttribLocation(m_shaderProgram, 0, "inputPosition");
        gl.BindAttribLocation(m_shaderProgram, 1, "inputTexCoord");
        gl.LinkProgram(m_shaderProgram);
        gl.GetProgram(m_shaderProgram, ProgramPropertyARB.LinkStatus, out int ls);
        if (ls != 1)
        {
            Console.WriteLine($"Link: {gl.GetProgramInfoLog(m_shaderProgram)}");
            return false;
        }
        return true;
    }
}
