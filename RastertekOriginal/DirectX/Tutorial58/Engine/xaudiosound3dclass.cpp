///////////////////////////////////////////////////////////////////////////////
// Filename: xaudiosound3dclass.cpp
///////////////////////////////////////////////////////////////////////////////
#include "xaudiosound3dclass.h"


XAudioSound3DClass::XAudioSound3DClass()
{
	m_waveData = 0;
	m_sourceVoice = 0;
}


XAudioSound3DClass::XAudioSound3DClass(const XAudioSound3DClass& other)
{
}


XAudioSound3DClass::~XAudioSound3DClass()
{
}


bool XAudioSound3DClass::LoadTrack(IXAudio2* XAudio2, char* filename, float volume)
{
	bool result;


	// Initialize the location of the emitter for this 3D sound.
	InitializeEmitter();

	// Load the wave file for the sound.
	result = LoadMonoWaveFile(XAudio2, filename, volume);
	if(!result)
	{
		return false;
	}

	return true;
}


void XAudioSound3DClass::ReleaseTrack()
{
	// Destory the voice.
	if(m_sourceVoice)
	{
		m_sourceVoice->DestroyVoice();
		m_sourceVoice = 0;
	}

	// Release the wave file buffers.
	ReleaseWaveFile();

	return;
}


bool XAudioSound3DClass::PlayTrack()
{
	HRESULT result;


	// Play the track.
	result = m_sourceVoice->Start(0, XAUDIO2_COMMIT_NOW);
	if(FAILED(result))
	{
		return false;
	}

	return true;
}


bool XAudioSound3DClass::StopTrack()
{
	HRESULT result;


	// Play the track.
	result = m_sourceVoice->Stop(0);
	if(FAILED(result))
	{
		return false;
	}

	// Flush the buffers to remove them and reset the audio position to the beginning.
	result = m_sourceVoice->FlushSourceBuffers();
	if(FAILED(result))
	{
		return false;
	}

	// Resubmit the buffer to the source voice after the reset so it is prepared to play again.
	result = m_sourceVoice->SubmitSourceBuffer(&m_audioBuffer);
	if(FAILED(result))
	{
		return false;
	}

	return true;
}


void XAudioSound3DClass::Update3DPosition(float x, float y, float z)
{
	m_emitter.Position.x = x;
	m_emitter.Position.y = y;
	m_emitter.Position.z = z;
	return;
}


IXAudio2SourceVoice* XAudioSound3DClass::GetSourceVoice()
{
	return m_sourceVoice;
}


X3DAUDIO_EMITTER XAudioSound3DClass::GetEmitter()
{
	return m_emitter;
}


void XAudioSound3DClass::InitializeEmitter()
{
	ZeroMemory(&m_emitter, sizeof(&m_emitter));

	m_emitter.ChannelCount = 1;
	m_emitter.CurveDistanceScaler = 1.0f;
	m_emitter.DopplerScaler = 1.0f;

	// Set an initial position for the sound.
	m_emitter.Position.x = 0.0f;
	m_emitter.Position.y = 0.0f;
	m_emitter.Position.z = 0.0f;

	return;
}


