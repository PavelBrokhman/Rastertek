using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial46.Graphics;

public class GL4
{
    private GL _driver;
    private Matrix4X4<float> _worldMatrix,
        _projectionMatrix,
        _orthoMatrix;
    private int _screenWidth,
        _screenHeight;
    public GL Driver => _driver;

    public bool Initialize(IWindow window, int sw, int sh, float sd, float sn, bool vsync)
    {
        _driver = GL.GetApi(window);
        _driver.ClearDepth(1.0f);
        _driver.Enable(EnableCap.DepthTest);
        _driver.FrontFace(FrontFaceDirection.CW);
        _driver.Enable(EnableCap.CullFace);
        _driver.CullFace(TriangleFace.Back);
        _driver.Viewport(0, 0, (uint)sw, (uint)sh);
        _screenWidth = sw;
        _screenHeight = sh;
        _worldMatrix = Matrix4X4<float>.Identity;
        _projectionMatrix = PerspectiveFovLH(MathF.PI / 4.0f, (float)sw / sh, sn, sd);
        _orthoMatrix = OrthographicLH(sw, sh, sn, sd);
        return true;
    }

    public void Shutdown() { _driver?.Dispose(); _driver = null; }

    public void BeginScene(float r, float g, float b, float a)
    {
        _driver.ClearColor(r, g, b, a);
        _driver.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void EndScene() { }

    public void SetBackBufferRenderTarget()
    {
        _driver.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void ResetViewport()
    {
        _driver.Viewport(0, 0, (uint)_screenWidth, (uint)_screenHeight);
    }

    public Matrix4X4<float> GetWorldMatrix() => _worldMatrix;
    public Matrix4X4<float> GetProjectionMatrix() => _projectionMatrix;
    public Matrix4X4<float> GetOrthoMatrix() => _orthoMatrix;

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

    // C++ openglclass.cpp MatrixTranspose
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
        return new Matrix4X4<float>(w, 0, 0, 0, 0, h, 0, 0, 0, 0, range, 1, 0, 0, -range * nearZ, 0);
    }

    private static Matrix4X4<float> OrthographicLH(float width, float height, float nearZ, float farZ)
    {
        float range = 1.0f / (farZ - nearZ);
        return new Matrix4X4<float>(
            2.0f / width, 0, 0, 0,
            0, 2.0f / height, 0, 0,
            0, 0, range, 0,
            0, 0, -range * nearZ, 1);
    }
}
