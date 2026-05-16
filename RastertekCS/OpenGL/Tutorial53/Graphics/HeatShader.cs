using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial53.Graphics;

public class HeatShader
{
    private uint m_vertexShader, m_fragmentShader, m_shaderProgram;

    public bool Initialize(GL4 OpenGL) => InitializeShader(OpenGL, "Shaders/Heat.vs", "Shaders/Heat.ps");

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DetachShader(m_shaderProgram, m_vertexShader); gl.DetachShader(m_shaderProgram, m_fragmentShader);
        gl.DeleteShader(m_vertexShader); gl.DeleteShader(m_fragmentShader); gl.DeleteProgram(m_shaderProgram);
    }

    public unsafe bool SetShaderParameters(GL4 OpenGL, Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        float emissiveMultiplier, float frameTime, float[] scrollSpeeds, float[] scales,
        float[] distortion1, float[] distortion2, float[] distortion3)
    {
        var gl = OpenGL.Gl;
        gl.UseProgram(m_shaderProgram);
        var tpWorld = GL4.MatrixTranspose(world);
        var tpView = GL4.MatrixTranspose(view);
        var tpProj = GL4.MatrixTranspose(projection);
        int loc = gl.GetUniformLocation(m_shaderProgram, "worldMatrix"); if (loc == -1) Console.WriteLine($"warn: uniform not found");
        gl.UniformMatrix4(loc, 1, false, (float*)&tpWorld);
        loc = gl.GetUniformLocation(m_shaderProgram, "viewMatrix"); if (loc == -1) Console.WriteLine($"warn: uniform not found");
        gl.UniformMatrix4(loc, 1, false, (float*)&tpView);
        loc = gl.GetUniformLocation(m_shaderProgram, "projectionMatrix"); if (loc == -1) Console.WriteLine($"warn: uniform not found");
        gl.UniformMatrix4(loc, 1, false, (float*)&tpProj);
        loc = gl.GetUniformLocation(m_shaderProgram, "frameTime"); if (loc == -1) Console.WriteLine($"warn: uniform not found"); gl.Uniform1(loc, frameTime);
        loc = gl.GetUniformLocation(m_shaderProgram, "scrollSpeeds"); if (loc == -1) Console.WriteLine($"warn: uniform not found");
        fixed (float* p = scrollSpeeds) gl.Uniform3(loc, 1, p);
        loc = gl.GetUniformLocation(m_shaderProgram, "scales"); if (loc == -1) Console.WriteLine($"warn: uniform not found");
        fixed (float* p = scales) gl.Uniform3(loc, 1, p);
        loc = gl.GetUniformLocation(m_shaderProgram, "colorTexture"); if (loc == -1) Console.WriteLine($"warn: uniform not found"); gl.Uniform1(loc, 0);
        loc = gl.GetUniformLocation(m_shaderProgram, "glowTexture"); if (loc == -1) Console.WriteLine($"warn: uniform not found"); gl.Uniform1(loc, 1);
        loc = gl.GetUniformLocation(m_shaderProgram, "noiseTexture"); if (loc == -1) Console.WriteLine($"warn: uniform not found"); gl.Uniform1(loc, 2);
        loc = gl.GetUniformLocation(m_shaderProgram, "emissiveMultiplier"); if (loc == -1) Console.WriteLine($"warn: uniform not found"); gl.Uniform1(loc, emissiveMultiplier);
        loc = gl.GetUniformLocation(m_shaderProgram, "distortion1"); if (loc == -1) Console.WriteLine($"warn: uniform not found");
        fixed (float* p = distortion1) gl.Uniform2(loc, 1, p);
        loc = gl.GetUniformLocation(m_shaderProgram, "distortion2"); if (loc == -1) Console.WriteLine($"warn: uniform not found");
        fixed (float* p = distortion2) gl.Uniform2(loc, 1, p);
        loc = gl.GetUniformLocation(m_shaderProgram, "distortion3"); if (loc == -1) Console.WriteLine($"warn: uniform not found");
        fixed (float* p = distortion3) gl.Uniform2(loc, 1, p);
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
