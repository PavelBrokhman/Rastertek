using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial22.Graphics;

public class ShaderManager
{
    private TextureShader _textureShader;
    private LightShader _lightShader;
    private NormalMapShader _normalMapShader;

    public bool Initialize(GL4 OpenGL)
    {
        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(OpenGL))
            return false;

        _lightShader = new LightShader();
        if (!_lightShader.Initialize(OpenGL))
            return false;

        _normalMapShader = new NormalMapShader();
        if (!_normalMapShader.Initialize(OpenGL))
            return false;

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        _normalMapShader?.Shutdown(OpenGL);
        _normalMapShader = null;
        _lightShader?.Shutdown(OpenGL);
        _lightShader = null;
        _textureShader?.Shutdown(OpenGL);
        _textureShader = null;
    }

    public bool RenderTextureShader(
        GL4 OpenGL,
        Matrix4X4<float> worldMatrix,
        Matrix4X4<float> viewMatrix,
        Matrix4X4<float> projectionMatrix,
        int textureUnit
    )
    {
        _textureShader.SetShader(OpenGL);
        return _textureShader.SetShaderParameters(
            OpenGL,
            worldMatrix,
            viewMatrix,
            projectionMatrix,
            textureUnit
        );
    }

    public bool RenderLightShader(
        GL4 OpenGL,
        Matrix4X4<float> worldMatrix,
        Matrix4X4<float> viewMatrix,
        Matrix4X4<float> projectionMatrix,
        float[] lightDirection,
        float[] diffuseLightColor,
        int textureUnit
    )
    {
        _lightShader.SetShader(OpenGL);
        return _lightShader.SetShaderParameters(
            OpenGL,
            worldMatrix,
            viewMatrix,
            projectionMatrix,
            lightDirection,
            diffuseLightColor,
            textureUnit
        );
    }

    public bool RenderNormalMapShader(
        GL4 OpenGL,
        Matrix4X4<float> worldMatrix,
        Matrix4X4<float> viewMatrix,
        Matrix4X4<float> projectionMatrix,
        float[] lightDirection,
        float[] diffuseLightColor,
        int textureUnit1,
        int textureUnit2
    )
    {
        _normalMapShader.SetShader(OpenGL);
        return _normalMapShader.SetShaderParameters(
            OpenGL,
            worldMatrix,
            viewMatrix,
            projectionMatrix,
            lightDirection,
            diffuseLightColor,
            textureUnit1,
            textureUnit2
        );
    }
}
