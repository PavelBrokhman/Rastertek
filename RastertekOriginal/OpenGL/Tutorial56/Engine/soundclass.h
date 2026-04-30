////////////////////////////////////////////////////////////////////////////////
// Filename: soundclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _SOUNDCLASS_H_
#define _SOUNDCLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "openalclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: SoundClass
////////////////////////////////////////////////////////////////////////////////
class SoundClass
{
private:
    struct RiffWaveHeaderType
    {
        char chunkId[4];
        unsigned int chunkSize;
        char format[4];
    };

    struct SubChunkHeaderType
    {
        char subChunkId[4];
        unsigned int subChunkSize;
    };

    struct FmtType
    {
        unsigned short audioFormat;
        unsigned short numChannels;
        unsigned int sampleRate;
        unsigned int bytesPerSecond;
        unsigned short blockAlign;
        unsigned short bitsPerSample;
    };
  
public:
    SoundClass();
    SoundClass(const SoundClass&);
    ~SoundClass();

    bool LoadTrack(char*, float);
    void ReleaseTrack();

    bool PlayTrack(bool);
    bool StopTrack();
    
private:
    bool LoadStereoWaveFile(char*);
    void ReleaseWaveFile();
  
private:
    unsigned int m_audioBufferId, m_audioSourceId;
    unsigned char* m_waveData;
    unsigned int m_waveSize;
};

#endif
