using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial05.Graphics;

public class GL4
{
    private GL _driver;
    private IWindow _window;
    private Matrix4X4<float> _worldMatrix;
    private Matrix4X4<float> _projectionMatrix;
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
        _window = window;
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

        float fov = MathF.PI / 4.0f;
        float aspect = (float)screenWidth / screenHeight;
        _projectionMatrix = PerspectiveFovLH(fov, aspect, screenNear, screenDepth);

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

    public Matrix4X4<float> GetWorldMatrix() => _worldMatrix;

    public Matrix4X4<float> GetProjectionMatrix() => _projectionMatrix;

    public string GetVideoCardInfo() => _videoCardDescription;

    // C++ openglclass.cpp BuildIdentityMatrix
    public static Matrix4X4<float> BuildIdentityMatrix() =>
        new(1, 0, 0, 0,  0, 1, 0, 0,  0, 0, 1, 0,  0, 0, 0, 1);

    // C++ openglclass.cpp BuildOrthoMatrix
    public static Matrix4X4<float> BuildOrthoMatrix(float width, float height, float nearZ, float farZ) =>
        new(
            2.0f / width, 0, 0, 0,
            0, 2.0f / height, 0, 0,
            0, 0, 1.0f / (farZ - nearZ), 0,
            0, 0, -nearZ / (farZ - nearZ), 1
        );

    // C++ openglclass.cpp MatrixRotationX
    public static Matrix4X4<float> MatrixRotationX(float angle) =>
        new(
            1, 0, 0, 0,
            0, MathF.Cos(angle), MathF.Sin(angle), 0,
            0, -MathF.Sin(angle), MathF.Cos(angle), 0,
            0, 0, 0, 1
        );

    // C++ openglclass.cpp MatrixRotationY
    public static Matrix4X4<float> MatrixRotationY(float angle) =>
        new(
            MathF.Cos(angle), 0, -MathF.Sin(angle), 0,
            0, 1, 0, 0,
            MathF.Sin(angle), 0, MathF.Cos(angle), 0,
            0, 0, 0, 1
        );

    // C++ openglclass.cpp MatrixRotationZ
    public static Matrix4X4<float> MatrixRotationZ(float angle) =>
        new(
            MathF.Cos(angle), MathF.Sin(angle), 0, 0,
            -MathF.Sin(angle), MathF.Cos(angle), 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        );

    // C++ openglclass.cpp MatrixTranslation
    public static Matrix4X4<float> MatrixTranslation(float x, float y, float z) =>
        new(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            x, y, z, 1
        );

    // C++ openglclass.cpp MatrixScale
    public static Matrix4X4<float> MatrixScale(float x, float y, float z) =>
        new(
            x, 0, 0, 0,
            0, y, 0, 0,
            0, 0, z, 0,
            0, 0, 0, 1
        );

    // C++ openglclass.cpp MatrixTranspose
    public static Matrix4X4<float> MatrixTranspose(Matrix4X4<float> m) =>
        new(
            m.M11, m.M21, m.M31, m.M41,
            m.M12, m.M22, m.M32, m.M42,
            m.M13, m.M23, m.M33, m.M43,
            m.M14, m.M24, m.M34, m.M44
        );

    // C++ openglclass.cpp MatrixMultiply
    public static Matrix4X4<float> MatrixMultiply(Matrix4X4<float> a, Matrix4X4<float> b) =>
        new(
            a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41,
            a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42,
            a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43,
            a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,
            a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41,
            a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42,
            a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43,
            a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,
            a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41,
            a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42,
            a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43,
            a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,
            a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41,
            a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42,
            a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43,
            a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44
        );

    // C++ openglclass.cpp BuildPerspectiveFovMatrix
    public static Matrix4X4<float> PerspectiveFovLH(float fov, float aspect, float nearZ, float farZ) =>
        new(
            1.0f / (aspect * MathF.Tan(fov * 0.5f)), 0, 0, 0,
            0, 1.0f / MathF.Tan(fov * 0.5f), 0, 0,
            0, 0, farZ / (farZ - nearZ), 1,
            0, 0, (-nearZ * farZ) / (farZ - nearZ), 0
        );
}
