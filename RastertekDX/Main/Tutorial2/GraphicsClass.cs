using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tutorial2
{
	public class GraphicsClass
	{
		public bool Initialize(SystemConfiguration Configuration, IntPtr intPtr)
		{
			return true;
		}

		public void Shutdown()
		{
		}

		public bool Frame()
		{
			return true;
		}

		public bool Render()
		{
			return true;
		}
	}
}
