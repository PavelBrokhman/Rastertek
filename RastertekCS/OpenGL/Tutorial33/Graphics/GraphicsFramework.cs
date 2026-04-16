using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial33.Graphics;

public class GraphicsFramework
{
    private GL4 _driver;
    private Camera _camera;
    private Model _model;
    private FireShader _fireShader;
    private float _frameTime;

    public bool Initialize(GL4 gl, int screenWidth, int screenHeight)
    {
        _driver = gl;
        _camera = new Camera();
        _camera.SetPosition(0, 0, -5);
        _camera.Render();
        _model = new Model();
        if (
            !_model.Initialize(
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
        _fireShader = new FireShader();
        if (!_fireShader.Initialize(gl))
            return false;
        return true;
    }

    public void Shutdown()
    {
        _fireShader?.Shutdown(_driver);
        _model?.Shutdown(_driver);
        _driver = null;
    }

    public bool Frame()
    {
        _frameTime += 0.01f;
        if (_frameTime > 1000f)
            _frameTime = 0f;
        return Render();
    }

    bool Render()
    {
        _driver.BeginScene(0, 0, 0, 1);
        var w = _driver.GetWorldMatrix();
        var v = _camera.GetViewMatrix();
        var p = _driver.GetProjectionMatrix();
        float[] ss = { 1.3f, 2.1f, 2.3f };
        float[] sc = { 1f, 2f, 3f };
        float[] d1 = { 0.1f, 0.2f };
        float[] d2 = { 0.1f, 0.3f };
        float[] d3 = { 0.1f, 0.1f };
        _driver.EnableAlphaBlending();
        _fireShader.SetShaderParameters(
            _driver,
            w,
            v,
            p,
            _frameTime,
            ss,
            sc,
            d1,
            d2,
            d3,
            0.8f,
            0.5f
        );
        _model.SetTexture1(_driver, 0);
        _model.SetTexture2(_driver, 1);
        _model.SetTexture3(_driver, 2);
        _model.Render(_driver);
        _driver.DisableAlphaBlending();
        _driver.EndScene();
        return true;
    }
}
