////////////////////////////////////////////////////////////////////////////////
// Filename: sound3dclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _SOUND3DCLASS_H_
#define _SOUND3DCLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "openalclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: Sound3DClass
////////////////////////////////////////////////////////////////////////////////
class Sound3DClass
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
    Sound3DClass();
    Sound3DClass(const Sound3DClass&);
    ~Sound3DClass();

    bool LoadTrack(char*, float);
    void ReleaseTrack();

    bool PlayTrack(bool);
    bool StopTrack();

    bool Update3DPosition(float, float, float);
  
private:
    bool LoadMonoWaveFile(char*);
    void ReleaseWaveFile();
  
private:
    unsigned int m_audioBufferId, m_audioSourceId;
    unsigned char* m_waveData;
    unsigned int m_waveSize;
};

#endif
