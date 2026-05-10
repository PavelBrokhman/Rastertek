using RastertekCS.Windows.Tutorial34.Inputs;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial34.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private TextureShader _textureShader;
    private Model _floorModel,
        _billboardModel;
    private Position _position;
    private Timer _timer;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(DirectX))
            return false;

        _floorModel = new Model();
        if (!_floorModel.Initialize(DirectX, "Models/floor.txt", "Data/grid01.tga", false))
            return false;

        _billboardModel = new Model();
        if (!_billboardModel.Initialize(DirectX, "Models/square.txt", "Data/stone01.tga", false))
            return false;

        _position = new Position();
        _position.SetPosition(0.0f, 1.5f, -11.0f);

        _timer = new Timer();
        _timer.Initialize();

        return true;
    }

    public void Shutdown()
    {
        _billboardModel?.Shutdown();
        _floorModel?.Shutdown();
        _textureShader?.Shutdown();
        _billboardModel = null;
        _floorModel = null;
        _textureShader = null;
        _camera = null;
        _directX = null;
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

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var worldMatrix = _directX.GetWorldMatrix();
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _directX.GetProjectionMatrix();

        _floorModel.Render(_directX);
        if (
            !_textureShader.Render(
                _directX,
                _floorModel.GetIndexCount(),
                worldMatrix,
                viewMatrix,
                projectionMatrix,
                _floorModel.GetTextureView()
            )
        )
            return false;

        var cameraPosition = _camera.GetPosition();
        var modelPosition = new Vector3D<float>(0.0f, 1.5f, 0.0f);
        float pi = 3.14159265358979323846f;
        double angle =
            Math.Atan2(modelPosition.X - cameraPosition.X, modelPosition.Z - cameraPosition.Z)
            * (180.0f / pi);
        float rotation = (float)angle * 0.0174532925f;
        var rotateMatrix = DXMath.RotationYLH(rotation);
        var translateMatrix = DXMath.Translation(modelPosition.X, modelPosition.Y, modelPosition.Z);
        worldMatrix = rotateMatrix * translateMatrix;

        _billboardModel.Render(_directX);
        if (
            !_textureShader.Render(
                _directX,
                _billboardModel.GetIndexCount(),
                worldMatrix,
                viewMatrix,
                projectionMatrix,
                _billboardModel.GetTextureView()
            )
        )
            return false;

        _directX.EndScene();
        return true;
    }
}