bool XAudioSound3DClass::LoadMonoWaveFile(IXAudio2* xAudio2, char* filename, float volume)
{
	FILE* filePtr;
	RiffWaveHeaderType riffWaveFileHeader;
	SubChunkHeaderType subChunkHeader;
	FmtType fmtData;
	WAVEFORMATEX waveFormat;
	int error;
	unsigned long long count;
	long seekSize;
	unsigned long dataSize;
	bool foundFormat, foundData;
	HRESULT result;


	// Open the wave file for reading in binary.
	error = fopen_s(&filePtr, filename, "rb");
	if(error != 0)
	{
		return false;
	}

	// Read in the riff wave file header.
	count = fread(&riffWaveFileHeader, sizeof(riffWaveFileHeader), 1, filePtr);
	if(count != 1)
	{
		return false;
	}

	// Check that the chunk ID is the RIFF format.
	if((riffWaveFileHeader.chunkId[0] != 'R') || (riffWaveFileHeader.chunkId[1] != 'I') || (riffWaveFileHeader.chunkId[2] != 'F') || (riffWaveFileHeader.chunkId[3] != 'F'))
	{
		return false;
	}

	// Check that the file format is the WAVE format.
	if((riffWaveFileHeader.format[0] != 'W') || (riffWaveFileHeader.format[1] != 'A') || (riffWaveFileHeader.format[2] != 'V') || (riffWaveFileHeader.format[3] != 'E'))
	{
		return false;
	}

	// Read in the sub chunk headers until you find the format chunk.
	foundFormat = false;
	while(foundFormat == false)
	{
		// Read in the sub chunk header.
		count = fread(&subChunkHeader, sizeof(subChunkHeader), 1, filePtr);
		if(count != 1)
		{
			return false;
		}

		// Determine if it is the fmt header.  If not then move to the end of the chunk and read in the next one.
		if((subChunkHeader.subChunkId[0] == 'f') && (subChunkHeader.subChunkId[1] == 'm') && (subChunkHeader.subChunkId[2] == 't') && (subChunkHeader.subChunkId[3] == ' '))
		{
			foundFormat = true;
		}
		else
		{
			fseek(filePtr, subChunkHeader.subChunkSize, SEEK_CUR);
		}
	}

	// Read in the format data.
	count = fread(&fmtData, sizeof(fmtData), 1, filePtr);
	if(count != 1)
	{
		return false;
	}

	// Check that the audio format is WAVE_FORMAT_PCM.
	if(fmtData.audioFormat != WAVE_FORMAT_PCM)
	{
		return false;
	}

	// Check that the wave file was recorded in mono format.
	if(fmtData.numChannels != 1)
	{
		return false;
	}

	// Check that the wave file was recorded at a sample rate of 44.1 KHz.
	if(fmtData.sampleRate != 44100)
	{
		return false;
	}

	// Ensure that the wave file was recorded in 16 bit format.
	if(fmtData.bitsPerSample != 16)
	{
		return false;
	}

	// Seek up to the next sub chunk.
	seekSize = subChunkHeader.subChunkSize - 16;
	fseek(filePtr, seekSize, SEEK_CUR);

	// Read in the sub chunk headers until you find the data chunk.
	foundData = false;
	while(foundData == false)
	{
		// Read in the sub chunk header.
		count = fread(&subChunkHeader, sizeof(subChunkHeader), 1, filePtr);
		if(count != 1)
		{
			return false;
		}

		// Determine if it is the data header.  If not then move to the end of the chunk and read in the next one.
		if((subChunkHeader.subChunkId[0] == 'd') && (subChunkHeader.subChunkId[1] == 'a') && (subChunkHeader.subChunkId[2] == 't') && (subChunkHeader.subChunkId[3] == 'a'))
		{
			foundData = true;
		}
		else
		{
			fseek(filePtr, subChunkHeader.subChunkSize, SEEK_CUR);
		}
	}

	// Store the size of the data chunk.
	dataSize = subChunkHeader.subChunkSize;

	// Create a temporary buffer to hold the wave file data.
	m_waveData = new unsigned char[dataSize];

	// Read in the wave file data into the newly created buffer.
	count = fread(m_waveData, 1, dataSize, filePtr);
	if(count != dataSize)
	{
		return false;
	}

	// Close the file once done reading.
	error = fclose(filePtr);
	if(error != 0)
	{
		return false;
	}

	// Set the wave format of secondary buffer that this wave file will be loaded onto.
	waveFormat.wFormatTag = WAVE_FORMAT_PCM;
	waveFormat.nSamplesPerSec = fmtData.sampleRate;
	waveFormat.wBitsPerSample = fmtData.bitsPerSample;
	waveFormat.nChannels = fmtData.numChannels;
	waveFormat.nBlockAlign = (waveFormat.wBitsPerSample / 8) * waveFormat.nChannels;
	waveFormat.nAvgBytesPerSec = waveFormat.nSamplesPerSec * waveFormat.nBlockAlign;
	waveFormat.cbSize = 0;

	// Fill in the audio buffer struct.
	m_audioBuffer.AudioBytes = dataSize;          // Size of the audio buffer in bytes.
	m_audioBuffer.pAudioData = m_waveData;        // Buffer containing audio data.
	m_audioBuffer.Flags = XAUDIO2_END_OF_STREAM;  // Tell the source voice not to expect any data after this buffer
	m_audioBuffer.LoopCount = 0;                  // Not looping.  Change to XAUDIO2_LOOP_INFINITE for looping. 
	m_audioBuffer.PlayBegin = 0;
	m_audioBuffer.PlayLength = 0;
	m_audioBuffer.LoopBegin = 0;
	m_audioBuffer.LoopLength = 0;
	m_audioBuffer.pContext = NULL;

	// Create the source voice for this buffer.	
	result = xAudio2->CreateSourceVoice(&m_sourceVoice, &waveFormat, 0, XAUDIO2_DEFAULT_FREQ_RATIO, NULL, NULL, NULL);
	if(FAILED(result))
	{
		return false;
	}

	// Submit the audio buffer to the source voice.
	result = m_sourceVoice->SubmitSourceBuffer(&m_audioBuffer);
	if(FAILED(result))
	{
		return false;
	}

	// Set volume of the buffer.
	m_sourceVoice->SetVolume(volume);

	return true;
}


void XAudioSound3DClass::ReleaseWaveFile()
{
	// Release the wave data only after voice destroyed.
	if(m_waveData)
	{
		delete [] m_waveData;
		m_waveData = 0;
	}

	return;
}