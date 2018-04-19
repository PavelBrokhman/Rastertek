using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using Engine.Graphics.Shaders;
using SharpDX;
using Engine.SystemData;
using System.Diagnostics;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using Engine.Graphics.Data;

namespace Engine.Graphics.Models
{
	public class HeightMapTerrain : Terrain
	{
		#region Structures
		public struct HeightMapType
		{
			public float x, y, z;
			public float nx, ny, nz;
			public float tu, tv;
			public static implicit operator HeightMapTerrainShader.Vertex(HeightMapType hmapVector)
			{
				return new HeightMapTerrainShader.Vertex()
					{
						normal = new Vector3(hmapVector.nx, hmapVector.ny, hmapVector.nz),
						position = new Vector3(hmapVector.x, hmapVector.y, hmapVector.z),
						texture = new Vector2(hmapVector.tu, hmapVector.tv)
					};
			}
		}
		#endregion

		#region Variables
		List<HeightMapType> HeightMap { get; set; }

		public Texture Texture { get; private set; }

		private int TextureRepeat { get; set; }

		private HeightMapTerrainShader.Vertex[] Vertices { get; set; }
		private int[] Indices { get; set; }
		#endregion

		#region Methods
		public bool Initialize(Device device, string heightMapFileName, string textureFileName)
		{
			// Set the number of vertices per quad (2 triangles)
			NumberOfVerticesPerQuad = 6;

			// Set the value of the topology
			CurrentTopology = PrimitiveTopology.TriangleList;

			// How many times the terrain texture will be repeated both over the width and length of the terrain.
			TextureRepeat = 8;

			// Load the height map for the terrain
			if (!LoadHeightMapFilename(heightMapFileName))
				return false;

			// Normalize the height of the height map
			NormalizeHeightMap();

			// Calculate the normals for the terrain data.
			if (!CalculateNormals())
				return false;

			// Calculate the texture coordinates.
			CalculateTextureCoordinates();

			// Load the texture for this model.
			if (!LoadTexture(device, textureFileName))
				return false;

			// Initialize the vertex and index buffer.
			if (!InitializeBuffers(device))
				return false;

			return true;
		}

		public override void Shutdown()
		{
			// Release the vertex and index buffers.
			base.Shutdown();

			// Release texture.
			ReleaseTexture();

			// Release the height map data.
			ShutdownHeightMap();
		}

		protected new bool InitializeBuffers(Device device)
		{
			try
			{
				// Calculate the number of the vertices in the terrain mesh.
				VertexCount = (TerrainWidth - 1) * (TerrainHeight - 1) * NumberOfVerticesPerQuad;
				// Set the index count to the same as the vertex count.
				IndexCount = VertexCount;

				FillArrays();

				return true;
			}
			catch
			{
				return false;
			}
		}

		protected override void RenderBuffers(DeviceContext deviceContext)
		{
			// Set the vertex buffer to active in the input assembler so it can be rendered.
			deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<HeightMapTerrainShader.Vertex>(), 0));
			// Set the index buffer to active in the input assembler so it can be rendered.
			deviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
			// Set the type of the primitive that should be rendered from this vertex buffer, in this case triangles.
			deviceContext.InputAssembler.PrimitiveTopology = CurrentTopology;
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

		private bool LoadTexture(Device device, string textureFileName)
		{
			textureFileName = SystemConfiguration.DataFilePath + textureFileName;

			// Create the texture object.
			Texture = new Texture();

			// Initialize the texture object.
			if (!Texture.Initialize(device, textureFileName))
				return false;

			return true;
		}

