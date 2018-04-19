using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Tutorial4.GameSystem
{
	public class SystemTime
	{
		Timer renderLoop;
		public delegate bool RenderDelegate();
		private RenderDelegate Render;
		public delegate void ExitDelegate();
		private ExitDelegate Exit;
		private object lockObject = new Object();
		private bool IsStopped = false;

		public void Initialize(RenderDelegate render, ExitDelegate exit)
		{
			renderLoop = new Timer(1);

			Render = render;
			Exit = exit;

			renderLoop.Elapsed += renderLoop_Elapsed;
		}

		public void Run()
		{
			IsStopped = false;
			renderLoop.Start();
		}

		void renderLoop_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (IsStopped)
				return;

			var result = true;
			lock(this)
			{
				result = Render();
			}
			if (!result)
				Stop();
		}

		internal void Stop()
		{
			if(!IsStopped)
			{
				IsStopped = true;
				renderLoop.Stop();
				Exit();
			}
		}
	}
}
