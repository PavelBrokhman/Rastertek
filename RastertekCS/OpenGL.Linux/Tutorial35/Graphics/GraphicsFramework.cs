namespace RastertekCS.OpenGL.Tutorial35.Graphics;
public class GraphicsFramework
{
    private GL4 m_gl; private Camera m_cam; private DepthShader m_depthShader; private Model m_model;
    public bool Initialize(GL4 gl, int sw, int sh)
    { m_gl = gl; m_cam = new Camera(); m_cam.SetPosition(0, 2, -10); m_cam.Render();
      m_depthShader = new DepthShader(); if (!m_depthShader.Initialize(gl)) return false;
      m_model = new Model(); if (!m_model.Initialize(gl, "Data/floor.txt")) return false; return true; }
    public void Shutdown() { m_depthShader?.Shutdown(m_gl); m_model?.Shutdown(m_gl); m_gl = null; }
    public bool Frame() => Render();
    bool Render()
    { m_gl.BeginScene(0, 0, 0, 1); var w = m_gl.GetWorldMatrix(); var v = m_cam.GetViewMatrix(); var p = m_gl.GetProjectionMatrix();
      m_depthShader.SetShaderParameters(m_gl, w, v, p); m_model.Render(m_gl); m_gl.EndScene(); return true; }
}
