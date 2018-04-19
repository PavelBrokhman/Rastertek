namespace Tutorial8
{
	partial class FormMain
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.textBoxFrom = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxTo = new System.Windows.Forms.TextBox();
			this.ButtonFrom = new System.Windows.Forms.Button();
			this.buttonTo = new System.Windows.Forms.Button();
			this.ButtonParse = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// textBoxFrom
			// 
			this.textBoxFrom.Location = new System.Drawing.Point(114, 44);
			this.textBoxFrom.Name = "textBoxFrom";
			this.textBoxFrom.Size = new System.Drawing.Size(467, 20);
			this.textBoxFrom.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(56, 44);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(30, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "From";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(56, 84);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(20, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "To";
			// 
			// textBoxTo
			// 
			this.textBoxTo.Location = new System.Drawing.Point(114, 84);
			this.textBoxTo.Name = "textBoxTo";
			this.textBoxTo.Size = new System.Drawing.Size(467, 20);
			this.textBoxTo.TabIndex = 2;
			// 
			// ButtonFrom
			// 
			this.ButtonFrom.Location = new System.Drawing.Point(588, 40);
			this.ButtonFrom.Name = "ButtonFrom";
			this.ButtonFrom.Size = new System.Drawing.Size(27, 23);
			this.ButtonFrom.TabIndex = 4;
			this.ButtonFrom.Text = "...";
			this.ButtonFrom.UseVisualStyleBackColor = true;
			this.ButtonFrom.Click += new System.EventHandler(this.ButtonFrom_Click);
			// 
			// buttonTo
			// 
			this.buttonTo.Location = new System.Drawing.Point(587, 84);
			this.buttonTo.Name = "buttonTo";
			this.buttonTo.Size = new System.Drawing.Size(27, 23);
			this.buttonTo.TabIndex = 5;
			this.buttonTo.Text = "...";
			this.buttonTo.UseVisualStyleBackColor = true;
			this.buttonTo.Click += new System.EventHandler(this.buttonTo_Click);
			// 
			// ButtonParse
			// 
			this.ButtonParse.Location = new System.Drawing.Point(330, 132);
			this.ButtonParse.Name = "ButtonParse";
			this.ButtonParse.Size = new System.Drawing.Size(75, 23);
			this.ButtonParse.TabIndex = 6;
			this.ButtonParse.Text = "Parse";
			this.ButtonParse.UseVisualStyleBackColor = true;
			this.ButtonParse.Click += new System.EventHandler(this.ButtonParse_Click);
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(642, 262);
			this.Controls.Add(this.ButtonParse);
			this.Controls.Add(this.buttonTo);
			this.Controls.Add(this.ButtonFrom);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textBoxTo);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBoxFrom);
			this.Name = "FormMain";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.TextBox textBoxFrom;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBoxTo;
		private System.Windows.Forms.Button ButtonFrom;
		private System.Windows.Forms.Button buttonTo;
		private System.Windows.Forms.Button ButtonParse;
	}
}

