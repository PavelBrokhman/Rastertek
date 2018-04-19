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
using GraphicsClass = Engine.Graphics.Graphics;
using InputClass = Engine.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using System.Runtime.InteropServices;
using Engine.System;

namespace Engine.Graphics.Shaders
{
	public class ClipPlaneShader : ICloneable
	{
		#region Structures
		[StructLayout(LayoutKind.Sequential)]
		public struct Vertex
		{
			public Vector3 position;
			public Vector2 texture;

			public void SetToZero()
			{
				position = Vector3.Zero;
				texture = Vector2.Zero;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct MatrixBuffer
		{
			public Matrix world;
			public Matrix view;
			public Matrix projection;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct ClipPlaneBuffer
		{
			public Vector4 clipPlane;
		}
		#endregion

		#region Variables / Properties
		VertexShader VertexShader { get; set; }
		PixelShader PixelShader { get; set; }
		InputLayout Layout { get; set; }
		Buffer ConstantMatrixBuffer { get; set; }
		Buffer ConstantClipPlaneBuffer { get; set; }
		SamplerState SampleState { get; set; }
		#endregion

		#region Constructors
		public ClipPlaneShader() { }
		#endregion

		#region Methods
		public bool Initialize(Device device, IntPtr windowHandler)
		{
			// Initialize the vertex and pixel shaders.
			return InitializeShader(device, windowHandler, "clipplane.vs", "clipplane.ps");
		}

		public void Shuddown()
		{
			// Shutdown the vertex and pixel shaders as well as the related objects.
			ShuddownShader();
		}

		public bool Render(DeviceContext deviceContext, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView[] textures, Vector4 clipPlane)
		{
			// Set the shader parameters that it will use for rendering.
			if (!SetShaderParameters(deviceContext, worldMatrix, viewMatrix, projectionMatrix, textures, clipPlane))
				return false;

			// Now render the prepared buffers with the shader.
			RenderShader(deviceContext, indexCount);

			return true;
		}

		private bool InitializeShader(Device device, IntPtr windowHandler, string vsFileName, string psFileName)
		{
			try
			{
				// Setup full pathes
				vsFileName = SystemConfiguration.ShadersFilePath + vsFileName;
				psFileName = SystemConfiguration.ShadersFilePath + psFileName;

				#region Initilize Shaders
				// Compile the vertex shader code.
				var vertexShaderByteCode = ShaderBytecode.CompileFromFile(vsFileName, "ClipPlaneVertexShader", SystemConfiguration.VertexShaderProfile, ShaderFlags.None, EffectFlags.None);
				// Compile the pixel shader code.
				var pixelShaderByteCode = ShaderBytecode.CompileFromFile(psFileName, "ClipPlanePixelShader", SystemConfiguration.PixelShaderProfile, ShaderFlags.None, EffectFlags.None);

				// Create the vertex shader from the buffer.
				VertexShader = new VertexShader(device, vertexShaderByteCode);
				// Create the pixel shader from the buffer.
				PixelShader = new PixelShader(device, pixelShaderByteCode);
				#endregion

				#region Initialize Input Layouts
				// Now setup the layout of the data that goes into the shader.
				// This setup needs to match the VertexType structure in the Model and in the shader.
				var inputElements = new InputElement[]
				{
					new InputElement()
					{
						SemanticName = "POSITION",
						SemanticIndex = 0,
						Format = Format.R32G32B32_Float,
						Slot = 0,
						AlignedByteOffset = InputElement.AppendAligned,
						Classification = InputClassification.PerVertexData,
						InstanceDataStepRate = 0
					},
					new InputElement()
					{
						SemanticName = "TEXCOORD",
						SemanticIndex = 0,
						Format = Format.R32G32_Float,
						Slot = 0,
						AlignedByteOffset = InputElement.AppendAligned,
						Classification = InputClassification.PerVertexData,
						InstanceDataStepRate = 0
					}
				};

				// Create the vertex input the layout.
				Layout = new InputLayout(device, ShaderSignature.GetInputSignature(vertexShaderByteCode), inputElements);
				#endregion

				// Release the vertex and pixel shader buffers, since they are no longer needed.
				vertexShaderByteCode.Dispose();
				pixelShaderByteCode.Dispose();

				#region Initialize Matrix Buffer
				// Setup the description of the dynamic matrix constant buffer that is in the vertex shader.
				var matrixBufferDesc = new BufferDescription()
				{
					Usage = ResourceUsage.Dynamic,
					SizeInBytes = Utilities.SizeOf<MatrixBuffer>(),
					BindFlags = BindFlags.ConstantBuffer,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					StructureByteStride = 0
				};

				// Create the constant buffer pointer so we can access the vertex shader constant buffer from within this class.
				ConstantMatrixBuffer = new Buffer(device, matrixBufferDesc);
				#endregion

				#region Initialize Sampler
				// Create a texture sampler state description.
				var samplerDesc = new SamplerStateDescription()
				{
					Filter = Filter.MinMagMipLinear,
					AddressU = TextureAddressMode.Wrap,
					AddressV = TextureAddressMode.Wrap,
					AddressW = TextureAddressMode.Wrap,
					MipLodBias = 0,
					MaximumAnisotropy = 1,
					ComparisonFunction = Comparison.Always,
					BorderColor = new Color4(0, 0, 0, 0),
					MinimumLod = 0,
					MaximumLod = 0
				};

				// Create the texture sampler state.
				SampleState = new SamplerState(device, samplerDesc);
				#endregion

				#region Initialize Clip Plane Buffer
				// Setup the description of the dynamic matrix constant buffer that is in the vertex shader.
				var clipPlaneBufferDesc = new BufferDescription()
				{
					Usage = ResourceUsage.Dynamic,
					SizeInBytes = Utilities.SizeOf<ClipPlaneBuffer>(),
					BindFlags = BindFlags.ConstantBuffer,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					StructureByteStride = 0
				};

				// Create the constant buffer pointer so we can access the vertex shader constant buffer from within this class.
				ConstantClipPlaneBuffer = new Buffer(device, clipPlaneBufferDesc);
				#endregion

				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error initializing shader. Error is " + ex.Message);
				return false;
			};
		}

		private void ShuddownShader()
		{
			// Release the matrix constant buffer.
			if (ConstantClipPlaneBuffer != null)
			{
				ConstantClipPlaneBuffer.Dispose();
				ConstantClipPlaneBuffer = null;
			}

			// Release the sampler state.
			if (SampleState != null)
			{
				SampleState.Dispose();
				SampleState = null;
			}

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

		private bool SetShaderParameters(DeviceContext deviceContext, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView[] textures, Vector4 clipPlane)
		{
			try
			{
				#region Set Matrix Shader Resources
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

				// Set shader resource in the pixel shader.
				deviceContext.PixelShader.SetShaderResources(0, textures);
				#endregion

				#region Set Clip Plane Shader Resources
				// Lock the constant buffer so it can be written to.
				deviceContext.MapSubresource(ConstantClipPlaneBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out mappedResource);

				// Copy the matrices into the constant buffer.
				var clipPlaneBuffer = new ClipPlaneBuffer()
				{
					clipPlane = clipPlane
				};

				mappedResource.Write(clipPlaneBuffer);

				// Unlock the constant buffer.
				deviceContext.UnmapSubresource(ConstantClipPlaneBuffer, 0);

				// Set the position of the constant buffer in the vertex shader.
				bufferNumber = 1;

				// Finally set the constant buffer in the vertex shader with the updated values.
				deviceContext.VertexShader.SetConstantBuffer(bufferNumber, ConstantClipPlaneBuffer);
				#endregion

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private void RenderShader(DeviceContext deviceContext, int indexCount)
		{
			// Set the vertex input layout.
			deviceContext.InputAssembler.InputLayout = Layout;

			// Set the vertex and pixel shaders that will be used to render this triangle.
			deviceContext.VertexShader.Set(VertexShader);
			deviceContext.PixelShader.Set(PixelShader);

			// Set the sampler state in the pixel shader.
			deviceContext.PixelShader.SetSampler(0, SampleState);
			// Render the triangle.
			deviceContext.DrawIndexed(indexCount, 0, 0);
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
