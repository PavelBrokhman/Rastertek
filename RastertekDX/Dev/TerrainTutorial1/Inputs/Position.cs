using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace Engine.Inputs
{
	public class Position : ICloneable
	{
		#region Structures / Enums
		private enum Movement
		{
			Forward,
			Backward,
			Upward,
			Downward,
			LeftTurn,
			RightTurn,
			LookUp,
			LookDown
		}
		#endregion
		#region Properties / Variables
		public float FrameTime { get; set; }

		private float positionX, positionY, positionZ;
		private float rotationX, rotationY, rotationZ;

		private float forwardSpeed, backwardSpeed;
		private float upwardSpeed, downwardSpeed;
		private float leftTurnSpeed, rightTurnSpeed;
		private float lookUpSpeed, lookDownSpeed;
		#endregion

		#region Public Methods
		public void SetPosition(float x, float y, float z)
		{
			positionX = x;
			positionY = y;
			positionZ = z;
		}
		public void SetPosition(Vector3 position)
		{
			SetPosition(position.X, position.Y, position.Z);
		}

		public void SetRotation(float x, float y, float z)
		{
			rotationX = x;
			rotationY = y;
			rotationZ = z;
		}
		public void SetRotation(Vector3 rotation)
		{
			SetRotation(rotation.X, rotation.Y, rotation.Z);
		}

		public Vector3 GetPosition()
		{
			return new Vector3(positionX, positionY, positionZ);
		}
		public void GetPosition(out float x, out float y, out float z)
		{
			x = positionX;
			y = positionY;
			z = positionZ;
		}

		public Vector3 GetRotation()
		{
			return new Vector3(rotationX, rotationY, rotationZ);
		}
		public void GetRotation(out float x, out float y, out float z)
		{
			x = rotationX;
			y = rotationY;
			z = rotationZ;
		}

		public void MoveForward(bool keydown)
		{
			// Update the forward speed movement based on the frame time and whether the user is holding the key down or not.
			if (keydown)
			{
				forwardSpeed += FrameTime * 0.001f;
				if (forwardSpeed > FrameTime * 0.03)
					forwardSpeed = FrameTime * 0.03f;
			}
			else
			{
				forwardSpeed -= FrameTime * 0.0007f;
				if (forwardSpeed < 0)
					forwardSpeed = 0;
			}

			// Convert degrees to radians.
			var radians = rotationY * 0.0174532925f;

			// Update the position.
			positionX += (float)Math.Sin(radians) * forwardSpeed;
			positionZ += (float)Math.Cos(radians) * forwardSpeed;
		}

		public void MoveBackward(bool keydown)
		{
			// Update the backward speed movement based on the frame time and whether the user is holding the key down or not.
			if (keydown)
			{
				backwardSpeed += FrameTime * 0.001f;
				if (backwardSpeed > FrameTime * 0.03)
					backwardSpeed = FrameTime * 0.03f;
			}
			else
			{
				backwardSpeed -= FrameTime * 0.0007f;
				if (backwardSpeed < 0)
					backwardSpeed = 0;
			}

			// Convert degrees to radians.
			var radians = rotationY * 0.0174532925f;

			// Update the position.
			positionX -= (float)Math.Sin(radians) * backwardSpeed;
			positionZ -= (float)Math.Cos(radians) * backwardSpeed;
		}

		public void MoveUpward(bool keydown)
		{
			// Update the upward speed movement based on the frame time and whether the user is holding the key down or not.
			if (keydown)
			{
				upwardSpeed += FrameTime * 0.003f;
				if (upwardSpeed > FrameTime * 0.03)
					upwardSpeed = FrameTime * 0.03f;
			}
			else
			{
				upwardSpeed -= FrameTime * 0.0002f;
				if (upwardSpeed < 0)
					upwardSpeed = 0;
			}

			// Update the height position.
			positionY += upwardSpeed;
		}

		public void MoveDownward(bool keydown)
		{
			// Update the upward speed movement based on the frame time and whether the user is holding the key down or not.
			if (keydown)
			{
				downwardSpeed += FrameTime * 0.003f;
				if (downwardSpeed > FrameTime * 0.03)
					downwardSpeed = FrameTime * 0.03f;
			}
			else
			{
				downwardSpeed -= FrameTime * 0.0002f;
				if (downwardSpeed < 0)
					downwardSpeed = 0;
			}

			// Update the height position.
			positionY -= downwardSpeed;
		}

		public void TurnLeft(bool keydown)
		{
			// If the key is pressed increase the speed at which the camera turns left. If not slow down the turn speed.
			if (keydown)
			{
				leftTurnSpeed += FrameTime * 0.01f;
				if (leftTurnSpeed > FrameTime * 0.15f)
					leftTurnSpeed = FrameTime * 0.15f;
			}
			else
			{
				leftTurnSpeed -= FrameTime * 0.005f;
				if (leftTurnSpeed < 0)
					leftTurnSpeed = 0;
			}

			// Update the rotation using the turning speed.
			rotationY -= leftTurnSpeed;

			// Keep the rotation in the 0 to 360
			if (rotationY < 0)
				rotationY += 360;
		}

		public void TurnRight(bool keydown)
		{
			// If the key is pressed increase the speed at which the camera turns right. If not slow down the turn speed.
			if (keydown)
			{
				rightTurnSpeed += FrameTime * 0.01f;
				if (rightTurnSpeed > FrameTime * 0.15)
					rightTurnSpeed = FrameTime * 0.15f;
			}
			else
			{
				rightTurnSpeed -= FrameTime * 0.005f;
				if (rightTurnSpeed < 0)
					rightTurnSpeed = 0;
			}

			// Update the rotation using the turning speed.
			rotationY += rightTurnSpeed;

			// Keep the rotation in the range 0 to 360 range.
			if (rotationY > 360)
				rotationY -= 360;
		}

		public void LookUpward(bool keydown)
		{
			// If the key is pressed increase the speed at which the camera turns up. If not slow down the turn speed.
			if (keydown)
			{
				lookUpSpeed += FrameTime * 0.01f;
				if (lookUpSpeed > FrameTime * 0.15)
					lookUpSpeed = FrameTime * 0.15f;
			}
			else
			{
				lookUpSpeed -= FrameTime * 0.005f;
				if (lookUpSpeed < 0)
					lookUpSpeed = 0;
			}

			// Update the rotation using the turning speed.
			rotationX -= lookUpSpeed;

			// Keep the rotation maximum 90 degrees.
			if (rotationX > 90)
				rotationX = 90;
		}

		public void LookDownward(bool keydown)
		{
			// If the key is pressed increase the speed at which the camera turns down. If not slow down the turn speed.
			if (keydown)
			{
				lookDownSpeed += FrameTime * 0.01f;
				if (lookDownSpeed > FrameTime * 0.15)
					lookDownSpeed = FrameTime * 0.15f;
			}
			else
			{
				lookDownSpeed -= FrameTime * 0.005f;
				if (lookDownSpeed < 0)
					lookDownSpeed = 0;
			}

			// Update the rotation using the turning speed.
			rotationX += lookDownSpeed;

			// Keep the rotation maximum 90 degrees
			if (rotationX < -90)
				rotationX = -90;
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
