using System;
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
using GraphicsClass = Tutorial5.Graphics.Graphics;
using InputClass = Tutorial5.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using System.Runtime.InteropServices;

namespace Tutorial5.Graphics
{
	public class Model : ICloneable
	{
		#region Variables / Properties
		Buffer VertexBuffer { get; set; }
		Buffer IndexBuffer { get; set; }
		int VertexCount { get; set; }
		public int IndexCount { get; private set; }

		public Texture Texture { get; private set; }
		#endregion

		#region Constructors
		public Model()
		{
		}
		#endregion

		#region Methods
		public bool Initialize(Device device, string textureFileName)
		{
			// Initialize the vertex and index buffer that hold the geometry for the triangle.
			if (!InitializeBuffers(device))
				return false;

			if (!LoadTexture(device, textureFileName))
				return false;

			return true;
		}

		private bool LoadTexture(Device device, string textureFileName)
		{
			// Create the texture object.
			Texture = new Texture();

			// Initialize the texture object.
			Texture.Initialize(device, textureFileName);

			return true;
		}
		public void Shutdown()
		{
			// Release the model texture.
			ReleaseTexture();

			// Release the vertex and index buffers.
			ShutdownBuffers();
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
		public void Render(DeviceContext deviceContext)
		{
			// Put the vertex and index buffers on the graphics pipeline to prepare for drawings.
			RenderBuffers(deviceContext);
		}

		private bool InitializeBuffers(Device device)
		{
			try
			{
				// Set number of vertices in the vertex array.
				VertexCount = 3;
				// Set number of vertices in the index array.
				IndexCount = 3;

				// Create the vertex array and load it with data.
				var vertices = new[]
					{
						// Bottom left.
						new TextureShader.Vertex()
						{
							position = new Vector3(-1, -1, 0),
							texture = new Vector2(0, 1)
						},
						// Top middle.
						new TextureShader.Vertex()
						{
							position = new Vector3(0, 1, 0),
							texture = new Vector2(.5f, 0)
						},
						// Bottom right.
						new TextureShader.Vertex()
						{
							position = new Vector3(1, -1, 0),
							texture = new Vector2(1, 1)
						}
					};

				var indices = new[] 
				{
					0, // Bottom left.
					1, // Top middle.
					2  // Bottom right.
				};

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
			deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<TextureShader.Vertex>(), 0));
			// Set the index buffer to active in the input assembler so it can be rendered.
			deviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
			// Set the type of the primitive that should be rendered from this vertex buffer, in this case triangles.
			deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
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