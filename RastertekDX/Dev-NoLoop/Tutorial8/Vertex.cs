using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tutorial8
{
	public class Vertex
	{
		public float x;
		public float y;
		public float z;

		public Vertex(string vertex)
		{
			var vertexCoords = vertex.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
			x = float.Parse(vertexCoords[0]);
			y = float.Parse(vertexCoords[1]);
			z = float.Parse(vertexCoords[2]);
		}
	}

	public class MayaVertex : Vertex
	{
		public MayaVertex(string vertex) : base(vertex)
		{
			z = -z;
		}
	}
}
