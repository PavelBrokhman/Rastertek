namespace RastertekCS.OpenGL.Tutorial44.Graphics;

public class Blur
{
    private RenderTexture _downSampleTexture1,
        _downSampleTexture2;
    private OrthoWindow _downSampleWindow,
        _upSampleWindow;
    private int _downSampleWidth,
        _downSampleHeight;

    public bool Initialize(
        GL4 OpenGL,
        int downSampleWidth,
        int downSampleHeight,
        float screenNear,
        float screenDepth,
        int renderWidth,
        int renderHeight
    )
    {
        _downSampleWidth = downSampleWidth;
        _downSampleHeight = downSampleHeight;
        _downSampleTexture1 = new RenderTexture();
        if (
            !_downSampleTexture1.Initialize(
                OpenGL,
                _downSampleWidth,
                _downSampleHeight,
                screenNear,
                screenDepth
            )
        )
            return false;
        _downSampleTexture2 = new RenderTexture();
        if (
            !_downSampleTexture2.Initialize(
                OpenGL,
                _downSampleWidth,
                _downSampleHeight,
                screenNear,
                screenDepth
            )
        )
            return false;
        _downSampleWindow = new OrthoWindow();
        if (!_downSampleWindow.Initialize(OpenGL, _downSampleWidth, _downSampleHeight))
            return false;
        _upSampleWindow = new OrthoWindow();
        if (!_upSampleWindow.Initialize(OpenGL, renderWidth, renderHeight))
            return false;
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        _upSampleWindow?.Shutdown(OpenGL);
        _upSampleWindow = null;
        _downSampleWindow?.Shutdown(OpenGL);
        _downSampleWindow = null;
        _downSampleTexture2?.Shutdown(OpenGL);
        _downSampleTexture2 = null;
        _downSampleTexture1?.Shutdown(OpenGL);
        _downSampleTexture1 = null;
    }

    public bool BlurTexture(
        RenderTexture renderTexture,
        GL4 OpenGL,
        Camera camera,
        TextureShader textureShader,
        BlurShader blurShader
    )
    {
        var worldMatrix = OpenGL.GetWorldMatrix();
        var baseViewMatrix = camera.GetBaseViewMatrix();

        OpenGL.TurnZBufferOff();

        // STEP 1: Down sample the render-to-texture into _downSampleTexture1.
        _downSampleTexture1.SetRenderTarget(OpenGL);
        _downSampleTexture1.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        var orthoMatrix = _downSampleTexture1.GetOrthoMatrix();
        if (!textureShader.SetShaderParameters(OpenGL, worldMatrix, baseViewMatrix, orthoMatrix))
            return false;
        renderTexture.SetTexture(OpenGL, 0);
        _downSampleWindow.Render(OpenGL);

        // STEP 2: Horizontal blur into _downSampleTexture2.
        float blurType = 0.0f;
        _downSampleTexture2.SetRenderTarget(OpenGL);
        _downSampleTexture2.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        orthoMatrix = _downSampleTexture2.GetOrthoMatrix();
        if (
            !blurShader.SetShaderParameters(
                OpenGL,
                worldMatrix,
                baseViewMatrix,
                orthoMatrix,
                _downSampleWidth,
                _downSampleHeight,
                blurType
            )
        )
            return false;
        _downSampleTexture1.SetTexture(OpenGL, 0);
        _downSampleWindow.Render(OpenGL);

        // STEP 3: Vertical blur back into _downSampleTexture1.
        blurType = 1.0f;
        _downSampleTexture1.SetRenderTarget(OpenGL);
        _downSampleTexture1.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        orthoMatrix = _downSampleTexture1.GetOrthoMatrix();
        if (
            !blurShader.SetShaderParameters(
                OpenGL,
                worldMatrix,
                baseViewMatrix,
                orthoMatrix,
                _downSampleWidth,
                _downSampleHeight,
                blurType
            )
        )
            return false;
        _downSampleTexture2.SetTexture(OpenGL, 0);
        _downSampleWindow.Render(OpenGL);

        // STEP 4: Up sample back into the input render texture.
        renderTexture.SetRenderTarget(OpenGL);
        renderTexture.ClearRenderTarget(OpenGL, 0, 0, 0, 1);
        orthoMatrix = renderTexture.GetOrthoMatrix();
        if (!textureShader.SetShaderParameters(OpenGL, worldMatrix, baseViewMatrix, orthoMatrix))
            return false;
        _downSampleTexture1.SetTexture(OpenGL, 0);
        _upSampleWindow.Render(OpenGL);

        OpenGL.TurnZBufferOn();
        OpenGL.SetBackBufferRenderTarget();
        OpenGL.ResetViewport();
        return true;
    }
}
