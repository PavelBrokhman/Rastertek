using RastertekCS.OpenGL.Tutorial34.Inputs;
using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial34.Graphics;

public class GraphicsFramework
{
    private GL4 _driver;
    private Camera _camera;
    private TextureShader _textureShader;
    private Model _floorModel,
        _billboardModel;
    private Position _position;
    private Timer _timer;

    public bool Initialize(GL4 gl, int screenWidth, int screenHeight)
    {
        _driver = gl;
        _camera = new Camera();
        _camera.SetPosition(0, 0, -10);
        _camera.Render();
        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(gl))
            return false;
        _floorModel = new Model();
        if (!_floorModel.Initialize(gl, "Models/floor.txt", "Data/grid01.tga", false))
            return false;
        _billboardModel = new Model();
        if (!_billboardModel.Initialize(gl, "Models/square.txt", "Data/stone01.tga", false))
            return false;
        _position = new Position();
        _position.SetPosition(0, 1.5f, -11);
        _timer = new Timer();
        _timer.Initialize();
        return true;
    }

    public void Shutdown()
    {
        _textureShader?.Shutdown(_driver);
        _floorModel?.Shutdown(_driver);
        _billboardModel?.Shutdown(_driver);
        _driver = null;
    }

    public bool Frame(Input input)
    {
        _timer.Frame();
        _position.SetFrameTime(_timer.GetTime());
        _position.MoveLeft(input.IsLeftArrowPressed());
        _position.MoveRight(input.IsRightArrowPressed());
        var (positionX, positionY, positionZ) = _position.GetPosition();
        _camera.SetPosition(positionX, positionY, positionZ);
        _camera.Render();
        return Render();
    }

    bool Render()
    {
        _driver.BeginScene(0, 0, 0, 1);
        var worldMatrix = _driver.GetWorldMatrix();
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _driver.GetProjectionMatrix();

        _textureShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix);
        _floorModel.SetTexture(_driver, 0);
        _floorModel.Render(_driver);

        var cameraPosition = _camera.GetPosition();
        float[] modelPosition = { 0f, 1.5f, 0f };
        float pi = 3.14159265358979323846f;
        double angle =
            Math.Atan2(modelPosition[0] - cameraPosition[0], modelPosition[2] - cameraPosition[2])
            * (180.0f / pi);
        float rotation = (float)angle * 0.0174532925f;
        var rotateMatrix = GL4.MatrixRotationY(rotation);
        var translateMatrix = GL4.MatrixTranslation(
            modelPosition[0],
            modelPosition[1],
            modelPosition[2]
        );
        worldMatrix = GL4.MatrixMultiply(rotateMatrix, translateMatrix);

        _textureShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix);
        _billboardModel.SetTexture(_driver, 0);
        _billboardModel.Render(_driver);

        _driver.EndScene();
        return true;
    }
}
