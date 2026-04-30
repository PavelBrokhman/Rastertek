////////////////////////////////////////////////////////////////////////////////
// Filename: sound3dclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _SOUND3DCLASS_H_
#define _SOUND3DCLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "directsoundclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: Sound3DClass
////////////////////////////////////////////////////////////////////////////////
class Sound3DClass
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
	Sound3DClass();
	Sound3DClass(const Sound3DClass&);
	~Sound3DClass();

	bool LoadTrack(IDirectSound8*, char*, long);
	void ReleaseTrack();

	bool PlayTrack();
	bool StopTrack();

	bool Update3DPosition(float, float, float);

private:
	bool LoadMonoWaveFile(IDirectSound8*, char*, long);
	void ReleaseWaveFile();

private:
	IDirectSoundBuffer8* m_secondaryBuffer;
	IDirectSound3DBuffer8* m_secondary3DBuffer;
};

#endif