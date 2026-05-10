using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial34.Graphics;

public class GL4
{
    private GL _driver;
    private Matrix4X4<float> _worldMatrix,
        _projectionMatrix,
        _orthoMatrix;
    private int _screenWidth,
        _screenHeight;
    public GL Driver => _driver;

    public bool Initialize(
        IWindow w,
        int screenWidth,
        int screenHeight,
        float screenDepth,
        float screenNear,
        bool vs
    )
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        _driver = GL.GetApi(w);
        _driver.ClearDepth(1f);
        _driver.Enable(EnableCap.DepthTest);
        _driver.FrontFace(FrontFaceDirection.CW);
        _driver.Enable(EnableCap.CullFace);
        _driver.CullFace(TriangleFace.Back);
        _driver.Viewport(0, 0, (uint)screenWidth, (uint)screenHeight);
        _worldMatrix = Matrix4X4<float>.Identity;
        _projectionMatrix = PerspectiveFovLH(
            MathF.PI / 4f,
            (float)screenWidth / screenHeight,
            screenNear,
            screenDepth
        );
        _orthoMatrix = OrthoLH(screenWidth, screenHeight, screenNear, screenDepth);
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

    public Matrix4X4<float> GetOrthoMatrix() => _orthoMatrix;

    public static Matrix4X4<float> MatrixRotationY(float angle) =>
        new(
            MathF.Cos(angle), 0, -MathF.Sin(angle), 0,
            0, 1, 0, 0,
            MathF.Sin(angle), 0, MathF.Cos(angle), 0,
            0, 0, 0, 1
        );

    public static Matrix4X4<float> MatrixTranslation(float x, float y, float z) =>
        new(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            x, y, z, 1
        );

    public static Matrix4X4<float> MatrixTranspose(Matrix4X4<float> m) =>
        new(
            m.M11, m.M21, m.M31, m.M41,
            m.M12, m.M22, m.M32, m.M42,
            m.M13, m.M23, m.M33, m.M43,
            m.M14, m.M24, m.M34, m.M44
        );

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

    private static Matrix4X4<float> OrthoLH(
        float width,
        float height,
        float nearZ,
        float farZ
    )
    {
        float range = 1.0f / (farZ - nearZ);
        return new Matrix4X4<float>(
            2f / width, 0, 0, 0,
            0, 2f / height, 0, 0,
            0, 0, range, 0,
            0, 0, -range * nearZ, 1
        );
    }

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
            w, 0, 0, 0,
            0, h, 0, 0,
            0, 0, range, 1,
            0, 0, -range * nearZ, 0
        );
    }
}
