using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RastertekDX.Common
{
	public class Graphics
	{
		public bool Initialize(SystemSettings settings, IntPtr intPtr)
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
