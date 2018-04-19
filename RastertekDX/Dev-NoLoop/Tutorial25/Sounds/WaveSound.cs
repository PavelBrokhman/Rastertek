using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.DirectSound;
using System.IO;
using Engine.System;
using SharpDX.Multimedia;
using SharpDX;

namespace Engine.Sounds
{
	public class WaveSound : Sound
	{
		#region Variables / Properties
		string chunkId;
		int chunkSize;
		string format;
		string subChunkId;
		int subChunkSize;
		WaveFormatEncoding audioFormat;
		short numChannels;
		int sampleRate;
		int bytesPerSecond;
		short blockAlign;
		short bitsPerSample;
		string dataChunkId;
		int dataSize;

		SecondarySoundBuffer _SecondaryBuffer;
		#endregion

		#region Constructors
		public WaveSound(string fileName)
			: base(fileName)
		{
		}
		#endregion

		#region Virtual Methods
		protected override bool LoadAudioFile(string audioFile, DirectSound directSound)
		{
			try
			{
				// Open the wave file in binary. 
				var reader = new BinaryReader(File.OpenRead(SystemConfiguration.DataFilePath + audioFile));

				// Read in the wave file header.
				chunkId = new string(reader.ReadChars(4));
				chunkSize = reader.ReadInt32();
				format = new string(reader.ReadChars(4));
				subChunkId = new string(reader.ReadChars(4));
				subChunkSize = reader.ReadInt32();
				audioFormat = (WaveFormatEncoding )reader.ReadInt16();
				numChannels = reader.ReadInt16();
				sampleRate = reader.ReadInt32();
				bytesPerSecond = reader.ReadInt32();
				blockAlign = reader.ReadInt16();
				bitsPerSample = reader.ReadInt16();
				dataChunkId = new string(reader.ReadChars(4));
				dataSize = reader.ReadInt32();

				// Check that the chunk ID is the RIFF format
				// and the file format is the WAVE format
				// and sub chunk ID is the fmt format
				// and the audio format is PCM
				// and the wave file was recorded in stereo format
				// and at a sample rate of 44.1 KHz
				// and at 16 bit format
				// and there is the data chunk header.
				// Otherwise return false.
				if (chunkId != "RIFF" || format != "WAVE" || subChunkId.Trim() != "fmt" || audioFormat != WaveFormatEncoding.Pcm || numChannels != 2 || sampleRate != 44100 || bitsPerSample != 16 || dataChunkId != "data")
					return false;

				// Set the buffer description of the secondary sound buffer that the wave file will be loaded onto and the wave format.
				var buffer = new SoundBufferDescription();
				buffer.Flags = BufferFlags.ControlVolume;
				buffer.BufferBytes = dataSize;
				buffer.Format = new WaveFormat(44100, 16, 2);
				buffer.AlgorithmFor3D = Guid.Empty;

				// Create a temporary sound buffer with the specific buffer settings.
				_SecondaryBuffer = new SecondarySoundBuffer(directSound, buffer);

				// Read in the wave file data into the temporary buffer.
				var waveData = reader.ReadBytes(dataSize);

				// Close the reader
				reader.Close();

				// Lock the secondary buffer to write wave data into it.
				DataStream waveBufferData2;
				var waveBufferData1 = _SecondaryBuffer.Lock(0, dataSize, LockFlags.None, out waveBufferData2);

				// Copy the wave data into the buffer.
				waveBufferData1.Write(waveData, 0, dataSize);

				// Unlock the secondary buffer after the data has been written to it.
				_SecondaryBuffer.Unlock(waveBufferData1, waveBufferData2);
			}
			catch
			{
				return false;
			}

			return true;
		}

		protected override void ShutdownAudioFile()
		{
			if (_SecondaryBuffer != null)
			{
				_SecondaryBuffer.Dispose();
				_SecondaryBuffer = null;
			}
		}

		protected override bool PlayAudioFile()
		{
			try
			{
				// Set the position at the beginning of the sound buffer.
				_SecondaryBuffer.CurrentPosition = 0;

				// Set volume of the buffer to 100%
				//_SecondaryBuffer.Volume = 100;

				// Play the contents of the secondary sound buffer.
				_SecondaryBuffer.Play(0, PlayFlags.None);
			}
			catch
			{
				return false;
			}

			return true;
		}
		#endregion
	}
}
