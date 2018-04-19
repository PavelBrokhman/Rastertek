using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SystemClass = Tutorial12.System.System;

namespace Tutorial12
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
