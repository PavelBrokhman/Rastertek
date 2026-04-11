namespace RastertekCS.OpenGL.Tutorial21.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT_COLOR = 0;
    private const uint TEXTURE_UNIT_NORMAL = 1;
    private const uint TEXTURE_UNIT_SPECULAR = 2;

    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_Model;
    private SpecMapShader m_SpecMapShader;
    private Light m_Light;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -5.0f);
        m_Camera.Render();

        m_SpecMapShader = new SpecMapShader();
        if (!m_SpecMapShader.Initialize(OpenGL)) return false;

        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Models/Cube.txt",
            "Data/stone02.tga", true,
            "Data/normal02.tga", true,
            "Data/spec02.tga", false)) return false;

        m_Light = new Light();
        m_Light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        m_Light.SetDirection(0.0f, 0.0f, 1.0f);
        m_Light.SetSpecularColor(1.0f, 1.0f, 1.0f, 1.0f);
        m_Light.SetSpecularPower(16.0f);

        return true;
    }

    public void Shutdown()
    {
        m_Light = null;
        m_Model?.Shutdown(m_OpenGL); m_Model = null;
        m_SpecMapShader?.Shutdown(m_OpenGL); m_SpecMapShader = null;
        m_Camera = null; m_OpenGL = null;
    }

    private float m_rotation = 360.0f;

    public bool Frame()
    {
        m_rotation -= 0.0174532925f * 1.0f;
        if (m_rotation < 0.0f) m_rotation += MathF.Tau;
        return Render(m_rotation);
    }

    private bool Render(float rotation)
    {
        m_OpenGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = Silk.NET.Maths.Matrix4X4.CreateRotationY<float>(rotation);
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        var lightDirection = m_Light.GetDirection();
        var diffuseLightColor = m_Light.GetDiffuseColor();
        var specularColor = m_Light.GetSpecularColor();
        float specularPower = m_Light.GetSpecularPower();
        var cameraPosition = m_Camera.GetPosition();

        m_SpecMapShader.SetShader(m_OpenGL);
        m_Model.SetTextures(m_OpenGL, TEXTURE_UNIT_COLOR, TEXTURE_UNIT_NORMAL, TEXTURE_UNIT_SPECULAR);

        if (!m_SpecMapShader.SetShaderParameters(m_OpenGL, world, view, projection,
            lightDirection, diffuseLightColor, cameraPosition,
            specularColor, specularPower,
            (int)TEXTURE_UNIT_COLOR, (int)TEXTURE_UNIT_NORMAL, (int)TEXTURE_UNIT_SPECULAR)) return false;

        m_Model.Render(m_OpenGL);

        m_OpenGL.EndScene();
        return true;
    }
}
