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
using GraphicsClass = Tutorial10.Graphics.Graphics;
using InputClass = Tutorial10.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Tutorial10.Graphics
{
	public class Texture
	{
		#region Variables / Properties
		public ShaderResourceView TextureResource { get; private set; }
		#endregion

		#region Methods
		public bool Initialize(Device device, string fileName)
		{
			try
			{
				// Load the texture file.
				TextureResource = ShaderResourceView.FromFile(device, fileName);
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
			if (TextureResource != null)
			{
				TextureResource.Dispose();
				TextureResource = null;
			}
		}


		#endregion
	}
}
