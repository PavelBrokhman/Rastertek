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
using GraphicsClass = Tutorial24.Graphics.Graphics;
using InputClass = Tutorial24.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using System.Runtime.InteropServices;
using Tutorial24.System;

namespace Tutorial24.Graphics.Shaders
{
	public class SpecMapShader : ICloneable
	{
		#region Structures
		[StructLayout(LayoutKind.Sequential)]
		internal struct Vertex
		{
			public Vector3 position;
			public Vector2 texture;
			public Vector3 normal;
			public Vector3 tangent;
			public Vector3 binormal;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct MatrixBuffer
		{
			public Matrix world;
			public Matrix view;
			public Matrix projection;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct CameraBuffer
		{
			public Vector3 cameraPosition;
			public float padding;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct LightBuffer
		{
			public Vector4 diffuseColor;
			public Vector3 lightDirection;
			public float specularPower;
			public Vector4 specularColor;
		}
		#endregion

		#region Variables / Properties
		VertexShader VertexShader { get; set; }
		PixelShader PixelShader { get; set; }
		InputLayout Layout { get; set; }
		Buffer ConstantMatrixBuffer { get; set; }
		Buffer ConstantLightBuffer { get; set; }
		Buffer ConstantCameraBuffer { get; set; }
		SamplerState SampleState { get; set; }
		#endregion

		#region Constructors
		public SpecMapShader() { }
		#endregion

		#region Methods
		public bool Initialize(Device device, IntPtr windowHandler)
		{
			// Initialize the vertex and pixel shaders.
			return InitializeShader(device, windowHandler, "specmap.vs", "specmap.ps");
		}

		public void Shuddown()
		{
			// Shutdown the vertex and pixel shaders as well as the related objects.
			ShuddownShader();
		}

		public bool Render(DeviceContext deviceContext, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView[] textures, Vector3 lightDirection, Vector4 diffuseColor, Vector3 cameraPosition, Vector4 specularColor, float specularPower)
		{
			// Set the shader parameters that it will use for rendering.
			if (!SetShaderParameters(deviceContext, worldMatrix, viewMatrix, projectionMatrix, textures, lightDirection, diffuseColor, cameraPosition, specularColor, specularPower))
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

				#region Initialize Shaders
				// Compile the vertex shader code.
				var vertexShaderByteCode = ShaderBytecode.CompileFromFile(vsFileName, "SpecMapVertexShader", SystemConfiguration.VertexShaderProfile, ShaderFlags.None, EffectFlags.None);
				// Compile the pixel shader code.
				var pixelShaderByteCode = ShaderBytecode.CompileFromFile(psFileName, "SpecMapPixelShader", SystemConfiguration.PixelShaderProfile, ShaderFlags.None, EffectFlags.None);

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
					},
					new InputElement()
					{
						SemanticName = "NORMAL",
						SemanticIndex = 0,
						Format = Format.R32G32B32_Float,
						Slot = 0,
						AlignedByteOffset = InputElement.AppendAligned,
						Classification = InputClassification.PerVertexData,
						InstanceDataStepRate = 0
					},
					new InputElement()
					{
						SemanticName = "TANGENT",
						SemanticIndex = 0,
						Format = Format.R32G32B32_Float,
						Slot = 0,
						AlignedByteOffset = InputElement.AppendAligned,
						Classification = InputClassification.PerVertexData,
						InstanceDataStepRate = 0
					},
					new InputElement()
					{
						SemanticName = "BINORMAL",
						SemanticIndex = 0,
						Format = Format.R32G32B32_Float,
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

				#region Initialize Light Buffer
				// Setup the description of the light dynamic constant buffer that is in the pixel shader.
				var lightBufferDesc = new BufferDescription()
				{
					Usage = ResourceUsage.Dynamic,
					SizeInBytes = Utilities.SizeOf<LightBuffer>(),
					BindFlags = BindFlags.ConstantBuffer,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					StructureByteStride = 0
				};

				// Create the constant buffer pointer so we can access the vertex shader constant buffer from within this class.
				ConstantLightBuffer = new Buffer(device, lightBufferDesc);
				#endregion

				#region Initialize Camera Buffer
				// Setup the description of the camera dynamic constant buffer that is in the vertex shader.
				var cameraBufferDesc = new BufferDescription()
				{
					Usage = ResourceUsage.Dynamic,
					SizeInBytes = Utilities.SizeOf<CameraBuffer>(),
					BindFlags = BindFlags.ConstantBuffer,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					StructureByteStride = 0
				};

				// Create the constant buffer pointer so we can access the vertex shader constant buffer from within this class.
				ConstantCameraBuffer = new Buffer(device, cameraBufferDesc);
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
			// Release the camera constant buffer.
			if (ConstantCameraBuffer != null)
			{
				ConstantCameraBuffer.Dispose();
				ConstantCameraBuffer = null;
			}

			// Release the light constant buffer.
			if (ConstantLightBuffer != null)
			{
				ConstantLightBuffer.Dispose();
				ConstantLightBuffer = null;
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

		private bool SetShaderParameters(DeviceContext deviceContext, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView[] textures, Vector3 lightDirection, Vector4 diffuseColor, Vector3 cameraPosition, Vector4 specularColor, float specularPower)
		{
			try
			{
				#region Constant Matrix Buffer
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

				#region Constant Light Buffer
				// Lock the light constant buffer so it can be written to.
				deviceContext.MapSubresource(ConstantLightBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out mappedResource);

				// Copy the lighting variables into the constant buffer.
				var lightBuffer = new LightBuffer()
				{
					diffuseColor = diffuseColor,
					lightDirection = lightDirection,
					specularColor = specularColor,
					specularPower = specularPower,
				};

				mappedResource.Write(lightBuffer);

				// Unlock the constant buffer.
				deviceContext.UnmapSubresource(ConstantLightBuffer, 0);

				// Set the position of the light constant buffer in the pixel shader.
				bufferNumber = 0;

				// Finally set the light constant buffer in the pixel shader with the updated values.
				deviceContext.PixelShader.SetConstantBuffer(bufferNumber, ConstantLightBuffer);
				#endregion

				#region Constant Camera Buffer
				// Lock the camera constant buffer so it can be written to.
				deviceContext.MapSubresource(ConstantCameraBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out mappedResource);

				// Copy the lighting variables into the constant buffer.
				var cameraBuffer = new CameraBuffer()
				{
					cameraPosition = cameraPosition,
					padding = 0.0f
				};

				mappedResource.Write(cameraBuffer);

				// Unlock the constant buffer.
				deviceContext.UnmapSubresource(ConstantCameraBuffer, 0);

				// Set the position of the light constant buffer in the pixel shader.
				bufferNumber = 1;

				// Now set the camera constant buffer in the vertex shader with the updated values.
				deviceContext.VertexShader.SetConstantBuffer(bufferNumber, ConstantCameraBuffer);
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
