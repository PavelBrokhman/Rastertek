using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial53.Graphics;

public class Blur
{
    private RenderTexture m_DownSampleTexture1, m_DownSampleTexture2;
    private OrthoWindow m_DownSampleWindow, m_UpSampleWindow;
    private int m_downSampleWidth, m_downSampleHeight;

    public bool Initialize(GL4 OpenGL, int downSampleWidth, int downSampleHeight, float screenNear, float screenDepth, int renderWidth, int renderHeight)
    {
        m_downSampleWidth = downSampleWidth; m_downSampleHeight = downSampleHeight;
        m_DownSampleTexture1 = new RenderTexture();
        if (!m_DownSampleTexture1.Initialize(OpenGL, downSampleWidth, downSampleHeight, screenNear, screenDepth, 0)) return false;
        m_DownSampleTexture2 = new RenderTexture();
        if (!m_DownSampleTexture2.Initialize(OpenGL, downSampleWidth, downSampleHeight, screenNear, screenDepth, 0)) return false;
        m_DownSampleWindow = new OrthoWindow();
        if (!m_DownSampleWindow.Initialize(OpenGL, downSampleWidth, downSampleHeight)) return false;
        m_UpSampleWindow = new OrthoWindow();
        if (!m_UpSampleWindow.Initialize(OpenGL, renderWidth, renderHeight)) return false;
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        m_UpSampleWindow?.Shutdown(OpenGL); m_UpSampleWindow = null;
        m_DownSampleWindow?.Shutdown(OpenGL); m_DownSampleWindow = null;
        m_DownSampleTexture2?.Shutdown(OpenGL); m_DownSampleTexture2 = null;
        m_DownSampleTexture1?.Shutdown(OpenGL); m_DownSampleTexture1 = null;
    }

    public bool BlurTexture(RenderTexture rt, GL4 OpenGL, Camera camera, TextureShader textureShader, BlurShader blurShader)
    {
        var world = OpenGL.GetWorldMatrix();
        var baseView = camera.GetBaseViewMatrix();

        OpenGL.TurnZBufferOff();

        // Step 1: down sample
        m_DownSampleTexture1.SetRenderTarget(OpenGL);
        m_DownSampleTexture1.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        var orthoDS = m_DownSampleTexture1.GetOrthoMatrix();
        if (!textureShader.SetShaderParameters(OpenGL, world, baseView, orthoDS)) return false;
        rt.SetTexture(OpenGL, 0);
        m_DownSampleWindow.Render(OpenGL);

        // Step 2: horizontal blur
        m_DownSampleTexture2.SetRenderTarget(OpenGL);
        m_DownSampleTexture2.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        orthoDS = m_DownSampleTexture2.GetOrthoMatrix();
        if (!blurShader.SetShaderParameters(OpenGL, world, baseView, orthoDS, m_downSampleWidth, m_downSampleHeight, 0.0f)) return false;
        m_DownSampleTexture1.SetTexture(OpenGL, 0);
        m_DownSampleWindow.Render(OpenGL);

        // Step 3: vertical blur
        m_DownSampleTexture1.SetRenderTarget(OpenGL);
        m_DownSampleTexture1.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        orthoDS = m_DownSampleTexture1.GetOrthoMatrix();
        if (!blurShader.SetShaderParameters(OpenGL, world, baseView, orthoDS, m_downSampleWidth, m_downSampleHeight, 1.0f)) return false;
        m_DownSampleTexture2.SetTexture(OpenGL, 0);
        m_DownSampleWindow.Render(OpenGL);

        // Step 4: up sample back into rt
        rt.SetRenderTarget(OpenGL);
        rt.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        var orthoUp = rt.GetOrthoMatrix();
        if (!textureShader.SetShaderParameters(OpenGL, world, baseView, orthoUp)) return false;
        m_DownSampleTexture1.SetTexture(OpenGL, 0);
        m_UpSampleWindow.Render(OpenGL);

        OpenGL.TurnZBufferOn();
        OpenGL.SetBackBufferRenderTarget();
        OpenGL.ResetViewport();
        return true;
    }
}
