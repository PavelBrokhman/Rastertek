using System;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using GraphicsClass = Tutorial11.Graphics.Graphics;
using InputClass = Tutorial11.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using System.Runtime.InteropServices;
using Tutorial11.System;
using System.IO;
using System.Collections.Generic;

namespace Tutorial11.Graphics
{
	public class Bitmap : ICloneable
	{
		#region Structures
		[StructLayout(LayoutKind.Sequential)]
		public struct ModelFormat
		{
			public float x, y, z;
			public float tu, tv;
			public float nx, ny, nz;
		}
		#endregion

		#region Variables / Properties
		Buffer VertexBuffer { get; set; }
		Buffer IndexBuffer { get; set; }
		int VertexCount { get; set; }
		public int IndexCount { get; private set; }

		public Texture Texture { get; private set; }

		public ModelFormat[] ModelObject { get; private set; }

		public int ScreenWidth { get; private set; }
		public int ScreenHeight { get; private set; }

		public int BitmapWidth { get; private set; }
		public int BitmapHeight { get; private set; }

		public int PreviousPosX { get; private set; }
		public int PreviousPosY { get; private set; }
		#endregion

		#region Constructors
		public Bitmap()
		{
		}
		#endregion

		#region Methods
		public bool Initialize(Device device, int screeenWidth, int screenHeight, string textureFileName, int bitmapWidth, int bitmapHeight)
		{
			// Store the screen size.
			ScreenWidth = screeenWidth;
			ScreenHeight = screenHeight;

			// Store the size in pixels that this bitmap should be rendered at.
			BitmapWidth = bitmapWidth;
			BitmapHeight = bitmapHeight;

			// Initialize the previous rendering position to negative one.
			PreviousPosX = -1;
			PreviousPosY = -1;

			// Initialize the vertex and index buffer.
			if (!InitializeBuffers(device))
				return false;

			// Load the texture for this model.
			if (!LoadTexture(device, textureFileName))
				return false;

			return true;
		}

		public void Shutdown()
		{
			// Release the model texture.
			ReleaseTexture();

			// Release the vertex and index buffers.
			ShutdownBuffers();
		}

		public bool Render(DeviceContext deviceContext, int positionX, int positionY)
		{
			// Re-build the dynamic vertex buffer for rendering to possibly a different location on the screen.
			if (!UpdateBuffers(deviceContext, positionX, positionY))
				return false;

			// Put the vertex and index buffers on the graphics pipeline to prepare for drawings.
			RenderBuffers(deviceContext);

			return true;
		}

		private bool InitializeBuffers(Device device)
		{
			try
			{
				// Set the number of the vertices and indices in the vertex and index array, accordingly.
				VertexCount = 6;
				IndexCount = 6;

				// Create the vertex array.
				var vertices = new TextureShader.Vertex[VertexCount];

				// Initialize vertex array to zeroes at first.
				foreach (var vertex in vertices)
					vertex.SetToZero();

				// Create the index array.
				var indices = new int[IndexCount];

				// Load the index array with data.
				for (var i = 0; i < IndexCount; i++)
					indices[i] = i;

				// Set up the description of the static vertex buffer.
				var vertexBuffer = new BufferDescription()
				{
					Usage = ResourceUsage.Dynamic,
					SizeInBytes = Utilities.SizeOf<TextureShader.Vertex>() * VertexCount,
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					StructureByteStride = 0
				};

				// Create the vertex buffer.
				VertexBuffer = Buffer.Create(device, vertices, vertexBuffer);

				// Create the index buffer.
				IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indices);

				return true;
			}
			catch
			{
				return false;
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

		private bool UpdateBuffers(DeviceContext deviceContext, int positionX, int positionY)
		{
			// If the position we rendering this bitmap to has not changed then don't update the vertex buffer since it.
			if (positionX == PreviousPosX && positionY == PreviousPosY)
				return true;

			// If it has changed then update the position it is being rendered to.
			PreviousPosX = positionX;
			PreviousPosY = positionY;

			// Calculate the screen coordinates of the left side of the bitmap.
			var left = (-(ScreenWidth >> 2)) + (float)positionX;
			// Calculate the screen coordinates of the right side of the bitmap.
			var right = left + BitmapWidth;
			// Calculate the screen coordinates of the top of the bitmap.
			var top = (ScreenHeight >> 2) - (float)positionY;
			// Calculate the screen coordinates of the bottom of the bitmap.
			var bottom = top - BitmapHeight;

			// Create and load the vertex array.
			var vertices = new[] 
			{
				new TextureShader.Vertex()
				{
					position = new Vector3(left, top, 0),
					texture = new Vector2(0, 0)
				},
				new TextureShader.Vertex()
				{
					position = new Vector3(right, bottom, 0),
					texture = new Vector2(1, 1)
				},
				new TextureShader.Vertex()
				{
					position = new Vector3(left, bottom, 0),
					texture = new Vector2(0, 1)
				},
				new TextureShader.Vertex()
				{
					position = new Vector3(left, top, 0),
					texture = new Vector2(0, 0)
				},
				new TextureShader.Vertex()
				{
					position = new Vector3(right, top, 0),
					texture = new Vector2(1, 0)
				},
				new TextureShader.Vertex()
				{
					position = new Vector3(right, bottom, 0),
					texture = new Vector2(1, 1)
				}
			};

			DataStream mappedResource;

			#region Vertex Buffer
			// Lock the vertex buffer so it can be written to.
			deviceContext.MapSubresource(VertexBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out mappedResource);

			// Copy the data into the vertex buffer.
			mappedResource.WriteRange<TextureShader.Vertex>(vertices);

			// Unlock the vertex buffer.
			deviceContext.UnmapSubresource(VertexBuffer, 0);
			#endregion

			return true;
		}

		private void RenderBuffers(DeviceContext deviceContext)
		{
			// Set vertex buffer stride and offset.
			var stride = Utilities.SizeOf<TextureShader.Vertex>();
			var offset = 0;

			// Set the vertex buffer to active in the input assembler so it can be rendered.
			deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, stride, offset));
			// Set the index buffer to active in the input assembler so it can be rendered.
			deviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
			// Set the type of the primitive that should be rendered from this vertex buffer, in this case triangles.
			deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
		}

		private bool LoadTexture(Device device, string textureFileName)
		{
			textureFileName = SystemConfiguration.DataFilePath + textureFileName;

			// Create the texture object.
			Texture = new Texture();

			// Initialize the texture object.
			Texture.Initialize(device, textureFileName);

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
		#endregion

		#region Override Methods
		public object Clone()
		{
			return MemberwiseClone();
		}
		#endregion
	}
}