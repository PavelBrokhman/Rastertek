using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial04.Graphics;

// Обёртка над Silk.NET GL — аналог DX11.cs в DirectX версии, или OpenGLClass в C++.
public class GL4
{
    private GL m_gl;
    private IWindow m_window;
    private Matrix4X4<float> m_worldMatrix;
    private Matrix4X4<float> m_projectionMatrix;
    private string m_videoCardDescription;

    public GL Gl => m_gl;

    public bool Initialize(IWindow window, int screenWidth, int screenHeight,
                           float screenDepth, float screenNear, bool vsync)
    {
        m_window = window;
        m_gl = GL.GetApi(window);

        var vendor = m_gl.GetStringS(StringName.Vendor) ?? "";
        var renderer = m_gl.GetStringS(StringName.Renderer) ?? "";
        m_videoCardDescription = vendor + " - " + renderer;

        m_gl.ClearDepth(1.0f);
        m_gl.Enable(EnableCap.DepthTest);
        m_gl.FrontFace(FrontFaceDirection.Ccw);
        m_gl.Enable(EnableCap.CullFace);
        m_gl.CullFace(TriangleFace.Back);
        m_gl.Viewport(0, 0, (uint)screenWidth, (uint)screenHeight);

        m_worldMatrix = Matrix4X4<float>.Identity;

        float fov = MathF.PI / 4.0f;
        float aspect = (float)screenWidth / screenHeight;
        m_projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(fov, aspect, screenNear, screenDepth);

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

    public void EndScene() { /* SwapBuffers выполняет Silk.NET */ }

    public Matrix4X4<float> GetWorldMatrix() => m_worldMatrix;
    public Matrix4X4<float> GetProjectionMatrix() => m_projectionMatrix;
    public string GetVideoCardInfo() => m_videoCardDescription;
}
