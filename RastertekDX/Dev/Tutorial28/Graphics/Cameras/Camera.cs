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

namespace Engine.Graphics.Cameras
{
	public class Camera : ICloneable
	{
		#region Variables / Properties
		private float PositionX { get; set; }
		private float PositionY { get; set; }
		private float PositionZ { get; set; }

		private float RotationX { get; set; }
		private float RotationY { get; set; }
		private float RotationZ { get; set; }

		public Matrix ViewMatrix { get; private set; }
		public Matrix ReflectionViewMatrix { get; private set; }
		#endregion

		#region Constructors
		public Camera()	{}
		#endregion

		#region Methods
		public void SetPosition(float x, float y, float z) 
		{
			PositionX = x;
			PositionY = y;
			PositionZ = z;
		}
		public void SetRotation(float x, float y, float z) 
		{
			RotationX = x;
			RotationY = y;
			RotationZ = z;
		}
		public Vector3 GetPosition() 
		{
			return new Vector3(PositionX, PositionY, PositionZ);
		}
		public Vector3 GetRotation() 
		{
			return new Vector3(RotationX, RotationY, RotationZ);
		}

		public void Render() 
		{
			//// Setup the position of the camera in the world.
			//var position = new Vector3(PositionX, PositionY, PositionZ);

			//// Setup where the camera is looking by default.
			//var lookAt = new Vector3(0, 0, 1);

			//// Set the yaw (Y axis), pitch (X axis), and roll (Z axis) rotations in radians.
			//var pitch = RotationX * 0.0174532925f;
			//var yaw = RotationY * 0.0174532925f;
			//var roll = RotationZ * 0.0174532925f;

			//// Create the rotation matrix from the yaw, pitch, and roll values.
			//var rotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);

			//// Transform the lookAt and up vector by the rotation matrix so the view is correctly rotated at the origin.
			//lookAt = Vector3.TransformCoordinate(lookAt, rotationMatrix);
			//var up = Vector3.TransformCoordinate(Vector3.UnitY, rotationMatrix);

			//// Translate the rotated camera position to the location of the viewer.
			//lookAt = position + lookAt;

			//// Finally create the view matrix from the three updated vectors.
			//ViewMatrix = Matrix.LookAtLH(position, lookAt, up);

			// Setup the position of the camera in the world.
			var position = new Vector3(PositionX, PositionY , PositionZ);

			// Calculate the rotation in radians.
			var yaw = RotationY * 0.0174532925f;

			// Setup where the camera is looking.
			var lookAt = new Vector3((float)Math.Sin(yaw) + position.X, position.Y, (float)Math.Cos(yaw) + position.Z);

			// Create the view matrix from the three vectors.
			ViewMatrix = Matrix.LookAtLH(position, lookAt, Vector3.UnitY);
		}
		public void RenderReflection(float height)
		{
			// Setup the position of the camera in the world.
			var position = new Vector3(PositionX, -PositionY + (height * 2), PositionZ);

			// Set the yaw (Y axis), pitch (X axis), and roll (Z axis) rotations in radians.
			var yaw = RotationY * 0.0174532925f;

			// Setup where the camera is looking by default.
			var lookAt = new Vector3((float)Math.Sin(yaw) + position.X, position.Y, (float)Math.Cos(yaw) + position.Z);

			// Finally create the reflection view matrix from the three updated vectors.
			ReflectionViewMatrix = Matrix.LookAtLH(position, lookAt, Vector3.UnitY);
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
