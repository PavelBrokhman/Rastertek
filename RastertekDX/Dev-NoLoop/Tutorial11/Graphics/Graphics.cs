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
using GraphicsClass = Tutorial11.Graphics.Graphics;
using InputClass = Tutorial11.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Tutorial11.System;

namespace Tutorial11.Graphics
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

				// Set the initial position of the camera.
				Camera.SetPosition(0, 0, -10);

				// Create the texture shader object.
				TextureShader = new TextureShader();

				// Initialize the texture shader object.
				if (!TextureShader.Initialize(D3D.Device, windowHandle))
				{
					MessageBox.Show("Could not initialize the texture shader object.");
					return false;
				}

				// Create the bitmap object.
				Bitmap = new Bitmap();

				// Initialize the bitmap object.
				if(!Bitmap.Initialize(D3D.Device, configuration.Width, configuration.Height, "seafloor.dds", 256, 256))
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
			// Release the model object.
			if (Bitmap != null)
			{
				Bitmap.Shutdown();
				Bitmap = null;
			}

			// Release the color shader object.
			if (TextureShader != null)
			{
				TextureShader.Shuddown();
				TextureShader = null;
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

			// Put the bitmap vertex and index buffers on the graphics pipeline to prepare them for drawing.
			if(!Bitmap.Render(D3D.DeviceContext, 100, 100))
				return false;

			// Render the bitmap with the texture shader.
			if (!TextureShader.Render(D3D.DeviceContext, Bitmap.IndexCount, worldMatrix, viewMatrix,  orthoMatrix, Bitmap.Texture.TextureResource))
				return false;

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
