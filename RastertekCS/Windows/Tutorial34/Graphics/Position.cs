namespace RastertekCS.Windows.Tutorial34.Graphics;

public class Position
{
    private float _positionX,
        _positionY,
        _positionZ;
    private float _rotationY;
    private float _frameTime;
    private float _leftSpeed,
        _rightSpeed;

    public void SetPosition(float x, float y, float z)
    {
        _positionX = x;
        _positionY = y;
        _positionZ = z;
    }

    public (float x, float y, float z) GetPosition() => (_positionX, _positionY, _positionZ);

    public void SetFrameTime(float time)
    {
        _frameTime = time;
    }

    public void MoveLeft(bool keyDown)
    {
        if (keyDown)
        {
            _leftSpeed += _frameTime * 1.0f;
            if (_leftSpeed > _frameTime * 50.0f)
                _leftSpeed = _frameTime * 50.0f;
        }
        else
        {
            _leftSpeed -= _frameTime * 1.0f;
            if (_leftSpeed < 0.0f)
                _leftSpeed = 0.0f;
        }
        float radians = _rotationY * 0.0174532925f;
        _positionX -= MathF.Cos(radians) * _leftSpeed;
        _positionZ -= MathF.Sin(radians) * _leftSpeed;
    }

    public void MoveRight(bool keyDown)
    {
        if (keyDown)
        {
            _rightSpeed += _frameTime * 1.0f;
            if (_rightSpeed > _frameTime * 50.0f)
                _rightSpeed = _frameTime * 50.0f;
        }
        else
        {
            _rightSpeed -= _frameTime * 1.0f;
            if (_rightSpeed < 0.0f)
                _rightSpeed = 0.0f;
        }
        float radians = _rotationY * 0.0174532925f;
        _positionX += MathF.Cos(radians) * _rightSpeed;
        _positionZ += MathF.Sin(radians) * _rightSpeed;
    }
}
