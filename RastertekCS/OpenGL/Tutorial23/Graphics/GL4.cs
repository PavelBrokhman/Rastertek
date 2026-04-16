using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial23.Graphics;

public class GL4
{
    private GL _driver;
    private Matrix4X4<float> _worldMatrix;
    private Matrix4X4<float> _projectionMatrix;
    private Matrix4X4<float> _orthoMatrix;
    private string _videoCardDescription;

    public GL Driver => _driver;

    public bool Initialize(
        IWindow window,
        int screenWidth,
        int screenHeight,
        float screenDepth,
        float screenNear,
        bool vsync
    )
    {
        _driver = GL.GetApi(window);
        var vendor = _driver.GetStringS(StringName.Vendor) ?? "";
        var renderer = _driver.GetStringS(StringName.Renderer) ?? "";
        _videoCardDescription = vendor + " - " + renderer;

        _driver.ClearDepth(1.0f);
        _driver.Enable(EnableCap.DepthTest);
        _driver.FrontFace(FrontFaceDirection.CW);
        _driver.Enable(EnableCap.CullFace);
        _driver.CullFace(TriangleFace.Back);
        _driver.Viewport(0, 0, (uint)screenWidth, (uint)screenHeight);

        _worldMatrix = Matrix4X4<float>.Identity;
        _projectionMatrix = PerspectiveFovLH(
            MathF.PI / 4.0f,
            (float)screenWidth / screenHeight,
            screenNear,
            screenDepth
        );
        _orthoMatrix = Matrix4X4.CreateOrthographicOffCenter(
            -screenWidth / 2f,
            screenWidth / 2f,
            -screenHeight / 2f,
            screenHeight / 2f,
            -1f,
            1f
        );

        _ = vsync;
        return true;
    }

    public void Shutdown()
    {
        _driver?.Dispose();
        _driver = null;
    }

    public void BeginScene(float r, float g, float b, float a)
    {
        _driver.ClearColor(r, g, b, a);
        _driver.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void EndScene() { }

    public void TurnZBufferOn() => _driver.Enable(EnableCap.DepthTest);

    public void TurnZBufferOff() => _driver.Disable(EnableCap.DepthTest);

    public void EnableAlphaBlending()
    {
        _driver.Enable(EnableCap.Blend);
        _driver.BlendFuncSeparate(
            BlendingFactor.SrcAlpha,
            BlendingFactor.OneMinusSrcAlpha,
            BlendingFactor.One,
            BlendingFactor.Zero
        );
    }

    public void DisableAlphaBlending() => _driver.Disable(EnableCap.Blend);

    public Matrix4X4<float> GetWorldMatrix() => _worldMatrix;

    public Matrix4X4<float> GetProjectionMatrix() => _projectionMatrix;

    public Matrix4X4<float> GetOrthoMatrix() => _orthoMatrix;

    public string GetVideoCardInfo() => _videoCardDescription;

    private static Matrix4X4<float> PerspectiveFovLH(
        float fov,
        float aspect,
        float nearZ,
        float farZ
    )
    {
        float h = 1.0f / MathF.Tan(fov * 0.5f);
        float w = h / aspect;
        float range = farZ / (farZ - nearZ);
        return new Matrix4X4<float>(
            w,
            0,
            0,
            0,
            0,
            h,
            0,
            0,
            0,
            0,
            range,
            1,
            0,
            0,
            -range * nearZ,
            0
        );
    }
}
