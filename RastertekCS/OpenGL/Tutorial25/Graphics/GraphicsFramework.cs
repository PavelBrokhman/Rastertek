using Silk.NET.Maths;
using RastertekCS.OpenGL.Tutorial25.System;

namespace RastertekCS.OpenGL.Tutorial25.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_Model;
    private TextureShader m_TextureShader;
    private RenderTexture m_RenderTexture;
    private DisplayPlane m_DisplayPlane;
    private float m_rotation;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;

        // Create and initialize the camera.
        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -5.0f);
        m_Camera.Render();

        // Create and initialize the model (cube with stone texture).
        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Data/cube.txt", "Data/stone01.tga"))
            return false;

        // Create and initialize the texture shader.
        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(OpenGL))
            return false;

        // Create and initialize the render-to-texture object (256x256).
        m_RenderTexture = new RenderTexture();
        if (!m_RenderTexture.Initialize(OpenGL, 256, 256,
            SystemConfiguration.ScreenNear, SystemConfiguration.ScreenDepth))
            return false;

        // Create and initialize the display plane (1x1 quad).
        m_DisplayPlane = new DisplayPlane();
        if (!m_DisplayPlane.Initialize(OpenGL, 1.0f, 1.0f))
            return false;

        m_rotation = 360.0f;

        return true;
    }

    public void Shutdown()
    {
        m_DisplayPlane?.Shutdown(m_OpenGL); m_DisplayPlane = null;
        m_RenderTexture?.Shutdown(); m_RenderTexture = null;
        m_TextureShader?.Shutdown(m_OpenGL); m_TextureShader = null;
        m_Model?.Shutdown(m_OpenGL); m_Model = null;
        m_Camera = null;
        m_OpenGL = null;
    }

    public bool Frame()
    {
        // Update the rotation variable each frame.
        m_rotation -= 0.0174532925f * 1.0f;
        if (m_rotation <= 0.0f)
            m_rotation += 360.0f;

        // First render the scene to the render texture.
        if (!RenderSceneToTexture(m_rotation))
            return false;

        // Then render the final graphics scene.
        if (!Render())
            return false;

        return true;
    }

    private bool RenderSceneToTexture(float rotation)
    {
        // Set the render target to the render texture and clear it.
        m_RenderTexture.SetRenderTarget();
        m_RenderTexture.ClearRenderTarget(1.0f, 0.5f, 0.0f, 1.0f);

        // Set camera for rendering the cube.
        m_Camera.SetPosition(0.0f, 0.0f, -5.0f);
        m_Camera.Render();

        // Get matrices.
        var world = Matrix4X4.CreateRotationY<float>(rotation);
        var view = m_Camera.GetViewMatrix();
        var projection = m_RenderTexture.GetProjectionMatrix();

        // Set shader parameters and render the model.
        if (!m_TextureShader.SetShaderParameters(m_OpenGL, world, view, projection, 0))
            return false;

        m_Model.SetTexture(m_OpenGL, 0);
        m_Model.Render(m_OpenGL);

        // Reset back to the original back buffer and viewport.
        m_OpenGL.SetBackBufferRenderTarget();
        m_OpenGL.ResetViewport();

        return true;
    }

    private bool Render()
    {
        // Clear the buffers.
        m_OpenGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        // Set camera for viewing the display planes.
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);
        m_Camera.Render();

        // Get matrices.
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        // Top display plane.
        var world = Matrix4X4.CreateTranslation<float>(0.0f, 1.5f, 0.0f);
        if (!m_TextureShader.SetShaderParameters(m_OpenGL, world, view, projection, 0))
            return false;
        m_RenderTexture.SetTexture(0);
        m_DisplayPlane.Render(m_OpenGL);

        // Bottom left display plane.
        world = Matrix4X4.CreateTranslation<float>(-1.5f, -1.5f, 0.0f);
        if (!m_TextureShader.SetShaderParameters(m_OpenGL, world, view, projection, 0))
            return false;
        m_RenderTexture.SetTexture(0);
        m_DisplayPlane.Render(m_OpenGL);

        // Bottom right display plane.
        world = Matrix4X4.CreateTranslation<float>(1.5f, -1.5f, 0.0f);
        if (!m_TextureShader.SetShaderParameters(m_OpenGL, world, view, projection, 0))
            return false;
        m_RenderTexture.SetTexture(0);
        m_DisplayPlane.Render(m_OpenGL);

        // Present the scene.
        m_OpenGL.EndScene();

        return true;
    }
}
