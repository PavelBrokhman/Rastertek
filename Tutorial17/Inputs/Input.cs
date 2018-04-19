using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using Tutorial17.System;
using SharpDX.RawInput;
using SharpDX.Win32;
using System.IO;
using SharpDX.Multimedia;
using SharpDX.DirectInput;

namespace Tutorial17.Inputs
{
	class Input : ICloneable
	{
		#region Variables / Properties
		DirectInput _DirectInput;
		Keyboard _Keyboard;
		Mouse _Mouse;

		KeyboardState _KeyboardState;
		MouseState _MouseState;

		int _ScreenWidth, _ScreenHeight;
		int _MouseX, _MouseY;
		#endregion

		#region Methods
		internal bool Initialize(SystemConfiguration configuration, IntPtr windowsHandle)
		{
			// Screen the screen size which will be used for positioning the mouse cursor.
			_ScreenWidth = configuration.Width;
			_ScreenHeight = configuration.Height;

			// Initialize the location of the mouse on the screen.
			_MouseX = 0;
			_MouseY = 0;

			// Initialize the main direct input interface.
			_DirectInput = new DirectInput();

			// Initialize the direct interface for the keyboard.
			_Keyboard = new Keyboard(_DirectInput);
			_Keyboard.Properties.BufferSize = 256;

			// Set the cooperative level of the keyboard to not share with other programs.
			_Keyboard.SetCooperativeLevel(windowsHandle, CooperativeLevel.Foreground | CooperativeLevel.Exclusive);

			// Now acquire the keyboard.
			_Keyboard.Acquire();

			// Initialize the direct interface for the mouse.
			_Mouse = new Mouse(_DirectInput);
			_Mouse.Properties.AxisMode = DeviceAxisMode.Relative;

			// Set the cooperative level of the mouse to share with other programs.
			_Mouse.SetCooperativeLevel(windowsHandle, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);

			// Now acquire the mouse.
			_Mouse.Acquire();

			return true;
		}

		public void Shutdown()
		{
			// Release the mouse.
			if (_Mouse != null)
			{
				_Mouse.Unacquire();
				_Mouse.Dispose();
				_Mouse = null;
			}

			// Release the keyboard.
			if (_Keyboard != null)
			{
				_Keyboard.Unacquire();
				_Keyboard.Dispose();
				_Keyboard = null;
			}

			// Release the main interface to direct input.
			if (_DirectInput != null)
			{
				_DirectInput.Dispose();
				_DirectInput = null;
			}
		}

		public bool Frame()
		{
			// Read the current state of the keyboard.
			if (!ReadKeyboard())
				return false;

			// Read the current state of the mouse.
			if (!ReadMouse())
				return false;

			// Process the changes in the mouse and keyboard.
			ProcessInput();

			return true;
		}

		private bool ReadKeyboard()
		{
			var resultCode = ResultCode.Ok;

			_KeyboardState = new KeyboardState();
			try
			{
				// Read the keyboard device.
				_Keyboard.GetCurrentState(ref _KeyboardState);
			}
			catch (SharpDX.SharpDXException ex)
			{
				resultCode = ex.Descriptor;
			}
			catch (Exception)
			{
				return false;
			}

			// If the mouse lost focus or was not acquired then try to get control back.
			if (resultCode == ResultCode.InputLost || resultCode == ResultCode.NotAcquired)
			{
				try
				{
					_Keyboard.Acquire();
				}
				catch
				{
				}

				return true;
			}

			if (resultCode == ResultCode.Ok)
				return true;

			return false;
		}

		private bool ReadMouse()
		{
			var resultCode = ResultCode.Ok;

			_MouseState = new MouseState();
			try
			{
				// Read the mouse device.
				_Mouse.GetCurrentState(ref _MouseState);
			}
			catch (SharpDX.SharpDXException ex)
			{
				resultCode = ex.Descriptor;
			}
			catch (Exception)
			{
				return false;
			}

			// If the mouse lost focus or was not acquired then try to get control back.
			if (resultCode == ResultCode.InputLost || resultCode == ResultCode.NotAcquired)
			{
				try
				{
					_Mouse.Acquire();
				}
				catch
				{
				}

				return true;
			}

			if (resultCode == ResultCode.Ok)
				return true;

			return false;
		}

		private void ProcessInput()
		{
			if (_MouseState != null)
			{
				_MouseX += _MouseState.X;
				_MouseY += _MouseState.Y;
			}

			// Ensure the mouse location doesn't exceed the screen width or height.
			if (_MouseX < 0) _MouseX = 0;
			if (_MouseY < 0) _MouseY = 0;

			if (_MouseX > _ScreenWidth) _MouseX = _ScreenWidth;
			if (_MouseY > _ScreenHeight) _MouseY = _ScreenHeight;
		}

		public bool IsEscapePressed()
		{
			// Do a bitwise and on the keyboard state to check if the escape key is currently being pressed.
			return _KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Escape);
		}

		public bool IsLeftArrowPressed()
		{
			// Do a bitwise and on the keyboard state to check if the escape key is currently being pressed.
			return _KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Left);
		}

		public bool IsRightArrowPressed()
		{
			// Do a bitwise and on the keyboard state to check if the escape key is currently being pressed.
			return _KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Right);
		}

		public bool IsUpArrowPressed()
		{
			return _KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.UpArrow);
		}

		public bool IsDownArrowPressed()
		{
			return _KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Down);
		}

		public void GetMouseLocation(out int mouseX, out int mouseY)
		{
			mouseX = _MouseX;
			mouseY = _MouseY;
		}
		#endregion

		#region Interface Methods
		public object Clone()
		{
			return MemberwiseClone();
		}
		#endregion
	}
}
