using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace Tutorial16.Inputs
{
	public class Position : ICloneable
	{
		#region Properties / Variables
		public float FrameTime { get; set; }
		public float RotationX { get; private set; }
		public float RotationY { get; private set; }

		private float leftTurnSpeed, rightTurnSpeed;
		private float upLookSpeed, downLookSpeed;
		#endregion

		#region Public Methods
		public void TurnLeft(bool keydown)
		{
			// If the key is pressed increase the speed at which the camera turns left. If not slow down the turn speed.
			if (keydown)
			{
				leftTurnSpeed += FrameTime * 0.01f;
				if (leftTurnSpeed > FrameTime * 0.15)
					leftTurnSpeed = FrameTime * 0.15f;
			}
			else
			{
				leftTurnSpeed -= FrameTime * 0.005f;
				if (leftTurnSpeed < 0)
					leftTurnSpeed = 0;
			}

			// Update the rotation using the turning speed.
			RotationY -= leftTurnSpeed;
			if (RotationY < 0)
				RotationY += 360;
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
			RotationY += rightTurnSpeed;
			if (RotationY > 360)
				RotationY -= 360;
		}

		public void LookDown(bool keydown)
		{
			// If the key is pressed increase the speed at which the camera turns down. If not slow down the turn speed.
			if (keydown)
			{
				downLookSpeed += FrameTime * 0.01f;
				if (downLookSpeed > FrameTime * 0.15)
					downLookSpeed = FrameTime * 0.15f;
			}
			else
			{
				downLookSpeed -= FrameTime * 0.005f;
				if (downLookSpeed < 0)
					downLookSpeed = 0;
			}

			// Update the rotation using the turning speed.
			RotationX += downLookSpeed;
			if (RotationX >= 90)
				RotationX = 90;
		}

		public void LookUp(bool keydown)
		{
			// If the key is pressed increase the speed at which the camera turns up. If not slow down the turn speed.
			if (keydown)
			{
				upLookSpeed += FrameTime * 0.01f;
				if (upLookSpeed > FrameTime * 0.15)
					upLookSpeed = FrameTime * 0.15f;
			}
			else
			{
				upLookSpeed -= FrameTime * 0.005f;
				if (upLookSpeed < 0)
					upLookSpeed = 0;
			}

			// Update the rotation using the turning speed.
			RotationX -= upLookSpeed;
			if (RotationX <= -90)
				RotationX = -90;
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
