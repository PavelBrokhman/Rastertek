using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial31.Graphics;

public class GraphicsFramework
{
    private const float SCREEN_DEPTH = 1000.0f;
    private const float SCREEN_NEAR = 0.3f;

    private DX11 m_DirectX;
    private Camera m_Camera;
    private Model m_GroundModel,
        m_WallModel,
        m_BathModel,
        m_WaterModel;
    private Light m_Light;
    private RenderTexture m_RefractionTexture,
        m_ReflectionTexture;
    private LightShader m_LightShader;
    private RefractionShader m_RefractionShader;
    private WaterShader m_WaterShader;
    private float m_waterHeight = 2.75f;
    private float m_waterTranslation;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(-10.0f, 6.0f, -10.0f);
        m_Camera.SetRotation(0.0f, 45.0f, 0.0f);
        m_Camera.Render();

        m_GroundModel = new Model();
        if (!m_GroundModel.Initialize(DirectX, "Models/ground.txt", "Data/ground01.tga", true))
            return false;

        m_WallModel = new Model();
        if (!m_WallModel.Initialize(DirectX, "Models/wall.txt", "Data/wall01.tga", true))
            return false;

        m_BathModel = new Model();
        if (!m_BathModel.Initialize(DirectX, "Models/bath.txt", "Data/marble01.tga", true))
            return false;

        m_WaterModel = new Model();
        if (!m_WaterModel.Initialize(DirectX, "Models/water.txt", "Data/water01.tga", true))
            return false;

        m_Light = new Light();
        m_Light.SetAmbientColor(0.15f, 0.15f, 0.15f, 1.0f);
        m_Light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        m_Light.SetDirection(0.0f, -1.0f, 0.5f);

        m_RefractionTexture = new RenderTexture();
        if (
            !m_RefractionTexture.Initialize(
                DirectX,
                screenWidth,
                screenHeight,
                SCREEN_DEPTH,
                SCREEN_NEAR
            )
        )
            return false;

        m_ReflectionTexture = new RenderTexture();
        if (
            !m_ReflectionTexture.Initialize(
                DirectX,
                screenWidth,
                screenHeight,
                SCREEN_DEPTH,
                SCREEN_NEAR
            )
        )
            return false;

        m_LightShader = new LightShader();
        if (!m_LightShader.Initialize(DirectX))
            return false;

        m_RefractionShader = new RefractionShader();
        if (!m_RefractionShader.Initialize(DirectX))
            return false;

        m_WaterShader = new WaterShader();
        if (!m_WaterShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        m_WaterShader?.Shutdown();
        m_RefractionShader?.Shutdown();
        m_LightShader?.Shutdown();
        m_ReflectionTexture?.Shutdown();
        m_RefractionTexture?.Shutdown();
        m_WaterModel?.Shutdown();
        m_BathModel?.Shutdown();
        m_WallModel?.Shutdown();
        m_GroundModel?.Shutdown();
        m_WaterShader = null;
        m_RefractionShader = null;
        m_LightShader = null;
        m_ReflectionTexture = null;
        m_RefractionTexture = null;
        m_WaterModel = null;
        m_BathModel = null;
        m_WallModel = null;
        m_GroundModel = null;
        m_Camera = null;
        m_DirectX = null;
    }

    public bool Frame()
    {
        m_waterTranslation += 0.001f;
        if (m_waterTranslation > 1.0f)
            m_waterTranslation -= 1.0f;

        if (!RenderRefractionToTexture())
            return false;
        if (!RenderReflectionToTexture())
            return false;
        return Render();
    }

