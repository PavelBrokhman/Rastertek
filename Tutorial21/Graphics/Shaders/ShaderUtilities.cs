using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tutorial21.Graphics.Shaders
{
	public static class ShaderUtilities
	{
		public const int ZeroAlignment = 0;
		public const int FloatAlignment = 4;
		public const int Vector2Alignment = FloatAlignment * 2;
		public const int Vector3Alignment = FloatAlignment * 3;
		public const int Vector4Alignment = FloatAlignment * 4;
		public const int MatrixAlignment = Vector4Alignment * 4;
	}
}
