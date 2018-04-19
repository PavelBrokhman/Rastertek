using System;
using System.Windows.Forms;
using System.Drawing;
using SharpDX.Windows;

namespace RastertekDX.Common
{
	public class SystemApp : ICloneable, IDisposable
	{
		#region Variables / Properties
		private SystemApp ApplicationHandle { get; set; }
		private string ApplicationName { get; set; }

		private Control RenderControl { get; set; }
		private FormWindowState CurrentFormWindowState { get; set; }

		public SystemSettings Settings { get; private set; }
		public Input Input { get; private set; }
		public Graphics Graphics { get; private set; }

		private SystemTime SystemTime { get; set; }
		private delegate void ExitCallback();
		private bool IsExiting = false;
		#endregion

		#region Static
		#endregion

		#region Constructors
		internal SystemApp()
		{
		}
		#endregion

		#region Methods
		public bool Initialize()
		{
			if (Settings == null)
				Settings = new SystemSettings();

			InitializeWindows(new RenderForm());

			if (Input == null)
			{
				Input = new Input();
				Input.Initialize();
			}

			if (Graphics == null)
			{
				Graphics = new Graphics();
				if (!Graphics.Initialize(Settings, RenderControl.Handle))
					return false;
			}

			return true;
		}

		private void InitializeWindows(Control renderControl)
		{
			if (RenderControl != null)
				return;

			ApplicationHandle = this;
			ApplicationName = "Engine";

			RenderControl = renderControl;
			var deltaWidth = Settings.Width - RenderControl.Width;
			var deltaHeight = Settings.Height - RenderControl.Height;

			var parentForm = RenderControl.FindForm();
			parentForm.Text = Settings.Title;
			parentForm.FormBorderStyle = SystemSettings.BorderStyle;
			parentForm.ClientSize = new Size(parentForm.ClientSize.Width + deltaWidth, parentForm.ClientSize.Height + deltaHeight);

			RenderControl.ClientSize = new Size(Settings.Width, Settings.Height);

			parentForm.Show();
		}

		public void Run()
		{
			Initialize();

			var isFormClosed = false;
			bool formIsResizing;
			var parentForm = RenderControl.FindForm();

			RenderControl.KeyDown += HandleKeyDown;
			RenderControl.KeyUp += HandleKeyUp;
			parentForm.Closed += (o, args) => { isFormClosed = true; };
			RenderControl.MouseEnter += (o, args) => { Cursor.Hide(); };
			RenderControl.MouseLeave += (o, args) => { Cursor.Show(); };
			RenderControl.Resize += (o, args) =>
			{
				if (parentForm.WindowState != CurrentFormWindowState)
				{
					HandleResize(o, args);
				}

				CurrentFormWindowState = parentForm.WindowState;
			};

			parentForm.ResizeBegin += (o, args) => { formIsResizing = true; };
			parentForm.ResizeEnd += (o, args) =>
			{
				formIsResizing = false;
				HandleResize(o, args);
			};

			parentForm.FormClosing += FormClosing;

			//MainForm.Paint += MainForm_Paint;
			if (isFormClosed)
			{
				return;
			}

			SystemTime = new SystemTime();
			SystemTime.Initialize(new SystemTime.RenderDelegate(Frame), new SystemTime.ExitDelegate(Exit));
			SystemTime.Run();
			Application.Run(parentForm);
		}

		void FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!IsExiting)
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
			if (!IsExiting)
			{
				if (RenderControl.InvokeRequired)
				{
					var d = new ExitCallback(Exit);
					RenderControl.Invoke(d);
				}
				else
				{
					IsExiting = true;
					RenderControl.FindForm().Close();
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
			if (RenderControl != null)
				RenderControl.Dispose();

			RenderControl = null;
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
			if (RenderControl.FindForm().WindowState == FormWindowState.Minimized)
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

		public void Dispose()
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
