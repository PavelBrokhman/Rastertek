namespace RastertekCS.Windows.Tutorial44.Graphics;

public class Blur
{
    private RenderTexture _downSampleTexture1,
        _downSampleTexture2;
    private OrthoWindow _downSampleWindow,
        _upSampleWindow;
    private int _downSampleWidth,
        _downSampleHeight;

    public bool Initialize(
        DX11 DirectX,
        int downSampleWidth,
        int downSampleHeight,
        float screenDepth,
        float screenNear,
        int renderWidth,
        int renderHeight
    )
    {
        _downSampleWidth = downSampleWidth;
        _downSampleHeight = downSampleHeight;
        _downSampleTexture1 = new RenderTexture();
        if (
            !_downSampleTexture1.Initialize(
                DirectX,
                _downSampleWidth,
                _downSampleHeight,
                screenDepth,
                screenNear
            )
        )
            return false;
        _downSampleTexture2 = new RenderTexture();
        if (
            !_downSampleTexture2.Initialize(
                DirectX,
                _downSampleWidth,
                _downSampleHeight,
                screenDepth,
                screenNear
            )
        )
            return false;
        _downSampleWindow = new OrthoWindow();
        if (!_downSampleWindow.Initialize(DirectX, _downSampleWidth, _downSampleHeight))
            return false;
        _upSampleWindow = new OrthoWindow();
        if (!_upSampleWindow.Initialize(DirectX, renderWidth, renderHeight))
            return false;
        return true;
    }

    public void Shutdown()
    {
        _upSampleWindow?.Shutdown();
        _downSampleWindow?.Shutdown();
        _downSampleTexture2?.Shutdown();
        _downSampleTexture1?.Shutdown();
        _upSampleWindow = null;
        _downSampleWindow = null;
        _downSampleTexture2 = null;
        _downSampleTexture1 = null;
    }

    public bool BlurTexture(
        RenderTexture renderTexture,
        DX11 DirectX,
        Camera camera,
        TextureShader textureShader,
        BlurShader blurShader
    )
    {
        var worldMatrix = DirectX.GetWorldMatrix();
        var baseViewMatrix = camera.GetBaseViewMatrix();

        DirectX.TurnZBufferOff();

        // STEP 1: Down sample input render-to-texture into _downSampleTexture1.
        _downSampleTexture1.SetRenderTarget(DirectX);
        _downSampleTexture1.ClearRenderTarget(DirectX, 0, 0, 0, 1);
        var orthoMatrix = _downSampleTexture1.GetOrthoMatrix();
        _downSampleWindow.Render(DirectX);
        if (
            !textureShader.Render(
                DirectX,
                _downSampleWindow.GetIndexCount(),
                worldMatrix,
                baseViewMatrix,
                orthoMatrix,
                renderTexture.GetShaderResourceView()
            )
        )
            return false;

        // STEP 2: Horizontal blur into _downSampleTexture2.
        float blurType = 0.0f;
        _downSampleTexture2.SetRenderTarget(DirectX);
        _downSampleTexture2.ClearRenderTarget(DirectX, 0, 0, 0, 1);
        orthoMatrix = _downSampleTexture2.GetOrthoMatrix();
        _downSampleWindow.Render(DirectX);
        if (
            !blurShader.Render(
                DirectX,
                _downSampleWindow.GetIndexCount(),
                worldMatrix,
                baseViewMatrix,
                orthoMatrix,
                _downSampleTexture1.GetShaderResourceView(),
                _downSampleWidth,
                _downSampleHeight,
                blurType
            )
        )
            return false;

        // STEP 3: Vertical blur back into _downSampleTexture1.
        blurType = 1.0f;
        _downSampleTexture1.SetRenderTarget(DirectX);
        _downSampleTexture1.ClearRenderTarget(DirectX, 0, 0, 0, 1);
        orthoMatrix = _downSampleTexture1.GetOrthoMatrix();
        _downSampleWindow.Render(DirectX);
        if (
            !blurShader.Render(
                DirectX,
                _downSampleWindow.GetIndexCount(),
                worldMatrix,
                baseViewMatrix,
                orthoMatrix,
                _downSampleTexture2.GetShaderResourceView(),
                _downSampleWidth,
                _downSampleHeight,
                blurType
            )
        )
            return false;

        // STEP 4: Up sample the blurred result back into the input render texture.
        renderTexture.SetRenderTarget(DirectX);
        renderTexture.ClearRenderTarget(DirectX, 0, 0, 0, 1);
        orthoMatrix = renderTexture.GetOrthoMatrix();
        _upSampleWindow.Render(DirectX);
        if (
            !textureShader.Render(
                DirectX,
                _upSampleWindow.GetIndexCount(),
                worldMatrix,
                baseViewMatrix,
                orthoMatrix,
                _downSampleTexture1.GetShaderResourceView()
            )
        )
            return false;

        DirectX.TurnZBufferOn();
        DirectX.SetBackBufferRenderTarget();
        DirectX.ResetViewport();
        return true;
    }
}
