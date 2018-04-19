using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InputClass = Engine.Inputs.Input;
using SoundClass = Engine.Sounds.Sound;
using Engine.Inputs;
using Engine.Graphics;
using Engine.Graphics.Cameras;
using Engine.Graphics.Models;
using Engine.Graphics.Shaders;
using Engine.Graphics.Data;
using System.Windows.Forms;
using SharpDX;

namespace Engine.SystemData
{
	public class Application : ICloneable
	{
		#region Variables / Properties
		public InputClass Input { get; private set; }
		public SoundClass Sound { get; private set; }

		public FPS FPS { get; private set; }
		public CPU CPU { get; private set; }
		public Timer Timer { get; private set; }

		public Position Position { get; private set; }

		private DX11 D3D { get; set; }
		private Camera Camera { get; set; }
		private Model Model { get; set; }
		private ColorShader ColorShader { get; set; }
		private LightShader LightShader { get; set; }
		private TextureShader TextureShader { get; set; }
		private FontShader FontShader { get; set; }
		private Text Text { get; set; }

		private Terrain Terrain { get; set; }
		#endregion

		#region Public Methods
		public bool Initialize(SystemConfiguration configuration, IntPtr windowHandle)
		{
			if (Input == null)
			{
				Input = new InputClass();
				if (!Input.Initialize(configuration, windowHandle))
				{
					MessageBox.Show("Could not initialize input object", "Error", MessageBoxButtons.OK);
					return false;
				}
			}

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

			// Set the initial position of the camera.
			var cameraX = 50f;
			var cameraY = 18f;
			var cameraZ = -7f;

			Camera.SetPosition(cameraX, cameraY, cameraZ);

			// Create the terrain object.
			Terrain = new HeightMapTerrain();

			// Initialize the terrain object.
			if (!(Terrain as HeightMapTerrain).Initialize(D3D.Device, "heightMap01.bmp"))
			{
				MessageBox.Show("Could not initialize the terrain object", "Error", MessageBoxButtons.OK);
				return false;
			}

			// Create the light shader object.
			ColorShader = new ColorShader();

			// Initialize the light shader object.
			if (!ColorShader.Initialize(D3D.Device, windowHandle))
			{
				MessageBox.Show("Could not initialize the light shader", "Error", MessageBoxButtons.OK);
				return false;
			}

			// Create and initialize Timer.
			Timer = new Timer();
			if (!Timer.Initialize())
			{
				MessageBox.Show("Could not initialize Timer object", "Error", MessageBoxButtons.OK);
				return false;
			}

			// Create the position object.
			Position = new Position();

			// Set the initial position of the viewer to the same as the initial camera position.
			Position.SetPosition(new Vector3(cameraX, cameraY, cameraZ));

			// Create and initialize the FPS object.
			FPS = new FPS();
			FPS.Initialize();

			// Create and initialize the CPU.
			CPU = new CPU();
			CPU.Initialize();

			// Create the font shader object.
			FontShader = new FontShader();

			// Initialize the font shader object.
			if (!FontShader.Initialize(D3D.Device, windowHandle))
			{
				MessageBox.Show("Could not initialize font shader object", "Error", MessageBoxButtons.OK);
				return false;
			}

			// Create the text object.
			Text = new Text();
			if (!Text.Initialize(D3D.Device, D3D.DeviceContext, windowHandle, configuration.Width, configuration.Height, baseViewMatrix))
			{
				MessageBox.Show("Could not initialize the text object", "Error", MessageBoxButtons.OK);
				return false;
			}

			if (!Text.SetVideoCard(D3D.VideoCardDescription, D3D.VideoCardMemory, D3D.DeviceContext))
			{
				MessageBox.Show("Could not set video card into the text object", "Error", MessageBoxButtons.OK);
				return false;
			}


			return true;
		}

