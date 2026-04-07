namespace RastertekCS.OpenGL.Tutorial34.Graphics;
public class Position
{
    private float m_px, m_py, m_pz, m_ry;
    private float m_frameTime, m_leftSpeed, m_rightSpeed;
    public void SetPosition(float x, float y, float z) { m_px = x; m_py = y; m_pz = z; }
    public (float x, float y, float z) GetPosition() => (m_px, m_py, m_pz);
    public void SetFrameTime(float t) { m_frameTime = t; }
    public void MoveLeft(bool keydown) { if (keydown) { m_leftSpeed += m_frameTime * 1f; if (m_leftSpeed > m_frameTime * 50f) m_leftSpeed = m_frameTime * 50f; } else { m_leftSpeed -= m_frameTime * 1f; if (m_leftSpeed < 0f) m_leftSpeed = 0f; } float rad = m_ry * 0.0174532925f; m_px -= MathF.Cos(rad) * m_leftSpeed; m_pz -= MathF.Sin(rad) * m_leftSpeed; }
    public void MoveRight(bool keydown) { if (keydown) { m_rightSpeed += m_frameTime * 1f; if (m_rightSpeed > m_frameTime * 50f) m_rightSpeed = m_frameTime * 50f; } else { m_rightSpeed -= m_frameTime * 1f; if (m_rightSpeed < 0f) m_rightSpeed = 0f; } float rad = m_ry * 0.0174532925f; m_px += MathF.Cos(rad) * m_rightSpeed; m_pz += MathF.Sin(rad) * m_rightSpeed; }
}
