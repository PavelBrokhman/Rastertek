namespace RastertekCS.Windows.Tutorial59.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Timer _timer;
    private ParticleSystem _particleSystem;
    private ParticleShader _particleShader;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;
        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();
        _timer = new Timer();
        _timer.Initialize();
        _particleSystem = new ParticleSystem();
        if (!_particleSystem.Initialize(DirectX, "Data/particle_config_01.txt"))
            return false;
        _particleShader = new ParticleShader();
        if (!_particleShader.Initialize(DirectX))
            return false;
        return true;
    }

    public void Shutdown()
    {
        _particleShader?.Shutdown();
        _particleSystem?.Shutdown();
        _particleShader = null;
        _particleSystem = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        _timer.Frame();
        _particleSystem.Frame(_directX, _timer.GetTime() / 1000.0f);
        return Render();
    }

    private bool Render()
    {
        _directX.BeginScene(0, 0, 0, 1);
        var worldMatrix = _directX.GetWorldMatrix();
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _directX.GetProjectionMatrix();
        _directX.EnableParticleAlphaBlending();
        _directX.TurnZBufferOff();

        _particleSystem.Render(_directX);
        if (!_particleShader.Render(_directX, _particleSystem.GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, _particleSystem.GetTextureView()))
            return false;

        _directX.DisableAlphaBlending();
        _directX.TurnZBufferOn();
        _directX.EndScene();
        return true;
    }
}
