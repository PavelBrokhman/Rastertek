using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace Tutorial21.Graphics.Models
{
	public class ModelList : ICloneable
	{
		#region Variables
		public int ModelCount { get; private set; }
		ModelInfo[] _ModelInfoList;
		#endregion

		#region Structs
		struct ModelInfo
		{
			public Vector4 color;
			public Vector3 position;
		}
		#endregion

		#region Public Methods
		public bool Initialize(int numModels)
		{
			// Store the number of models.
			ModelCount = numModels;

			// Create a list array of the model information.
			_ModelInfoList = new ModelInfo[ModelCount];

			// Seed the random generator with the current time.
			var random = new Random(DateTime.Now.TimeOfDay.Seconds);

			// Go through all the models and randomly generate the model color and position.
			for(int i=0; i < ModelCount; i++)
			{
				// Generate a random color for the model.
				var red = (float)random.Next() / int.MaxValue;
				var green = (float)random.Next() / int.MaxValue;
				var blue = (float)random.Next() / int.MaxValue;

				_ModelInfoList[i].color = new Vector4(red, green, blue, 1);

				// Generate a random position in front of the viewer for the mode.
				_ModelInfoList[i].position = new Vector3
				{
					X = (float)(random.Next() - random.Next()) / int.MaxValue * 10,
					Y = (float)(random.Next() - random.Next()) / int.MaxValue * 10,
					Z = ((float)(random.Next() - random.Next()) / int.MaxValue * 10) + 5
				};
			}

			return true;
		}

		public void Shutdown()
		{
			// Release the model information list.
			_ModelInfoList = null;
		}

		public void GetData(int index, out Vector3 position, out Vector4 color)
		{
			var modelInfo = _ModelInfoList[index];
			position = modelInfo.position;
			color = modelInfo.color;
		}
		public void GetData(int index, out float positionX, out float positionY, out float positionZ, Vector4 color)
		{
			var modelInfo = _ModelInfoList[index];
			positionX = modelInfo.position.X;
			positionY = modelInfo.position.Y;
			positionZ = modelInfo.position.Z;
			color = modelInfo.color;
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
