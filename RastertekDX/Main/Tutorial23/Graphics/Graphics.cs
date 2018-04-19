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
using GraphicsClass = Tutorial23.Graphics.Graphics;
using InputClass = Tutorial23.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Tutorial23.System;
using Tutorial23.Graphics.Cameras;
using Tutorial23.Graphics.Models;
using Tutorial23.Graphics.Shaders;
using Tutorial23.Graphics.Data;
using System.Threading;
using Tutorial23.Inputs;
using Tutorial23.Graphics.Data;

namespace Tutorial23.Graphics
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

		#region Models
		private Model Model { get; set; }
		private BumpMapModel BumpMapModel { get; set; }
		private Light Light { get; set; }
		private Texture Texture { get; set; }
		private Bitmap Bitmap { get; set; }
		private Text Text { get; set; }
		private ModelList ModelList { get; set; }
		private Frustum Frustum { get; set; }
		private DebugWindow DebugWindow { get; set; }
		#endregion

		#region Shaders
		private FogShader FogShader { get; set; }
		private MultiTextureLightShader MultiTextureLightShader { get; set; }
		private MultiTextureShader MultiTextureShader { get; set; }
		private BumpMapShader BumpMapShader { get; set; }
		private SpecMapShader SpecMapShader { get; set; }
		private AlphaMapShader AlphaMapShader { get; set; }
		private LightShader LightShader { get; set; }
		private LightMapShader LightMapShader { get; set; }
		private TextureShader TextureShader { get; set; }
		#endregion

		#region Resources
		private RenderTexture RenderTexture { get; set; }
		#endregion
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
				Camera.SetPosition(0, 0, -10);
				Camera.Render();
				var baseViewMatrix = Camera.ViewMatrix;

				// Create the model class.
				Model = new Model();

				// Initialize the model object.
				if (!Model.Initialize(D3D.Device, "cube.txt", new[] { "seafloor.dds" }))
				{
					MessageBox.Show("Could not initialize the model object", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the shader object.
				FogShader = new FogShader();

				// Initialize the shader object.
				if (!FogShader.Initialize(D3D.Device, windowHandle))
				{
					MessageBox.Show("Could not initialize the fog shader", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the light object.
				Light = new Light();

				// Initialize the light object.
				Light.SetAmbientColor(0.15f, 0.15f, 0.15f, 1.0f);
				Light.SetDiffuseColor(1, 1, 1, 1f);
				Light.SetDirection(0, 0, 1);
				Light.SetSpecularColor(0, 1, 1, 1);
				Light.SetSpecularPower(16);

				if (SystemConfiguration.DebugWindowOn)
				{
					// Create the render to texture object.
					RenderTexture = new RenderTexture();

					// Initialize the render to texture object.
					if (!RenderTexture.Initialize(D3D.Device, configuration))
						return false;

					// Create the debug window object.
					DebugWindow = new DebugWindow();

					// Initialize the debug window object.
					if (!DebugWindow.Initialize(D3D.Device, configuration.Width, configuration.Height, 100, 100 * configuration.Height / configuration.Width))
					{
						MessageBox.Show("Could not initialize the debug window object.", "Error", MessageBoxButtons.OK);
						return false;
					}

					// Create the texture shader object.
					TextureShader = new TextureShader();

					// Initialize the texture shader object.
					if (!TextureShader.Initialize(D3D.Device, windowHandle))
					{
						MessageBox.Show("Could not initialize the texture shader object.", "Error", MessageBoxButtons.OK);
						return false;
					}
				}
				
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
			// Release the fog shader object.
			if (FogShader != null)
			{
				FogShader.Shuddown();
				FogShader = null;
			}

			// Release the texture shader object.
			if (TextureShader != null)
			{
				TextureShader.Shuddown();
				TextureShader = null;
			}

			// Release the debug window object.
			if (DebugWindow != null)
			{
				DebugWindow.Shutdown();
				DebugWindow = null;
			}

			// Release the render to texture object.
			if (RenderTexture != null)
			{
				RenderTexture.Shutdown();
				RenderTexture = null;
			}

			// Release the light object.
			Light = null;

			// Release the shader object.
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

			// Release the light shader object.
			if (BumpMapShader != null)
			{
				BumpMapShader.Shuddown();
				BumpMapShader = null;
			}

			// Release the model object.
			if (BumpMapModel != null)
			{
				BumpMapModel.Shutdown();
				BumpMapModel = null;
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

		public bool Render()
		{
			if (SystemConfiguration.DebugWindowOn)
			{
				// Render the entire scene to the texture first.
				if (!RenderToTexture())
					return false;
			}

			// Set the color of the fog to grey.
			var fogColor = 0.0f;

			// Clear the buffer to begin the scene.
			D3D.BeginScene(fogColor, fogColor, fogColor, 1f);

			// Render the scene as normal to the back buffer.
			if (!RenderScene())
				return false;

			if (SystemConfiguration.DebugWindowOn)
			{
				// Turn off the Z buffer to begin all 2D rendering.
				D3D.TurnZBufferOff();

				// Get the world, view, and projection matrices from camera and d3d objects.
				var viewMatrix = Camera.ViewMatrix;
				var worldMatrix = D3D.WorldMatrix;
				var orthoMatrix = D3D.OrthoMatrix;

				// Put the debug window vertex and index buffer on the graphics pipeline them for drawing.
				if (!DebugWindow.Render(D3D.DeviceContext, 50, 50))
					return false;

				// Render the debug window using the texture shader.
				if (!TextureShader.Render(D3D.DeviceContext, DebugWindow.IndexCount, worldMatrix, viewMatrix, orthoMatrix, RenderTexture.ShaderResourceView))
					return false;

				// Turn the Z buffer back on now that all 2D rendering has completed.
				D3D.TurnZBufferOn();
			}

			// Present the rendered scene to the screen.
			D3D.EndScene();

			return true;
		}

		private bool RenderToTexture()
		{
			// Set the render to be the render to the texture.
			RenderTexture.SetRenderTarget(D3D.DeviceContext, D3D.DepthStencilView);

			// Clear the render to texture.
			RenderTexture.ClearRenderTarget(D3D.DeviceContext, D3D.DepthStencilView, 0, 0, 1, 1);

			// Render the scene now and it will draw to the render to texture instead of the back buffer.
			if (!RenderScene())
				return false;

			// Reset the render target back to the original back buffer and not to texture anymore.
			D3D.SetBackBufferRenderTarget();

			return true;
		}

		private bool RenderScene()
		{
			// Generate the view matrix based on the camera position.
			Camera.Render();

			// Get the world, view, and projection matrices from camera and d3d objects.
			var viewMatrix = Camera.ViewMatrix;
			var worldMatrix = D3D.WorldMatrix;
			var projectionMatrix = D3D.ProjectionMatrix;

			// Rotate the world matrix by the rotation value so that the triangle will spin.
			Rotate();

			// Rotate the world matrix by the rotation value so that the triangle will spin.
			Matrix.RotationY(Rotation, out worldMatrix);

			// Put the model vertex and index buffers on the graphics pipeline to prepare them for drawing.
			Model.Render(D3D.DeviceContext);

			// Set the start and end of the fog.
			var fogStart = 0.0f;
			var fogEnd = 10.0f;

			// Render the model using the color shader.
			if (!FogShader.Render(D3D.DeviceContext, Model.IndexCount, worldMatrix, viewMatrix, projectionMatrix, Model.TextureCollection.Select(item => item.TextureResource).ToArray(), fogStart, fogEnd))
				return false;

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
