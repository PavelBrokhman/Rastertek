using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Engine.System
{
	public class Timer
	{
		#region Variables / Properties
		public long FrameTime { get; private set; }

		Stopwatch _StopWatch;
		#endregion

		#region Public Methods
		public bool Initialize()
		{
			// Check to see if this system supports high performance timers.
			if (Stopwatch.Frequency == 0)
				return false;

			_StopWatch = Stopwatch.StartNew();

			return true;
		}

		public void Frame()
		{
			_StopWatch.Stop();

			FrameTime = _StopWatch.ElapsedMilliseconds;

			_StopWatch.Restart();
		}
		#endregion

		#region Private Methods
		#endregion

		#region Interface Methods
		public object Clone()
		{
			return this.MemberwiseClone();
		}
		#endregion
	}
}
