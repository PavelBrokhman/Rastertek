///////////////////////////////////////////////////////////////////////////////
// Filename: xaudioclass.cpp
///////////////////////////////////////////////////////////////////////////////
#include "xaudioclass.h"


XAudioClass::XAudioClass()
{
	m_xAudio2 = 0;
	m_masterVoice = 0;
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

	return true;
}


void XAudioClass::Shutdown()
{
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


IXAudio2* XAudioClass::GetXAudio2()
{
	return m_xAudio2;
}