using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial41.Graphics;

public class Camera
{
    private float _positionX,
        _positionY,
        _positionZ;
    private float _rotationX,
        _rotationY,
        _rotationZ;
    private Matrix4X4<float> _viewMatrix;
    private Matrix4X4<float> _baseViewMatrix;

    public void SetPosition(float x, float y, float z)
    {
        _positionX = x;
        _positionY = y;
        _positionZ = z;
    }

    public void SetRotation(float x, float y, float z)
    {
        _rotationX = x;
        _rotationY = y;
        _rotationZ = z;
    }

    public void Render() => _viewMatrix = BuildView();

    public void RenderBaseViewMatrix() => _baseViewMatrix = BuildView();

    public Matrix4X4<float> GetViewMatrix() => _viewMatrix;

    public Matrix4X4<float> GetBaseViewMatrix() => _baseViewMatrix;

    private Matrix4X4<float> BuildView()
    {
        var up = new Vector3D<float>(0.0f, 1.0f, 0.0f);
        var position = new Vector3D<float>(_positionX, _positionY, _positionZ);
        var lookAt = new Vector3D<float>(0.0f, 0.0f, 1.0f);
        float pitch = _rotationX * (MathF.PI / 180.0f);
        float yaw = _rotationY * (MathF.PI / 180.0f);
        float roll = _rotationZ * (MathF.PI / 180.0f);
        var rotationMatrix = Matrix4X4.CreateFromYawPitchRoll(yaw, pitch, roll);
        lookAt = Vector3D.Transform(lookAt, rotationMatrix);
        up = Vector3D.Transform(up, rotationMatrix);
        lookAt = position + lookAt;
        return DXMath.LookAtLH(position, lookAt, up);
    }
}
