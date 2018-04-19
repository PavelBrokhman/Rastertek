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
using Engine.System;
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

		#region Data
		private Light Light { get; set; }
		private Texture Texture { get; set; }
		private RenderTexture RenderDebugTexture { get; set; }
		private RenderTexture RenderReflectionTexture { get; set; }
		#endregion

		#region Models
		private Model Model { get; set; }
		private Model Model2 { get; set; }
		private Model FloorModel { get; set; }
		private BumpMapModel BumpMapModel { get; set; }
		private Bitmap Bitmap { get; set; }
		private Text Text { get; set; }
		private ModelList ModelList { get; set; }
		private Frustum Frustum { get; set; }
		private DebugWindow DebugWindow { get; set; }
		#endregion

		#region Shaders
		private ReflectionShader ReflectionShader { get; set; }
		private TransparentShader TransparentShader { get; set; }
		private TranslateShader TranslateShader { get; set; }
		private ClipPlaneShader ClipPlaneShader { get; set; }
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
		#endregion

		#region Static Variables
		static float Rotation { get; set; }
		static float TextureTranslation { get; set; }
		#endregion

		#region Methods
		public bool Initialize(SystemConfiguration configuration, IntPtr windowHandle)
		{
			try
			{
				#region Initialize System
				// Create the Direct3D object.
				D3D = new DX11();
				// Initialize the Direct3D object.
				if (!D3D.Initialize(configuration, windowHandle))
				{
					MessageBox.Show("Could not initialize Direct3D", "Error", MessageBoxButtons.OK);
					return false;
				}
				#endregion

				#region Initialize Camera
				// Create the camera object
				Camera = new Camera();
				#endregion

				#region Initialize Models
				// Create the model class.
				Model = new Model();

				// Initialize the model object.
				if (!Model.Initialize(D3D.Device, "sphere.txt", new[] { "seafloor.dds" }))
				{
					MessageBox.Show("Could not initialize the model object", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the model class.
				FloorModel = new Model();

				// Initialize the model object.
				if (!FloorModel.Initialize(D3D.Device, "floor.txt", new[] { "blue01.dds" }))
				{
					MessageBox.Show("Could not initialize the model object", "Error", MessageBoxButtons.OK);
					return false;
				}
				#endregion

				#region Initialize Shaders
				// Create the shader object.
				TextureShader = new TextureShader();

				// Initialize the shader object.
				if (!TextureShader.Initialize(D3D.Device, windowHandle))
				{
					MessageBox.Show("Could not initialize the shader", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the shader object.
				ReflectionShader = new ReflectionShader();

				// Initialize the shader object.
				if (!ReflectionShader.Initialize(D3D.Device, windowHandle))
				{
					MessageBox.Show("Could not initialize the shader", "Error", MessageBoxButtons.OK);
					return false;
				}
				#endregion

				#region Initialize Data
				// Create the render to texture object.
				RenderReflectionTexture = new RenderTexture();

				// Initialize the render to texture object.
				if (!RenderReflectionTexture.Initialize(D3D.Device, configuration))
					return false;

				// Create the light object.
				Light = new Light();

				// Initialize the light object.
				Light.SetAmbientColor(0.15f, 0.15f, 0.15f, 1.0f);
				Light.SetDiffuseColor(1, 1, 1, 1f);
				Light.SetDirection(0, 0, 1);
				Light.SetSpecularColor(0, 1, 1, 1);
				Light.SetSpecularPower(16);
				#endregion

				#region Debug Window Initialize
				if (SystemConfiguration.DebugWindowOn)
				{
					// Create the render to texture object.
					RenderDebugTexture = new RenderTexture();

					// Initialize the render to texture object.
					if (!RenderDebugTexture.Initialize(D3D.Device, configuration))
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
				#endregion
				
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
			// Release the reflection shader object.
			if (ReflectionShader != null)
			{
				ReflectionShader.Shuddown();
				ReflectionShader = null;
			}

			// Release the transparent shader object.
			if (TransparentShader != null)
			{
				TransparentShader.Shuddown();
				TransparentShader = null;
			}

			// Release the translate shader object.
			if (TranslateShader != null)
			{
				TranslateShader.Shuddown();
				TranslateShader = null;
			}

			// Release the clip plane shader object.
			if (ClipPlaneShader != null)
			{
				ClipPlaneShader.Shuddown();
				ClipPlaneShader = null;
			}

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

			// Release the debug render to texture object.
			if (RenderDebugTexture != null)
			{
				RenderDebugTexture.Shutdown();
				RenderDebugTexture = null;
			}

			// Release the render to texture object.
			if (RenderReflectionTexture != null)
			{
				RenderReflectionTexture.Shutdown();
				RenderReflectionTexture = null;
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

		public bool Frame()
		{
			// Set the position of the camera;
			Camera.SetPosition(0, 0, -10);

			return true;
		}

		public bool Render()
		{
			if (SystemConfiguration.DebugWindowOn)
			{
				// Render the entire scene to the texture first.
				if (!RenderToDebugTexture())
					return false;
			}

			// Render the entire scene as a reflection to the texture first.
			if (!RenderToReflectionTexture())
				return false;

			// Clear the buffer to begin the scene.
			D3D.BeginScene(0, 0, 0, 1f);

			// Render the scene as normal to the back buffer.
			if (!RenderScene())
				return false;
	
			if (SystemConfiguration.DebugWindowOn)
			{
				// Turn off the Z buffer to begin all 2D rendering.
				D3D.TurnZBufferOff();

				// Get the world, view, and orthotic matrices from camera and d3d objects.
				var viewMatrix = Camera.ViewMatrix;
				var worldMatrix = D3D.WorldMatrix;
				var orthoMatrix = D3D.OrthoMatrix;

				// Put the debug window vertex and index buffer on the graphics pipeline them for drawing.
				if (!DebugWindow.Render(D3D.DeviceContext, 50, 50))
					return false;

				// Render the debug window using the texture shader.
				if (!TextureShader.Render(D3D.DeviceContext, DebugWindow.IndexCount, worldMatrix, viewMatrix, orthoMatrix, RenderDebugTexture.ShaderResourceView))
					return false;

				// Turn the Z buffer back on now that all 2D rendering has completed.
				D3D.TurnZBufferOn();
			}

			// Present the rendered scene to the screen.
			D3D.EndScene();

			return true;
		}

		private bool RenderToReflectionTexture()
		{
			// Set the render to be the render to the texture.
			RenderReflectionTexture.SetRenderTarget(D3D.DeviceContext, D3D.DepthStencilView);

			// Clear the render to texture.
			RenderReflectionTexture.ClearRenderTarget(D3D.DeviceContext, D3D.DepthStencilView, 1, 0, 0, 1);

			// Render the scene now and it will draw to the render to texture instead of the back buffer.
			if (!RenderReflectionScene())
			    return false;

			// Reset the render target back to the original back buffer and not to texture anymore.
			D3D.SetBackBufferRenderTarget();

			return true;
		}

		private bool RenderReflectionScene()
		{
			// Use the camera to calculate the reflection matrix.
			Camera.RenderReflection(-1.5f);

			// Get the world, view, and projection matrices from camera and d3d objects.
			var viewMatrix = Camera.ReflectionViewMatrix;
			var worldMatrix = D3D.WorldMatrix;
			var projectionMatrix = D3D.ProjectionMatrix;

			// Rotate the world matrix by the rotation value so that the triangle will spin.
			Rotate();

			// Rotate the world matrix by the rotation value so that the triangle will spin.
			Matrix.RotationY(Rotation, out worldMatrix);

			// Put the model vertex and index buffers on the graphics pipeline to prepare them for drawing.
			Model.Render(D3D.DeviceContext);

			// Render the model using the color shader.
			if (!TextureShader.Render(D3D.DeviceContext, Model.IndexCount, worldMatrix, viewMatrix, projectionMatrix, Model.TextureCollection.Select(item => item.TextureResource).First()))
				return false;

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

			// Render the model using the color shader.
			if (!TextureShader.Render(D3D.DeviceContext, Model.IndexCount, worldMatrix, viewMatrix, projectionMatrix, Model.TextureCollection.Select(item => item.TextureResource).First()))
				return false;

			// Get the world matrix again and translate down for the floor model to render underneath the cube.
			worldMatrix = D3D.WorldMatrix;
			Matrix.Translation(0, -1.5f, 0, out worldMatrix);

			// Get the camera reflection view matrix.
			var reflectionMatrix = Camera.ReflectionViewMatrix;

			// Put the second model on the graphics pipeline.
			FloorModel.Render(D3D.DeviceContext);

			// Render the floor model using the reflection shader, reflection texture, and reflection view matrix.
			if (!ReflectionShader.Render(D3D.DeviceContext, FloorModel.IndexCount, worldMatrix, viewMatrix, projectionMatrix, FloorModel.TextureCollection.Select(item => item.TextureResource).First(), RenderReflectionTexture.ShaderResourceView, reflectionMatrix))
				return false;

			return true;
		}

		private bool RenderToDebugTexture()
		{
			// Set the render to be the render to the texture.
			RenderDebugTexture.SetRenderTarget(D3D.DeviceContext, D3D.DepthStencilView);

			// Clear the render to texture.
			RenderDebugTexture.ClearRenderTarget(D3D.DeviceContext, D3D.DepthStencilView, 0, 0, 0, 1);

			// Render the scene now and it will draw to the render to texture instead of the back buffer.
			if (!RenderScene() || !RenderReflectionScene())
				return false;

			// Reset the render target back to the original back buffer and not to texture anymore.
			D3D.SetBackBufferRenderTarget();

			return true;
		}
		#endregion

		#region Static Methods
		public static void Rotate()
		{
			Rotation += (float)Math.PI * 0.0005f;
			if (Rotation > 360)
				Rotation -= 360;
		}

		public static void TextureTranslate()
		{
			TextureTranslation += 0.01f;
			if (TextureTranslation > 1)
				TextureTranslation -= 1;
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
