using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using SharpDX;

namespace Tutorial6.Graphics
{
	public class Light
	{
		#region Variables / Properties
		public Vector4 DiffuseColor { get; private set; }
		public Vector3 Direction { get; private set; }
		#endregion

		#region Methods
		public void SetDiffuseColor(float red, float green, float blue, float alpha)
		{
			DiffuseColor = new Vector4(red, green, blue, alpha);
		}

		public void SetDirection(float x, float y, float z)
		{
			Direction = new Vector3(x, y, z);
		}
		#endregion
	}
}