		private void ReleaseTexture()
		{
			// Release the texture object.
			if (Texture != null)
			{
				Texture.Shutdown();
				Texture = null;
			}
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

		private bool CalculateNormals()
		{
			// Create a temporary array to hold the un-normalized normal verctors.
			var normals = new Vector3[(TerrainHeight - 1) * (TerrainWidth - 1)];

			// Go through all the faces in the mesh and calculate their normals.
			for (var j = 0; j < (TerrainHeight - 1); j++)
			{
				for (var i = 0; i < (TerrainWidth - 1); i++)
				{
					var index1 = (j * TerrainHeight) + i;
					var index2 = (j * TerrainHeight) + (i + 1);
					var index3 = ((j + 1) * TerrainHeight) + i;

					// Get three vertices from the face.
					var vertex1 = new[] { HeightMap[index1].x, HeightMap[index1].y, HeightMap[index1].z };
					var vertex2 = new[] { HeightMap[index2].x, HeightMap[index2].y, HeightMap[index2].z };
					var vertex3 = new[] { HeightMap[index3].x, HeightMap[index3].y, HeightMap[index3].z };

					// Calculate the two vectors for this face.
					var vector1 = new[] 
						{ 
							vertex1[0] - vertex3[0], 
							vertex1[1] - vertex3[1],
							vertex1[2] - vertex3[2]
						};
					var vector2 = new[] 
						{ 
							vertex3[0] - vertex2[0], 
							vertex3[1] - vertex2[1],
							vertex3[2] - vertex2[2]
						};

					var index = (j * (TerrainHeight - 1)) + i;

					// Calculate the cross product of those two vectors to get the un-normalized value for this face normal.
					normals[index] = new Vector3()
					{
						X = (vector1[1] * vector2[2]) - (vector1[2] * vector2[1]),
						Y = (vector1[2] * vector2[0]) - (vector1[0] * vector2[2]),
						Z = (vector1[0] * vector2[1]) - (vector1[1] * vector2[0])
					};
				}
			}

			// Now go through all the vertices and take an average of each face normal
			// that the vertex touches to get the averaged normal fot that vertex.
			for (var j = 0; j < (TerrainHeight - 1); j++)
			{
				for (var i = 0; i < (TerrainWidth - 1); i++)
				{
					int index;
					// Initialize the sum
					var sum = new[] { 0f, 0f, 0f };

					// Initialize the count
					var count = 0;

					// Bottom left face
					if (((i - 1) >= 0) && ((j - 1) >= 0))
					{
						index = ((j - 1) * (TerrainHeight - 1)) + (i - 1);
						sum[0] += normals[index].X;
						sum[1] += normals[index].Y;
						sum[2] += normals[index].Z;
						count++;
					}

					// Bottom right face
					if ((i < (TerrainWidth - 1)) && ((j - 1) >= 0))
					{
						index = ((j - 1) * (TerrainHeight - 1)) + i;
						sum[0] += normals[index].X;
						sum[1] += normals[index].Y;
						sum[2] += normals[index].Z;
						count++;
					}

					// Upper left face
					if (((i - 1) >= 0) && (j < (TerrainHeight - 1)))
					{
						index = (j * (TerrainHeight - 1)) + i;
						sum[0] += normals[index].X;
						sum[1] += normals[index].Y;
						sum[2] += normals[index].Z;
						count++;
					}

					// Upper right face
					if ((i < (TerrainWidth - 1)) && (j < (TerrainHeight - 1)))
					{
						index = (j * (TerrainHeight - 1)) + i;
						sum[0] += normals[index].X;
						sum[1] += normals[index].Y;
						sum[2] += normals[index].Z;
						count++;
					}

					// Take the average of the faces touching this vertex.
					sum[0] /= count;
					sum[1] /= count;
					sum[2] /= count;

					// Calculate the length of this normal.
					var length = (float)Math.Sqrt(sum[0] * sum[0] + sum[1] * sum[1] + sum[2] * sum[2]);

					// Get the index to the vertex location in the height map array.
					index = (j * TerrainHeight) + i;

					// Normalize the final share normal fot this vertex and store it in the height map array.
					var heightMap = HeightMap[index];
					heightMap.nx = sum[0] / length;
					heightMap.ny = sum[1] / length;
					heightMap.nz = sum[2] / length;
					HeightMap[index] = heightMap;
				}
			}

			return true;
		}

		private void CalculateTextureCoordinates()
		{
			// Calculate how much to increment the texture coordinates by.
			var incrementValue = (float)TextureRepeat / TerrainWidth;

			// Calculate how many times to repeat the texture.
			var incrementCount = TerrainWidth / TextureRepeat;

			// Initialize the tu and tv coordinate values.
			var tuCoordinate = 0f;
			var tvCoordinate = 1f;

			// Initialize the tu and tv coordinate indexes.
			var tuCount = 0;
			var tvCount = 0;

			// Loop through the entire height map and calculate the tu and tv coordinates for each vertex.
			for (var j = 0; j < TerrainHeight; j++)
			{
				for (var i = 0; i < TerrainWidth; i++)
				{
					// Store the texture coordinate in the height map.
					var heightMap = HeightMap[(TerrainHeight * j) + i];
					heightMap.tu = tuCoordinate;
					heightMap.tv = tvCoordinate;
					HeightMap[(TerrainHeight * j) + i] = heightMap;

					// Increment the tu texture coordinate by the increment value and increment the index by one.
					tuCoordinate += incrementValue;
					tuCount++;

					// Check if at the far right end of the texture and if so then start at the beginning again.
					if (tuCount == incrementCount)
					{
						tuCoordinate = 0.0f;
						tuCount = 0;
					}
				}

				// Increment the tv texture coordinate by the increment value and increment the index by one.
				tvCoordinate -= incrementValue;
				tvCount++;

				// Check if at the top of the texture and if so then start at the bottom again.
				if (tvCount == incrementCount)
				{
					tvCoordinate = 1.0f;
					tvCount = 0;
				}
			}
		}

		protected void FillArrays()
		{
			// Create the vertex array.
			Vertices = new HeightMapTerrainShader.Vertex[VertexCount];
			// Create the index array.
			Indices = new int[IndexCount];
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
					Vertices[index] = HeightMap[indexUpperLeft];
					// Modify the texture coordinates to cover the top edge.
					if (Vertices[index].texture.Y == 1.0f) Vertices[index].texture.Y = 0.0f;
					Indices[index] = index++;

					// Upper right
					Vertices[index] = HeightMap[indexUpperRight];
					// Modify the texture coordinates to cover the top and right edges.
					if (Vertices[index].texture.X == 0.0f) Vertices[index].texture.X = 1.0f;
					if (Vertices[index].texture.Y == 1.0f) Vertices[index].texture.Y = 0.0f;
					Indices[index] = index++;

					// Bottom Left
					Vertices[index] = HeightMap[indexBottomLeft];
					Indices[index] = index++;
					#endregion

					#region Second Triangle
					// Bottom Left
					Vertices[index] = HeightMap[indexBottomLeft];
					Indices[index] = index++;

					// Upper right
					Vertices[index] = HeightMap[indexUpperRight];
					// Modify the texture coordinates to cover the top and right edges.
					if (Vertices[index].texture.X == 0.0f) Vertices[index].texture.X = 1.0f;
					if (Vertices[index].texture.Y == 1.0f) Vertices[index].texture.Y = 0.0f;
					Indices[index] = index++;

					// Bottom right
					Vertices[index] = HeightMap[indexBottomRight];
					// Modify the texture coordinates to cover the right edge.
					if (Vertices[index].texture.X == 0.0f) Vertices[index].texture.X = 1.0f;
					Indices[index] = index++;
					#endregion
				}
			}
		}

		internal void CopyVertexArray(List<HeightMapTerrainShader.Vertex> VertexList)
		{
			VertexList.AddRange(Vertices);
		}
		#endregion
	}
}
