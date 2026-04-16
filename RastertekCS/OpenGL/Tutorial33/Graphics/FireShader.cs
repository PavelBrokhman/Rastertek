using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial33.Graphics;

public class FireShader
{
    private uint _vertexShader,
        _fragmentShader,
        _shaderProgram;

    public bool Initialize(GL4 gl) => Init(gl, "Shaders/fire.vs", "Shaders/fire.ps");

    public void Shutdown(GL4 gl)
    {
        var g = gl.Gl;
        g.DetachShader(_shaderProgram, _vertexShader);
        g.DetachShader(_shaderProgram, _fragmentShader);
        g.DeleteShader(_vertexShader);
        g.DeleteShader(_fragmentShader);
        g.DeleteProgram(_shaderProgram);
    }

    public unsafe bool SetShaderParameters(
        GL4 gl,
        Matrix4X4<float> w,
        Matrix4X4<float> v,
        Matrix4X4<float> p,
        float frameTime,
        float[] scrollSpeeds,
        float[] scales,
        float[] d1,
        float[] d2,
        float[] d3,
        float dScale,
        float dBias
    )
    {
        var g = gl.Gl;
        g.UseProgram(_shaderProgram);
        int loc;
        loc = g.GetUniformLocation(_shaderProgram, "worldMatrix");
        if (loc >= 0)
            g.UniformMatrix4(loc, 1, false, (float*)&w);
        loc = g.GetUniformLocation(_shaderProgram, "viewMatrix");
        if (loc >= 0)
            g.UniformMatrix4(loc, 1, false, (float*)&v);
        loc = g.GetUniformLocation(_shaderProgram, "projectionMatrix");
        if (loc >= 0)
            g.UniformMatrix4(loc, 1, false, (float*)&p);
        loc = g.GetUniformLocation(_shaderProgram, "frameTime");
        if (loc >= 0)
            g.Uniform1(loc, frameTime);
        loc = g.GetUniformLocation(_shaderProgram, "scrollSpeeds");
        if (loc >= 0)
            fixed (float* pp = scrollSpeeds)
                g.Uniform3(loc, 1, pp);
        loc = g.GetUniformLocation(_shaderProgram, "scales");
        if (loc >= 0)
            fixed (float* pp = scales)
                g.Uniform3(loc, 1, pp);
        loc = g.GetUniformLocation(_shaderProgram, "fireTexture");
        if (loc >= 0)
            g.Uniform1(loc, 0);
        loc = g.GetUniformLocation(_shaderProgram, "noiseTexture");
        if (loc >= 0)
            g.Uniform1(loc, 1);
        loc = g.GetUniformLocation(_shaderProgram, "alphaTexture");
        if (loc >= 0)
            g.Uniform1(loc, 2);
        loc = g.GetUniformLocation(_shaderProgram, "distortion1");
        if (loc >= 0)
            fixed (float* pp = d1)
                g.Uniform2(loc, 1, pp);
        loc = g.GetUniformLocation(_shaderProgram, "distortion2");
        if (loc >= 0)
            fixed (float* pp = d2)
                g.Uniform2(loc, 1, pp);
        loc = g.GetUniformLocation(_shaderProgram, "distortion3");
        if (loc >= 0)
            fixed (float* pp = d3)
                g.Uniform2(loc, 1, pp);
        loc = g.GetUniformLocation(_shaderProgram, "distortionScale");
        if (loc >= 0)
            g.Uniform1(loc, dScale);
        loc = g.GetUniformLocation(_shaderProgram, "distortionBias");
        if (loc >= 0)
            g.Uniform1(loc, dBias);
        return true;
    }

    bool Init(GL4 gl, string vsf, string psf)
    {
        var g = gl.Gl;
        _vertexShader = g.CreateShader(ShaderType.VertexShader);
        g.ShaderSource(_vertexShader, File.ReadAllText(vsf));
        g.CompileShader(_vertexShader);
        g.GetShader(_vertexShader, ShaderParameterName.CompileStatus, out int s);
        if (s != 1)
        {
            Console.WriteLine($"VS: {g.GetShaderInfoLog(_vertexShader)}");
            return false;
        }
        _fragmentShader = g.CreateShader(ShaderType.FragmentShader);
        g.ShaderSource(_fragmentShader, File.ReadAllText(psf));
        g.CompileShader(_fragmentShader);
        g.GetShader(_fragmentShader, ShaderParameterName.CompileStatus, out s);
        if (s != 1)
        {
            Console.WriteLine($"PS: {g.GetShaderInfoLog(_fragmentShader)}");
            return false;
        }
        _shaderProgram = g.CreateProgram();
        g.AttachShader(_shaderProgram, _vertexShader);
        g.AttachShader(_shaderProgram, _fragmentShader);
        g.BindAttribLocation(_shaderProgram, 0, "inputPosition");
        g.BindAttribLocation(_shaderProgram, 1, "inputTexCoord");
        g.BindAttribLocation(_shaderProgram, 2, "inputNormal");
        g.LinkProgram(_shaderProgram);
        g.GetProgram(_shaderProgram, ProgramPropertyARB.LinkStatus, out int ls);
        if (ls != 1)
        {
            Console.WriteLine($"Link: {g.GetProgramInfoLog(_shaderProgram)}");
            return false;
        }
        return true;
    }
}
