////////////////////////////////////////////////////////////////////////////////
// Filename: soundclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _SOUNDCLASS_H_
#define _SOUNDCLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "directsoundclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: SoundClass
////////////////////////////////////////////////////////////////////////////////
class SoundClass
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
	SoundClass();
	SoundClass(const SoundClass&);
	~SoundClass();

	bool LoadTrack(IDirectSound8*, char*, long);
	void ReleaseTrack();

	bool PlayTrack();
	bool StopTrack();

private:
	bool LoadStereoWaveFile(IDirectSound8*, char*, long);
	void ReleaseWaveFile();

private:
	IDirectSoundBuffer8* m_secondaryBuffer;
};

#endif