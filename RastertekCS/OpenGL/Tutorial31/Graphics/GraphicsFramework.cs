using RastertekCS.OpenGL.Tutorial31.System;
using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial31.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
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
    private float m_waterHeight = 2.75f,
        m_waterTranslation;

    public bool Initialize(GL4 OpenGL, int sw, int sh)
    {
        m_OpenGL = OpenGL;
        m_Camera = new Camera();
        m_Camera.SetPosition(-10, 6, -10);
        m_Camera.SetRotation(0, 45, 0);
        m_Camera.Render();

        m_GroundModel = new Model();
        if (!m_GroundModel.Initialize(OpenGL, "Models/ground.txt", "Data/ground01.tga", true))
            return false;
        m_WallModel = new Model();
        if (!m_WallModel.Initialize(OpenGL, "Models/wall.txt", "Data/wall01.tga", true))
            return false;
        m_BathModel = new Model();
        if (!m_BathModel.Initialize(OpenGL, "Models/bath.txt", "Data/marble01.tga", true))
            return false;
        m_WaterModel = new Model();
        if (!m_WaterModel.Initialize(OpenGL, "Models/water.txt", "Data/water01.tga", true))
            return false;

        m_Light = new Light();
        m_Light.SetAmbientLight(0.15f, 0.15f, 0.15f, 1);
        m_Light.SetDiffuseColor(1, 1, 1, 1);
        m_Light.SetDirection(0, -1, 0.5f);

        m_RefractionTexture = new RenderTexture();
        if (
            !m_RefractionTexture.Initialize(
                OpenGL,
                sw,
                sh,
                SystemConfiguration.ScreenNear,
                SystemConfiguration.ScreenDepth
            )
        )
            return false;
        m_ReflectionTexture = new RenderTexture();
        if (
            !m_ReflectionTexture.Initialize(
                OpenGL,
                sw,
                sh,
                SystemConfiguration.ScreenNear,
                SystemConfiguration.ScreenDepth
            )
        )
            return false;

        m_LightShader = new LightShader();
        if (!m_LightShader.Initialize(OpenGL))
            return false;
        m_RefractionShader = new RefractionShader();
        if (!m_RefractionShader.Initialize(OpenGL))
            return false;
        m_WaterShader = new WaterShader();
        if (!m_WaterShader.Initialize(OpenGL))
            return false;

        return true;
    }

    public void Shutdown()
    {
        m_WaterShader?.Shutdown(m_OpenGL);
        m_RefractionShader?.Shutdown(m_OpenGL);
        m_LightShader?.Shutdown(m_OpenGL);
        m_ReflectionTexture?.Shutdown(m_OpenGL);
        m_RefractionTexture?.Shutdown(m_OpenGL);
        m_WaterModel?.Shutdown(m_OpenGL);
        m_BathModel?.Shutdown(m_OpenGL);
        m_WallModel?.Shutdown(m_OpenGL);
        m_GroundModel?.Shutdown(m_OpenGL);
        m_OpenGL = null;
    }

    public bool Frame()
    {
        m_waterTranslation += 0.001f;
        if (m_waterTranslation > 1)
            m_waterTranslation -= 1;
        if (!RenderRefractionToTexture())
            return false;
        if (!RenderReflectionToTexture())
            return false;
        return Render();
    }

    private bool RenderRefractionToTexture()
    {
        float[] clipPlane = { 0, -1, 0, m_waterHeight + 0.1f };
        m_RefractionTexture.SetRenderTarget(m_OpenGL);
        m_RefractionTexture.ClearRenderTarget(m_OpenGL, 0, 0, 0, 1);
        var world = Matrix4X4.CreateTranslation<float>(0, 2, 0);
        var view = m_Camera.GetViewMatrix();
        var proj = m_RefractionTexture.GetProjectionMatrix();
        m_OpenGL.EnableClipping();
        m_RefractionShader.SetShaderParameters(
            m_OpenGL,
            world,
            view,
            proj,
            m_Light.Direction,
            m_Light.DiffuseColor,
            m_Light.AmbientLight,
            clipPlane
        );
        m_BathModel.SetTexture(m_OpenGL, 0);
        m_BathModel.Render(m_OpenGL);
        m_OpenGL.DisableClipping();
        m_OpenGL.SetBackBufferRenderTarget();
        m_OpenGL.ResetViewport();
        return true;
    }

    private bool RenderReflectionToTexture()
    {
        m_ReflectionTexture.SetRenderTarget(m_OpenGL);
        m_ReflectionTexture.ClearRenderTarget(m_OpenGL, 0, 0, 0, 1);
        m_Camera.RenderReflection(m_waterHeight);
        var reflView = m_Camera.GetReflectionViewMatrix();
        var world = Matrix4X4.CreateTranslation<float>(0, 6, 8);
        var proj = m_ReflectionTexture.GetProjectionMatrix();
        m_LightShader.SetShaderParameters(
            m_OpenGL,
            world,
            reflView,
            proj,
            m_Light.Direction,
            m_Light.DiffuseColor,
            m_Light.AmbientLight
        );
        m_WallModel.SetTexture(m_OpenGL, 0);
        m_WallModel.Render(m_OpenGL);
        m_OpenGL.SetBackBufferRenderTarget();
        m_OpenGL.ResetViewport();
        return true;
    }

    private bool Render()
    {
        m_OpenGL.BeginScene(0, 0, 0, 1);
        var view = m_Camera.GetViewMatrix();
        var proj = m_OpenGL.GetProjectionMatrix();

        var world = Matrix4X4.CreateTranslation<float>(0, 1, 0);
        m_LightShader.SetShaderParameters(
            m_OpenGL,
            world,
            view,
            proj,
            m_Light.Direction,
            m_Light.DiffuseColor,
            m_Light.AmbientLight
        );
        m_GroundModel.SetTexture(m_OpenGL, 0);
        m_GroundModel.Render(m_OpenGL);

        world = Matrix4X4.CreateTranslation<float>(0, 6, 8);
        m_LightShader.SetShaderParameters(
            m_OpenGL,
            world,
            view,
            proj,
            m_Light.Direction,
            m_Light.DiffuseColor,
            m_Light.AmbientLight
        );
        m_WallModel.SetTexture(m_OpenGL, 0);
        m_WallModel.Render(m_OpenGL);

        world = Matrix4X4.CreateTranslation<float>(0, 2, 0);
        m_LightShader.SetShaderParameters(
            m_OpenGL,
            world,
            view,
            proj,
            m_Light.Direction,
            m_Light.DiffuseColor,
            m_Light.AmbientLight
        );
        m_BathModel.SetTexture(m_OpenGL, 0);
        m_BathModel.Render(m_OpenGL);

        var reflView = m_Camera.GetReflectionViewMatrix();
        world = Matrix4X4.CreateTranslation<float>(0, m_waterHeight, 0);
        m_WaterShader.SetShaderParameters(
            m_OpenGL,
            world,
            view,
            proj,
            reflView,
            m_waterTranslation,
            0.01f
        );
        m_RefractionTexture.SetTexture(m_OpenGL, 1);
        m_ReflectionTexture.SetTexture(m_OpenGL, 2);
        m_WaterModel.SetTexture(m_OpenGL, 0);
        m_WaterModel.Render(m_OpenGL);

        m_OpenGL.EndScene();
        return true;
    }
}
