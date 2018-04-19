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
using GraphicsClass = Tutorial7.Graphics.Graphics;
using InputClass = Tutorial7.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using System.Runtime.InteropServices;
using Tutorial7.System;
using System.IO;
using System.Collections.Generic;

namespace Tutorial7.Graphics
{
	public class Model : ICloneable
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
		#endregion

		#region Constructors
		public Model()
		{
		}
		#endregion

		#region Methods
		public bool Initialize(Device device, string modelFormatFilename, string textureFileName)
		{
			// Load in the model data.
			if (!LoadModel(modelFormatFilename))
				return false;

			// Initialize the vertex and index buffer.
			if (!InitializeBuffers(device))
				return false;

			// Load the texture for this model.
			if (!LoadTexture(device, textureFileName))
				return false;

			return true;
		}

		private bool LoadModel(string modelFormatFilename)
		{
			modelFormatFilename = SystemConfiguration.ModelFilePath + modelFormatFilename;
			List<string> lines = null;

			try
			{
				lines = File.ReadLines(modelFormatFilename).ToList();

				var vertexCountString = lines[0].Split(new char[] { ':' })[1].Trim();
				VertexCount = int.Parse(vertexCountString);
				IndexCount = VertexCount;
				ModelObject = new ModelFormat[VertexCount];

				for (var i = 4; i < lines.Count && i < 4 + VertexCount; i++)
				{
					var modelArray = lines[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

					ModelObject[i - 4] = new ModelFormat()
					{
						x = float.Parse(modelArray[0]),
						y = float.Parse(modelArray[1]),
						z = float.Parse(modelArray[2]),
						tu = float.Parse(modelArray[3]),
						tv = float.Parse(modelArray[4]),
						nx = float.Parse(modelArray[5]),
						ny = float.Parse(modelArray[6]),
						nz = float.Parse(modelArray[7])
					};
				}

				return true;
			}
			catch (Exception)
			{
				return false;
			}
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
		public void Shutdown()
		{
			// Release the model texture.
			ReleaseTexture();

			// Release the vertex and index buffers.
			ShutdownBuffers();

			// Release the model data.
			ReleaseModel();
		}

		private void ReleaseModel()
		{
			ModelObject = null;
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
				// Create the vertex array.
				var vertices = new LightShader.Vertex[VertexCount];
				// Create the index array.
				var indices = new int[IndexCount];

				for (var i = 0; i < VertexCount; i++)
				{
					vertices[i] = new LightShader.Vertex()
					{
						position = new Vector3(ModelObject[i].x, ModelObject[i].y, ModelObject[i].z),
						texture = new Vector2(ModelObject[i].tu, ModelObject[i].tv),
						normal = new Vector3(ModelObject[i].nx, ModelObject[i].ny, ModelObject[i].nz)
					};

					indices[i] = i;
				}

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
			deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<LightShader.Vertex>(), 0));
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