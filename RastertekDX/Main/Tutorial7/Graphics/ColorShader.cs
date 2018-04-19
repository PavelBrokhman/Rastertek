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
using GraphicsClass = Tutorial7.Graphics.Graphics;
using InputClass = Tutorial7.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using System.Runtime.InteropServices;
using Tutorial7.System;

namespace Tutorial7.Graphics
{
	public class ColorShader : ICloneable
	{
		#region Structures
		[StructLayout(LayoutKind.Sequential)]
		internal struct Vertex
		{
			public static int AppendAlignedElement = 12;

			public Vector3 position;
			public Vector4 color;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct MatrixBuffer
		{
			public Matrix world;
			public Matrix view;
			public Matrix projection;
		}
		#endregion

		#region Variables / Properties
		VertexShader VertexShader { get; set; }
		PixelShader PixelShader { get; set; }
		InputLayout Layout { get; set; }
		Buffer ConstantMatrixBuffer { get; set; }
		#endregion

		#region Constructors
		public ColorShader() { }
		#endregion

		#region Methods
		public bool Initialize(Device device, IntPtr windowHandler)
		{
			// Initialize the vertex and pixel shaders.
			return InitializeShader(device, windowHandler, "color.vs", "color.ps");
		}

		public void Shuddown()
		{
			// Shutdown the vertex and pixel shaders as well as the related objects.
			ShuddownShader();
		}

		public bool Render(DeviceContext deviceContext, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
		{
			// Set the shader parameters that it will use for rendering.
			if (!SetShaderParameters(deviceContext, worldMatrix, viewMatrix, projectionMatrix))
				return false;

			// Now render the prepared buffers with the shader.
			RenderShader(deviceContext, indexCount);

			return true;
		}

		private void RenderShader(DeviceContext deviceContext, int indexCount)
		{
			// Set the vertex input layout.
			deviceContext.InputAssembler.InputLayout = Layout;

			// Set the vertex and pixel shaders that will be used to render this triangle.
			deviceContext.VertexShader.Set(VertexShader);
			deviceContext.PixelShader.Set(PixelShader);

			// Render the triangle.
			deviceContext.DrawIndexed(indexCount, 0, 0);
		}

		private bool SetShaderParameters(DeviceContext deviceContext, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
		{
			try
			{
				// Transpose the matrices to prepare them for shader.
				worldMatrix.Transpose();
				viewMatrix.Transpose();
				projectionMatrix.Transpose();

				// Lock the constant buffer so it can be written to.
				DataStream mappedResource;
				deviceContext.MapSubresource(ConstantMatrixBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out mappedResource);

				// Copy the matrices into the constant buffer.
				var matrixBuffer = new MatrixBuffer()
				{
					world = worldMatrix,
					view = viewMatrix,
					projection = projectionMatrix
				};
				mappedResource.Write(matrixBuffer);

				// Unlock the constant buffer.
				deviceContext.UnmapSubresource(ConstantMatrixBuffer, 0);

				// Set the position of the constant buffer in the vertex shader.
				var bufferNumber = 0;

				// Finally set the constant buffer in the vertex shader with the updated values.
				deviceContext.VertexShader.SetConstantBuffer(bufferNumber, ConstantMatrixBuffer);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private void ShuddownShader()
		{
			// Release the matrix constant buffer.
			if (ConstantMatrixBuffer != null)
			{
				ConstantMatrixBuffer.Dispose();
				ConstantMatrixBuffer = null;
			}

			// Release the layout.
			if (Layout != null)
			{
				Layout.Dispose();
				Layout = null;
			}

			// Release the pixel shader.
			if (PixelShader != null)
			{
				PixelShader.Dispose();
				PixelShader = null;
			}

			// Release the vertex shader.
			if (VertexShader != null)
			{
				VertexShader.Dispose();
				VertexShader = null;
			}
		}

		private bool InitializeShader(Device device, IntPtr windowHandler, string vsFileName, string psFileName)
		{
			try
			{
				// Setup full pathes
				vsFileName = SystemConfiguration.ShadersFilePath + vsFileName;
				psFileName = SystemConfiguration.ShadersFilePath + psFileName;

				// Compile the vertex shader code.
				var vertexShaderByteCode = ShaderBytecode.CompileFromFile(vsFileName, "ColorVertexShader", "vs_4_0", ShaderFlags.None, EffectFlags.None);
				// Compile the pixel shader code.
				var pixelShaderByteCode = ShaderBytecode.CompileFromFile(psFileName, "ColorPixelShader", "ps_4_0", ShaderFlags.None, EffectFlags.None);

				// Create the vertex shader from the buffer.
				VertexShader = new VertexShader(device, vertexShaderByteCode);
				// Create the pixel shader from the buffer.
				PixelShader = new PixelShader(device, pixelShaderByteCode);

				// Now setup the layout of the data that goes into the shader.
				// This setup needs to match the VertexType structure in the Model and in the shader.
				var inputElements = new InputElement[]
				{
					new InputElement()
					{
						SemanticName = "POSITION",
						SemanticIndex = 0,
						Format = Format.R32G32B32A32_Float,
						Slot = 0,
						AlignedByteOffset = 0,
						Classification = InputClassification.PerVertexData,
						InstanceDataStepRate = 0
					},
					new InputElement()
					{
						SemanticName = "COLOR",
						SemanticIndex = 0,
						Format = Format.R32G32B32A32_Float,
						Slot = 0,
						AlignedByteOffset = ColorShader.Vertex.AppendAlignedElement,
						Classification = InputClassification.PerVertexData,
						InstanceDataStepRate = 0
					}
				};

				// Create the vertex input the layout.
				Layout = new InputLayout(device, ShaderSignature.GetInputSignature(vertexShaderByteCode), inputElements);

				// Release the vertex and pixel shader buffers, since they are no longer needed.
				vertexShaderByteCode.Dispose();
				pixelShaderByteCode.Dispose();

				// Setup the description of the dynamic matrix constant buffer that is in the vertex shader.
				var matrixBufferDesc = new BufferDescription()
				{
					Usage = ResourceUsage.Dynamic,
					SizeInBytes = Utilities.SizeOf<Matrix>(),
					BindFlags = BindFlags.ConstantBuffer,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					StructureByteStride = 0
				};

				// Create the constant buffer pointer so we can access the vertex shader constant buffer from within this class.
				ConstantMatrixBuffer = new Buffer(device, matrixBufferDesc);

				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error initializing shader. Error is " + ex.Message);
				return false;
			};
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