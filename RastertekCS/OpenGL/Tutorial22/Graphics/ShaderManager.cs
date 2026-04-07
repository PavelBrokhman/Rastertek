using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial22.Graphics;

public class ShaderManager
{
    private TextureShader m_TextureShader;
    private LightShader m_LightShader;
    private NormalMapShader m_NormalMapShader;

    public bool Initialize(GL4 OpenGL)
    {
        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(OpenGL)) return false;

        m_LightShader = new LightShader();
        if (!m_LightShader.Initialize(OpenGL)) return false;

        m_NormalMapShader = new NormalMapShader();
        if (!m_NormalMapShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        m_NormalMapShader?.Shutdown(OpenGL); m_NormalMapShader = null;
        m_LightShader?.Shutdown(OpenGL); m_LightShader = null;
        m_TextureShader?.Shutdown(OpenGL); m_TextureShader = null;
    }

    public bool RenderTextureShader(GL4 OpenGL,
        Matrix4X4<float> worldMatrix, Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix,
        int textureUnit)
    {
        m_TextureShader.SetShader(OpenGL);
        return m_TextureShader.SetShaderParameters(OpenGL, worldMatrix, viewMatrix, projectionMatrix, textureUnit);
    }

    public bool RenderLightShader(GL4 OpenGL,
        Matrix4X4<float> worldMatrix, Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix,
        float[] lightDirection, float[] diffuseLightColor,
        int textureUnit)
    {
        m_LightShader.SetShader(OpenGL);
        return m_LightShader.SetShaderParameters(OpenGL, worldMatrix, viewMatrix, projectionMatrix,
            lightDirection, diffuseLightColor, textureUnit);
    }

    public bool RenderNormalMapShader(GL4 OpenGL,
        Matrix4X4<float> worldMatrix, Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix,
        float[] lightDirection, float[] diffuseLightColor,
        int textureUnit1, int textureUnit2)
    {
        m_NormalMapShader.SetShader(OpenGL);
        return m_NormalMapShader.SetShaderParameters(OpenGL, worldMatrix, viewMatrix, projectionMatrix,
            lightDirection, diffuseLightColor, textureUnit1, textureUnit2);
    }
}
