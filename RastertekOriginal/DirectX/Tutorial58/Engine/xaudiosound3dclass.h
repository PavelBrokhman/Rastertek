////////////////////////////////////////////////////////////////////////////////
// Filename: xaudiosound3dclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _XAUDIOSOUND3DCLASS_H_
#define _XAUDIOSOUND3DCLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "xaudioclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: XAudioSound3DClass
////////////////////////////////////////////////////////////////////////////////
class XAudioSound3DClass
{
private:
	struct RiffWaveHeaderType
	{
		char chunkId[4];
		unsigned long chunkSize;
		char format[4];
	};

	struct SubChunkHeaderType
	{
		char subChunkId[4];
		unsigned long subChunkSize;
	};

	struct FmtType
	{
		unsigned short audioFormat;
		unsigned short numChannels;
		unsigned long sampleRate;
		unsigned long bytesPerSecond;
		unsigned short blockAlign;
		unsigned short bitsPerSample;
	};

public:
	XAudioSound3DClass();
	XAudioSound3DClass(const XAudioSound3DClass&);
	~XAudioSound3DClass();

	bool LoadTrack(IXAudio2*, char*, float);
	void ReleaseTrack();

	IXAudio2SourceVoice* GetSourceVoice();
	X3DAUDIO_EMITTER GetEmitter();

	bool PlayTrack();
	bool StopTrack();

	void Update3DPosition(float, float, float);

private:
	void InitializeEmitter();

	bool LoadMonoWaveFile(IXAudio2*, char*, float);
	void ReleaseWaveFile();

private:
	X3DAUDIO_EMITTER m_emitter;
	unsigned char* m_waveData;
	XAUDIO2_BUFFER m_audioBuffer;
	IXAudio2SourceVoice* m_sourceVoice;
};

#endif