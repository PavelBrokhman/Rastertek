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
using GraphicsClass = Tutorial10.Graphics.Graphics;
using InputClass = Tutorial10.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Tutorial10.System;

namespace Tutorial10.Graphics
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

				// Create the model object.
				Model = new Model();

				// Initialize the model object.
				if (!Model.Initialize(D3D.Device, "Cube.txt", "seafloor.dds"))
				{
					MessageBox.Show("Could not initialize the model object.");
					return false;
				}

				// Create the texture shader object.
				LightShader = new LightShader();

				// Initialize the texture shader object.
				if (!LightShader.Initialize(D3D.Device, windowHandle))
				{
					MessageBox.Show("Could not initialize the texture shader object.");
					return false;
				}

				// Create the light object.
				Light = new Light();

				// Iniialize the light object.
				Light.SetAmbientColor(0.15f, 0.15f, 0.15f, 1.0f);
				Light.SetDiffuseColor(1, 1, 1f, 1f);
				Light.SetDirection(0, 0, 1);
				Light.SetSpecularColor(1, 1, 1, 1);
				Light.SetSpecularPower(32);

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
			if (LightShader != null)
			{
				LightShader.Shuddown();
				LightShader = null;
				Light = null;
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

			// Rotate the world matrix by the rotation value so that the triangle will spin.
			Matrix.RotationY(rotation, out worldMatrix);

			// Put the model vertex and index buffers on the graphics pipeline to prepare them for drawing.
			Model.Render(D3D.DeviceContext);

			// Render the model using the color shader.
			if (!LightShader.Render(D3D.DeviceContext, Model.IndexCount, worldMatrix, viewMatrix, projectionMatrix, Model.Texture.TextureResource, Light.Direction, Light.AmbientColor, Light.DiffuseColor, Camera.GetPosition(), Light.SpecularColor, Light.SpecularPower))
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
