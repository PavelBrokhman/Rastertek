using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial19.Graphics;

public class GL4
{
    private GL _gl;
    private Matrix4X4<float> _worldMatrix;
    private Matrix4X4<float> _projectionMatrix;
    private string _videoCardDescription;

    public GL Gl => _gl;

    public bool Initialize(
        IWindow window,
        int screenWidth,
        int screenHeight,
        float screenDepth,
        float screenNear,
        bool vsync
    )
    {
        _gl = GL.GetApi(window);
        var vendor = _gl.GetStringS(StringName.Vendor) ?? "";
        var renderer = _gl.GetStringS(StringName.Renderer) ?? "";
        _videoCardDescription = vendor + " - " + renderer;

        _gl.ClearDepth(1.0f);
        _gl.Enable(EnableCap.DepthTest);
        _gl.FrontFace(FrontFaceDirection.CW);
        _gl.Enable(EnableCap.CullFace);
        _gl.CullFace(TriangleFace.Back);
        _gl.Viewport(0, 0, (uint)screenWidth, (uint)screenHeight);

        _worldMatrix = Matrix4X4<float>.Identity;
        _projectionMatrix = PerspectiveFovLH(
            MathF.PI / 4.0f,
            (float)screenWidth / screenHeight,
            screenNear,
            screenDepth
        );
        _ = vsync;
        return true;
    }

    public void Shutdown()
    {
        _gl?.Dispose();
        _gl = null;
    }

    public void BeginScene(float r, float g, float b, float a)
    {
        _gl.ClearColor(r, g, b, a);
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void EndScene() { }

    public Matrix4X4<float> GetWorldMatrix() => _worldMatrix;

    public Matrix4X4<float> GetProjectionMatrix() => _projectionMatrix;

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
