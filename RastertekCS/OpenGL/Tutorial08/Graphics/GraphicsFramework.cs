using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial08.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_Model;
    private LightShader m_LightShader;
    private Light m_Light;
    private float m_rotation;

    public bool Initialize(GL4 OpenGL)
    {
        m_OpenGL = OpenGL;
        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);

        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Models/Cube.txt", "Data/Stone01.tga", TEXTURE_UNIT, true))
            return false;

        m_LightShader = new LightShader();
        if (!m_LightShader.Initialize(OpenGL))
            return false;

        m_Light = new Light();
        m_Light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        m_Light.SetDirection(0.0f, 0.0f, 1.0f);
        return true;
    }

    public void Shutdown()
    {
        m_LightShader?.Shutdown(m_OpenGL);
        m_Model?.Shutdown(m_OpenGL);
        m_LightShader = null;
        m_Model = null;
        m_Camera = null;
        m_Light = null;
        m_OpenGL = null;
    }

    public bool Frame()
    {
        // Вращаем куб для наглядности освещения.
        m_rotation += 0.01f;
        if (m_rotation > MathF.Tau)
            m_rotation -= MathF.Tau;
        return Render();
    }

    private bool Render()
    {
        m_OpenGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        m_Camera.Render();
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        m_LightShader.SetShader(m_OpenGL);

        // Первый куб: rotation + translation (слева).
        var rotate1 = Matrix4X4.CreateRotationY(m_rotation);
        var translate1 = Matrix4X4.CreateTranslation(-2.0f, 0.0f, 0.0f);
        var world1 = rotate1 * translate1;

        if (
            !m_LightShader.SetShaderParameters(
                m_OpenGL,
                world1,
                view,
                projection,
                (int)TEXTURE_UNIT,
                m_Light.GetDirection(),
                m_Light.GetDiffuseColor()
            )
        )
            return false;
        m_Model.Render(m_OpenGL);

        // Второй куб: scale + rotation + translation (справа).
        var scale2 = Matrix4X4.CreateScale(0.5f);
        var rotate2 = Matrix4X4.CreateRotationY(m_rotation);
        var translate2 = Matrix4X4.CreateTranslation(2.0f, 0.0f, 0.0f);
        var world2 = scale2 * rotate2 * translate2;

        if (
            !m_LightShader.SetShaderParameters(
                m_OpenGL,
                world2,
                view,
                projection,
                (int)TEXTURE_UNIT,
                m_Light.GetDirection(),
                m_Light.GetDiffuseColor()
            )
        )
            return false;
        m_Model.Render(m_OpenGL);

        m_OpenGL.EndScene();
        return true;
    }
}
