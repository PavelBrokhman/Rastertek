using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial51.Graphics;

public class LightShader
{
    private uint m_vertexShader, m_fragmentShader, m_shaderProgram;

    public bool Initialize(GL4 OpenGL) => InitializeShader(OpenGL, "Shaders/Light.vs", "Shaders/Light.ps");

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DetachShader(m_shaderProgram, m_vertexShader); gl.DetachShader(m_shaderProgram, m_fragmentShader);
        gl.DeleteShader(m_vertexShader); gl.DeleteShader(m_fragmentShader); gl.DeleteProgram(m_shaderProgram);
    }

    public unsafe bool SetShaderParameters(GL4 OpenGL, Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        Matrix4X4<float> cameraViewMatrix, float[] lightDirection)
    {
        var gl = OpenGL.Gl;
        gl.UseProgram(m_shaderProgram);
        int loc = gl.GetUniformLocation(m_shaderProgram, "worldMatrix"); if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&world);
        loc = gl.GetUniformLocation(m_shaderProgram, "viewMatrix"); if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&view);
        loc = gl.GetUniformLocation(m_shaderProgram, "projectionMatrix"); if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&projection);
        loc = gl.GetUniformLocation(m_shaderProgram, "cameraViewMatrix"); if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&cameraViewMatrix);
        loc = gl.GetUniformLocation(m_shaderProgram, "normalTexture"); if (loc == -1) return false; gl.Uniform1(loc, 0);
        loc = gl.GetUniformLocation(m_shaderProgram, "ssaoTexture"); if (loc == -1) return false; gl.Uniform1(loc, 1);
        loc = gl.GetUniformLocation(m_shaderProgram, "colorTexture"); if (loc == -1) return false; gl.Uniform1(loc, 2);
        loc = gl.GetUniformLocation(m_shaderProgram, "lightDirection"); if (loc == -1) return false;
        fixed (float* p = lightDirection) gl.Uniform3(loc, 1, p);
        return true;
    }

    private bool InitializeShader(GL4 OpenGL, string vsFile, string psFile)
    {
        var gl = OpenGL.Gl;
        m_vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(m_vertexShader, File.ReadAllText(vsFile)); gl.CompileShader(m_vertexShader);
        gl.GetShader(m_vertexShader, ShaderParameterName.CompileStatus, out int s);
        if (s != 1) { Console.WriteLine($"Compile {vsFile}: {gl.GetShaderInfoLog(m_vertexShader)}"); return false; }
        m_fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(m_fragmentShader, File.ReadAllText(psFile)); gl.CompileShader(m_fragmentShader);
        gl.GetShader(m_fragmentShader, ShaderParameterName.CompileStatus, out s);
        if (s != 1) { Console.WriteLine($"Compile {psFile}: {gl.GetShaderInfoLog(m_fragmentShader)}"); return false; }
        m_shaderProgram = gl.CreateProgram();
        gl.AttachShader(m_shaderProgram, m_vertexShader); gl.AttachShader(m_shaderProgram, m_fragmentShader);
        gl.BindAttribLocation(m_shaderProgram, 0, "inputPosition");
        gl.BindAttribLocation(m_shaderProgram, 1, "inputTexCoord");
        gl.LinkProgram(m_shaderProgram);
        gl.GetProgram(m_shaderProgram, ProgramPropertyARB.LinkStatus, out int ls);
        if (ls != 1) { Console.WriteLine($"Link: {gl.GetProgramInfoLog(m_shaderProgram)}"); return false; }
        return true;
    }
}
