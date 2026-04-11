using RastertekCS.OpenGL.Tutorial37.System;
using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial37.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_Model;
    private TextureShader m_TextureShader;
    private RenderTexture m_RenderTexture;
    private OrthoWindow m_FullScreenWindow;
    private FadeShader m_FadeShader;
    private float m_rotation;
    private float m_accumulatedTime;
    private float m_fadeInTime;

    public bool Initialize(GL4 OpenGL, int sw, int sh)
    {
        m_OpenGL = OpenGL;
        m_rotation = 360.0f;
        m_accumulatedTime = 0;
        m_fadeInTime = 5.0f;

        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -10);
        m_Camera.Render();
        m_Camera.RenderBaseViewMatrix();

        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(OpenGL)) return false;

        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Models/cubeGL.txt", "Data/stone01.tga")) return false;

        m_RenderTexture = new RenderTexture();
        if (!m_RenderTexture.Initialize(OpenGL, sw, sh, SystemConfiguration.ScreenNear, SystemConfiguration.ScreenDepth)) return false;

        m_FullScreenWindow = new OrthoWindow();
        if (!m_FullScreenWindow.Initialize(OpenGL, sw, sh)) return false;

        m_FadeShader = new FadeShader();
        if (!m_FadeShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_FadeShader?.Shutdown(m_OpenGL); m_FadeShader = null;
        m_FullScreenWindow?.Shutdown(m_OpenGL); m_FullScreenWindow = null;
        m_RenderTexture?.Shutdown(m_OpenGL); m_RenderTexture = null;
        m_Model?.Shutdown(m_OpenGL); m_Model = null;
        m_TextureShader?.Shutdown(m_OpenGL); m_TextureShader = null;
        m_Camera = null; m_OpenGL = null;
    }

    public bool Frame(float dt)
    {
        m_rotation -= 0.0174532925f * 1.0f;
        if (m_rotation <= 0.0f) m_rotation += 360.0f;

        m_accumulatedTime += dt;
        float fadePercentage = m_accumulatedTime < m_fadeInTime ? m_accumulatedTime / m_fadeInTime : 1.0f;

        if (!RenderSceneToTexture(m_rotation)) return false;
        if (!Render(fadePercentage)) return false;
        return true;
    }

    private bool RenderSceneToTexture(float rotation)
    {
        m_RenderTexture.SetRenderTarget(m_OpenGL);
        m_RenderTexture.ClearRenderTarget(m_OpenGL, 0, 0, 0, 1);
        var world = Matrix4X4.CreateRotationY<float>(rotation);
        var view = m_Camera.GetViewMatrix();
        var proj = m_RenderTexture.GetProjectionMatrix();
        if (!m_TextureShader.SetShaderParameters(m_OpenGL, world, view, proj)) return false;
        m_Model.SetTexture(m_OpenGL, 0);
        m_Model.Render(m_OpenGL);
        m_OpenGL.SetBackBufferRenderTarget();
        m_OpenGL.ResetViewport();
        return true;
    }

    private bool Render(float fadeAmount)
    {
        m_OpenGL.BeginScene(0, 0, 0, 1);
        m_OpenGL.TurnZBufferOff();
        var world = m_OpenGL.GetWorldMatrix();
        var baseView = m_Camera.GetBaseViewMatrix();
        var ortho = m_OpenGL.GetOrthoMatrix();
        if (!m_FadeShader.SetShaderParameters(m_OpenGL, world, baseView, ortho, fadeAmount)) return false;
        m_RenderTexture.SetTexture(m_OpenGL, 0);
        m_FullScreenWindow.Render(m_OpenGL);
        m_OpenGL.TurnZBufferOn();
        m_OpenGL.EndScene();
        return true;
    }
}
