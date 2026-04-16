using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial14.Graphics;

public class GL4
{
    private GL m_gl;
    private Matrix4X4<float> m_worldMatrix;
    private Matrix4X4<float> m_projectionMatrix;
    private Matrix4X4<float> m_orthoMatrix;
    private string m_videoCardDescription;

    public GL Gl => m_gl;

    public bool Initialize(IWindow window, int sw, int sh, float sd, float sn, bool vsync)
    {
        m_gl = GL.GetApi(window);
        var vendor = m_gl.GetStringS(StringName.Vendor) ?? "";
        var renderer = m_gl.GetStringS(StringName.Renderer) ?? "";
        m_videoCardDescription = vendor + " - " + renderer;

        m_gl.ClearDepth(1.0f);
        m_gl.Enable(EnableCap.DepthTest);
        // Left-handed system: clockwise front face (matches original Rastertek).
        m_gl.FrontFace(FrontFaceDirection.CW);
        m_gl.Enable(EnableCap.CullFace);
        m_gl.CullFace(TriangleFace.Back);
        m_gl.Viewport(0, 0, (uint)sw, (uint)sh);

        m_worldMatrix = Matrix4X4<float>.Identity;

        float fov = MathF.PI / 4.0f;
        float aspect = (float)sw / sh;
        BuildPerspectiveFovLH(out m_projectionMatrix, fov, aspect, sn, sd);
        BuildOrthoLH(out m_orthoMatrix, sw, sh, sn, sd);

        _ = vsync;
        return true;
    }

    public void Shutdown()
    {
        m_gl?.Dispose();
        m_gl = null;
    }

    public void BeginScene(float r, float g, float b, float a)
    {
        m_gl.ClearColor(r, g, b, a);
        m_gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void EndScene() { }

    public void TurnZBufferOn() => m_gl.Enable(EnableCap.DepthTest);

    public void TurnZBufferOff() => m_gl.Disable(EnableCap.DepthTest);

    public Matrix4X4<float> GetWorldMatrix() => m_worldMatrix;

    public Matrix4X4<float> GetProjectionMatrix() => m_projectionMatrix;

    public Matrix4X4<float> GetOrthoMatrix() => m_orthoMatrix;

    public string GetVideoCardInfo() => m_videoCardDescription;

    // Matches original Rastertek BuildPerspectiveFovMatrix (LH, column-major).
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

    // Matches original Rastertek BuildOrthoMatrix (LH, column-major).
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
