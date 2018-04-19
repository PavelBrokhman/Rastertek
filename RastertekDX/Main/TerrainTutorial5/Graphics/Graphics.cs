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
using GraphicsClass = Engine.Graphics.Graphics;
using InputClass = Engine.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Engine.SystemData;
using Engine.Graphics.Cameras;
using Engine.Graphics.Models;
using Engine.Graphics.Shaders;
using Engine.Graphics.Data;
using System.Threading;
using Engine.Inputs;

namespace Engine.Graphics
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
		private ModelList ModelList { get; set; }
		private Frustum Frustum { get; set; }
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
				{
					MessageBox.Show("Could not initialize Direct3D", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the camera object
				Camera = new Camera();

				// Initialize a base view matrix the camera for 2D user interface rendering.
				Camera.SetPosition(0, 0, -1);
				Camera.Render();
				var baseViewMatrix = Camera.ViewMatrix;

				// Create the text object.
				Text = new Text();
				if (!Text.Initialize(D3D.Device, D3D.DeviceContext, windowHandle, configuration.Width, configuration.Height, baseViewMatrix))
				{
					MessageBox.Show("Could not initialize the text object", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the model class.
				Model = new Model();

				// Initialize the model object.
				if (!Model.Initialize(D3D.Device, "sphere.txt", "seafloor.dds"))
				{
					MessageBox.Show("Could not initialize the model object", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the light shader object.
				LightShader = new LightShader();

				// Initialize the light shader object.
				if (!LightShader.Initialize(D3D.Device, windowHandle))
				{
					MessageBox.Show("Could not initialize the light shader", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the light object.
				Light = new Light();

				// Initialize the light object.
				Light.SetAmbientColor(0.15f, 0.15f, 0.15f, 1.0f);
				Light.SetDiffuseColor(1, 0, 0, 1f);
				Light.SetDirection(1, 0, 1);
				Light.SetSpecularColor(0, 1, 1, 1);
				Light.SetSpecularPower(32);

				// Create the model list object.
				ModelList = new ModelList();

				// Initialize the model list object.
				if (!ModelList.Initialize(25))
				{
					MessageBox.Show("Could not initialize the model list object", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the frustum object.
				Frustum = new Frustum();

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
			// Release the frustum object.
			Frustum = null;

			// Release the model list object.
			if (ModelList != null)
			{
				ModelList.Shutdown();
				ModelList = null;
			}

			// Release the light object.
			Light = null;

			// Release the light shader object.
			if (LightShader != null)
			{
				LightShader.Shuddown();
				LightShader = null;
			}

			// Release the model object.
			if (Model != null)
			{
				Model.Shutdown();
				Model = null;
			}

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

		public bool Frame(float rotationY)
		{
			// Set the position of the camera.
			Camera.SetPosition(0, 0, -10f);

			// Set the rotation of the camera.
			Camera.SetRotation(0, rotationY, 0);

			return true;
		}

		internal bool Frame(Position Position)
		{
			// Set the position of the camera.
			var position = Position.GetPosition();
			Camera.SetPosition(position.X, position.Y, position.Z);

			// Set the rotation of the camera.
			var rotation = Position.GetRotation();
			Camera.SetRotation(rotation.X, rotation.Y, rotation.Z);

			return true;
		}

		public bool Frame(int mouseX, int mouseY)
		{
			// Set the location of the mouse.
			if (!Text.SetMousePosition(mouseX, mouseY, D3D.DeviceContext))
				return false;

			// Set the position of the camera.
			Camera.SetPosition(0, 0, -10f);

			return true;
		}

		public bool Frame(int fps, int cpu, float frameTime)
		{
			// Set the frames per second.
			if (!Text.SetFps(fps, D3D.DeviceContext))
				return false;

			// Set the cpu usage.
			if (!Text.SetCpu(cpu, D3D.DeviceContext))
				return false;

			// Set the position of the camera.
			Camera.SetPosition(0, 0, -10f);

			return true;
		}

		public bool Render()
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

			// Construct the frustum.
			Frustum.ConstructFrustum(SystemConfiguration.ScreenDepth, projectionMatrix, viewMatrix);

			// Initialize the count of the models that have been rendered.
			var renderCount = 0;

			Vector3 position;
			Vector4 color;
			// Go through all models and render them only if they can seen by the camera view.
			for (int index = 0; index < ModelList.ModelCount; index++)
			{
				// Get the position and color of the sphere model at this index.
				ModelList.GetData(index, out position, out color);

				// Set the radius of the sphere to 1.0 since this is already known.
				var radius = 1.0f;

				// Check if the sphere model is in the view frustum.
				var renderModel = Frustum.CheckSphere(position, radius);

				// If it can be seen then render it, if not skip this model and check the next sphere.
				if (renderModel)
				{
					// Move the model to the location it should be rendered at.
					worldMatrix = Matrix.Translation(position);

					// Put the model vertex and index buffer on the graphics pipeline to prepare them for drawing.
					Model.Render(D3D.DeviceContext);

					// Render the model using the color shader.
					if (!LightShader.Render(D3D.DeviceContext, Model.IndexCount, worldMatrix, viewMatrix, projectionMatrix, Model.Texture.TextureResource, Light.Direction, Light.AmbientColor, color, Camera.GetPosition(), Light.SpecularColor, Light.SpecularPower))
						return false;

					// Reset to the original world matrix.
					worldMatrix = D3D.WorldMatrix;

					// Since this model was rendered then increase the count for this frame.
					renderCount++;
				}
			}

			// Set the number of the models that was actually rendered this frame.
			if (!Text.SetRenderCount(renderCount, D3D.DeviceContext))
				return false;

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

		public bool Render(float rotation)
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
		public static void Rotate()
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
