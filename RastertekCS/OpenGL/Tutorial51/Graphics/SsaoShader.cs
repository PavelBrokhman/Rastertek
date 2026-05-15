using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial51.Graphics;

public class SsaoShader
{
    private uint m_vertexShader, m_fragmentShader, m_shaderProgram;

    public bool Initialize(GL4 OpenGL) => InitializeShader(OpenGL, "Shaders/Ssao.vs", "Shaders/Ssao.ps");

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DetachShader(m_shaderProgram, m_vertexShader); gl.DetachShader(m_shaderProgram, m_fragmentShader);
        gl.DeleteShader(m_vertexShader); gl.DeleteShader(m_fragmentShader); gl.DeleteProgram(m_shaderProgram);
    }

    public unsafe bool SetShaderParameters(GL4 OpenGL, Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        float screenWidth, float screenHeight, float randomTextureSize, float sampleRadius, float ssaoScale, float ssaoBias, float ssaoIntensity)
    {
        var gl = OpenGL.Gl;
        gl.UseProgram(m_shaderProgram);
        var tpWorld = GL4.MatrixTranspose(world);
        var tpView = GL4.MatrixTranspose(view);
        var tpProj = GL4.MatrixTranspose(projection);
        int loc = gl.GetUniformLocation(m_shaderProgram, "worldMatrix"); if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&tpWorld);
        loc = gl.GetUniformLocation(m_shaderProgram, "viewMatrix"); if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&tpView);
        loc = gl.GetUniformLocation(m_shaderProgram, "projectionMatrix"); if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&tpProj);
        loc = gl.GetUniformLocation(m_shaderProgram, "positionTexture"); if (loc == -1) return false; gl.Uniform1(loc, 0);
        loc = gl.GetUniformLocation(m_shaderProgram, "normalTexture"); if (loc == -1) return false; gl.Uniform1(loc, 1);
        loc = gl.GetUniformLocation(m_shaderProgram, "randomTexture"); if (loc == -1) return false; gl.Uniform1(loc, 2);
        loc = gl.GetUniformLocation(m_shaderProgram, "screenWidth"); if (loc == -1) return false; gl.Uniform1(loc, screenWidth);
        loc = gl.GetUniformLocation(m_shaderProgram, "screenHeight"); if (loc == -1) return false; gl.Uniform1(loc, screenHeight);
        loc = gl.GetUniformLocation(m_shaderProgram, "randomTextureSize"); if (loc == -1) return false; gl.Uniform1(loc, randomTextureSize);
        loc = gl.GetUniformLocation(m_shaderProgram, "sampleRadius"); if (loc == -1) return false; gl.Uniform1(loc, sampleRadius);
        loc = gl.GetUniformLocation(m_shaderProgram, "ssaoScale"); if (loc == -1) return false; gl.Uniform1(loc, ssaoScale);
        loc = gl.GetUniformLocation(m_shaderProgram, "ssaoBias"); if (loc == -1) return false; gl.Uniform1(loc, ssaoBias);
        loc = gl.GetUniformLocation(m_shaderProgram, "ssaoIntensity"); if (loc == -1) return false; gl.Uniform1(loc, ssaoIntensity);
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
