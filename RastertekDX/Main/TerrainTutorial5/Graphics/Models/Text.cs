using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Graphics.Data;
using Engine.Graphics.Shaders;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace Engine.Graphics.Models
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

		Sentence[] sentences = new Sentence[11];
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

			// Initialize the sentences.
			if (!InitializeSentence(out sentences[0], 150, device))
				return false;
			if (!InitializeSentence(out sentences[1], 32, device))
				return false;
			if (!InitializeSentence(out sentences[2], 16, device))
				return false;
			if (!InitializeSentence(out sentences[3], 16, device))
				return false;
			if (!InitializeSentence(out sentences[4], 16, device))
				return false;
			if (!InitializeSentence(out sentences[5], 16, device))
				return false;
			if (!InitializeSentence(out sentences[6], 16, device))
				return false;
			if (!InitializeSentence(out sentences[7], 16, device))
				return false;
			if (!InitializeSentence(out sentences[8], 16, device))
				return false;
			if (!InitializeSentence(out sentences[9], 16, device))
				return false;
			if (!InitializeSentence(out sentences[10], 32, device))
				return false;

			return true;
		}

		public void Shutdown()
		{
			// Release the sentences
			ReleaseSentences(sentences);

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
			var allSentencesFailed = true;
			foreach (var sentence in sentences)
			{
				var result = RenderSentence(deviceContext, sentence, worldMatrix, orthoMatrix);
				allSentencesFailed &= !result;
			}

			//return !allSentencesFailed;
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
			for (var i = 0; i < sentence.IndexCount; i++)
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

		private bool UpdateSentence(ref Sentence sentence, string text, int positionX, int positionY, float red, float green, float blue, DeviceContext deviceContext)
		{
			// Store the color of the sentence.
			sentence.red = red;
			sentence.green = green;
			sentence.blue = blue;

			// Get the number of the letter in the sentence.
			var numLetters = sentence.MaxLength;

			// Adjust text to show on the screen.
			for (var i = 0; i < sentence.MaxLength - 1; i++)
				text += " ";
			text = text.Substring(0, sentence.MaxLength);

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

		private void ReleaseSentences(Sentence[] sentences)
		{
			foreach (var sentence in sentences)
				ReleaseSentence(sentence);
		}

		private void ReleaseSentence(Sentence sentence)
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

		#region Set Sentences
		public bool SetMousePosition(int mouseX, int mouseY, DeviceContext deviceContext)
		{
			string mouseString;

			// Setup the mouseX string.
			mouseString = "Mouse X: " + mouseX.ToString();
			// Update the sentence vertex buffer with the new string information.
			if (!UpdateSentence(ref sentences[0], mouseString, 20, 20, 1, 1, 1, deviceContext))
				return false;

			// Setup the mouseY string.
			mouseString = "Mouse Y: " + mouseY.ToString();

			// Update the sentence vertex buffer with the new string information.
			if (!UpdateSentence(ref sentences[1], mouseString, 20, 40, 1, 1, 1, deviceContext))
				return false;

			return true;
		}

		public bool SetFps(int fps, DeviceContext deviceContext)
		{
			// Truncate the fps to below 10,000
			if (fps > 9999)
				fps = 9999;

			// Convert the fps integer to string format.
			var fpsString = fps.ToString();

			// Setup the fps string.
			fpsString = "Fps: " + fpsString;

			float red = 1, green = 1, blue = 1;

			// If fps is 60 or above set the fps color to green.
			if (fps >= 60)
			{
				red = 0;
				green = 1;
				blue = 0;
			}

			// If fps is below 60 set the fps color to yellow
			if (fps < 60)
			{
				red = 1;
				green = 1;
				blue = 0;
			}

			// If fps is below 30 set the fps to red.
			if (fps < 30)
			{
				red = 1;
				green = 0;
				blue = 0;
			}

			// Update the sentence vertex buffer with the new string information.
			if (!UpdateSentence(ref sentences[2], fpsString, 10, 70, red, green, blue, deviceContext))
				return false;

			return true;
		}

		public bool SetCpu(int cpu, DeviceContext deviceContext)
		{
			var cpuString = string.Format("Cpu: {0}%", cpu);

			// Update the sentence vertex buffer with the new string information.
			if (!UpdateSentence(ref sentences[3], cpuString, 10, 90, 0, 1, 0, deviceContext))
				return false;

			return true;
		}

		public bool SetRenderCount(int renderCount, DeviceContext deviceContext)
		{
			var renderCountString = string.Format("Render Count: {0}", renderCount);

			// Update the sentence vertex buffer with the new string information.
			if (!UpdateSentence(ref sentences[10], renderCountString, 10, 290, 0, 1, 0, deviceContext))
				return false;

			return true;
		}

		public bool SetVideoCard(string videoCardName, int videoCardMemory, DeviceContext deviceContext)
		{
			// Setup the video card name string.
			var dataString = "Video Card: " + videoCardName;

			// Update the sentence vertex buffer with the new string information.
			if (!UpdateSentence(ref sentences[0], dataString, 10, 10, 1.0f, 1.0f, 1.0f, deviceContext))
				return false;

			// Truncate the memory value to prevent buffer overflow.
			if (videoCardMemory > 9999999)
				videoCardMemory = 9999999;

			// Setup the video memory string.
			var memoryString = "Video Memory: " + videoCardMemory.ToString() + " MB";

			// Update the sentence vertex buffer with the new string information.
			if (!UpdateSentence(ref sentences[1], memoryString, 10, 30, 1.0f, 1.0f, 1.0f, deviceContext))
				return false;

			return true;
		}

		public bool SetCameraPosition(Vector3 position, DeviceContext deviceContext)
		{
			// Truncate the position if it exceeds either 9999 or -9999.
			if (position.X > 9999)
				position.X = 9999;
			if (position.Y > 9999)
				position.Y = 9999;
			if (position.Z > 9999)
				position.Z = 9999;

			if (position.X < -9999)
				position.X = -9999;
			if (position.Y < -9999)
				position.Y = -9999;
			if (position.Z < -9999)
				position.Z = -9999;

			// Setup the position strings.
			var XString = "X: " + (int)position.X;
			var YString = "Y: " + (int)position.Y;
			var ZString = "Z: " + (int)position.Z;

			// Update the sentence vertex buffer with the new string information.
			if (!UpdateSentence(ref sentences[4], XString, 10, 130, 0.0f, 1.0f, 0.0f, deviceContext))
				return false;
			if (!UpdateSentence(ref sentences[5], YString, 10, 150, 0.0f, 1.0f, 0.0f, deviceContext))
				return false;
			if (!UpdateSentence(ref sentences[6], ZString, 10, 170, 0.0f, 1.0f, 0.0f, deviceContext))
				return false;

			return true;
		}

		public bool SetCameraRotation(Vector3 rotation, DeviceContext deviceContext)
		{
			// Setup the rotation strings.
			var XString = "rX: " + (int)rotation.X;
			var YString = "rY: " + (int)rotation.Y;
			var ZString = "rZ: " + (int)rotation.Z;

			// Update the sentence vertex buffer with the new string information.
			if (!UpdateSentence(ref sentences[7], XString, 10, 210, 0.0f, 1.0f, 0.0f, deviceContext))
				return false;
			if (!UpdateSentence(ref sentences[8], YString, 10, 230, 0.0f, 1.0f, 0.0f, deviceContext))
				return false;
			if (!UpdateSentence(ref sentences[9], ZString, 10, 250, 0.0f, 1.0f, 0.0f, deviceContext))
				return false;

			return true;
		}
		#endregion
		#endregion
	}
}