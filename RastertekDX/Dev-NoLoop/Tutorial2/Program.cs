using System;

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
