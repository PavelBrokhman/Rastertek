using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial50.Graphics;

public class GL4
{
    private GL _gl;
    private Matrix4X4<float> _worldMatrix;
    private Matrix4X4<float> _projectionMatrix;
    private Matrix4X4<float> _orthoMatrix;
    private string _videoCardDescription;
    private int _screenWidth, _screenHeight;

    public GL Gl => _gl;

    public bool Initialize(IWindow window, int sw, int sh, float sd, float sn, bool vsync)
    {
        _gl = GL.GetApi(window);
        var vendor = _gl.GetStringS(StringName.Vendor) ?? "";
        var renderer = _gl.GetStringS(StringName.Renderer) ?? "";
        _videoCardDescription = vendor + " - " + renderer;

        _screenWidth = sw;
        _screenHeight = sh;

        _gl.ClearDepth(1.0f);
        _gl.Enable(EnableCap.DepthTest);
        _gl.FrontFace(FrontFaceDirection.CW);
        _gl.Enable(EnableCap.CullFace);
        _gl.CullFace(TriangleFace.Back);
        _gl.Viewport(0, 0, (uint)sw, (uint)sh);

        _worldMatrix = Matrix4X4<float>.Identity;
        _projectionMatrix = PerspectiveFovLH(MathF.PI / 4.0f, (float)sw / sh, sn, sd);
        _orthoMatrix = Matrix4X4.CreateOrthographicOffCenter(-sw / 2f, sw / 2f, -sh / 2f, sh / 2f, sn, sd);
        _ = vsync;
        return true;
    }

    public void Shutdown() { _gl?.Dispose(); _gl = null; }

    public void BeginScene(float r, float g, float b, float a)
    {
        _gl.ClearColor(r, g, b, a);
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void EndScene() { }

    public void TurnZBufferOn() => _gl.Enable(EnableCap.DepthTest);
    public void TurnZBufferOff() => _gl.Disable(EnableCap.DepthTest);

    public void SetBackBufferRenderTarget() => _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    public void ResetViewport() => _gl.Viewport(0, 0, (uint)_screenWidth, (uint)_screenHeight);

    public Matrix4X4<float> GetWorldMatrix() => _worldMatrix;
    public Matrix4X4<float> GetProjectionMatrix() => _projectionMatrix;
    public Matrix4X4<float> GetOrthoMatrix() => _orthoMatrix;
    public string GetVideoCardInfo() => _videoCardDescription;

    public static Matrix4X4<float> MatrixTranspose(Matrix4X4<float> m) =>
        new(
            m.M11, m.M21, m.M31, m.M41,
            m.M12, m.M22, m.M32, m.M42,
            m.M13, m.M23, m.M33, m.M43,
            m.M14, m.M24, m.M34, m.M44
        );

    private static Matrix4X4<float> PerspectiveFovLH(float fov, float aspect, float nearZ, float farZ)
    {
        float h = 1.0f / MathF.Tan(fov * 0.5f);
        float w = h / aspect;
        float range = farZ / (farZ - nearZ);
        return new Matrix4X4<float>(
            w, 0, 0, 0,
            0, h, 0, 0,
            0, 0, range, 1,
            0, 0, -range * nearZ, 0);
    }
}
