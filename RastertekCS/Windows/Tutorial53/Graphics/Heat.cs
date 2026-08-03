using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial53.Graphics;

/// <summary>
/// Holds the noise and distortion parameters for the heat effect and advances
/// its animation. Values are heatclass.cpp's verbatim.
/// </summary>
public class Heat
{
    private Vector3D<float> _scrollSpeeds;
    private Vector3D<float> _scales;
    private Vector2D<float> _distortion1;
    private Vector2D<float> _distortion2;
    private Vector2D<float> _distortion3;
    private float _distortionScale;
    private float _distortionBias;
    private float _emissiveMultiplier;
    private float _noiseFrameTime;
    private Texture _noiseTexture;

    public bool Initialize(DX11 DirectX, string textureFilename)
    {
        _scrollSpeeds = new Vector3D<float>(1.3f, 2.1f, 2.3f);
        _scales = new Vector3D<float>(1.0f, 2.0f, 3.0f);
        _distortion1 = new Vector2D<float>(0.1f, 0.2f);
        _distortion2 = new Vector2D<float>(0.1f, 0.3f);
        _distortion3 = new Vector2D<float>(0.1f, 0.1f);
        _distortionScale = 0.8f;
        _distortionBias = 0.5f;
        _emissiveMultiplier = 1.6f;
        _noiseFrameTime = 0.0f;

        _noiseTexture = new Texture();
        return _noiseTexture.Initialize(DirectX, textureFilename, true);
    }

    public void Shutdown()
    {
        _noiseTexture?.Shutdown();
        _noiseTexture = null;
    }

    public ComPtr<ID3D11ShaderResourceView> GetTextureView() => _noiseTexture.GetTextureView();

    public void Frame(float frameTime)
    {
        _noiseFrameTime += frameTime * 0.075f;
        if (_noiseFrameTime > 1000.0f)
            _noiseFrameTime = 0.0f;
    }

    public Vector3D<float> GetScrollSpeeds() => _scrollSpeeds;

    public Vector3D<float> GetScales() => _scales;

    public Vector2D<float> GetDistortion1() => _distortion1;

    public Vector2D<float> GetDistortion2() => _distortion2;

    public Vector2D<float> GetDistortion3() => _distortion3;

    public float GetDistortionScale() => _distortionScale;

    public float GetDistortionBias() => _distortionBias;

    public float GetEmissiveMultiplier() => _emissiveMultiplier;

    public float GetNoiseFrameTime() => _noiseFrameTime;
}
