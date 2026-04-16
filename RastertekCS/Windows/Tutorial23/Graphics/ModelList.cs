namespace RastertekCS.Windows.Tutorial23.Graphics;

public class ModelList
{
    private struct ModelInfoType
    {
        public float PositionX,
            PositionY,
            PositionZ;
    }

    private ModelInfoType[] m_modelInfoList;
    private int m_modelCount;

    public void Initialize(int numModels)
    {
        m_modelCount = numModels;
        m_modelInfoList = new ModelInfoType[m_modelCount];

        var rng = new Random();

        for (int i = 0; i < m_modelCount; i++)
        {
            m_modelInfoList[i].PositionX = (float)(rng.NextDouble() * 2.0 - 1.0) * 10.0f;
            m_modelInfoList[i].PositionY = (float)(rng.NextDouble() * 2.0 - 1.0) * 10.0f;
            m_modelInfoList[i].PositionZ = (float)(rng.NextDouble() * 2.0 - 1.0) * 10.0f + 5.0f;
        }
    }

    public void Shutdown()
    {
        m_modelInfoList = null;
    }

    public int GetModelCount() => m_modelCount;

    public void GetData(int index, out float posX, out float posY, out float posZ)
    {
        posX = m_modelInfoList[index].PositionX;
        posY = m_modelInfoList[index].PositionY;
        posZ = m_modelInfoList[index].PositionZ;
    }
}
