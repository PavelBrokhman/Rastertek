using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial22.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT_COLOR = 0;
    private const uint TEXTURE_UNIT_NORMAL = 1;

    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_Model;
    private Light m_Light;
    private ShaderManager m_ShaderManager;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;

        // Create and initialize the camera.
        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);
        m_Camera.Render();

        // Create and initialize the model with color and normal map textures.
        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Models/sphere.txt",
            "Data/stone01.tga", true,
            "Data/normal01.tga", true)) return false;

        // Create and initialize the light.
        m_Light = new Light();
        m_Light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        m_Light.SetDirection(0.0f, 0.0f, 1.0f);

        // Create and initialize the shader manager.
        m_ShaderManager = new ShaderManager();
        if (!m_ShaderManager.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_ShaderManager?.Shutdown(m_OpenGL); m_ShaderManager = null;
        m_Light = null;
        m_Model?.Shutdown(m_OpenGL); m_Model = null;
        m_Camera = null; m_OpenGL = null;
    }

    private float m_rotation = 360.0f;

    public bool Frame()
    {
        m_rotation -= 0.0174532925f * 1.0f;
        if (m_rotation <= 0.0f) m_rotation += 360.0f;
        return Render(m_rotation);
    }

    private bool Render(float rotation)
    {
        m_OpenGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        var rotateMatrix = Matrix4X4.CreateRotationY<float>(rotation);

        var lightDirection = m_Light.GetDirection();
        var diffuseLightColor = m_Light.GetDiffuseColor();

        // Render sphere 1 (top) - texture shader only.
        var translateMatrix = Matrix4X4.CreateTranslation<float>(0.0f, 1.0f, 0.0f);
        var worldMatrix = rotateMatrix * translateMatrix;

        if (!m_ShaderManager.RenderTextureShader(m_OpenGL, worldMatrix, view, projection,
            (int)TEXTURE_UNIT_COLOR)) return false;
        m_Model.SetTexture1(m_OpenGL, TEXTURE_UNIT_COLOR);
        m_Model.Render(m_OpenGL);

        // Render sphere 2 (bottom-left) - light shader.
        translateMatrix = Matrix4X4.CreateTranslation<float>(-1.5f, -1.0f, 0.0f);
        worldMatrix = rotateMatrix * translateMatrix;

        if (!m_ShaderManager.RenderLightShader(m_OpenGL, worldMatrix, view, projection,
            lightDirection, diffuseLightColor,
            (int)TEXTURE_UNIT_COLOR)) return false;
        m_Model.SetTexture1(m_OpenGL, TEXTURE_UNIT_COLOR);
        m_Model.Render(m_OpenGL);

        // Render sphere 3 (bottom-right) - normal map shader.
        translateMatrix = Matrix4X4.CreateTranslation<float>(1.5f, -1.0f, 0.0f);
        worldMatrix = rotateMatrix * translateMatrix;

        if (!m_ShaderManager.RenderNormalMapShader(m_OpenGL, worldMatrix, view, projection,
            lightDirection, diffuseLightColor,
            (int)TEXTURE_UNIT_COLOR, (int)TEXTURE_UNIT_NORMAL)) return false;
        m_Model.SetTexture1(m_OpenGL, TEXTURE_UNIT_COLOR);
        m_Model.SetTexture2(m_OpenGL, TEXTURE_UNIT_NORMAL);
        m_Model.Render(m_OpenGL);

        m_OpenGL.EndScene();
        return true;
    }
}
