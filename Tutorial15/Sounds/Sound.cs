using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.DirectSound;
using SharpDX;
using SharpDX.Multimedia;

namespace Tutorial15.Sounds
{
	public class Sound : ICloneable
	{
		#region
		DirectSound _DirectSound;
		PrimarySoundBuffer _PrimaryBuffer;

		string _AudioFileName;
		#endregion

		#region Constructors
		public Sound(string fileName)
		{
			_AudioFileName = fileName;
		}
		#endregion

		#region Public Methods
		public bool Initialize(IntPtr windowHandle)
		{
			// Initialize direct sound and the primary sound buffer.
			if(!InitializeDirectSound(windowHandle))
				return false;

			// Load a audio file onto a secondary buffer.
			if(!LoadAudioFile(_AudioFileName, _DirectSound))
				return false;

			// Play the audio file now that it had been loaded.
			if(!PlayAudioFile())
				return false;

			return true;
		}

		public void Shutdown()
		{
			// Release the secondary buffer.
			ShutdownAudioFile();

			// Shutdown the Direct Sound API
			ShutdownDirectSound();
		}
		#endregion

		#region Virtual Methods
		protected virtual bool LoadAudioFile(string audioFile, DirectSound directSound)
		{
			return true;
		}

		protected virtual void ShutdownAudioFile()
		{
		}

		protected virtual bool PlayAudioFile()
		{
			return true;
		}
		#endregion

		#region Private Methods
		bool InitializeDirectSound(IntPtr windowHandler)
		{
			try
			{
				// Initialize the direct sound interface pointer for the default sound device.
				_DirectSound = new DirectSound();

				// Set the cooperative level to priority so the format of the primary sound buffer can be modified.
				_DirectSound.SetCooperativeLevel(windowHandler, CooperativeLevel.Priority);

				// Setup the primary buffer description.
				var buffer = new SoundBufferDescription();
				buffer.Flags = BufferFlags.PrimaryBuffer | BufferFlags.ControlVolume;
				buffer.AlgorithmFor3D = Guid.Empty;

				// Get control of the primary sound buffer on the default sound device.
				_PrimaryBuffer = new PrimarySoundBuffer(_DirectSound, buffer);

				// Setup the format of the primary sound buffer.
				// In this case it is a .
				_PrimaryBuffer.Format = new WaveFormat(44100, 16, 2);
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		void ShutdownDirectSound()
		{
			// Release the primary sound buffer pointer.
			if (_PrimaryBuffer != null)
			{
				_PrimaryBuffer.Dispose();
				_PrimaryBuffer = null;
			}

			// Release the direct sound interface pointer.
			if (_DirectSound != null)
			{
				_DirectSound.Dispose();
				_DirectSound = null;
			}
		}
		#endregion

		#region Interface Methods
		public object Clone()
		{
			return MemberwiseClone();
		}
		#endregion
	}
}
