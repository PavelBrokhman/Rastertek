using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tutorial12.Graphics.Data;
using Tutorial12.Graphics.Shaders;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace Tutorial12.Graphics.Models
{
	public class Text
	{
		#region Structures
		struct Sentence
		{
			public Buffer VertexBuffer;
			public Buffer IndexBuffer;
			public int VertexCount;
			public int IndexCount;
			public int MaxLength;
			public float red;
			public float green;
			public float blue;
		}
		#endregion

		#region Variables / Properties
		Font Font;
		FontShader FontShader;
		int ScreenWidth;
		int ScreenHeight;
		Matrix BaseViewMatrix;

		Sentence[] sentences = new Sentence[2];
		#endregion

		#region Methods
		public bool Initialize(Device device, DeviceContext deviceContext, IntPtr windowHandle, int screanWidth, int screenHeight, Matrix baseViewMatrix)
		{
			// Store the screen width and height.
			ScreenWidth = screanWidth;
			ScreenHeight = screenHeight;

			// Store the base view matrix.
			BaseViewMatrix = baseViewMatrix;

			// Create the font object.
			Font = new Font();

			// Initialize the font object.
			if (!Font.Initialize(device, "fontdata.txt", "font.dds"))
				return false;

			// Create the font shader object.
			FontShader = new FontShader();

			// Initialize the font shader object.
			if (!FontShader.Initialize(device, windowHandle))
				return false;

			// Initialize the first sentence.
			if (!InitializeSentence(out sentences[0], 16, device))
				return false;

			// Now update the sentence vertex buffer with the new string information.
			if (!UpdateSentece(ref sentences[0], "Hello", 100, 100, 1, 1, 1, deviceContext))
				return false;

			// Initialize the second sentence.
			if (!InitializeSentence(out sentences[1], 16, device))
				return false;

			// Now update the sentence vertex buffer with the new string information.
			if (!UpdateSentece(ref sentences[1], "Goodbye", 100, 200, 1, 1, 0, deviceContext))
				return false;

			return true;
		}

		public void Shutdown()
		{
			// Release the first sentence.
			ReleaseSentences(sentences[0]);

			// Release the second sentence.
			ReleaseSentences(sentences[1]);

			// Release the font shader object.
			if (FontShader != null)
			{
				FontShader.Shuddown();
				FontShader = null;
			}

			// Release the font object.
			if (Font != null)
			{
				Font.Shutdown();
				Font = null;
			}
		}

		public bool Render(DeviceContext deviceContext, Matrix worldMatrix, Matrix orthoMatrix)
		{
			// Draw the first sentence.
			if (!RenderSentence(deviceContext, sentences[0], worldMatrix, orthoMatrix))
				return false;

			// Draw the second sentence.
			if (!RenderSentence(deviceContext, sentences[1], worldMatrix, orthoMatrix))
				return false;

			return true;
		}

		private bool InitializeSentence(out Sentence sentence, int maxLength, Device device)
		{
			// Create a new sentence object.
			sentence = new Sentence();

			// Initialize the sentence buffers to null;
			sentence.VertexBuffer = null;
			sentence.IndexBuffer = null;

			// Set the maximum length of the sentence.
			sentence.MaxLength = maxLength;

			// Set the number of vertices in vertex array.
			sentence.VertexCount = 6 * maxLength;

			// Set the number of vertices in the vertex array.
			sentence.IndexCount = sentence.VertexCount;

			// Create the vertex array.
			var vertices = new TextureShader.Vertex[sentence.VertexCount];

			// Create the index array.
			var indices = new int[sentence.IndexCount];

			// Initialize vertex array to zeros at first.
			foreach (var vertex in vertices)
				vertex.SetToZero();

			// Initialize the index array.
			for (var i=0; i < sentence.IndexCount; i++)
				indices[i] = i;

			// Set up the description of the dynamic vertex buffer.
			var vertexBufferDesc = new BufferDescription()
			{
				Usage = ResourceUsage.Dynamic,
				SizeInBytes = Utilities.SizeOf<TextureShader.Vertex>() * sentence.VertexCount,
				BindFlags = BindFlags.VertexBuffer,
				CpuAccessFlags = CpuAccessFlags.Write,
				OptionFlags = ResourceOptionFlags.None,
				StructureByteStride = 0
			};

			// Create the vertex buffer.
			sentence.VertexBuffer = Buffer.Create(device, vertices, vertexBufferDesc);

			// Create the index buffer.
			sentence.IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indices);

			return true;
		}

		private bool UpdateSentece(ref Sentence sentence, string text, int positionX, int positionY, float red, float green, float blue, DeviceContext deviceContext)
		{
			// Store the color of the sentence.
			sentence.red = red;
			sentence.green = green;
			sentence.blue = blue;

			// Get the number of the letter in the sentence.
			var numLetters = text.Length;

			// Check for possible buffer overflow.
			if (numLetters > sentence.MaxLength)
				return false;

			// Calculate the X and Y pixel position on screen to start drawing to.
			var drawX = -(ScreenWidth >> 1) + positionX;
			var drawY = (ScreenHeight >> 1) - positionY;

			// Use the font class to build the vertex array from the sentence text and sentence draw location.
			List<TextureShader.Vertex> vertices;
			Font.BuildVertexArray(out vertices, text, drawX, drawY);

			DataStream mappedResource;

			#region Vertex Buffer
			// Lock the vertex buffer so it can be written to.
			deviceContext.MapSubresource(sentence.VertexBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out mappedResource);

			// Copy the data into the vertex buffer.
			mappedResource.WriteRange<TextureShader.Vertex>(vertices.ToArray());

			// Unlock the vertex buffer.
			deviceContext.UnmapSubresource(sentence.VertexBuffer, 0);
			#endregion

			return true;
		}

		private void ReleaseSentences(Sentence sentence)
		{
			// Release the sentence vertex buffer.
			if (sentence.VertexBuffer != null)
			{
				sentence.VertexBuffer.Dispose();
				sentence.VertexBuffer = null;
			}

			// Release the sentence index buffer.
			if (sentence.IndexBuffer != null)
			{
				sentence.IndexBuffer.Dispose();
				sentence.IndexBuffer = null;
			}
		}

		private bool RenderSentence(DeviceContext deviceContext, Sentence sentence, Matrix worldMatrix, Matrix orthoMatrix)
		{
			// Set vertex buffer stride and offset.
			var stride = Utilities.SizeOf<TextureShader.Vertex>();
			var offset = 0;

			// Set the vertex buffer to active in the input assembler so it can be rendered.
			deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(sentence.VertexBuffer, stride, offset));
			// Set the index buffer to active in the input assembler so it can be rendered.
			deviceContext.InputAssembler.SetIndexBuffer(sentence.IndexBuffer, Format.R32_UInt, 0);
			// Set the type of the primitive that should be rendered from this vertex buffer, in this case triangles.
			deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
			// Create a pixel color vector with the input sentence color.
			var pixelColor = new Vector4(sentence.red, sentence.green, sentence.blue, 1);
			// Render the text using the font shader.
			if (!FontShader.Render(deviceContext, sentence.IndexCount, worldMatrix, BaseViewMatrix, orthoMatrix, Font.Texture.TextureResource, pixelColor))
				return false;

			return true;
		}
		#endregion
	}
}
