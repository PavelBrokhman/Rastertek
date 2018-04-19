using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using SharpDX;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SharpDX.Direct3D;
using System.Diagnostics;

namespace Tutorial2
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			SystemClass system;
			bool result;

			system = new SystemClass();
			try
			{
				result = system.Initialize();
				if (result)
					system.Run();
			}
			finally
			{
				system.Shutdown();
			}
		}
	}
}
