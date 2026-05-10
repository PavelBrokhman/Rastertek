using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial06.Graphics;

public class LightShader
{
    private uint _vertexShader;
    private uint _fragmentShader;
    private uint _shaderProgram;

    public bool Initialize(GL4 OpenGL)
    {
        return InitializeShader(OpenGL, "Shaders/Light.vs", "Shaders/Light.ps");
    }

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
        Matrix4X4<float> worldMatrix,
        Matrix4X4<float> viewMatrix,
        Matrix4X4<float> projectionMatrix,
        int textureUnit,
        float[] lightDirection,
        float[] diffuseLightColor
    )
    {
        var gl = OpenGL.Driver;

        // C++ lightshaderclass.cpp: transpose matrices before glUniformMatrix4fv(false).
        var tpWorld = Transpose(worldMatrix);
        var tpView = Transpose(viewMatrix);
        var tpProj = Transpose(projectionMatrix);

        int loc = gl.GetUniformLocation(_shaderProgram, "worldMatrix");
        if (loc == -1)
            return false;
        unsafe
        {
            gl.UniformMatrix4(loc, 1, false, (float*)&tpWorld);
        }

        loc = gl.GetUniformLocation(_shaderProgram, "viewMatrix");
        if (loc == -1)
            return false;
        unsafe
        {
            gl.UniformMatrix4(loc, 1, false, (float*)&tpView);
        }

        loc = gl.GetUniformLocation(_shaderProgram, "projectionMatrix");
        if (loc == -1)
            return false;
        unsafe
        {
            gl.UniformMatrix4(loc, 1, false, (float*)&tpProj);
        }

        loc = gl.GetUniformLocation(_shaderProgram, "shaderTexture");
        if (loc == -1)
            return false;
        gl.Uniform1(loc, textureUnit);

        loc = gl.GetUniformLocation(_shaderProgram, "lightDirection");
        if (loc == -1)
            return false;
        unsafe
        {
            fixed (float* p = lightDirection)
                gl.Uniform3(loc, 1, p);
        }

        loc = gl.GetUniformLocation(_shaderProgram, "diffuseLightColor");
        if (loc == -1)
            return false;
        unsafe
        {
            fixed (float* p = diffuseLightColor)
                gl.Uniform4(loc, 1, p);
        }

        return true;
    }

    private bool InitializeShader(GL4 OpenGL, string vsFilename, string psFilename)
    {
        var gl = OpenGL.Driver;
        string vsSource = File.ReadAllText(vsFilename);
        string psSource = File.ReadAllText(psFilename);

        _vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(_vertexShader, vsSource);
        gl.CompileShader(_vertexShader);
        if (!CheckShaderCompile(gl, _vertexShader, vsFilename))
            return false;

        _fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(_fragmentShader, psSource);
        gl.CompileShader(_fragmentShader);
        if (!CheckShaderCompile(gl, _fragmentShader, psFilename))
            return false;

        _shaderProgram = gl.CreateProgram();
        gl.AttachShader(_shaderProgram, _vertexShader);
        gl.AttachShader(_shaderProgram, _fragmentShader);

        gl.BindAttribLocation(_shaderProgram, 0, "inputPosition");
        gl.BindAttribLocation(_shaderProgram, 1, "inputTexCoord");
        gl.BindAttribLocation(_shaderProgram, 2, "inputNormal");

        gl.LinkProgram(_shaderProgram);
        gl.GetProgram(_shaderProgram, ProgramPropertyARB.LinkStatus, out int linkStatus);
        if (linkStatus != 1)
        {
            string log = gl.GetProgramInfoLog(_shaderProgram);
            global::System.Console.WriteLine($"Ошибка линковки шейдеров: {log}");
            return false;
        }
        return true;
    }

    private static Matrix4X4<float> Transpose(Matrix4X4<float> m) =>
        new(
            m.M11, m.M21, m.M31, m.M41,
            m.M12, m.M22, m.M32, m.M42,
            m.M13, m.M23, m.M33, m.M43,
            m.M14, m.M24, m.M34, m.M44
        );

    private static bool CheckShaderCompile(GL gl, uint shader, string filename)
    {
        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
        if (status != 1)
        {
            string log = gl.GetShaderInfoLog(shader);
            global::System.Console.WriteLine($"Ошибка компиляции {filename}: {log}");
            return false;
        }
        return true;
    }
}
