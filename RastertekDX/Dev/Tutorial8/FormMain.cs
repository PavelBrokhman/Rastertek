using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Tutorial8
{
	public partial class FormMain : Form
	{
		// Collection of the file lines.
		IEnumerable<string> fileLines = null;

		public FormMain()
		{
			InitializeComponent();
		}

		private void ButtonFrom_Click(object sender, EventArgs e)
		{
			// Read in the name of the model file.
			openFileDialog1.ShowDialog();
			textBoxFrom.Text = openFileDialog1.FileName;
			fileLines = File.ReadAllLines(textBoxFrom.Text);
		}

		private void buttonTo_Click(object sender, EventArgs e)
		{
			// Get the name of the file to save in.
			saveFileDialog1.ShowDialog();
			textBoxTo.Text = saveFileDialog1.FileName;
		}

		private void ButtonParse_Click(object sender, EventArgs e)
		{
			FileStream fileSave = null;
			try
			{
				//fileSave = File.Create(textBoxTo.Text);
				// Get all the main formats from the file: vertices, textures, normals, faces.
				var vertices =
					(from line in fileLines
					 where line.Trim().StartsWith("v ")
					 select new MayaVertex(line.Substring(1))).ToList();
				var textures =
					(from line in fileLines
					 where line.Trim().StartsWith("vt ")
					 select new MayaTexture(line.Substring(2))).ToList();
				var normals =
					(from line in fileLines
					 where line.Trim().StartsWith("vn ")
					 select new MayaNormal(line.Substring(2))).ToList();
				var faces =
					(from line in fileLines
					 where line.Trim().StartsWith("f ")
					 select new MayaFace(line.Substring(2))).ToList();

				var saveFile = new StringBuilder();
				saveFile.AppendLine("Vertex Count: " + faces.Count * 3);
				saveFile.AppendLine();
				saveFile.AppendLine("Data:");
				saveFile.AppendLine();

				foreach (var face in faces)
				{
					foreach (var faceIndices in face.vertices)
					{
						var vertex = vertices[faceIndices.Vertex - 1];
						var texture = textures[faceIndices.Texture - 1];
						var normal = normals[faceIndices.Normal - 1];

						saveFile.AppendFormat("{0} {1} {2} ", vertex.x, vertex.y, vertex.z);
						saveFile.AppendFormat("{0} {1} ", texture.x, texture.y);
						saveFile.AppendFormat("{0} {1} {2}", normal.x, normal.y, normal.z);
						saveFile.AppendLine();
					}
				}

				File.WriteAllText(textBoxTo.Text, saveFile.ToString());
			}
			finally
			{
				if (fileSave != null)
				{
					fileSave.Flush();
					fileSave.Close();
					fileSave.Dispose();
					fileSave = null;
				}
			}
		}
	}
}
