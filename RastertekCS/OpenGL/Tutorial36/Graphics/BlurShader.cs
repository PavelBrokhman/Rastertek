using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial36.Graphics;

public class BlurShader
{
    private uint m_vertexShader, m_fragmentShader, m_shaderProgram;

    public bool Initialize(GL4 OpenGL) => InitializeShader(OpenGL, "Shaders/Blur.vs", "Shaders/Blur.ps");

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DetachShader(m_shaderProgram, m_vertexShader);
        gl.DetachShader(m_shaderProgram, m_fragmentShader);
        gl.DeleteShader(m_vertexShader);
        gl.DeleteShader(m_fragmentShader);
        gl.DeleteProgram(m_shaderProgram);
    }

    public bool SetShaderParameters(GL4 OpenGL,
        Matrix4X4<float> worldMatrix, Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix,
        float screenWidth, float screenHeight, float blurType)
    {
        var gl = OpenGL.Gl;
        gl.UseProgram(m_shaderProgram);

        int loc = gl.GetUniformLocation(m_shaderProgram, "worldMatrix");
        if (loc == -1) return false;
        unsafe { gl.UniformMatrix4(loc, 1, false, (float*)&worldMatrix); }

        loc = gl.GetUniformLocation(m_shaderProgram, "viewMatrix");
        if (loc == -1) return false;
        unsafe { gl.UniformMatrix4(loc, 1, false, (float*)&viewMatrix); }

        loc = gl.GetUniformLocation(m_shaderProgram, "projectionMatrix");
        if (loc == -1) return false;
        unsafe { gl.UniformMatrix4(loc, 1, false, (float*)&projectionMatrix); }

        loc = gl.GetUniformLocation(m_shaderProgram, "shaderTexture");
        if (loc == -1) return false;
        gl.Uniform1(loc, 0);

        loc = gl.GetUniformLocation(m_shaderProgram, "screenWidth");
        if (loc == -1) return false;
        gl.Uniform1(loc, screenWidth);

        loc = gl.GetUniformLocation(m_shaderProgram, "screenHeight");
        if (loc == -1) return false;
        gl.Uniform1(loc, screenHeight);

        loc = gl.GetUniformLocation(m_shaderProgram, "blurType");
        if (loc == -1) return false;
        gl.Uniform1(loc, blurType);

        return true;
    }

    private bool InitializeShader(GL4 OpenGL, string vsFilename, string psFilename)
    {
        var gl = OpenGL.Gl;
        string vsSrc = File.ReadAllText(vsFilename);
        string psSrc = File.ReadAllText(psFilename);

        m_vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(m_vertexShader, vsSrc);
        gl.CompileShader(m_vertexShader);
        gl.GetShader(m_vertexShader, ShaderParameterName.CompileStatus, out int s);
        if (s != 1) { Console.WriteLine($"Compile {vsFilename}: {gl.GetShaderInfoLog(m_vertexShader)}"); return false; }

        m_fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(m_fragmentShader, psSrc);
        gl.CompileShader(m_fragmentShader);
        gl.GetShader(m_fragmentShader, ShaderParameterName.CompileStatus, out s);
        if (s != 1) { Console.WriteLine($"Compile {psFilename}: {gl.GetShaderInfoLog(m_fragmentShader)}"); return false; }

        m_shaderProgram = gl.CreateProgram();
        gl.AttachShader(m_shaderProgram, m_vertexShader);
        gl.AttachShader(m_shaderProgram, m_fragmentShader);
        gl.BindAttribLocation(m_shaderProgram, 0, "inputPosition");
        gl.BindAttribLocation(m_shaderProgram, 1, "inputTexCoord");
        gl.LinkProgram(m_shaderProgram);
        gl.GetProgram(m_shaderProgram, ProgramPropertyARB.LinkStatus, out int ls);
        if (ls != 1) { Console.WriteLine($"Link: {gl.GetProgramInfoLog(m_shaderProgram)}"); return false; }
        return true;
    }
}
