﻿using System;
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
		private RenderTexture RenderRefractionTexture { get; set; }
		private RenderTexture RenderTexture { get; set; }
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
		private Model GroundModel { get; set; }
		private Model WallModel { get; set; }
		private Model BathModel { get; set; }
		private Model WaterModel { get; set; }
		#endregion

		#region Shaders
		private ReflectionShader ReflectionShader { get; set; }
		private RefractionShader RefractionShader { get; set; }
		private WaterShader WaterShader { get; set; }
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
		private FadeShader FadeShader { get; set; }
		#endregion
		#endregion

		#region Variables
		private float FadeInTime { get; set; }
		private float AccumulatedTime { get; set; }
		private float FadePercentage { get; set; }
		private bool FadeDone { get; set; }
		private float WaterHeight { get; set; }
		private float WaterTranslation { get; set; }
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
				// Create the ground model class.
				GroundModel = new Model();

				// Initialize the ground model object.
				if (!GroundModel.Initialize(D3D.Device, "ground.txt", new[] { "ground01.dds" }))
				{
					MessageBox.Show("Could not initialize the ground model object", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the wall model class.
				WallModel = new Model();

				// Initialize the wall model object.
				if (!WallModel.Initialize(D3D.Device, "wall.txt", new[] { "wall01.dds" }))
				{
					MessageBox.Show("Could not initialize the wall model object", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the bath model class.
				BathModel = new Model();

				// Initialize the bath model object.
				if (!BathModel.Initialize(D3D.Device, "bath.txt", new[] { "marble01.dds" }))
				{
					MessageBox.Show("Could not initialize the bath model object", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the water model class.
				WaterModel = new Model();

				// Initialize the water model object.
				if (!WaterModel.Initialize(D3D.Device, "water.txt", new[] { "water01.dds" }))
				{
					MessageBox.Show("Could not initialize the bath model object", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the light object.
				Light = new Light();

				// Initialize the light object.
				Light.SetAmbientColor(0.15f, 0.15f, 0.15f, 1.0f);
				Light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
				Light.SetDirection(0.0f, -1.0f, 0.5f);
				Light.SetSpecularColor(0, 1, 1, 1);
				Light.SetSpecularPower(16);
				#endregion

				#region Initialize Data
				// Create the refraction render to texture object.
				RenderRefractionTexture = new RenderTexture();

				// Initialize the refraction render to texture object.
				if (!RenderRefractionTexture.Initialize(D3D.Device, configuration))
				{
					MessageBox.Show("Could not initialize the refraction render to texture object.", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the refraction render to texture object.
				RenderReflectionTexture = new RenderTexture();

				// Initialize the refraction render to texture object.
				if (!RenderReflectionTexture.Initialize(D3D.Device, configuration))
				{
					MessageBox.Show("Could not initialize the reflection render to texture object.", "Error", MessageBoxButtons.OK);
					return false;
				}
				#endregion

				#region Initialize Shaders
				// Create the light shader object.
				LightShader = new LightShader();
				// Initialize the light shader object.
				if (!LightShader.Initialize(D3D.Device, windowHandle))
				{
					MessageBox.Show("Could not initialize the light shader object.", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the refraction shader object.
				RefractionShader = new RefractionShader();
				// Initialize the refraction shader object.
				if (!RefractionShader.Initialize(D3D.Device, windowHandle))
				{
					MessageBox.Show("Could not initialize the refraction shader object.", "Error", MessageBoxButtons.OK);
					return false;
				}

				// Create the water shader object.
				WaterShader = new WaterShader();
				// Initialize the water shader object.
				if (!WaterShader.Initialize(D3D.Device, windowHandle))
				{
					MessageBox.Show("Could not initialize the water shader object.", "Error", MessageBoxButtons.OK);
					return false;
				}
				#endregion

				// Set the height of the water.
				WaterHeight = 2.75f;
				// Initialize the position of the water.
				WaterTranslation = 0f;

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
			// Release the water shader object.
			if (WaterShader != null)
			{
				WaterShader.Shuddown();
				WaterShader = null;
			}

			// Release the refraction shader object.
			if (RefractionShader != null)
			{
				RefractionShader.Shuddown();
				RefractionShader = null;
			}

			// Release the fade shader object.
			if (FadeShader != null)
			{
				FadeShader.Shuddown();
				FadeShader = null;
			}

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

			// Release the render to texture object.
			if (RenderTexture != null)
			{
				RenderTexture.Shutdown();
				RenderTexture = null;
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
			// Update the position of the water to simulate motion.
			WaterTranslation += 0.001f;
			if (WaterTranslation > 1.0f)
				WaterTranslation -= 1.0f;

			// Set the position and rotation of the camera;
			Camera.SetPosition(-13f, 6.0f, -10f);
			Camera.SetRotation(-0.0f, 45.0f, 0.0f);

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

			// Clear the buffer to begin the scene.
			D3D.BeginScene(0, 0, 0, 1f);

			// Render the refraction of the scene to a texture.
			if (!RenderRefractionToTexture())
				return false;

			// Render the reflection of the scene to a texture.
			if (!RenderReflectionToTexture())
				return false;

			// Render the scene as normal to the back buffer.
			if (!RenderScene())
				return false;

			if (SystemConfiguration.DebugWindowOn)
			{
				// Render Debug Scene
				if (!RenderDebugScene())
					return false;
			}

			// Present the rendered scene to the screen.
			D3D.EndScene();

			return true;
		}

		private bool RenderDebugScene()
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

			return true;
		}

		private bool RenderRefractionToTexture()
		{
			// Set the render target to be the refraction render to texture.
			RenderRefractionTexture.SetRenderTarget(D3D.DeviceContext, D3D.DepthStencilView);
			// Clear the render to texture.
			RenderRefractionTexture.ClearRenderTarget(D3D.DeviceContext, D3D.DepthStencilView, 0, 0, 0, 1);

			// Render the scene now and it will draw to the render to texture instead of the back buffer.
			if (!RenderRefractionScene())
				return false;

			// Reset the render target back to the original back buffer and not to texture anymore.
			D3D.SetBackBufferRenderTarget();

			return true;
		}

		private bool RenderRefractionScene()
		{
			// Setup a clipping plane based on the height of the water to clip everything above it.
			var clipPlane = new Vector4(0f, -1f, 0f, WaterHeight + 0.1f);

			// Generate the view matrix based on the camera position.
			Camera.Render();

			// Get the world, view, and projection matrices from camera and d3d objects.
			var viewMatrix = Camera.ViewMatrix;
			var worldMatrix = D3D.WorldMatrix;
			var projectionMatrix = D3D.ProjectionMatrix;

			// Translate to where the bath model will be rendered.
			Matrix.Translation(0f, 2f, 0f, out worldMatrix);

			// Put the bath model vertex and index buffers on the graphics pipeline to prepare them for drawing.
			BathModel.Render(D3D.DeviceContext);

			// Render the bath model using the light shader.
			if (!RefractionShader.Render(D3D.DeviceContext, BathModel.IndexCount, worldMatrix, viewMatrix, projectionMatrix, BathModel.TextureCollection.Select(item => item.TextureResource).First(), Light.Direction, Light.AmbientColor, Light.DiffuseColor, clipPlane))
				return false;

			return true;
		}

		private bool RenderReflectionToTexture()
		{
			// Set the render target to be the refraction render to texture.
			RenderReflectionTexture.SetRenderTarget(D3D.DeviceContext, D3D.DepthStencilView);
			// Clear the render to texture.
			RenderReflectionTexture.ClearRenderTarget(D3D.DeviceContext, D3D.DepthStencilView, 0, 0, 0, 1);

			// Render the scene now and it will draw to the render to texture instead of the back buffer.
			if (!RenderReflectionScene())
				return false;

			// Reset the render target back to the original back buffer and not to texture anymore.
			D3D.SetBackBufferRenderTarget();

			return true;
		}

		private bool RenderReflectionScene()
		{
			// Use the camera to render the reflection and create a reflection view matrix.
			Camera.RenderReflection(WaterHeight);

			// Get the camera reflection view matrix instead of the normal view matrix.
			var viewMatrix = Camera.ReflectionViewMatrix;
			// Get the world and projection matrices from the d3d object.
			var worldMatrix = D3D.WorldMatrix;
			var projectionMatrix = D3D.ProjectionMatrix;

			// Translate to where the bath model will be rendered.
			Matrix.Translation(0f, 6f, 8f, out worldMatrix);

			// Put the wall model vertex and index buffers on the graphics pipeline to prepare them for drawing.
			WallModel.Render(D3D.DeviceContext);

			// Render the wall model using the light shader and the reflection view matrix.
			if (!LightShader.Render(D3D.DeviceContext, WallModel.IndexCount, worldMatrix, viewMatrix, projectionMatrix, WallModel.TextureCollection.Select(item => item.TextureResource).First(), Light.Direction, Light.AmbientColor, Light.DiffuseColor, Camera.GetPosition(), Light.SpecularColor, Light.SpecularPower))
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

			#region Render Ground Model
			// Translate to where the ground model will be rendered.
			Matrix.Translation(0f, 1f, 0f, out worldMatrix);

			// Put the ground model vertex and index buffers on the graphics pipeline to prepare them for drawing.
			GroundModel.Render(D3D.DeviceContext);

			// Render the ground model using the light shader.
			if (!LightShader.Render(D3D.DeviceContext, GroundModel.IndexCount, worldMatrix, viewMatrix, projectionMatrix, GroundModel.TextureCollection.Select(item => item.TextureResource).First(), Light.Direction, Light.AmbientColor, Light.DiffuseColor, Camera.GetPosition(), Light.SpecularColor, Light.SpecularPower))
				return false;
			#endregion

			// Reset the world matrix.
			worldMatrix = D3D.WorldMatrix;

			#region Render Wall Model
			// Translate to where the ground model will be rendered.
			Matrix.Translation(0f, 6f, 8f, out worldMatrix);

			// Put the wall model vertex and index buffers on the graphics pipeline to prepare them for drawing.
			WallModel.Render(D3D.DeviceContext);

			// Render the wall model using the light shader.
			if (!LightShader.Render(D3D.DeviceContext, WallModel.IndexCount, worldMatrix, viewMatrix, projectionMatrix, WallModel.TextureCollection.Select(item => item.TextureResource).First(), Light.Direction, Light.AmbientColor, Light.DiffuseColor, Camera.GetPosition(), Light.SpecularColor, Light.SpecularPower))
				return false;
			#endregion

			// Reset the world matrix.
			worldMatrix = D3D.WorldMatrix;

			#region Render Bath Model
			// Translate to where the bath model will be rendered.
			Matrix.Translation(0f, 2f, 0f, out worldMatrix);

			// Put the bath model vertex and index buffers on the graphics pipeline to prepare them for drawing.
			BathModel.Render(D3D.DeviceContext);

			// Render the bath model using the light shader.
			if (!LightShader.Render(D3D.DeviceContext, BathModel.IndexCount, worldMatrix, viewMatrix, projectionMatrix, BathModel.TextureCollection.Select(item => item.TextureResource).First(), Light.Direction, Light.AmbientColor, Light.DiffuseColor, Camera.GetPosition(), Light.SpecularColor, Light.SpecularPower))
				return false;
			#endregion

			// Reset the world matrix.
			worldMatrix = D3D.WorldMatrix;

			#region Render Bath Model
			// Get the camera reflection view matrix.
			var reflectionMatrix = Camera.ReflectionViewMatrix;

			// Translate to where the water model will be rendered.
			Matrix.Translation(0f, WaterHeight, 0f, out worldMatrix);

			// Put the water model vertex and index buffers on the graphics pipeline to prepare them for drawing.
			WaterModel.Render(D3D.DeviceContext);

			// Render the bath model using the light shader.
			if (!WaterShader.Render(D3D.DeviceContext, WaterModel.IndexCount, worldMatrix, viewMatrix, projectionMatrix, reflectionMatrix, RenderReflectionTexture.ShaderResourceView, RenderRefractionTexture.ShaderResourceView, WaterModel.TextureCollection.Select(item => item.TextureResource).First(), WaterTranslation, 0.01f))
				return false;
			#endregion
			return true;
		}

		private bool RenderToDebugTexture()
		{
			// Set the render to be the render to the texture.
			RenderDebugTexture.SetRenderTarget(D3D.DeviceContext, D3D.DepthStencilView);

			// Clear the render to texture.
			RenderDebugTexture.ClearRenderTarget(D3D.DeviceContext, D3D.DepthStencilView, 0, 0, 0, 1);

			// Render the scene now and it will draw to the render to texture instead of the back buffer.
			if (!RenderScene())
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
