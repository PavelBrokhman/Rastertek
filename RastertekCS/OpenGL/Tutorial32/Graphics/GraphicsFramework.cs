using Silk.NET.Maths;
using RastertekCS.OpenGL.Tutorial32.System;
namespace RastertekCS.OpenGL.Tutorial32.Graphics;
public class GraphicsFramework
{
    private GL4 m_gl; private Camera m_cam; private Model m_model, m_winModel;
    private RenderTexture m_rt; private TextureShader m_texShader; private GlassShader m_glassShader;
    private float m_rotation = 360f;

    public bool Initialize(GL4 gl, int sw, int sh)
    {
        m_gl = gl; m_cam = new Camera(); m_cam.SetPosition(0, 0, -5); m_cam.Render();
        m_model = new Model(); if (!m_model.Initialize(gl, "Models/Cube.txt", "Data/stone01.tga", false, "Data/normal03.tga", false)) return false;
        m_winModel = new Model(); if (!m_winModel.Initialize(gl, "Models/square.txt", "Data/glass01.tga", false, "Data/normal03.tga", false)) return false;
        m_rt = new RenderTexture(); if (!m_rt.Initialize(gl, sw, sh, SystemConfiguration.ScreenNear, SystemConfiguration.ScreenDepth)) return false;
        m_texShader = new TextureShader(); if (!m_texShader.Initialize(gl)) return false;
        m_glassShader = new GlassShader(); if (!m_glassShader.Initialize(gl)) return false;
        return true;
    }
    public void Shutdown()
    { m_glassShader?.Shutdown(m_gl); m_texShader?.Shutdown(m_gl); m_rt?.Shutdown(m_gl); m_winModel?.Shutdown(m_gl); m_model?.Shutdown(m_gl); m_gl = null; }
    public bool Frame()
    { m_rotation -= 0.0174532925f; if (m_rotation <= 0) m_rotation += 360f; if (!RenderToTex(m_rotation)) return false; return Render(m_rotation); }

    bool RenderToTex(float rot)
    {
        m_rt.SetRenderTarget(m_gl); m_rt.ClearRenderTarget(m_gl, 0, 0, 0, 1);
        var w = Matrix4X4.CreateRotationY<float>(rot); var v = m_cam.GetViewMatrix(); var p = m_gl.GetProjectionMatrix();
        m_texShader.SetShaderParameters(m_gl, w, v, p); m_model.SetTexture1(m_gl, 0); m_model.Render(m_gl);
        m_gl.SetBackBufferRenderTarget(); m_gl.ResetViewport(); return true;
    }
    bool Render(float rot)
    {
        m_gl.BeginScene(0, 0, 0, 1);
        var v = m_cam.GetViewMatrix(); var p = m_gl.GetProjectionMatrix();
        var w = Matrix4X4.CreateRotationY<float>(rot);
        m_texShader.SetShaderParameters(m_gl, w, v, p); m_model.SetTexture1(m_gl, 0); m_model.Render(m_gl);
        w = Matrix4X4.CreateTranslation<float>(0, 0, -1.5f);
        m_glassShader.SetShaderParameters(m_gl, w, v, p, 0.01f);
        m_rt.SetTexture(m_gl, 2); m_winModel.SetTexture1(m_gl, 0); m_winModel.SetTexture2(m_gl, 1); m_winModel.Render(m_gl);
        m_gl.EndScene(); return true;
    }
}
