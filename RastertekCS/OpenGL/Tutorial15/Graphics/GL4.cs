using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial15.Graphics;

public class GL4
{
    private GL _gl;
    private Matrix4X4<float> _worldMatrix;
    private Matrix4X4<float> _projectionMatrix;
    private Matrix4X4<float> _orthoMatrix;
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

        float fov = MathF.PI / 4.0f;
        float aspect = (float)screenWidth / screenHeight;
        BuildPerspectiveFovLH(out _projectionMatrix, fov, aspect, screenNear, screenDepth);
        BuildOrthoLH(out _orthoMatrix, screenWidth, screenHeight, screenNear, screenDepth);

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

    public void TurnZBufferOn() => _gl.Enable(EnableCap.DepthTest);

    public void TurnZBufferOff() => _gl.Disable(EnableCap.DepthTest);

    public Matrix4X4<float> GetWorldMatrix() => _worldMatrix;

    public Matrix4X4<float> GetProjectionMatrix() => _projectionMatrix;

    public Matrix4X4<float> GetOrthoMatrix() => _orthoMatrix;

    public string GetVideoCardInfo() => _videoCardDescription;

    private static void BuildPerspectiveFovLH(
        out Matrix4X4<float> m,
        float fov,
        float aspect,
        float near,
        float far
    )
    {
        float tanHalf = MathF.Tan(fov * 0.5f);
        m = default;
        m.M11 = 1.0f / (aspect * tanHalf);
        m.M22 = 1.0f / tanHalf;
        m.M33 = far / (far - near);
        m.M34 = 1.0f;
        m.M43 = (-near * far) / (far - near);
    }

    private static void BuildOrthoLH(
        out Matrix4X4<float> m,
        float w,
        float h,
        float near,
        float far
    )
    {
        m = default;
        m.M11 = 2.0f / w;
        m.M22 = 2.0f / h;
        m.M33 = 1.0f / (far - near);
        m.M43 = near / (near - far);
        m.M44 = 1.0f;
    }
}