    private bool RenderRefractionToTexture()
    {
        var clipPlane = new Vector4D<float>(0.0f, -1.0f, 0.0f, m_waterHeight + 0.1f);

        m_RefractionTexture.SetRenderTarget(m_DirectX);
        m_RefractionTexture.ClearRenderTarget(m_DirectX, 0.0f, 0.0f, 0.0f, 1.0f);

        var view = m_Camera.GetViewMatrix();
        var projection = m_RefractionTexture.GetProjectionMatrix();
        var world = Matrix4X4.CreateTranslation(0.0f, 2.0f, 0.0f);

        m_BathModel.Render(m_DirectX);
        if (
            !m_RefractionShader.Render(
                m_DirectX,
                m_BathModel.GetIndexCount(),
                world,
                view,
                projection,
                m_BathModel.GetTextureView(),
                m_Light.Direction,
                m_Light.DiffuseColor,
                m_Light.AmbientColor,
                clipPlane
            )
        )
            return false;

        m_DirectX.SetBackBufferRenderTarget();
        m_DirectX.ResetViewport();
        return true;
    }

    private bool RenderReflectionToTexture()
    {
        m_ReflectionTexture.SetRenderTarget(m_DirectX);
        m_ReflectionTexture.ClearRenderTarget(m_DirectX, 0.0f, 0.0f, 0.0f, 1.0f);

        m_Camera.RenderReflection(m_waterHeight);
        var reflectionView = m_Camera.GetReflectionViewMatrix();
        var projection = m_ReflectionTexture.GetProjectionMatrix();
        var world = Matrix4X4.CreateTranslation(0.0f, 6.0f, 8.0f);

        m_WallModel.Render(m_DirectX);
        if (
            !m_LightShader.Render(
                m_DirectX,
                m_WallModel.GetIndexCount(),
                world,
                reflectionView,
                projection,
                m_WallModel.GetTextureView(),
                m_Light.Direction,
                m_Light.DiffuseColor,
                m_Light.AmbientColor
            )
        )
            return false;

        m_DirectX.SetBackBufferRenderTarget();
        m_DirectX.ResetViewport();
        return true;
    }

    private bool Render()
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var view = m_Camera.GetViewMatrix();
        var projection = m_DirectX.GetProjectionMatrix();

        var world = Matrix4X4.CreateTranslation(0.0f, 1.0f, 0.0f);
        m_GroundModel.Render(m_DirectX);
        if (
            !m_LightShader.Render(
                m_DirectX,
                m_GroundModel.GetIndexCount(),
                world,
                view,
                projection,
                m_GroundModel.GetTextureView(),
                m_Light.Direction,
                m_Light.DiffuseColor,
                m_Light.AmbientColor
            )
        )
            return false;

        world = Matrix4X4.CreateTranslation(0.0f, 6.0f, 8.0f);
        m_WallModel.Render(m_DirectX);
        if (
            !m_LightShader.Render(
                m_DirectX,
                m_WallModel.GetIndexCount(),
                world,
                view,
                projection,
                m_WallModel.GetTextureView(),
                m_Light.Direction,
                m_Light.DiffuseColor,
                m_Light.AmbientColor
            )
        )
            return false;

        world = Matrix4X4.CreateTranslation(0.0f, 2.0f, 0.0f);
        m_BathModel.Render(m_DirectX);
        if (
            !m_LightShader.Render(
                m_DirectX,
                m_BathModel.GetIndexCount(),
                world,
                view,
                projection,
                m_BathModel.GetTextureView(),
                m_Light.Direction,
                m_Light.DiffuseColor,
                m_Light.AmbientColor
            )
        )
            return false;

        var reflectionView = m_Camera.GetReflectionViewMatrix();
        world = Matrix4X4.CreateTranslation(0.0f, m_waterHeight, 0.0f);
        m_WaterModel.Render(m_DirectX);
        if (
            !m_WaterShader.Render(
                m_DirectX,
                m_WaterModel.GetIndexCount(),
                world,
                view,
                projection,
                reflectionView,
                m_ReflectionTexture.GetShaderResourceView(),
                m_RefractionTexture.GetShaderResourceView(),
                m_WaterModel.GetTextureView(),
                m_waterTranslation,
                0.01f
            )
        )
            return false;

        m_DirectX.EndScene();
        return true;
    }
}
