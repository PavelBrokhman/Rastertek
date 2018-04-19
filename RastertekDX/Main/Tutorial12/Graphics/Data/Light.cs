using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using SharpDX;

namespace Tutorial12.Graphics.Data
{
	public class Light
	{
		#region Variables / Properties
		public Vector4 AmbientColor { get; private set; }
		public Vector4 DiffuseColor { get; private set; }
		public Vector3 Direction { get; private set; }
		public Vector4 SpecularColor { get; private set; }
		public float SpecularPower { get; private set; }
		#endregion

		#region Methods
		public void SetAmbientColor(float red, float green, float blue, float alpha)
		{
			AmbientColor = new Vector4(red, green, blue, alpha);
		}

		public void SetDiffuseColor(float red, float green, float blue, float alpha)
		{
			DiffuseColor = new Vector4(red, green, blue, alpha);
		}

		public void SetDirection(float x, float y, float z)
		{
			Direction = new Vector3(x, y, z);
		}

		public void SetSpecularColor(float red, float green, float blue, float alpha)
		{
			SpecularColor = new Vector4(red, green, blue, alpha);
		}

		public void SetSpecularPower(float power)
		{
			SpecularPower = power;
		}
		#endregion
	}
}
