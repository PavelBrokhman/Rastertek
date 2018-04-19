using System;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;


namespace Engine.Graphics.Data
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
