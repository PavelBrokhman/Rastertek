namespace RastertekCS.OpenGL.Tutorial46.Graphics;

public class Blur
{
    private RenderTexture _downSampleTexture1;
    private RenderTexture _downSampleTexture2;
    private OrthoWindow _downSampleWindow;
    private OrthoWindow _upSampleWindow;
    private int _downSampleWidth, _downSampleHeight;

    public bool Initialize(GL4 OpenGL, int downSampleWidth, int downSampleHeight,
        float screenNear, float screenDepth, int renderWidth, int renderHeight)
    {
        _downSampleWidth = downSampleWidth;
        _downSampleHeight = downSampleHeight;

        _downSampleTexture1 = new RenderTexture();
        if (!_downSampleTexture1.Initialize(OpenGL, downSampleWidth, downSampleHeight, screenDepth, screenNear)) return false;

        _downSampleTexture2 = new RenderTexture();
        if (!_downSampleTexture2.Initialize(OpenGL, downSampleWidth, downSampleHeight, screenDepth, screenNear)) return false;

        _downSampleWindow = new OrthoWindow();
        if (!_downSampleWindow.Initialize(OpenGL, downSampleWidth, downSampleHeight)) return false;

        _upSampleWindow = new OrthoWindow();
        if (!_upSampleWindow.Initialize(OpenGL, renderWidth, renderHeight)) return false;

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        _upSampleWindow?.Shutdown(OpenGL); _upSampleWindow = null;
        _downSampleWindow?.Shutdown(OpenGL); _downSampleWindow = null;
        _downSampleTexture2?.Shutdown(OpenGL); _downSampleTexture2 = null;
        _downSampleTexture1?.Shutdown(OpenGL); _downSampleTexture1 = null;
    }

    public bool BlurTexture(GL4 OpenGL, Camera camera, RenderTexture renderTexture,
        TextureShader textureShader, BlurShader blurShader)
    {
        var worldMatrix = OpenGL.GetWorldMatrix();
        var viewMatrix = camera.GetViewMatrix();

        OpenGL.TurnZBufferOff();

        // STEP 1: Down sample.
        _downSampleTexture1.SetRenderTarget(OpenGL);
        _downSampleTexture1.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        var orthoMatrix = _downSampleTexture1.GetOrthoMatrix();
        if (!textureShader.SetShaderParameters(OpenGL, worldMatrix, viewMatrix, orthoMatrix)) return false;
        renderTexture.SetTexture(OpenGL, 0);
        _downSampleWindow.Render(OpenGL);

        // STEP 2: Horizontal blur.
        _downSampleTexture2.SetRenderTarget(OpenGL);
        _downSampleTexture2.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        orthoMatrix = _downSampleTexture2.GetOrthoMatrix();
        if (!blurShader.SetShaderParameters(OpenGL, worldMatrix, viewMatrix, orthoMatrix,
                _downSampleWidth, _downSampleHeight, 0.0f)) return false;
        _downSampleTexture1.SetTexture(OpenGL, 0);
        _downSampleWindow.Render(OpenGL);

        // STEP 3: Vertical blur.
        _downSampleTexture1.SetRenderTarget(OpenGL);
        _downSampleTexture1.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        orthoMatrix = _downSampleTexture1.GetOrthoMatrix();
        if (!blurShader.SetShaderParameters(OpenGL, worldMatrix, viewMatrix, orthoMatrix,
                _downSampleWidth, _downSampleHeight, 1.0f)) return false;
        _downSampleTexture2.SetTexture(OpenGL, 0);
        _downSampleWindow.Render(OpenGL);

        // STEP 4: Up sample back into renderTexture.
        renderTexture.SetRenderTarget(OpenGL);
        renderTexture.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        orthoMatrix = renderTexture.GetOrthoMatrix();
        if (!textureShader.SetShaderParameters(OpenGL, worldMatrix, viewMatrix, orthoMatrix)) return false;
        _downSampleTexture1.SetTexture(OpenGL, 0);
        _upSampleWindow.Render(OpenGL);

        OpenGL.TurnZBufferOn();
        OpenGL.SetBackBufferRenderTarget();
        OpenGL.ResetViewport();
        return true;
    }
}
