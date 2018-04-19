using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tutorial8
{
	public class Texture
	{
		public float x;
		public float y;

		public Texture(string texture)
		{
			var textureCoords = texture.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
			x = float.Parse(textureCoords[0]);
			y = float.Parse(textureCoords[1]);
		}
	}

	public class MayaTexture : Texture
	{
		public MayaTexture(string texture)
			: base(texture)
		{
			y = 1 - y;
		}
	}
}
