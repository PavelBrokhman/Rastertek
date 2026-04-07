using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial12.Graphics;

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
        m_gl.FrontFace(FrontFaceDirection.Ccw);
        m_gl.Enable(EnableCap.CullFace);
        m_gl.CullFace(TriangleFace.Back);
        m_gl.Viewport(0, 0, (uint)sw, (uint)sh);

        m_worldMatrix = Matrix4X4<float>.Identity;
        m_projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(MathF.PI / 4.0f, (float)sw / sh, sn, sd);
        // Ortho: центр (0,0), ширина=sw, высота=sh, координаты в пикселях.
        m_orthoMatrix = Matrix4X4.CreateOrthographic<float>(sw, sh, sn, sd);
        _ = vsync;
        return true;
    }

    public void Shutdown() { m_gl?.Dispose(); m_gl = null; }

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
}
