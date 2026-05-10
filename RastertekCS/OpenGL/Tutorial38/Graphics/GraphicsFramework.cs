namespace RastertekCS.OpenGL.Tutorial38.Graphics;

public class GraphicsFramework
{
    private GL4 _driver;
    private Camera _camera;
    private Timer _timer;
    private ParticleSystem _particleSystem;
    private ParticleShader _particleShader;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _driver = OpenGL;
        _camera = new Camera();
        _camera.SetPosition(0.0f, -1.0f, -10.0f);
        _camera.Render();
        _timer = new Timer();
        _timer.Initialize();
        _particleSystem = new ParticleSystem();
        if (!_particleSystem.Initialize(OpenGL, "Data/star01.tga"))
            return false;
        _particleShader = new ParticleShader();
        if (!_particleShader.Initialize(OpenGL))
            return false;
        return true;
    }

    public void Shutdown()
    {
        _particleShader?.Shutdown(_driver);
        _particleSystem?.Shutdown(_driver);
        _particleShader = null;
        _particleSystem = null;
        _camera = null;
        _driver = null;
    }

    public bool Frame()
    {
        _timer.Frame();
        // Convert ms to seconds for particle physics.
        _particleSystem.Frame(_driver, _timer.GetTime() / 1000.0f);
        return Render();
    }

    private bool Render()
    {
        _driver.BeginScene(0, 0, 0, 1);
        var worldMatrix = _driver.GetWorldMatrix();
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _driver.GetProjectionMatrix();
        _driver.EnableAlphaBlending();
        if (!_particleShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix))
            return false;
        _particleSystem.Render(_driver);
        _driver.DisableAlphaBlending();
        _driver.EndScene();
        return true;
    }
}
