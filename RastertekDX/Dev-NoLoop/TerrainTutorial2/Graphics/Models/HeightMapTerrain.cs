using Device = SharpDX.Direct3D11.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using Engine.Graphics.Shaders;
using SharpDX;
using Engine.SystemData;
using System.Diagnostics;

namespace Engine.Graphics.Models
{
	public class HeightMapTerrain : Terrain
	{
		#region Structures
		public struct HeightMapType
		{
			public float x, y, z;
			public static implicit operator Vector3(HeightMapType hmapVector)
			{
				return new Vector3(hmapVector.x, hmapVector.y, hmapVector.z);
			}
		}
		#endregion

		#region Variables
		List<HeightMapType> HeightMap { get; set; }
		#endregion

		#region Methods
		public bool Initialize(Device device, string heightMapFileName)
		{
			// Load the height map for the terrain
			if (!LoadHeightMapFilename(heightMapFileName))
				return false;

			// Normalize the height of the height map
			NormalizeHeightMap();

			// Set the number of vertices per quad (2 triangles)
			NumberOfVerticesPerQuad = 12;
			// Initialize the vertex and index buffer.
			if (!InitializeBuffers(device))
				return false;

			return true;
		}

		protected override void FillArrays(out ColorShader.Vertex[] vertices, out int[] indices)
		{
			// Create the vertex array.
			vertices = new ColorShader.Vertex[VertexCount];
			// Create the index array.
			indices = new int[IndexCount];
			var index = 0;

			for (var j = 0; j < TerrainHeight - 1; j++)
			{
				for (var i = 0; i < TerrainWidth - 1; i++)
				{
					var indexBottomLeft = TerrainHeight * j + i;
					var indexBottomRight = TerrainHeight * j + (i + 1);
					var indexUpperLeft = TerrainHeight * (j + 1) + i;
					var indexUpperRight = TerrainHeight * (j + 1) + (i + 1);

					#region First Triangle
					// Upper left
					vertices[index] = new ColorShader.Vertex()
					{
						position = HeightMap[indexUpperLeft],
						color = Vector4.One
					};
					indices[index] = index++;

					// Upper right
					vertices[index] = new ColorShader.Vertex()
					{
						position = HeightMap[indexUpperRight],
						color = Vector4.One
					};
					indices[index] = index++;

					// Upper right
					vertices[index] = new ColorShader.Vertex()
					{
						position = HeightMap[indexUpperRight],
						color = Vector4.One
					};
					indices[index] = index++;

					// Bottom Left
					vertices[index] = new ColorShader.Vertex()
					{
						position = HeightMap[indexBottomLeft],
						color = Vector4.One
					};
					indices[index] = index++;

					// Bottom Left
					vertices[index] = new ColorShader.Vertex()
					{
						position = HeightMap[indexBottomLeft],
						color = Vector4.One
					};
					indices[index] = index++;

					// Upper left
					vertices[index] = new ColorShader.Vertex()
					{
						position = HeightMap[indexUpperLeft],
						color = Vector4.One
					};
					indices[index] = index++;
					#endregion

					#region Second Triangle
					// Bottom Left
					vertices[index] = new ColorShader.Vertex()
					{
						position = HeightMap[indexBottomLeft],
						color = Vector4.One
					};
					indices[index] = index++;

					// Upper right
					vertices[index] = new ColorShader.Vertex()
					{
						position = HeightMap[indexUpperRight],
						color = Vector4.One
					};
					indices[index] = index++;

					// Upper right
					vertices[index] = new ColorShader.Vertex()
					{
						position = HeightMap[indexUpperRight],
						color = Vector4.One
					};
					indices[index] = index++;

					// Bottom right
					vertices[index] = new ColorShader.Vertex()
					{
						position = HeightMap[indexBottomRight],
						color = Vector4.One
					};
					indices[index] = index++;

					// Bottom right
					vertices[index] = new ColorShader.Vertex()
					{
						position = HeightMap[indexBottomRight],
						color = Vector4.One
					};
					indices[index] = index++;

					// Bottom Left
					vertices[index] = new ColorShader.Vertex()
					{
						position = HeightMap[indexBottomLeft],
						color = Vector4.One
					};
					indices[index] = index++;
					#endregion
				}
			}
		}
		public override void Shutdown()
		{
			// Release the vertex and index buffers.
			base.Shutdown();

			// Release the height map data.
			ShutdownHeightMap();
		}

		private bool LoadHeightMapFilename(string heightMapFileName)
		{
			System.Drawing.Bitmap bitmap;
			try
			{
				// Open heightmap file
				bitmap = new System.Drawing.Bitmap(SystemConfiguration.DataFilePath + heightMapFileName);
			}
			catch (Exception)
			{
				return false;
			}

			// Save the dimensions of the terrain
			TerrainHeight = bitmap.Height;
			TerrainWidth = bitmap.Width;

			// Create the structure to hold the height map data
			HeightMap = new List<HeightMapType>(TerrainWidth * TerrainHeight);

			// Read the image data into the height map
			for(var j = 0; j < TerrainHeight; j++)
				for (var i = 0; i < TerrainWidth; i++)
					HeightMap.Add(new HeightMapType()
					{
						x = i,
						y = bitmap.GetPixel(i, j).R,
						z = j
					});

			return true;
		}

		private void NormalizeHeightMap()
		{
			for (var i = 0; i < HeightMap.Count; i++)
			{
				var temp = HeightMap[i];
				temp.y /= 15;
				HeightMap[i] = temp;
			}
		}

		private void ShutdownHeightMap()
		{
			HeightMap.Clear();
			HeightMap = null;
		}
		#endregion
	}
}
