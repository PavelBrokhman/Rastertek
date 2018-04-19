using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using GraphicsClass = Tutorial12.Graphics.Graphics;
using InputClass = Tutorial12.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Tutorial12.System;
using Tutorial12.Graphics.Cameras;
using Tutorial12.Graphics.Models;
using Tutorial12.Graphics.Shaders;
using Tutorial12.Graphics.Data;

namespace Tutorial12.Graphics
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
		private LightShader LightShader { get; set; }
		private Light Light { get; set; }

		private TextureShader TextureShader { get; set; }
		private Texture Texture { get; set; }
		private Bitmap Bitmap { get; set; }
		private Text Text { get; set; }
		#endregion

		#region Static Variables
		static float Rotation { get; set; }
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

				// Initialize a base view matrix the camera for 2D user interface rendering.
				Camera.SetPosition(0, 0, -1);
				Camera.Render();
				var baseViewMatrix = Camera.ViewMatrix;

				// Create the text object.
				Text = new Text();
				if (!Text.Initialize(D3D.Device, D3D.DeviceContext, windowHandle, configuration.Width, configuration.Height, baseViewMatrix))
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
			// Release the text object.
			if (Text != null)
			{
				Text.Shutdown();
				Text = null;
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
			// Update the rotation variables each frame.
			Rotate();

			// Render the graphics scene.
			return Render(Rotation);
		}

		private bool Render(float rotation)
		{
			// Clear the buffer to begin the scene.
			D3D.BeginScene(0f, 0f, 0f, 1f);

			// Generate the view matrix based on the camera position.
			Camera.Render();

			// Get the world, view, and projection matrices from camera and d3d objects.
			var viewMatrix = Camera.ViewMatrix;
			var worldMatrix = D3D.WorldMatrix;
			var projectionMatrix = D3D.ProjectionMatrix;
			var orthoMatrix = D3D.OrthoMatrix;

			// Turn off the Z buffer to begin all 2D rendering.
			D3D.TurnZBufferOff();

			// Turn on the alpha blending before rendering the text.
			D3D.TurnOnAlphaBlending();

			// Render the text string.
			if (!Text.Render(D3D.DeviceContext, worldMatrix, orthoMatrix))
				return false;

			// Turn off the alpha blending before rendering the text.
			D3D.TurnOffAlphaBlending();

			// Turn on the Z buffer to begin all 2D rendering.
			D3D.TurnZBufferOn();

			// Present the rendered scene to the screen.
			D3D.EndScene();

			return true;
		}
		#endregion

		#region Static Methods
		static void Rotate()
		{
			Rotation += (float)Math.PI * 0.005f;
			if (Rotation > 360)
				Rotation -= 360;
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
