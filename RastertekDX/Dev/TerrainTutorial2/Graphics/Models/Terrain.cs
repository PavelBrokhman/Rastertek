using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using Engine.Graphics.Shaders;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;

namespace Engine.Graphics.Models
{
	public class Terrain : ICloneable
	{
		#region Structures
		#endregion

		#region Variables / Properties
		Buffer VertexBuffer { get; set; }
		Buffer IndexBuffer { get; set; }
		protected int VertexCount { get; private set; }
		public int IndexCount { get; private set; }

		protected int TerrainWidth { get; set; }
		protected int TerrainHeight { get; set; }
		protected int NumberOfVerticesPerQuad { get; set; }
		#endregion

		#region Constructors
		public Terrain()
		{
		}
		#endregion

		#region Methods
		public bool Initialize(Device device)
		{
			// Manually set the width and height of the terrain.
			TerrainWidth = 100;
			TerrainHeight = 100;
			NumberOfVerticesPerQuad = 8;

			// Initialize the vertex and index buffer.
			if (!InitializeBuffers(device))
				return false;

			return true;
		}

		public virtual void Shutdown()
		{
			// Release the vertex and index buffers.
			ShutdownBuffers();
		}

		public void Render(DeviceContext deviceContext)
		{
			// Put the vertex and index buffers on the graphics pipeline to prepare for drawings.
			RenderBuffers(deviceContext);
		}

		protected bool InitializeBuffers(Device device)
		{
			try
			{
				// Calculate the number of the vertices in the terrain mesh.
				VertexCount = (TerrainWidth - 1) * (TerrainHeight - 1) * NumberOfVerticesPerQuad;
				// Set the index count to the same as the vertex count.
				IndexCount = VertexCount;

				ColorShader.Vertex[] vertices;
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

		protected virtual void FillArrays(out ColorShader.Vertex[] vertices, out int[] indices)
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
					// LINE 1
					// Upper left
					vertices[index] = new ColorShader.Vertex()
					{
						position = new Vector3(i, 0, j + 1),
						color = new Vector4(1, 1, 1, 1)
					};
					indices[index] = index++;

					// Upper right
					vertices[index] = new ColorShader.Vertex()
					{
						position = new Vector3(i + 1, 0, j + 1),
						color = new Vector4(1, 1, 1, 1)
					};
					indices[index] = index++;

					// LINE 2
					// Upper right
					vertices[index] = new ColorShader.Vertex()
					{
						position = new Vector3(i + 1, 0, j + 1),
						color = new Vector4(1, 1, 1, 1)
					};
					indices[index] = index++;

					// Bottom right
					vertices[index] = new ColorShader.Vertex()
					{
						position = new Vector3(i + 1, 0, j),
						color = new Vector4(1, 1, 1, 1)
					};
					indices[index] = index++;

					// LINE 3
					// Bottom right
					vertices[index] = new ColorShader.Vertex()
					{
						position = new Vector3(i + 1, 0, j),
						color = new Vector4(1, 1, 1, 1)
					};
					indices[index] = index++;

					// Bottom left
					vertices[index] = new ColorShader.Vertex()
					{
						position = new Vector3(i, 0, j),
						color = new Vector4(1, 1, 1, 1)
					};
					indices[index] = index++;

					// LINE 4
					// Bottom left
					vertices[index] = new ColorShader.Vertex()
					{
						position = new Vector3(i, 0, j),
						color = new Vector4(1, 1, 1, 1)
					};
					indices[index] = index++;

					// Upper left
					vertices[index] = new ColorShader.Vertex()
					{
						position = new Vector3(i, 0, j + 1),
						color = new Vector4(1, 1, 1, 1)
					};
					indices[index] = index++;
				}
			}
		}

		private void ShutdownBuffers()
		{
			// Return the index buffer.
			if (IndexBuffer != null)
			{
				IndexBuffer.Dispose();
				IndexBuffer = null;
			}

			// Release the vertex buffer.
			if (VertexBuffer != null)
			{
				VertexBuffer.Dispose();
				VertexBuffer = null;
			}
		}

		private void RenderBuffers(DeviceContext deviceContext)
		{
			// Set the vertex buffer to active in the input assembler so it can be rendered.
			deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<ColorShader.Vertex>(), 0));
			// Set the index buffer to active in the input assembler so it can be rendered.
			deviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
			// Set the type of the primitive that should be rendered from this vertex buffer, in this case triangles.
			deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
		}
		#endregion

		#region Override Methods
		public object Clone()
		{
			return MemberwiseClone();
		}
		#endregion
	}
}
