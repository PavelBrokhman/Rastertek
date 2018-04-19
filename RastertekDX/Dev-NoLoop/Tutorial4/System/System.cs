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
using GraphicsClass = Tutorial4.Graphics.Graphics;
using InputClass = Tutorial4.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Tutorial4.GameSystem
{
	internal class GameSystem : ICloneable
	{
		#region Variables / Properties
		private GameSystem ApplicationHandle { get; set; }
		private string ApplicationName { get; set; }

		private RenderForm MainForm { get; set; }
		private FormWindowState CurrentFormWindowState { get; set; }

		public SystemConfiguration Configuration { get; private set; }
		public InputClass Input { get; private set; }
		public GraphicsClass Graphics { get; private set; }

		private SystemTime SystemTime { get; set; }
		private delegate void ExitCallback();
		private bool IsExiting = false;
		#endregion

		#region Static
		#endregion

		#region Constructors
		internal GameSystem()
		{
		}
		#endregion

		#region Methods
		public bool Initialize()
		{
			if(Configuration == null)
				Configuration = new SystemConfiguration();

			InitializeWindows();

			if (Input == null)
			{
				Input = new InputClass();
				Input.Initialize();
			}

			if (Graphics == null)
			{
				Graphics = new GraphicsClass();
				if (!Graphics.Initialize(Configuration, MainForm.Handle))
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

			MainForm.KeyDown += HandleKeyDown;
			MainForm.KeyUp += HandleKeyUp;
			MainForm.Closed += (o, args) => { isFormClosed = true; };
			MainForm.MouseEnter += (o, args) => { Cursor.Hide(); };
			MainForm.MouseLeave += (o, args) => { Cursor.Show(); };
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

			MainForm.FormClosing += MainForm_FormClosing;

			//MainForm.Paint += MainForm_Paint;
			if (isFormClosed)
			{
				return;
			}

			SystemTime = new SystemTime();
			SystemTime.Initialize(new SystemTime.RenderDelegate(Frame), new SystemTime.ExitDelegate(Exit));
			SystemTime.Run();
			Application.Run(MainForm);

			//RenderLoop.Run(MainForm, () =>
			//{
				//var result = Frame();
				//if (!result)
				//	Exit();
			//});
		}

		void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if(!IsExiting)
			{
				IsExiting = true;
				SystemTime.Stop();
			}
		}

		void MainForm_Paint(object sender, PaintEventArgs e)
		{
			//var result = Frame();
			//if (!result)
			//	Exit();
		}

		private void Exit()
		{
			if(!IsExiting)
			{
				if (MainForm.InvokeRequired)
				{
					var d = new ExitCallback(Exit);
					MainForm.Invoke(d);
				}
				else
				{
					IsExiting = true;
					MainForm.Close();
				}
			}
		}

		private bool Frame()
		{
			// Check if the user pressed escape and wants to exit the application.
			if (Input.IsKeyDown(Keys.Escape) || IsExiting)
				return false;

			// Do the frame processing for the graphics object.
			return Graphics.Frame();
		}

		public void Shutdown()
		{
			if (Graphics != null)
			{
				Graphics.Shutdown();
				Graphics = null;
			}

			if (Input != null)
			{
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
		/// <summary>
		///   Handles a key down event.
		/// </summary>
		/// <param name = "sender">The sender.</param>
		/// <param name = "e">The <see cref = "GameSystem.Windows.Forms.KeyEventArgs" /> instance containing the event data.</param>
		private void HandleKeyDown(object sender, KeyEventArgs e)
		{
			KeyDown(e);
		}

		/// <summary>
		///   Handles a key up event.
		/// </summary>
		/// <param name = "sender">The sender.</param>
		/// <param name = "e">The <see cref = "GameSystem.Windows.Forms.KeyEventArgs" /> instance containing the event data.</param>
		private void HandleKeyUp(object sender, KeyEventArgs e)
		{
			KeyUp(e);
		}

		private void HandleResize(object sender, EventArgs e)
		{
			if (MainForm.WindowState == FormWindowState.Minimized)
			{
				return;
			}
		}

		protected virtual void KeyDown(KeyEventArgs e)
		{
			Input.KeyDown(e.KeyCode);
		}

		protected virtual void KeyUp(KeyEventArgs e)
		{
			Input.KeyUp(e.KeyCode);
		}
		#endregion
	}
}
