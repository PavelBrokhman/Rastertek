////////////////////////////////////////////////////////////////////////////////
// Filename: ssaoshaderclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _SSAOSHADERCLASS_H_
#define _SSAOSHADERCLASS_H_


//////////////
// INCLUDES //
//////////////
#include <d3d11.h>
#include <d3dcompiler.h>
#include <directxmath.h>
#include <fstream>
using namespace DirectX;
using namespace std;


////////////////////////////////////////////////////////////////////////////////
// Class name: SsaoShaderClass
////////////////////////////////////////////////////////////////////////////////
class SsaoShaderClass
{
private:
	struct MatrixBufferType
	{
		XMMATRIX world;
		XMMATRIX view;
		XMMATRIX projection;
	};

	struct SsaoBufferType
	{
		float screenWidth;
		float screenHeight;
		float randomTextureSize;
		float sampleRadius;
		float ssaoScale;
		float ssaoBias;
		float ssaoIntensity;
		float padding;
	};

public:
	SsaoShaderClass();
	SsaoShaderClass(const SsaoShaderClass&);
	~SsaoShaderClass();

	bool Initialize(ID3D11Device*, HWND);
	void Shutdown();
	bool Render(ID3D11DeviceContext*, int, XMMATRIX, XMMATRIX, XMMATRIX, ID3D11ShaderResourceView*, ID3D11ShaderResourceView*, ID3D11ShaderResourceView*,
				float, float, float, float, float, float, float);

private:
	bool InitializeShader(ID3D11Device*, HWND, WCHAR*, WCHAR*);
	void ShutdownShader();
	void OutputShaderErrorMessage(ID3D10Blob*, HWND, WCHAR*);

	bool SetShaderParameters(ID3D11DeviceContext*, XMMATRIX, XMMATRIX, XMMATRIX, ID3D11ShaderResourceView*, ID3D11ShaderResourceView*, ID3D11ShaderResourceView*,
							 float, float, float, float, float, float, float);
	void RenderShader(ID3D11DeviceContext*, int);

private:
	ID3D11VertexShader* m_vertexShader;
	ID3D11PixelShader* m_pixelShader;
	ID3D11InputLayout* m_layout;
	ID3D11Buffer* m_matrixBuffer;
	ID3D11SamplerState* m_sampleStateWrap;
	ID3D11SamplerState* m_sampleStateClamp;
	ID3D11Buffer* m_ssaoBuffer;
};

#endif