		public void Shutdown()
		{
			// Release the text object.
			if (Text != null)
			{
				Text.Shutdown();
				Text = null;
			}

			// Release the font shader object.
			if (FontShader != null)
			{
				FontShader.Shuddown();
				FontShader = null;
			}

			// Release the CPU object
			if (CPU != null)
			{
				CPU.Shutdown();
				CPU = null;
			}

			// Release the FPS object
			FPS = null;

			// Release the position object.
			Position = null;

			// Release the Timer object
			Timer = null;

			// Release the light shader object.
			if (ColorShader != null)
			{
				ColorShader.Shuddown();
				ColorShader = null;
			}

			// Release the terrain object.
			if (Terrain != null)
			{
				Terrain.Shutdown();
				Terrain = null;
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

			// Release the input object.
			if (Input != null)
			{
				Input.Shutdown();
				Input = null;
			}
		}

		public bool Frame()
		{
			// Do the input frame processing.
			if (!Input.Frame())
				return false;

			// Check if the user pressed escape and wants to quit.
			if (Input.IsEscapePressed())
				return false;

			// Update the system stats.
			Timer.Frame();
			FPS.Frame();
			CPU.Frame();

			//// Set the frames per second.
			//if (!Text.SetFps(FPS.Value, D3D.DeviceContext))
			//    return false;

			//// Set the cpu usage.
			//if (!Text.SetCpu(CPU.Value, D3D.DeviceContext))
			//    return false;

			// Do the frame input processing.
			if (!HandleInput(Timer.FrameTime))
				return false;

			// Render the graphics.
			if (!RenderGraphics())
				return false;

			return true;
		}
		#endregion

		#region Private Methods
		private bool HandleInput(float frameTime)
		{
			// Set the frame time for calculating the updating position.
			Position.FrameTime = frameTime;

			// Handle Input.
			var keydown = Input.IsLeftPressed();
			Position.TurnLeft(keydown);

			keydown = Input.IsRightPressed();
			Position.TurnRight(keydown);

			keydown = Input.IsUpPressed();
			Position.MoveForward(keydown);

			keydown = Input.IsDownPressed();
			Position.MoveBackward(keydown);

			keydown = Input.IsAPressed();
			Position.MoveUpward(keydown);

			keydown = Input.IsZPressed();
			Position.MoveDownward(keydown);

			keydown = Input.IsPgUpPressed();
			Position.LookUpward(keydown);

			keydown = Input.IsPgDownPressed();
			Position.LookDownward(keydown);

			// Get the view point position / rotation.
			var position = Position.GetPosition();
			var rotation = Position.GetRotation();

			// Set position of the camera.
			Camera.SetPosition(position.X, position.Y, position.Z);
			Camera.SetRotation(rotation.X, rotation.Y, rotation.Z);

			// Update the position values in the text object.
			if (!Text.SetCameraPosition(position, D3D.DeviceContext))
				return false;

			// Update the rotation values in the text object.
			if (!Text.SetCameraRotation(rotation, D3D.DeviceContext))
				return false;

			return true;
		}

		private bool RenderGraphics()
		{
			// Clear the scene.
			D3D.BeginScene(0, 0, 0, 1);

			// Generate the view matrix based on the camera's position.
			Camera.Render();

			// Get the world, view, and projection matrices from camera and d3d objects.
			var viewMatrix = Camera.ViewMatrix;
			var worldMatrix = D3D.WorldMatrix;
			var projectionMatrix = D3D.ProjectionMatrix;
			var orthoMatrix = D3D.OrthoMatrix;

			// Render the terrain buffers.
			Terrain.Render(D3D.DeviceContext);

			// Render the model using the color shader.
			if (!ColorShader.Render(D3D.DeviceContext, Terrain.IndexCount, worldMatrix, viewMatrix, projectionMatrix))
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
		#endregion

		#region Override Methods
		public object Clone()
		{
			return MemberwiseClone();
		}
		#endregion
	}
}
