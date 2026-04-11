using RastertekCS.Windows.Tutorial23.Inputs;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial23.Graphics;

public class GraphicsFramework
{
    private DX11 m_DirectX;
    private Input m_Input;
    private Camera m_Camera;
    private Model m_Model;
    private Light m_Light;
    private LightShader m_LightShader;
    private FontShader m_FontShader;
    private Font m_Font;
    private Text m_RenderCountString;
    private Position m_Position;
    private ModelList m_ModelList;
    private Frustum m_Frustum;
    private int m_screenWidth, m_screenHeight;

    public bool Initialize(DX11 DirectX, Input input, int screenWidth, int screenHeight)
    {
        m_DirectX = DirectX;
        m_Input = input;
        m_screenWidth = screenWidth;
        m_screenHeight = screenHeight;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);
        m_Camera.Render();

        m_Model = new Model();
        if (!m_Model.Initialize(DirectX, "Models/sphere.txt", "Data/stone01.tga", true)) return false;

        m_Light = new Light();
        m_Light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        m_Light.SetDirection(0.0f, 0.0f, 1.0f);

        m_LightShader = new LightShader();
        if (!m_LightShader.Initialize(DirectX)) return false;

        m_FontShader = new FontShader();
        if (!m_FontShader.Initialize(DirectX)) return false;

        m_Font = new Font();
        if (!m_Font.Initialize(DirectX, 0)) return false;

        m_RenderCountString = new Text();
        if (!m_RenderCountString.Initialize(DirectX, screenWidth, screenHeight, 32, m_Font,
                                             "Render Count: 0", 10, 10, 1.0f, 1.0f, 1.0f)) return false;

        m_Position = new Position();
        m_ModelList = new ModelList();
        m_ModelList.Initialize(25);
        m_Frustum = new Frustum();

        return true;
    }

    public void Shutdown()
    {
        m_RenderCountString?.Shutdown();
        m_Font?.Shutdown();
        m_FontShader?.Shutdown();
        m_LightShader?.Shutdown();
        m_Model?.Shutdown();
        m_Frustum = null;
        m_ModelList?.Shutdown();
        m_ModelList = null;
        m_Position = null;
        m_RenderCountString = null;
        m_Font = null;
        m_FontShader = null;
        m_LightShader = null;
        m_Light = null;
        m_Model = null;
        m_Camera = null;
        m_Input = null;
        m_DirectX = null;
    }

    public bool Frame(float frameTimeMs)
    {
        m_Position.SetFrameTime(frameTimeMs / 1000.0f);
        m_Position.TurnLeft(m_Input.IsKeyDown(Key.Left));
        m_Position.TurnRight(m_Input.IsKeyDown(Key.Right));
        m_Camera.SetRotation(0.0f, m_Position.GetRotation(), 0.0f);
        m_Camera.Render();
        return Render();
    }

    private bool Render()
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = m_DirectX.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var projection = m_DirectX.GetProjectionMatrix();
        var ortho = m_DirectX.GetOrthoMatrix();

        m_Frustum.ConstructFrustum(view, projection);

        int renderCount = 0;
        int modelCount = m_ModelList.GetModelCount();
        for (int i = 0; i < modelCount; i++)
        {
            m_ModelList.GetData(i, out float px, out float py, out float pz);
            if (m_Frustum.CheckSphere(px, py, pz, 1.0f))
            {
                var worldT = Matrix4X4.CreateTranslation(px, py, pz);
                m_Model.Render(m_DirectX);
                m_Model.SetTexture(m_DirectX, 0);
                if (!m_LightShader.Render(m_DirectX, m_Model.GetIndexCount(), worldT, view, projection,
                    m_Light.GetDirection(), m_Light.GetDiffuseColor()))
                    return false;
                renderCount++;
            }
        }

        m_DirectX.TurnZBufferOff();
        m_DirectX.EnableAlphaBlending();

        m_RenderCountString.UpdateText(m_DirectX, m_Font, $"Render Count: {renderCount}", 10, 10, 1.0f, 1.0f, 1.0f);
        m_Font.SetTexture(m_DirectX, 0);
        m_RenderCountString.Render(m_DirectX);
        if (!m_FontShader.Render(m_DirectX, m_RenderCountString.GetIndexCount(), world, view, ortho,
                                  m_RenderCountString.GetPixelColor()))
            return false;

        m_DirectX.DisableAlphaBlending();
        m_DirectX.TurnZBufferOn();

        m_DirectX.EndScene();
        return true;
    }
}
