namespace RastertekCS.Windows.Tutorial23.Graphics;

public class Position
{
    private float _frameTime;
    private float _rotationY;
    private float _leftTurnSpeed;
    private float _rightTurnSpeed;

    public void SetFrameTime(float time)
    {
        _frameTime = time;
    }

    public float GetRotation() => _rotationY;

    public void TurnLeft(bool keyDown)
    {
        if (keyDown)
        {
            _leftTurnSpeed += _frameTime * 10.0f;
            if (_leftTurnSpeed > _frameTime * 100.0f)
                _leftTurnSpeed = _frameTime * 100.0f;
        }
        else
        {
            _leftTurnSpeed -= _frameTime * 1.0f;
            if (_leftTurnSpeed < 0.0f)
                _leftTurnSpeed = 0.0f;
        }
        _rotationY -= _leftTurnSpeed;
        if (_rotationY < 0.0f)
            _rotationY += 360.0f;
    }

    public void TurnRight(bool keyDown)
    {
        if (keyDown)
        {
            _rightTurnSpeed += _frameTime * 10.0f;
            if (_rightTurnSpeed > _frameTime * 100.0f)
                _rightTurnSpeed = _frameTime * 100.0f;
        }
        else
        {
            _rightTurnSpeed -= _frameTime * 1.0f;
            if (_rightTurnSpeed < 0.0f)
                _rightTurnSpeed = 0.0f;
        }
        _rotationY += _rightTurnSpeed;
        if (_rotationY > 360.0f)
            _rotationY -= 360.0f;
    }
}
