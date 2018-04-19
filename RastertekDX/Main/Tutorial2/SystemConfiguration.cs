using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tutorial2
{
	public class SystemConfiguration
	{
		#region Variables / Properties
		/// <summary>
		/// Gets or sets the window title.
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// Gets or sets the width of the window.
		/// </summary>
		public int Width { get; set; }
		/// <summary>
		/// Gets or sets the height of the window.
		/// </summary>
		public int Height { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether [wait vertical blanking].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [wait vertical blanking]; otherwise, <c>false</c>.
		/// </value>
		public bool WaitVerticalBlanking
		{
			get;
			set;
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="DemoConfiguration"/> class.
		/// </summary>
		public SystemConfiguration()
			: this("SharpDX Demo")
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DemoConfiguration"/> class.
		/// </summary>
		public SystemConfiguration(string title)
			: this(title, 800, 600)
		{
		}

		public SystemConfiguration(string title, int width, int height)
		{
			Title = title;
			Width = width;
			Height = height;
			WaitVerticalBlanking = false;
		}
		#endregion
	}
}
