using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial30.Graphics;

public class GraphicsFramework
{
    private const float SCREEN_DEPTH = 1000.0f;
    private const float SCREEN_NEAR = 0.3f;

    private DX11 m_DirectX;
    private Camera m_Camera;
    private Model m_CubeModel;
    private Model m_FloorModel;
    private RenderTexture m_RenderTexture;
    private TextureShader m_TextureShader;
    private ReflectionShader m_ReflectionShader;
    private float m_rotation = MathF.Tau;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);
        m_Camera.Render();

        m_CubeModel = new Model();
        if (!m_CubeModel.Initialize(DirectX, "Models/Cube.txt", "Data/stone01.tga", true))
            return false;

        m_FloorModel = new Model();
        if (!m_FloorModel.Initialize(DirectX, "Models/floor.txt", "Data/blue01.tga", true))
            return false;

        m_RenderTexture = new RenderTexture();
        if (
            !m_RenderTexture.Initialize(
                DirectX,
                screenWidth,
                screenHeight,
                SCREEN_DEPTH,
                SCREEN_NEAR
            )
        )
            return false;

        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(DirectX))
            return false;

        m_ReflectionShader = new ReflectionShader();
        if (!m_ReflectionShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        m_ReflectionShader?.Shutdown();
        m_TextureShader?.Shutdown();
        m_RenderTexture?.Shutdown();
        m_FloorModel?.Shutdown();
        m_CubeModel?.Shutdown();
        m_ReflectionShader = null;
        m_TextureShader = null;
        m_RenderTexture = null;
        m_FloorModel = null;
        m_CubeModel = null;
        m_Camera = null;
        m_DirectX = null;
    }

    public bool Frame()
    {
        m_rotation -= 0.0174532925f * 1.0f;
        if (m_rotation < 0.0f)
            m_rotation += MathF.Tau;
        if (!RenderReflectionToTexture())
            return false;
        return Render();
    }

    private bool RenderReflectionToTexture()
    {
        m_RenderTexture.SetRenderTarget(m_DirectX);
        m_RenderTexture.ClearRenderTarget(m_DirectX, 0.0f, 0.0f, 0.0f, 1.0f);

        m_Camera.RenderReflection(-1.5f);
        var reflectionView = m_Camera.GetReflectionViewMatrix();
        var projection = m_DirectX.GetProjectionMatrix();
        var world = Matrix4X4.CreateRotationY(m_rotation);

        m_CubeModel.Render(m_DirectX);
        if (
            !m_TextureShader.Render(
                m_DirectX,
                m_CubeModel.GetIndexCount(),
                world,
                reflectionView,
                projection,
                m_CubeModel.GetTextureView()
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
        var world = Matrix4X4.CreateRotationY(m_rotation);

        m_CubeModel.Render(m_DirectX);
        if (
            !m_TextureShader.Render(
                m_DirectX,
                m_CubeModel.GetIndexCount(),
                world,
                view,
                projection,
                m_CubeModel.GetTextureView()
            )
        )
            return false;

        var floorWorld = Matrix4X4.CreateTranslation(0.0f, -1.5f, 0.0f);
        var reflectionView = m_Camera.GetReflectionViewMatrix();
        m_FloorModel.Render(m_DirectX);
        if (
            !m_ReflectionShader.Render(
                m_DirectX,
                m_FloorModel.GetIndexCount(),
                floorWorld,
                view,
                projection,
                m_FloorModel.GetTextureView(),
                m_RenderTexture.GetShaderResourceView(),
                reflectionView
            )
        )
            return false;

        m_DirectX.EndScene();
        return true;
    }
}
