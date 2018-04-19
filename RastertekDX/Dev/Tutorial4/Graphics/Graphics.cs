using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using GraphicsClass = Tutorial4.Graphics.Graphics;
using InputClass = Tutorial4.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Tutorial4.System;

namespace Tutorial4.Graphics
{
	public class Graphics : ICloneable
	{
		#region Constructors
		public Graphics()
		{
		}
		#endregion

		#region Variables / Properties
		private DX11 D3D { get; set; }
		private Camera Camera { get; set; }
		private Model Model { get; set; }
		private ColorShader ColorShader { get; set; }
		#endregion

		#region Methods
		public bool Initialize(SystemConfiguration configuration, IntPtr windowHandle)
		{
			try
			{
				// Create the Direct3D object.
				D3D = new DX11();
				// Initialize the Direct3D object.
				if (!D3D.Initialize(configuration, windowHandle))
					return false;

				// Create the camera object
				Camera = new Camera();

				// Set the initial position of the camera.
				Camera.SetPosition(0, 0, -10);

				// Create the model object.
				Model = new Model();

				// Initialize the model object.
				if (!Model.Initialize(D3D.Device))
					return false;

				// Create the color shader object.
				ColorShader = new ColorShader();

				// Initialize the color shader object.
				if (!ColorShader.Initialize(D3D.Device, windowHandle))
					return false;

				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Could not initialize Direct3D\nError is '" + ex.Message + "'");
				return false;
			}
		}

		public void Shutdown()
		{
			// Release the color shader object.
			if (ColorShader != null)
			{
				ColorShader.Shuddown();
				ColorShader = null;
			}

			// Release the model object.
			if (Model != null)
			{
				Model.Shutdown();
				Model = null;
			}

			// Release the camera object.
			if (Camera != null)
			{
				Camera = null;
			}

			// Release the Direct3D object.
			if (D3D != null)
			{
				D3D.Shutdown();
				D3D = null;
			}
		}

		public bool Frame()
		{
			// Render the graphics scene.
			return Render();
		}

		private bool Render()
		{
			// Clear the buffer to begin the scene.
			D3D.BeginScene(0f, 0f, 0f, 1f);

			// Generate the view matrix based on the camera position.
			Camera.Render();

			// Get the world, view, and projection matrices from camera and d3d objects.
			var viewMatrix = Camera.ViewMatrix;
			var worldMatrix = D3D.WorldMatrix;
			var projectionMatrix = D3D.ProjectionMatrix;

			// Put the model vertex and index buffers on the graphics pipeline to prepare them for drawing.
			Model.Render(D3D.DeviceContext);

			// Render the model using the color shader.
			if (!ColorShader.Render(D3D.DeviceContext, Model.IndexCount, worldMatrix, viewMatrix, projectionMatrix))
				return false;

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
