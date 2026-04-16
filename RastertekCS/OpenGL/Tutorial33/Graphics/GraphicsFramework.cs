using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial33.Graphics;

public class GraphicsFramework
{
    private GL4 m_gl;
    private Camera m_cam;
    private Model m_model;
    private FireShader m_fireShader;
    private float m_frameTime;

    public bool Initialize(GL4 gl, int sw, int sh)
    {
        m_gl = gl;
        m_cam = new Camera();
        m_cam.SetPosition(0, 0, -5);
        m_cam.Render();
        m_model = new Model();
        if (
            !m_model.Initialize(
                gl,
                "Models/square.txt",
                "Data/fire01.tga",
                false,
                "Data/noise01.tga",
                true,
                "Data/alpha01.tga",
                false
            )
        )
            return false;
        m_fireShader = new FireShader();
        if (!m_fireShader.Initialize(gl))
            return false;
        return true;
    }

    public void Shutdown()
    {
        m_fireShader?.Shutdown(m_gl);
        m_model?.Shutdown(m_gl);
        m_gl = null;
    }

    public bool Frame()
    {
        m_frameTime += 0.01f;
        if (m_frameTime > 1000f)
            m_frameTime = 0f;
        return Render();
    }

    bool Render()
    {
        m_gl.BeginScene(0, 0, 0, 1);
        var w = m_gl.GetWorldMatrix();
        var v = m_cam.GetViewMatrix();
        var p = m_gl.GetProjectionMatrix();
        float[] ss = { 1.3f, 2.1f, 2.3f };
        float[] sc = { 1f, 2f, 3f };
        float[] d1 = { 0.1f, 0.2f };
        float[] d2 = { 0.1f, 0.3f };
        float[] d3 = { 0.1f, 0.1f };
        m_gl.EnableAlphaBlending();
        m_fireShader.SetShaderParameters(
            m_gl,
            w,
            v,
            p,
            m_frameTime,
            ss,
            sc,
            d1,
            d2,
            d3,
            0.8f,
            0.5f
        );
        m_model.SetTexture1(m_gl, 0);
        m_model.SetTexture2(m_gl, 1);
        m_model.SetTexture3(m_gl, 2);
        m_model.Render(m_gl);
        m_gl.DisableAlphaBlending();
        m_gl.EndScene();
        return true;
    }
}
