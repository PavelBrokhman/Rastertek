using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial53.Graphics;

public class Blur
{
    private RenderTexture _downSampleTexture1;
    private RenderTexture _downSampleTexture2;
    private OrthoWindow _downSampleWindow;
    private OrthoWindow _upSampleWindow;
    private int _downSampleWidth, _downSampleHeight;

    public bool Initialize(DX11 DirectX, int downSampleWidth, int downSampleHeight,
        float screenNear, float screenDepth, int renderWidth, int renderHeight)
    {
        _downSampleWidth = downSampleWidth;
        _downSampleHeight = downSampleHeight;

        _downSampleTexture1 = new RenderTexture();
        if (!_downSampleTexture1.Initialize(DirectX, downSampleWidth, downSampleHeight, screenDepth, screenNear)) return false;

        _downSampleTexture2 = new RenderTexture();
        if (!_downSampleTexture2.Initialize(DirectX, downSampleWidth, downSampleHeight, screenDepth, screenNear)) return false;

        _downSampleWindow = new OrthoWindow();
        if (!_downSampleWindow.Initialize(DirectX, downSampleWidth, downSampleHeight)) return false;

        _upSampleWindow = new OrthoWindow();
        if (!_upSampleWindow.Initialize(DirectX, renderWidth, renderHeight)) return false;

        return true;
    }

    public void Shutdown()
    {
        _upSampleWindow?.Shutdown(); _upSampleWindow = null;
        _downSampleWindow?.Shutdown(); _downSampleWindow = null;
        _downSampleTexture2?.Shutdown(); _downSampleTexture2 = null;
        _downSampleTexture1?.Shutdown(); _downSampleTexture1 = null;
    }

    public bool BlurTexture(DX11 DirectX, Camera camera, RenderTexture renderTexture,
        TextureShader textureShader, BlurShader blurShader)
    {
        var worldMatrix = DirectX.GetWorldMatrix();
        var viewMatrix = camera.GetViewMatrix();

        DirectX.TurnZBufferOff();

        // STEP 1: Down sample.
        _downSampleTexture1.SetRenderTarget(DirectX);
        _downSampleTexture1.ClearRenderTarget(DirectX, 0, 0, 0, 1);
        var orthoMatrix = _downSampleTexture1.GetOrthoMatrix();
        _downSampleWindow.Render(DirectX);
        if (!textureShader.Render(DirectX, _downSampleWindow.GetIndexCount(),
            worldMatrix, viewMatrix, orthoMatrix, renderTexture.GetShaderResourceView())) return false;

        // STEP 2: Horizontal blur.
        _downSampleTexture2.SetRenderTarget(DirectX);
        _downSampleTexture2.ClearRenderTarget(DirectX, 0, 0, 0, 1);
        orthoMatrix = _downSampleTexture2.GetOrthoMatrix();
        _downSampleWindow.Render(DirectX);
        if (!blurShader.Render(DirectX, _downSampleWindow.GetIndexCount(),
            worldMatrix, viewMatrix, orthoMatrix, _downSampleTexture1.GetShaderResourceView(),
            _downSampleWidth, _downSampleHeight, 0.0f)) return false;

        // STEP 3: Vertical blur.
        _downSampleTexture1.SetRenderTarget(DirectX);
        _downSampleTexture1.ClearRenderTarget(DirectX, 0, 0, 0, 1);
        orthoMatrix = _downSampleTexture1.GetOrthoMatrix();
        _downSampleWindow.Render(DirectX);
        if (!blurShader.Render(DirectX, _downSampleWindow.GetIndexCount(),
            worldMatrix, viewMatrix, orthoMatrix, _downSampleTexture2.GetShaderResourceView(),
            _downSampleWidth, _downSampleHeight, 1.0f)) return false;

        // STEP 4: Up sample back into renderTexture.
        renderTexture.SetRenderTarget(DirectX);
        renderTexture.ClearRenderTarget(DirectX, 0, 0, 0, 1);
        orthoMatrix = renderTexture.GetOrthoMatrix();
        _upSampleWindow.Render(DirectX);
        if (!textureShader.Render(DirectX, _upSampleWindow.GetIndexCount(),
            worldMatrix, viewMatrix, orthoMatrix, _downSampleTexture1.GetShaderResourceView())) return false;

        DirectX.TurnZBufferOn();
        DirectX.SetBackBufferRenderTarget();
        DirectX.ResetViewport();
        return true;
    }
}
