using Silk.NET.Maths;
using RastertekCS.OpenGL.Tutorial30.System;

namespace RastertekCS.OpenGL.Tutorial30.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_CubeModel, m_FloorModel;
    private TextureShader m_TextureShader;
    private ReflectionShader m_ReflectionShader;
    private RenderTexture m_RenderTexture;
    private float m_rotation = 360.0f;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;
        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -10);
        m_Camera.Render();

        m_CubeModel = new Model();
        if (!m_CubeModel.Initialize(OpenGL, "Models/cubeGL.txt", "Data/stone01.tga", true)) return false;

        m_FloorModel = new Model();
        if (!m_FloorModel.Initialize(OpenGL, "Models/floor.txt", "Data/blue01.tga", true)) return false;

        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(OpenGL)) return false;

        m_ReflectionShader = new ReflectionShader();
        if (!m_ReflectionShader.Initialize(OpenGL)) return false;

        m_RenderTexture = new RenderTexture();
        if (!m_RenderTexture.Initialize(OpenGL, screenWidth, screenHeight,
            SystemConfiguration.ScreenNear, SystemConfiguration.ScreenDepth)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_RenderTexture?.Shutdown(m_OpenGL); m_RenderTexture = null;
        m_ReflectionShader?.Shutdown(m_OpenGL); m_ReflectionShader = null;
        m_TextureShader?.Shutdown(m_OpenGL); m_TextureShader = null;
        m_FloorModel?.Shutdown(m_OpenGL); m_FloorModel = null;
        m_CubeModel?.Shutdown(m_OpenGL); m_CubeModel = null;
        m_Camera = null; m_OpenGL = null;
    }

    public bool Frame()
    {
        m_rotation += 0.0174532925f * 1.0f;
        if (m_rotation > MathF.Tau) m_rotation -= MathF.Tau;
        if (!RenderReflectionToTexture(m_rotation)) return false;
        return Render(m_rotation);
    }

    private bool RenderReflectionToTexture(float rotation)
    {
        m_RenderTexture.SetRenderTarget(m_OpenGL);
        m_RenderTexture.ClearRenderTarget(m_OpenGL, 0, 0, 0, 1);

        m_Camera.RenderReflection(-1.5f);
        var reflectionView = m_Camera.GetReflectionViewMatrix();
        var world = Matrix4X4.CreateRotationY<float>(rotation);
        var projection = m_RenderTexture.GetProjectionMatrix();

        m_TextureShader.SetShaderParameters(m_OpenGL, world, reflectionView, projection);
        m_CubeModel.SetTexture(m_OpenGL, 0);
        m_CubeModel.Render(m_OpenGL);

        m_OpenGL.SetBackBufferRenderTarget();
        m_OpenGL.ResetViewport();
        return true;
    }

    private bool Render(float rotation)
    {
        m_OpenGL.BeginScene(0, 0, 0, 1);

        var world = Matrix4X4.CreateRotationY<float>(rotation);
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        m_TextureShader.SetShaderParameters(m_OpenGL, world, view, projection);
        m_CubeModel.SetTexture(m_OpenGL, 0);
        m_CubeModel.Render(m_OpenGL);

        world = Matrix4X4.CreateTranslation<float>(0, -1.5f, 0);
        var reflectionView = m_Camera.GetReflectionViewMatrix();

        m_ReflectionShader.SetShaderParameters(m_OpenGL, world, view, projection, reflectionView);
        m_RenderTexture.SetTexture(m_OpenGL, 1);
        m_FloorModel.SetTexture(m_OpenGL, 0);
        m_FloorModel.Render(m_OpenGL);

        m_OpenGL.EndScene();
        return true;
    }
}
