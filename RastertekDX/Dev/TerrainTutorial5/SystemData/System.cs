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
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Application = Engine.SystemData.Application;
using GraphicsClass = Engine.Graphics.Graphics;

namespace Engine.SystemData
{
	internal class System : ICloneable
	{
		#region Variables / Properties
		private System ApplicationHandle { get; set; }
		private string ApplicationName { get; set; }

		private RenderForm MainForm { get; set; }
		private FormWindowState CurrentFormWindowState { get; set; }

		private Application Application { get; set; }

		public SystemConfiguration Configuration { get; private set; }
		public GraphicsClass Graphics { get; private set; }
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

			// Create the application wrapper object.
			Application = new Application();

			// Initialize the application wrapper object.
			if (!Application.Initialize(Configuration, MainForm.Handle))
				return false;

			return true;
		}

		public void Shutdown()
		{

			//// Release the sound object
			//if (Sound != null)
			//{
			//    Sound.Shutdown();
			//    Sound = null;
			//}

			//if (Graphics != null)
			//{
			//    Graphics.Shutdown();
			//    Graphics = null;
			//}

			// Release the application wrapper object.
			if (Application != null)
			{
				Application.Shutdown();
				Application = null;
			}

			ShutdownWindows();
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
					Exit();
			});
		}

		private bool Frame()
		{
			// Do the frame processing for the application object.
			if (!Application.Frame())
				return false;

			//// Set the frame time for calculating the updated position.
			//Position.FrameTime = Timer.FrameTime;

			//// Do the frame processing for the graphics object.
			//if (!Graphics.Frame(Position))
			//    return false;

			//// Finally render the graphics to the screen.
			//if (!Graphics.Render())
			//    return false;

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

		private void ShutdownWindows()
		{
			if (MainForm != null)
				MainForm.Dispose();

			MainForm = null;
			ApplicationHandle = null;
		}

		private void Exit()
		{
			MainForm.Close();
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
