using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial20.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT_COLOR = 0;
    private const uint TEXTURE_UNIT_NORMAL = 1;

    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_Model;
    private NormalMapShader m_NormalMapShader;
    private Light m_Light;
    private float m_rotation = 360.0f;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;

        // Create and initialize camera.
        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -5);
        m_Camera.Render();

        // Create and initialize the normal map shader.
        m_NormalMapShader = new NormalMapShader();
        if (!m_NormalMapShader.Initialize(OpenGL)) return false;

        // Create and initialize the model with color texture and normal map.
        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Models/Cube.txt",
            "Data/stone01.tga", TEXTURE_UNIT_COLOR,
            "Data/normal01.tga", TEXTURE_UNIT_NORMAL)) return false;

        // Create and initialize the light.
        m_Light = new Light();
        m_Light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        m_Light.SetDirection(0.0f, 0.0f, 1.0f);

        return true;
    }

    public void Shutdown()
    {
        m_Light = null;
        m_NormalMapShader?.Shutdown(m_OpenGL); m_NormalMapShader = null;
        m_Model?.Shutdown(m_OpenGL); m_Model = null;
        m_Camera = null; m_OpenGL = null;
    }

    public bool Frame()
    {
        // Update rotation each frame.
        m_rotation -= 0.0174532925f * 1.0f;
        if (m_rotation < 0.0f)
        {
            m_rotation += MathF.Tau;
        }

        return Render(m_rotation);
    }

    private bool Render(float rotation)
    {
        m_OpenGL.BeginScene(0, 0, 0, 1);

        var world = Silk.NET.Maths.Matrix4X4.CreateRotationY<float>(rotation);
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        // Get light properties.
        float[] lightDirection = m_Light.GetDirection();
        float[] diffuseColor = m_Light.GetDiffuseColor();

        // Set shader and parameters.
        m_NormalMapShader.SetShader(m_OpenGL);

        // Set textures.
        m_Model.SetTextures(m_OpenGL, TEXTURE_UNIT_COLOR, TEXTURE_UNIT_NORMAL);

        if (!m_NormalMapShader.SetShaderParameters(m_OpenGL, world, view, projection,
            lightDirection, diffuseColor,
            (int)TEXTURE_UNIT_COLOR, (int)TEXTURE_UNIT_NORMAL)) return false;

        // Render model.
        m_Model.Render(m_OpenGL);

        m_OpenGL.EndScene();
        return true;
    }
}
