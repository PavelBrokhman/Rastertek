using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial36.Graphics;

public class Blur
{
    private RenderTexture m_DownSampleTexture1, m_DownSampleTexture2;
    private OrthoWindow m_DownSampleWindow, m_UpSampleWindow;
    private int m_downSampleWidth, m_downSampleHeight;

    public bool Initialize(GL4 OpenGL, int downSampleWidth, int downSampleHeight,
        float screenNear, float screenDepth, int renderWidth, int renderHeight)
    {
        m_downSampleWidth = downSampleWidth;
        m_downSampleHeight = downSampleHeight;

        m_DownSampleTexture1 = new RenderTexture();
        if (!m_DownSampleTexture1.Initialize(OpenGL, m_downSampleWidth, m_downSampleHeight, screenNear, screenDepth))
            return false;

        m_DownSampleTexture2 = new RenderTexture();
        if (!m_DownSampleTexture2.Initialize(OpenGL, m_downSampleWidth, m_downSampleHeight, screenNear, screenDepth))
            return false;

        m_DownSampleWindow = new OrthoWindow();
        if (!m_DownSampleWindow.Initialize(OpenGL, m_downSampleWidth, m_downSampleHeight))
            return false;

        m_UpSampleWindow = new OrthoWindow();
        if (!m_UpSampleWindow.Initialize(OpenGL, renderWidth, renderHeight))
            return false;

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        m_UpSampleWindow?.Shutdown(OpenGL); m_UpSampleWindow = null;
        m_DownSampleWindow?.Shutdown(OpenGL); m_DownSampleWindow = null;
        m_DownSampleTexture2?.Shutdown(OpenGL); m_DownSampleTexture2 = null;
        m_DownSampleTexture1?.Shutdown(OpenGL); m_DownSampleTexture1 = null;
    }

    public bool BlurTexture(RenderTexture renderTexture, GL4 OpenGL, Camera camera,
        TextureShader textureShader, BlurShader blurShader)
    {
        var worldMatrix = OpenGL.GetWorldMatrix();
        var baseViewMatrix = camera.GetBaseViewMatrix();

        OpenGL.TurnZBufferOff();

        // Step 1: Down sample the render to texture
        m_DownSampleTexture1.SetRenderTarget(OpenGL);
        m_DownSampleTexture1.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        var orthoMatrix = m_DownSampleTexture1.GetOrthoMatrix();

        if (!textureShader.SetShaderParameters(OpenGL, worldMatrix, baseViewMatrix, orthoMatrix))
            return false;

        renderTexture.SetTexture(OpenGL, 0);
        m_DownSampleWindow.Render(OpenGL);

        // Step 2: Horizontal blur
        float blurType = 0.0f;
        m_DownSampleTexture2.SetRenderTarget(OpenGL);
        m_DownSampleTexture2.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        orthoMatrix = m_DownSampleTexture2.GetOrthoMatrix();

        if (!blurShader.SetShaderParameters(OpenGL, worldMatrix, baseViewMatrix, orthoMatrix,
            m_downSampleWidth, m_downSampleHeight, blurType))
            return false;

        m_DownSampleTexture1.SetTexture(OpenGL, 0);
        m_DownSampleWindow.Render(OpenGL);

        // Step 3: Vertical blur
        blurType = 1.0f;
        m_DownSampleTexture1.SetRenderTarget(OpenGL);
        m_DownSampleTexture1.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        orthoMatrix = m_DownSampleTexture1.GetOrthoMatrix();

        if (!blurShader.SetShaderParameters(OpenGL, worldMatrix, baseViewMatrix, orthoMatrix,
            m_downSampleWidth, m_downSampleHeight, blurType))
            return false;

        m_DownSampleTexture2.SetTexture(OpenGL, 0);
        m_DownSampleWindow.Render(OpenGL);

        // Step 4: Up sample the blurred result
        renderTexture.SetRenderTarget(OpenGL);
        renderTexture.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        orthoMatrix = renderTexture.GetOrthoMatrix();

        if (!textureShader.SetShaderParameters(OpenGL, worldMatrix, baseViewMatrix, orthoMatrix))
            return false;

        m_DownSampleTexture1.SetTexture(OpenGL, 0);
        m_UpSampleWindow.Render(OpenGL);

        OpenGL.TurnZBufferOn();

        OpenGL.SetBackBufferRenderTarget();
        OpenGL.ResetViewport();

        return true;
    }
}
