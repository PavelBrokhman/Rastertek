using RastertekCS.OpenGL.Tutorial36.System;
using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial36.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_Model;
    private TextureShader m_TextureShader;
    private RenderTexture m_RenderTexture;
    private OrthoWindow m_FullScreenWindow;
    private Blur m_Blur;
    private BlurShader m_BlurShader;
    private float m_rotation;
    private int m_screenWidth, m_screenHeight;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;
        m_screenWidth = screenWidth;
        m_screenHeight = screenHeight;
        m_rotation = 360.0f;

        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -10);
        m_Camera.Render();
        m_Camera.RenderBaseViewMatrix();

        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(OpenGL)) return false;

        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Models/cube.txt", "Data/stone01.tga")) return false;

        m_RenderTexture = new RenderTexture();
        if (!m_RenderTexture.Initialize(OpenGL, screenWidth, screenHeight,
            SystemConfiguration.ScreenNear, SystemConfiguration.ScreenDepth)) return false;

        m_FullScreenWindow = new OrthoWindow();
        if (!m_FullScreenWindow.Initialize(OpenGL, screenWidth, screenHeight)) return false;

        int downSampleWidth = screenWidth / 2;
        int downSampleHeight = screenHeight / 2;

        m_Blur = new Blur();
        if (!m_Blur.Initialize(OpenGL, downSampleWidth, downSampleHeight,
            SystemConfiguration.ScreenNear, SystemConfiguration.ScreenDepth,
            screenWidth, screenHeight)) return false;

        m_BlurShader = new BlurShader();
        if (!m_BlurShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_BlurShader?.Shutdown(m_OpenGL); m_BlurShader = null;
        m_Blur?.Shutdown(m_OpenGL); m_Blur = null;
        m_FullScreenWindow?.Shutdown(m_OpenGL); m_FullScreenWindow = null;
        m_RenderTexture?.Shutdown(m_OpenGL); m_RenderTexture = null;
        m_Model?.Shutdown(m_OpenGL); m_Model = null;
        m_TextureShader?.Shutdown(m_OpenGL); m_TextureShader = null;
        m_Camera = null;
        m_OpenGL = null;
    }

    public bool Frame()
    {
        m_rotation -= 0.0174532925f * 1.0f;
        if (m_rotation <= 0.0f)
            m_rotation += 360.0f;

        if (!RenderSceneToTexture(m_rotation)) return false;
        if (!m_Blur.BlurTexture(m_RenderTexture, m_OpenGL, m_Camera, m_TextureShader, m_BlurShader)) return true;
        if (!Render()) return false;

        return true;
    }

    private bool RenderSceneToTexture(float rotation)
    {
        m_RenderTexture.SetRenderTarget(m_OpenGL);
        m_RenderTexture.ClearRenderTarget(m_OpenGL, 0, 0, 0, 1);

        var worldMatrix = Matrix4X4.CreateRotationY<float>(rotation);
        var viewMatrix = m_Camera.GetViewMatrix();
        var projectionMatrix = m_RenderTexture.GetProjectionMatrix();

        if (!m_TextureShader.SetShaderParameters(m_OpenGL, worldMatrix, viewMatrix, projectionMatrix))
            return false;

        m_Model.SetTexture(m_OpenGL, 0);
        m_Model.Render(m_OpenGL);

        m_OpenGL.SetBackBufferRenderTarget();
        m_OpenGL.ResetViewport();

        return true;
    }

    private bool Render()
    {
        m_OpenGL.BeginScene(0, 0, 0, 1);
        m_OpenGL.TurnZBufferOff();

        var worldMatrix = m_OpenGL.GetWorldMatrix();
        var baseViewMatrix = m_Camera.GetBaseViewMatrix();
        var orthoMatrix = m_OpenGL.GetOrthoMatrix();

        if (!m_TextureShader.SetShaderParameters(m_OpenGL, worldMatrix, baseViewMatrix, orthoMatrix))
            return false;

        m_RenderTexture.SetTexture(m_OpenGL, 0);
        m_FullScreenWindow.Render(m_OpenGL);

        m_OpenGL.TurnZBufferOn();
        m_OpenGL.EndScene();

        return true;
    }
}
