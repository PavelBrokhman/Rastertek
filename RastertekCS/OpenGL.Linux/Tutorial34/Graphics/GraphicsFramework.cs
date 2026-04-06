using Silk.NET.Maths;
using RastertekCS.OpenGL.Tutorial34.Inputs;
namespace RastertekCS.OpenGL.Tutorial34.Graphics;
public class GraphicsFramework
{
    private GL4 m_gl; private Camera m_cam; private Model m_floor, m_billboard; private TextureShader m_texShader; private Position m_pos; private Timer m_timer;
    public bool Initialize(GL4 gl, int sw, int sh)
    { m_gl = gl; m_cam = new Camera(); m_cam.SetPosition(0, 0, -10); m_cam.Render();
      m_texShader = new TextureShader(); if (!m_texShader.Initialize(gl)) return false;
      m_floor = new Model(); if (!m_floor.Initialize(gl, "Data/floor.txt", "Data/grid01.tga", false)) return false;
      m_billboard = new Model(); if (!m_billboard.Initialize(gl, "Data/square.txt", "Data/stone01.tga", false)) return false;
      m_pos = new Position(); m_pos.SetPosition(0, 1.5f, -11);
      m_timer = new Timer(); m_timer.Initialize(); return true; }
    public void Shutdown() { m_texShader?.Shutdown(m_gl); m_floor?.Shutdown(m_gl); m_billboard?.Shutdown(m_gl); m_gl = null; }
    public bool Frame(Input input)
    { m_timer.Frame(); m_pos.SetFrameTime(m_timer.GetTime());
      m_pos.MoveLeft(input.IsLeftArrowPressed()); m_pos.MoveRight(input.IsRightArrowPressed());
      var (px, py, pz) = m_pos.GetPosition(); m_cam.SetPosition(px, py, pz); m_cam.Render(); return Render(); }
    bool Render()
    { m_gl.BeginScene(0, 0, 0, 1); var w = m_gl.GetWorldMatrix(); var v = m_cam.GetViewMatrix(); var p = m_gl.GetProjectionMatrix();
      m_texShader.SetShaderParameters(m_gl, w, v, p); m_floor.SetTexture(m_gl, 0); m_floor.Render(m_gl);
      var camPos = m_cam.GetPosition(); float[] modelPos = { 0, 1.5f, 0 };
      float angle = MathF.Atan2(modelPos[0] - camPos[0], modelPos[2] - camPos[2]);
      var rotMat = Matrix4X4.CreateRotationY<float>(angle);
      var transMat = Matrix4X4.CreateTranslation<float>(modelPos[0], modelPos[1], modelPos[2]);
      w = rotMat * transMat;
      m_texShader.SetShaderParameters(m_gl, w, v, p); m_billboard.SetTexture(m_gl, 0); m_billboard.Render(m_gl);
      m_gl.EndScene(); return true; }
}
