using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tutorial3
{
	public class GraphicsClass : ICloneable
	{
		#region Constructors
		public GraphicsClass()
		{
		}
		#endregion

		#region Variables / Properties
		private DX11Class D3D { get; set; }
		#endregion

		#region Methods
		public bool Initialize(SystemConfiguration configuration, IntPtr windowHandle)
		{
			try
			{
				// Create the Direct3D object.
				D3D = new DX11Class();
				// Initialize the Direct3D object.
				D3D.Initialize(configuration, windowHandle);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Could not initialize Direct3D\nError is '" + ex.Message + "'");
				return false;
			}

			return true;
		}

		public void Shutdown()
		{
			if (D3D == null)
				return;

			D3D.Shutdown();
			D3D = null;
		}

		public bool Frame()
		{
			// Render the graphics scene.
			return Render();
		}

		private bool Render()
		{
			// Clear the buffer to begin the scene.
			D3D.BeginScene(.5f, .5f, .5f, 1f);

			// Present the rendered scene to the screen.
			D3D.EndScene();

			return true;
		}
		#endregion

		#region Override Methods
		public object Clone()
		{
			return MemberwiseClone();
		}
		#endregion
	}
}
