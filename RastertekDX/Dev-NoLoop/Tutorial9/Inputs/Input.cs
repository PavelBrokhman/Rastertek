using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;

namespace Tutorial9.Inputs
{
	class Input : ICloneable
	{
		#region Variables / Properties
		Dictionary<Keys, bool> InputKeys = new Dictionary<Keys, bool>();
		#endregion

		#region Methods
		internal void Initialize()
		{
			foreach (var key in Enum.GetValues(typeof(Keys)))
			{
				InputKeys[(Keys)key] = false;
			}

		}

		internal bool IsKeyDown(Keys key)
		{
			return InputKeys[key];
		}

		internal void KeyDown(Keys key)
		{
			InputKeys[key] = true;
		}

		internal void KeyUp(Keys key)
		{
			InputKeys[key] = false;
		}
		#endregion

		#region Interface Methods
		public object Clone()
		{
			return MemberwiseClone();
		}
		#endregion
	}
}
