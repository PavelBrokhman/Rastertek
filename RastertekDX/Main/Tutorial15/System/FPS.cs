using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tutorial15.System
{
	public class FPS : ICloneable
	{
		#region Variables / Properties
		public int Value { get; private set; }

		private int _Count;
		private TimeSpan _StartTime;
		#endregion

		#region Public Methods
		public void Initialize()
		{
			Value = 0;
			_Count = 0;
			_StartTime = DateTime.Now.TimeOfDay;
		}

		public void Frame()
		{
			_Count++;

			var delta = (DateTime.Now.TimeOfDay - _StartTime).Seconds;
			if (delta >= 1)
			{
				Value = _Count;
				_Count = 0;

				_StartTime = DateTime.Now.TimeOfDay;
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
