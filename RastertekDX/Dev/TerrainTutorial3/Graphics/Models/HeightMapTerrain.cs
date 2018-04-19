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

namespace Engine.Graphics.Models
{
	public class HeightMapTerrain : Terrain
	{
		#region Structures
		public struct HeightMapType
		{
			public float x, y, z;
			public float nx, ny, nz;
			public static implicit operator HeightMapTerrainShader.Vertex(HeightMapType hmapVector)
			{
				return new HeightMapTerrainShader.Vertex()
					{
						normal = new Vector3(hmapVector.nx, hmapVector.ny, hmapVector.nz),
						position = new Vector3(hmapVector.x, hmapVector.y, hmapVector.z)
					};
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

			// Calculate the normals for the terrain data.
			if (!CalculateNormals())
				return false;

			// Set the number of vertices per quad (2 triangles)
			NumberOfVerticesPerQuad = 6;

			// Set the value of the topology
			CurrentTopology = PrimitiveTopology.TriangleList;

			// Initialize the vertex and index buffer.
			if (!InitializeBuffers(device))
				return false;

			return true;
		}

		protected new bool InitializeBuffers(Device device)
		{
			try
			{
				// Calculate the number of the vertices in the terrain mesh.
				VertexCount = (TerrainWidth - 1) * (TerrainHeight - 1) * NumberOfVerticesPerQuad;
				// Set the index count to the same as the vertex count.
				IndexCount = VertexCount;

				HeightMapTerrainShader.Vertex[] vertices;
				int[] indices;
				FillArrays(out vertices, out indices);

				// Create the vertex buffer.
				VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, vertices);

				// Create the index buffer.
				IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indices);

				return true;
			}
			catch
			{
				return false;
			}
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
					if ((i < (TerrainWidth-1)) && ((j - 1) >= 0))
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

		protected new void FillArrays(out HeightMapTerrainShader.Vertex[] vertices, out int[] indices)
		{
			// Create the vertex array.
			vertices = new HeightMapTerrainShader.Vertex[VertexCount];
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
					vertices[index] = HeightMap[indexUpperLeft];
					indices[index] = index++;

					// Upper right
					vertices[index] = HeightMap[indexUpperRight];
					indices[index] = index++;

					// Bottom Left
					vertices[index] = HeightMap[indexBottomLeft];
					indices[index] = index++;
					#endregion

					#region Second Triangle
					// Bottom Left
					vertices[index] = HeightMap[indexBottomLeft];
					indices[index] = index++;

					// Upper right
					vertices[index] = HeightMap[indexUpperRight];
					indices[index] = index++;

					// Bottom right
					vertices[index] = HeightMap[indexBottomRight];
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
