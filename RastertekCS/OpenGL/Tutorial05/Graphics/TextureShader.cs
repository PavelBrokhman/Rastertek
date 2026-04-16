using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial05.Graphics;

public class TextureShader
{
    private uint m_vertexShader;
    private uint m_fragmentShader;
    private uint m_shaderProgram;

    public bool Initialize(GL4 OpenGL)
    {
        return InitializeShader(OpenGL, "Shaders/Texture.vs", "Shaders/Texture.ps");
    }

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

    public bool SetShaderParameters(
        GL4 OpenGL,
        Matrix4X4<float> worldMatrix,
        Matrix4X4<float> viewMatrix,
        Matrix4X4<float> projectionMatrix,
        int textureUnit
    )
    {
        var gl = OpenGL.Gl;

        int location = gl.GetUniformLocation(m_shaderProgram, "worldMatrix");
        if (location == -1)
            return false;
        unsafe
        {
            gl.UniformMatrix4(location, 1, false, (float*)&worldMatrix);
        }

        location = gl.GetUniformLocation(m_shaderProgram, "viewMatrix");
        if (location == -1)
            return false;
        unsafe
        {
            gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
        }

        location = gl.GetUniformLocation(m_shaderProgram, "projectionMatrix");
        if (location == -1)
            return false;
        unsafe
        {
            gl.UniformMatrix4(location, 1, false, (float*)&projectionMatrix);
        }

        // Привязываем sampler2D к нужному texture unit.
        location = gl.GetUniformLocation(m_shaderProgram, "shaderTexture");
        if (location == -1)
            return false;
        gl.Uniform1(location, textureUnit);

        return true;
    }

    private bool InitializeShader(GL4 OpenGL, string vsFilename, string psFilename)
    {
        var gl = OpenGL.Gl;

        string vsSource = File.ReadAllText(vsFilename);
        string psSource = File.ReadAllText(psFilename);

        m_vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(m_vertexShader, vsSource);
        gl.CompileShader(m_vertexShader);
        if (!CheckShaderCompile(gl, m_vertexShader, vsFilename))
            return false;

        m_fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(m_fragmentShader, psSource);
        gl.CompileShader(m_fragmentShader);
        if (!CheckShaderCompile(gl, m_fragmentShader, psFilename))
            return false;

        m_shaderProgram = gl.CreateProgram();
        gl.AttachShader(m_shaderProgram, m_vertexShader);
        gl.AttachShader(m_shaderProgram, m_fragmentShader);

        gl.BindAttribLocation(m_shaderProgram, 0, "inputPosition");
        gl.BindAttribLocation(m_shaderProgram, 1, "inputTexCoord");

        gl.LinkProgram(m_shaderProgram);
        gl.GetProgram(m_shaderProgram, ProgramPropertyARB.LinkStatus, out int linkStatus);
        if (linkStatus != 1)
        {
            string log = gl.GetProgramInfoLog(m_shaderProgram);
            global::System.Console.WriteLine($"Ошибка линковки шейдеров: {log}");
            return false;
        }
        return true;
    }

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
