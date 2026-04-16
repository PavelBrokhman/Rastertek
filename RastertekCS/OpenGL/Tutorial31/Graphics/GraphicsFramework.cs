using RastertekCS.OpenGL.Tutorial31.System;
using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial31.Graphics;

public class GraphicsFramework
{
    private GL4 _openGL;
    private Camera _camera;
    private Model _groundModel,
        _wallModel,
        _bathModel,
        _waterModel;
    private Light _light;
    private RenderTexture _refractionTexture,
        _reflectionTexture;
    private LightShader _lightShader;
    private RefractionShader _refractionShader;
    private WaterShader _waterShader;
    private float _waterHeight = 2.75f,
        _waterTranslation;

    public bool Initialize(GL4 OpenGL, int sw, int sh)
    {
        _openGL = OpenGL;
        _camera = new Camera();
        _camera.SetPosition(-10, 6, -10);
        _camera.SetRotation(0, 45, 0);
        _camera.Render();

        _groundModel = new Model();
        if (!_groundModel.Initialize(OpenGL, "Models/ground.txt", "Data/ground01.tga", true))
            return false;
        _wallModel = new Model();
        if (!_wallModel.Initialize(OpenGL, "Models/wall.txt", "Data/wall01.tga", true))
            return false;
        _bathModel = new Model();
        if (!_bathModel.Initialize(OpenGL, "Models/bath.txt", "Data/marble01.tga", true))
            return false;
        _waterModel = new Model();
        if (!_waterModel.Initialize(OpenGL, "Models/water.txt", "Data/water01.tga", true))
            return false;

        _light = new Light();
        _light.SetAmbientLight(0.15f, 0.15f, 0.15f, 1);
        _light.SetDiffuseColor(1, 1, 1, 1);
        _light.SetDirection(0, -1, 0.5f);

        _refractionTexture = new RenderTexture();
        if (
            !_refractionTexture.Initialize(
                OpenGL,
                sw,
                sh,
                SystemConfiguration.ScreenNear,
                SystemConfiguration.ScreenDepth
            )
        )
            return false;
        _reflectionTexture = new RenderTexture();
        if (
            !_reflectionTexture.Initialize(
                OpenGL,
                sw,
                sh,
                SystemConfiguration.ScreenNear,
                SystemConfiguration.ScreenDepth
            )
        )
            return false;

        _lightShader = new LightShader();
        if (!_lightShader.Initialize(OpenGL))
            return false;
        _refractionShader = new RefractionShader();
        if (!_refractionShader.Initialize(OpenGL))
            return false;
        _waterShader = new WaterShader();
        if (!_waterShader.Initialize(OpenGL))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _waterShader?.Shutdown(_openGL);
        _refractionShader?.Shutdown(_openGL);
        _lightShader?.Shutdown(_openGL);
        _reflectionTexture?.Shutdown(_openGL);
        _refractionTexture?.Shutdown(_openGL);
        _waterModel?.Shutdown(_openGL);
        _bathModel?.Shutdown(_openGL);
        _wallModel?.Shutdown(_openGL);
        _groundModel?.Shutdown(_openGL);
        _openGL = null;
    }

    public bool Frame()
    {
        _waterTranslation += 0.001f;
        if (_waterTranslation > 1)
            _waterTranslation -= 1;
        if (!RenderRefractionToTexture())
            return false;
        if (!RenderReflectionToTexture())
            return false;
        return Render();
    }

    private bool RenderRefractionToTexture()
    {
        float[] clipPlane = { 0, -1, 0, _waterHeight + 0.1f };
        _refractionTexture.SetRenderTarget(_openGL);
        _refractionTexture.ClearRenderTarget(_openGL, 0, 0, 0, 1);
        var world = Matrix4X4.CreateTranslation<float>(0, 2, 0);
        var view = _camera.GetViewMatrix();
        var proj = _refractionTexture.GetProjectionMatrix();
        _openGL.EnableClipping();
        _refractionShader.SetShaderParameters(
            _openGL,
            world,
            view,
            proj,
            _light.Direction,
            _light.DiffuseColor,
            _light.AmbientLight,
            clipPlane
        );
        _bathModel.SetTexture(_openGL, 0);
        _bathModel.Render(_openGL);
        _openGL.DisableClipping();
        _openGL.SetBackBufferRenderTarget();
        _openGL.ResetViewport();
        return true;
    }

    private bool RenderReflectionToTexture()
    {
        _reflectionTexture.SetRenderTarget(_openGL);
        _reflectionTexture.ClearRenderTarget(_openGL, 0, 0, 0, 1);
        _camera.RenderReflection(_waterHeight);
        var reflView = _camera.GetReflectionViewMatrix();
        var world = Matrix4X4.CreateTranslation<float>(0, 6, 8);
        var proj = _reflectionTexture.GetProjectionMatrix();
        _lightShader.SetShaderParameters(
            _openGL,
            world,
            reflView,
            proj,
            _light.Direction,
            _light.DiffuseColor,
            _light.AmbientLight
        );
        _wallModel.SetTexture(_openGL, 0);
        _wallModel.Render(_openGL);
        _openGL.SetBackBufferRenderTarget();
        _openGL.ResetViewport();
        return true;
    }

    private bool Render()
    {
        _openGL.BeginScene(0, 0, 0, 1);
        var view = _camera.GetViewMatrix();
        var proj = _openGL.GetProjectionMatrix();

        var world = Matrix4X4.CreateTranslation<float>(0, 1, 0);
        _lightShader.SetShaderParameters(
            _openGL,
            world,
            view,
            proj,
            _light.Direction,
            _light.DiffuseColor,
            _light.AmbientLight
        );
        _groundModel.SetTexture(_openGL, 0);
        _groundModel.Render(_openGL);

        world = Matrix4X4.CreateTranslation<float>(0, 6, 8);
        _lightShader.SetShaderParameters(
            _openGL,
            world,
            view,
            proj,
            _light.Direction,
            _light.DiffuseColor,
            _light.AmbientLight
        );
        _wallModel.SetTexture(_openGL, 0);
        _wallModel.Render(_openGL);

        world = Matrix4X4.CreateTranslation<float>(0, 2, 0);
        _lightShader.SetShaderParameters(
            _openGL,
            world,
            view,
            proj,
            _light.Direction,
            _light.DiffuseColor,
            _light.AmbientLight
        );
        _bathModel.SetTexture(_openGL, 0);
        _bathModel.Render(_openGL);

        var reflView = _camera.GetReflectionViewMatrix();
        world = Matrix4X4.CreateTranslation<float>(0, _waterHeight, 0);
        _waterShader.SetShaderParameters(
            _openGL,
            world,
            view,
            proj,
            reflView,
            _waterTranslation,
            0.01f
        );
        _refractionTexture.SetTexture(_openGL, 1);
        _reflectionTexture.SetTexture(_openGL, 2);
        _waterModel.SetTexture(_openGL, 0);
        _waterModel.Render(_openGL);

        _openGL.EndScene();
        return true;
    }
}
