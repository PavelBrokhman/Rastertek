namespace RastertekCS.OpenGL.Tutorial23.Graphics;

public class ModelList
{
    private struct ModelInfoType
    {
        public float PositionX,
            PositionY,
            PositionZ;
    }

    private ModelInfoType[] _modelInfoList;
    private int _modelCount;

    public void Initialize(int numModels)
    {
        _modelCount = numModels;
        _modelInfoList = new ModelInfoType[_modelCount];

        var rng = new Random();

        for (int i = 0; i < _modelCount; i++)
        {
            _modelInfoList[i].PositionX = (float)(rng.NextDouble() * 2.0 - 1.0) * 10.0f;
            _modelInfoList[i].PositionY = (float)(rng.NextDouble() * 2.0 - 1.0) * 10.0f;
            _modelInfoList[i].PositionZ = (float)(rng.NextDouble() * 2.0 - 1.0) * 10.0f + 5.0f;
        }
    }

    public void Shutdown()
    {
        _modelInfoList = null;
    }

    public int GetModelCount() => _modelCount;

    public void GetData(int index, out float posX, out float posY, out float posZ)
    {
        posX = _modelInfoList[index].PositionX;
        posY = _modelInfoList[index].PositionY;
        posZ = _modelInfoList[index].PositionZ;
    }
}
