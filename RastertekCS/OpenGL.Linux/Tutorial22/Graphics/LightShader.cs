using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial22.Graphics;

public class LightShader
{
    private uint m_vertexShader;
    private uint m_fragmentShader;
    private uint m_shaderProgram;

    public bool Initialize(GL4 OpenGL) => InitializeShader(OpenGL, "Shaders/Light.vs", "Shaders/Light.ps");

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

    public unsafe bool SetShaderParameters(GL4 OpenGL,
        Matrix4X4<float> worldMatrix, Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix,
        float[] lightDirection, float[] diffuseLightColor,
        int textureUnit)
    {
        var gl = OpenGL.Gl;
        int loc;

        loc = gl.GetUniformLocation(m_shaderProgram, "worldMatrix");
        if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&worldMatrix);

        loc = gl.GetUniformLocation(m_shaderProgram, "viewMatrix");
        if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&viewMatrix);

        loc = gl.GetUniformLocation(m_shaderProgram, "projectionMatrix");
        if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&projectionMatrix);

        loc = gl.GetUniformLocation(m_shaderProgram, "shaderTexture");
        if (loc == -1) return false;
        gl.Uniform1(loc, textureUnit);

        loc = gl.GetUniformLocation(m_shaderProgram, "lightDirection");
        if (loc == -1) return false;
        fixed (float* p = lightDirection) gl.Uniform3(loc, 1, p);

        loc = gl.GetUniformLocation(m_shaderProgram, "diffuseLightColor");
        if (loc == -1) return false;
        fixed (float* p = diffuseLightColor) gl.Uniform4(loc, 1, p);

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
        if (!CheckShaderCompile(gl, m_vertexShader, vsFilename)) return false;

        m_fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(m_fragmentShader, psSrc);
        gl.CompileShader(m_fragmentShader);
        if (!CheckShaderCompile(gl, m_fragmentShader, psFilename)) return false;

        m_shaderProgram = gl.CreateProgram();
        gl.AttachShader(m_shaderProgram, m_vertexShader);
        gl.AttachShader(m_shaderProgram, m_fragmentShader);
        gl.BindAttribLocation(m_shaderProgram, 0, "inputPosition");
        gl.BindAttribLocation(m_shaderProgram, 1, "inputTexCoord");
        gl.BindAttribLocation(m_shaderProgram, 2, "inputNormal");
        gl.LinkProgram(m_shaderProgram);
        gl.GetProgram(m_shaderProgram, ProgramPropertyARB.LinkStatus, out int ls);
        if (ls != 1) { Console.WriteLine($"Link: {gl.GetProgramInfoLog(m_shaderProgram)}"); return false; }
        return true;
    }

    private static bool CheckShaderCompile(GL gl, uint shader, string filename)
    {
        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int s);
        if (s != 1) { Console.WriteLine($"Compile {filename}: {gl.GetShaderInfoLog(shader)}"); return false; }
        return true;
    }
}
