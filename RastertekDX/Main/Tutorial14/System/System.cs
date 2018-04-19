using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using GraphicsClass = Tutorial14.Graphics.Graphics;
using InputClass = Tutorial14.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SoundClass = Tutorial14.Sounds.Sound;
using Tutorial14.Sounds;

namespace Tutorial14.System
{
	internal class System : ICloneable
	{
		#region Variables / Properties
		private System ApplicationHandle { get; set; }
		private string ApplicationName { get; set; }

		private RenderForm MainForm { get; set; }
		private FormWindowState CurrentFormWindowState { get; set; }

		public SystemConfiguration Configuration { get; private set; }
		public InputClass Input { get; private set; }
		public GraphicsClass Graphics { get; private set; }
		public SoundClass Sound { get; private set; }
		#endregion

		#region Static
		#endregion

		#region Constructors
		internal System()
		{
		}
		#endregion

		#region Methods
		public bool Initialize()
		{
			// Initialize the system configuration.
			if(Configuration == null)
				Configuration = new SystemConfiguration();

			// Initialize windows api.
			InitializeWindows();

			if (Input == null)
			{
				Input = new InputClass();
				if (!Input.Initialize(Configuration, MainForm.Handle))
					return false;
			}

			if (Graphics == null)
			{
				Graphics = new GraphicsClass();
				if (!Graphics.Initialize(Configuration, MainForm.Handle))
					return false;
			}

			// Create the sound object
			Sound = new WaveSound("sound01.wav");

			// Initialize the sound object.
			if (!Sound.Initialize(MainForm.Handle))
			{
				MessageBox.Show("Could not initialize Direct Sound", "Error", MessageBoxButtons.OK);
				return false;
			}

			return true;
		}

		private void InitializeWindows()
		{
			if (MainForm != null)
				return;

			ApplicationHandle = this;
			ApplicationName = "Engine";

			MainForm = new RenderForm(Configuration.Title)
			{
				ClientSize = new Size(Configuration.Width, Configuration.Height),
				FormBorderStyle = SystemConfiguration.BorderStyle
			};

			MainForm.Show();
		}

		public void Run()
		{
			Initialize();

			var isFormClosed = false;
			var formIsResizing = false;

			MainForm.Closed += (o, args) => { isFormClosed = true; };
			//MainForm.MouseEnter += (o, args) => { Cursor.Hide(); };
			//MainForm.MouseLeave += (o, args) => { Cursor.Show(); };
			MainForm.Resize += (o, args) =>
			{
				if (MainForm.WindowState != CurrentFormWindowState)
				{
					HandleResize(o, args);
				}

				CurrentFormWindowState = MainForm.WindowState;
			};

			MainForm.ResizeBegin += (o, args) => { formIsResizing = true; };
			MainForm.ResizeEnd += (o, args) =>
			{
				formIsResizing = false;
				HandleResize(o, args);
			};

			RenderLoop.Run(MainForm, () =>
			{
				if (isFormClosed)
				{
					return;
				}

				var result = Frame();
				if (!result)
				{
					MessageBox.Show("Frame Processing Failed");
					Exit();
				}

				// Check if the user pressed escape and wants to quit.
				if (Input.IsEscapePressed())
					Exit();
			});
		}

		private void Exit()
		{
			MainForm.Close();
		}

		private bool Frame()
		{
			// Do the input frame processing.
			if (!Input.Frame())
				return false;

			// Get the location of the mouse from the input object.
			int mouseX, mouseY;
			Input.GetMouseLocation(out mouseX, out mouseY);

			// Do the frame processing for the graphics object.
			if (!Graphics.Frame(mouseX, mouseY))
				return false;

			// Finally render the graphics to the screen.
			if (!Graphics.Render())
				return false;

			return true;
		}

		public void Shutdown()
		{
			// Release the sound object
			if (Sound != null)
			{
				Sound.Shutdown();
				Sound = null;
			}

			if (Graphics != null)
			{
				Graphics.Shutdown();
				Graphics = null;
			}

			if (Input != null)
			{
				Input.Shutdown();
				Input = null;
			}

			ShutdownWindows();
		}

		private void ShutdownWindows()
		{
			if (MainForm != null)
				MainForm.Dispose();

			MainForm = null;
			ApplicationHandle = null;
		}
		#endregion

		#region Interface Methods
		public object Clone()
		{
			return this.MemberwiseClone();
		}
		#endregion

		#region Virtual Methods
		#endregion

		#region Events
		private void HandleResize(object sender, EventArgs e)
		{
			if (MainForm.WindowState == FormWindowState.Minimized)
			{
				return;
			}
		}
		#endregion
	}
}
