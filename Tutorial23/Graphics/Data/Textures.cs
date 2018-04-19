using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using GraphicsClass = Tutorial23.Graphics.Graphics;
using InputClass = Tutorial23.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using System.Collections.Generic;

namespace Tutorial23.Graphics.Data
{
	public class Textures : ICollection<Texture>
	{
		#region Variables / Properties
		public List<Texture> TextureList { get; private set; }
		private Device _Device;
		#endregion

		#region Methods
		public bool Initialize(Device device, string[] fileNames)
		{
			try
			{
				// Load the texture file.
				TextureList = new List<Texture>();
				_Device = device;
				foreach (var fileName in fileNames)
					if (!AddFromFile(fileName))
						return false;

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public void Shutdown()
		{
			// Release the texture resource.
			Clear();
		}

		public bool AddFromFile(string fileName)
		{
			var texture = new Texture();
			if (!texture.Initialize(_Device, fileName))
				return false;

			this.Add(texture);

			return true;
		}
		#endregion

		#region ICollection Methods
		public void Add(Texture item)
		{
			if (TextureList != null)
				TextureList.Add(item);
		}

		public void Clear()
		{
			foreach (var texture in TextureList)
				texture.Shutdown();

			TextureList.Clear();
		}

		public bool Contains(Texture item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(Texture[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public int Count
		{
			get { return TextureList.Count; }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public bool Remove(Texture item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<Texture> GetEnumerator()
		{
			return TextureList.GetEnumerator();
		}

		global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		#endregion
	}
}
