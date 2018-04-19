using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SystemClass = Engine.System.System;
using System.Threading;
using System.Globalization;

namespace Engine
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			SystemClass system;
			bool result;

			Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
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
