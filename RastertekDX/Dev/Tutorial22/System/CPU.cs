using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Tutorial22.System
{
	public class CPU : ICloneable
	{
		#region Variables / Properties
		public int Value 
		{
			get
			{
				return _CanReadCpu ? (int)_CpuUsage : 0;
			}
		}

		bool _CanReadCpu;
		PerformanceCounter counter;
		TimeSpan _LastSampleTime;
		long _CpuUsage;
		#endregion

		#region Public Methods
		public void Initialize()
		{
			// Initialize the flag indicating whether this object can read the system cpu usage or not.
			_CanReadCpu = true;

			try
			{
				// Create performance counter.
				counter = new PerformanceCounter();
				counter.CategoryName = "Processor";
				counter.CounterName = "% Processor Time";
				counter.InstanceName = "_Total";

				_LastSampleTime = DateTime.Now.TimeOfDay;

				_CpuUsage = 0;
			}
			catch
			{
				_CanReadCpu = false;
			}
		}

		public void Shutdown()
		{
			if(_CanReadCpu)
				counter.Close();
		}

		public void Frame()
		{
			if (_CanReadCpu)
			{
				var delta = (DateTime.Now.TimeOfDay - _LastSampleTime).Seconds;

				if (delta >= 1)
				{
					_LastSampleTime = DateTime.Now.TimeOfDay;
					_CpuUsage = (int)counter.NextValue();
				}
			}
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
