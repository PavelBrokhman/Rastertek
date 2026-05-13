using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial49.Graphics;

public class ColorShader
{
    private uint _vertexShader, _hullShader, _domainShader, _fragmentShader, _shaderProgram;

    public bool Initialize(GL4 OpenGL)
        => InitializeShader(OpenGL, "Shaders/Color.vs", "Shaders/Color.hs", "Shaders/Color.ds", "Shaders/Color.ps");

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        gl.DetachShader(_shaderProgram, _vertexShader);
        gl.DetachShader(_shaderProgram, _hullShader);
        gl.DetachShader(_shaderProgram, _domainShader);
        gl.DetachShader(_shaderProgram, _fragmentShader);
        gl.DeleteShader(_vertexShader);
        gl.DeleteShader(_hullShader);
        gl.DeleteShader(_domainShader);
        gl.DeleteShader(_fragmentShader);
        gl.DeleteProgram(_shaderProgram);
    }

    public unsafe bool SetShaderParameters(GL4 OpenGL,
        Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        float tessellationAmount)
    {
        var gl = OpenGL.Driver;
        // Transpose the matrices so the row-major Silk.NET storage is uploaded in the
        // orientation the Rastertek `vec * mat` GLSL convention expects.
        var tpWorld = GL4.MatrixTranspose(world);
        var tpView = GL4.MatrixTranspose(view);
        var tpProj = GL4.MatrixTranspose(projection);
        gl.UseProgram(_shaderProgram);
        int loc;
        loc = gl.GetUniformLocation(_shaderProgram, "worldMatrix"); if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&tpWorld);
        loc = gl.GetUniformLocation(_shaderProgram, "viewMatrix"); if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&tpView);
        loc = gl.GetUniformLocation(_shaderProgram, "projectionMatrix"); if (loc == -1) return false;
        gl.UniformMatrix4(loc, 1, false, (float*)&tpProj);
        loc = gl.GetUniformLocation(_shaderProgram, "tessellationAmount"); if (loc == -1) return false;
        gl.Uniform1(loc, tessellationAmount);
        return true;
    }

    private bool InitializeShader(GL4 OpenGL, string vsFilename, string hsFilename, string dsFilename, string psFilename)
    {
        var gl = OpenGL.Driver;

        _vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(_vertexShader, File.ReadAllText(vsFilename));
        gl.CompileShader(_vertexShader);
        gl.GetShader(_vertexShader, ShaderParameterName.CompileStatus, out int s);
        if (s != 1) { Console.WriteLine($"VS: {gl.GetShaderInfoLog(_vertexShader)}"); return false; }

        _hullShader = gl.CreateShader(ShaderType.TessControlShader);
        gl.ShaderSource(_hullShader, File.ReadAllText(hsFilename));
        gl.CompileShader(_hullShader);
        gl.GetShader(_hullShader, ShaderParameterName.CompileStatus, out s);
        if (s != 1) { Console.WriteLine($"HS: {gl.GetShaderInfoLog(_hullShader)}"); return false; }

        _domainShader = gl.CreateShader(ShaderType.TessEvaluationShader);
        gl.ShaderSource(_domainShader, File.ReadAllText(dsFilename));
        gl.CompileShader(_domainShader);
        gl.GetShader(_domainShader, ShaderParameterName.CompileStatus, out s);
        if (s != 1) { Console.WriteLine($"DS: {gl.GetShaderInfoLog(_domainShader)}"); return false; }

        _fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(_fragmentShader, File.ReadAllText(psFilename));
        gl.CompileShader(_fragmentShader);
        gl.GetShader(_fragmentShader, ShaderParameterName.CompileStatus, out s);
        if (s != 1) { Console.WriteLine($"PS: {gl.GetShaderInfoLog(_fragmentShader)}"); return false; }

        _shaderProgram = gl.CreateProgram();
        gl.AttachShader(_shaderProgram, _vertexShader);
        gl.AttachShader(_shaderProgram, _hullShader);
        gl.AttachShader(_shaderProgram, _domainShader);
        gl.AttachShader(_shaderProgram, _fragmentShader);
        gl.BindAttribLocation(_shaderProgram, 0, "inputPosition");
        gl.BindAttribLocation(_shaderProgram, 1, "inputColor");
        gl.LinkProgram(_shaderProgram);
        gl.GetProgram(_shaderProgram, ProgramPropertyARB.LinkStatus, out int ls);
        if (ls != 1) { Console.WriteLine($"Link: {gl.GetProgramInfoLog(_shaderProgram)}"); return false; }
        return true;
    }
}
