namespace RastertekCS.Windows.Tutorial23.Graphics;

public class Position
{
    private float m_frameTime;
    private float m_rotationY;
    private float m_leftTurnSpeed;
    private float m_rightTurnSpeed;

    public void SetFrameTime(float time)
    {
        m_frameTime = time;
    }

    public float GetRotation() => m_rotationY;

    public void TurnLeft(bool keyDown)
    {
        if (keyDown)
        {
            m_leftTurnSpeed += m_frameTime * 10.0f;
            if (m_leftTurnSpeed > m_frameTime * 100.0f)
                m_leftTurnSpeed = m_frameTime * 100.0f;
        }
        else
        {
            m_leftTurnSpeed -= m_frameTime * 1.0f;
            if (m_leftTurnSpeed < 0.0f)
                m_leftTurnSpeed = 0.0f;
        }
        m_rotationY -= m_leftTurnSpeed;
        if (m_rotationY < 0.0f)
            m_rotationY += 360.0f;
    }

    public void TurnRight(bool keyDown)
    {
        if (keyDown)
        {
            m_rightTurnSpeed += m_frameTime * 10.0f;
            if (m_rightTurnSpeed > m_frameTime * 100.0f)
                m_rightTurnSpeed = m_frameTime * 100.0f;
        }
        else
        {
            m_rightTurnSpeed -= m_frameTime * 1.0f;
            if (m_rightTurnSpeed < 0.0f)
                m_rightTurnSpeed = 0.0f;
        }
        m_rotationY += m_rightTurnSpeed;
        if (m_rotationY > 360.0f)
            m_rotationY -= 360.0f;
    }
}
