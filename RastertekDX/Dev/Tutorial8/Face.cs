using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tutorial8
{
	public class FaceIndices : ICloneable
	{
		public FaceIndices(string faceIndices)
		{
			var indices = faceIndices.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
			Vertex = int.Parse(indices[0]);
			Texture = int.Parse(indices[1]);
			Normal = int.Parse(indices[2]);
		}

		public int Vertex;
		public int Texture;
		public int Normal;

		public object Clone()
		{
			return MemberwiseClone();
		}
	}

	public class Face
	{
		public FaceIndices[] vertices;

		public Face(string face)
		{
			vertices = new FaceIndices[3];
			var vertexIndices = face.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
			vertices[0] = new FaceIndices(vertexIndices[0]);
			vertices[1] = new FaceIndices(vertexIndices[1]);
			vertices[2] = new FaceIndices(vertexIndices[2]);
		}
	}

	public class MayaFace : Face
	{
		public MayaFace(string face) : base(face)
		{
			var tempVertex = (FaceIndices)vertices[0].Clone();
			vertices[0] = (FaceIndices)vertices[2].Clone();
			vertices[2] = (FaceIndices)tempVertex.Clone();
		}
	}
}
