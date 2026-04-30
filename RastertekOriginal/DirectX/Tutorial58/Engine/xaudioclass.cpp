///////////////////////////////////////////////////////////////////////////////
// Filename: xaudioclass.cpp
///////////////////////////////////////////////////////////////////////////////
#include "xaudioclass.h"


XAudioClass::XAudioClass()
{
	m_xAudio2 = 0;
	m_masterVoice = 0;
	m_matrixCoefficients = 0;
}


XAudioClass::XAudioClass(const XAudioClass& other)
{
}


XAudioClass::~XAudioClass()
{
}


bool XAudioClass::Initialize()
{
	HRESULT result;
	DWORD dwChannelMask;
	

	// Initialize COM first.
	result = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
	if(FAILED(result))
	{
		return false;
	}

	// Create an instance of the XAudio2 engine.
	result = XAudio2Create(&m_xAudio2, 0, XAUDIO2_USE_DEFAULT_PROCESSOR);
	if(FAILED(result))
	{
		return false;
	}

	// Create the mastering voice.
	result = m_xAudio2->CreateMasteringVoice(&m_masterVoice);
	if(FAILED(result))
	{
		return false;
	}

	// Get the speaker setup for 3D audio settings.
	m_masterVoice->GetChannelMask(&dwChannelMask);

	// Initialize X3DAudio.
	result = X3DAudioInitialize(dwChannelMask, X3DAUDIO_SPEED_OF_SOUND, m_X3DInstance);
	if(FAILED(result))
	{
		return false;
	}

	// Initialize the position and orientation of the 3D listener structure.
	ZeroMemory(&m_listener, sizeof(&m_listener));

	m_listener.Position.x = 0.0f;
	m_listener.Position.y = 0.0f;
	m_listener.Position.z = 0.0f;

	m_listener.OrientFront.x = 0.0f;
	m_listener.OrientFront.y = 0.0f;
	m_listener.OrientFront.z = 1.0f;

	m_listener.OrientTop.x = 0.0f;
	m_listener.OrientTop.y = 1.0f;
	m_listener.OrientTop.z = 0.0f;

	// Get the voice details for setting up the DSP settings.
	m_masterVoice->GetVoiceDetails(&m_deviceDetails);

	// Create the matrix coefficients array for the DSP struct.
	m_matrixCoefficients = new float[m_deviceDetails.InputChannels];

	// Create an instance of the dsp settings structure.
	ZeroMemory(&m_DSPSettings, sizeof(&m_DSPSettings));

	m_DSPSettings.SrcChannelCount = 1;
	m_DSPSettings.DstChannelCount = m_deviceDetails.InputChannels;
	m_DSPSettings.pMatrixCoefficients = m_matrixCoefficients;

	return true;
}


void XAudioClass::Shutdown()
{
	// Release the matrix coefficients array.
	if(m_matrixCoefficients)
	{
		delete [] m_matrixCoefficients;
		m_matrixCoefficients = 0;
	}

	// Release the master voice.
	if(m_masterVoice)
	{
		m_masterVoice->DestroyVoice();
		m_masterVoice = 0;
	}

	// Release the Xaudio2 interface.
	if(m_xAudio2)
	{
		m_xAudio2->Release();
		m_xAudio2 = 0;
	}

	// Uninitialize COM.
	CoUninitialize();

	return;
}


bool XAudioClass::Frame(X3DAUDIO_EMITTER emitter, IXAudio2SourceVoice* sourceVoice)
{
	HRESULT result;


	// Call X3DAudioCalculate to calculate new settings for the voices.
	X3DAudioCalculate(m_X3DInstance, &m_listener, &emitter, X3DAUDIO_CALCULATE_MATRIX | X3DAUDIO_CALCULATE_DOPPLER | X3DAUDIO_CALCULATE_LPF_DIRECT | X3DAUDIO_CALCULATE_REVERB, &m_DSPSettings);

	// Use SetOutputMatrix and SetFrequencyRatio to apply the volume and pitch values to the source voice.
	result = sourceVoice->SetOutputMatrix(m_masterVoice, 1, m_deviceDetails.InputChannels, m_DSPSettings.pMatrixCoefficients);
	if(FAILED(result))
	{
		return false;
	}

	result = sourceVoice->SetFrequencyRatio(m_DSPSettings.DopplerFactor);
	if(FAILED(result))
	{
		return false;
	}

	return true;
}


IXAudio2* XAudioClass::GetXAudio2()
{
	return m_xAudio2;
}