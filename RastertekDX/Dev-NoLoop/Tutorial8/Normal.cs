using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tutorial8
{
	public class Normal
	{
		public float x;
		public float y;
		public float z;

		public Normal(string normal)
		{
			var normalCoords = normal.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
			x = float.Parse(normalCoords[0]);
			y = float.Parse(normalCoords[1]);
			z = float.Parse(normalCoords[2]);
		}
	}

	public class MayaNormal : Normal
	{
		public MayaNormal(string normal)
			: base(normal)
		{
			z = -z;
		}
	}
}
