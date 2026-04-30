////////////////////////////////////////////////////////////////////////////////
// Filename: xaudioclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _XAUDIOCLASS_H_
#define _XAUDIOCLASS_H_


/////////////
// LINKING //
/////////////
#pragma comment(lib, "Xaudio2.lib")


//////////////
// INCLUDES //
//////////////
#include <xaudio2.h>
#include <x3daudio.h>
#include <stdio.h>


////////////////////////////////////////////////////////////////////////////////
// Class name: XAudioClass
////////////////////////////////////////////////////////////////////////////////
class XAudioClass
{
public:
	XAudioClass();
	XAudioClass(const XAudioClass&);
	~XAudioClass();

	bool Initialize();
	void Shutdown();
	bool Frame(X3DAUDIO_EMITTER, IXAudio2SourceVoice*);

	IXAudio2* GetXAudio2();

private:
	IXAudio2* m_xAudio2;
	IXAudio2MasteringVoice* m_masterVoice;
	X3DAUDIO_HANDLE m_X3DInstance;
	X3DAUDIO_LISTENER m_listener;
	XAUDIO2_VOICE_DETAILS m_deviceDetails;
	float* m_matrixCoefficients;
	X3DAUDIO_DSP_SETTINGS m_DSPSettings;
};

#endif