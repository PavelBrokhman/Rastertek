using System.Windows.Forms;

namespace RastertekDX.Common
{
	public class SystemSettings
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
		/// Initializes a new instance of the <see cref="SystemSettings"/> class.
		/// </summary>
		public SystemSettings()
			: this("SharpDX Demo")
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SystemSettings"/> class.
		/// </summary>
		public SystemSettings(string title)
			: this(title, 1024, 768)
		{
		}

		public SystemSettings(string title, int width, int height)
		{
			Title = title;
			Width = width;
			Height = height;
			WaitVerticalBlanking = false;
		}
		#endregion

		#region Static Variables
		public static bool FullScreen { get; private set; }
		public static bool VerticalSyncEnabled { get; private set; }
		public static float ScreenDepth { get; private set; }
		public static float ScreenNear { get; private set; }
		public static FormBorderStyle BorderStyle { get; private set; }
		#endregion

		#region Static Constructor
		static SystemSettings()
		{
			FullScreen = true;
			VerticalSyncEnabled = true;
			ScreenDepth = 1000.0f;
			ScreenNear = 0.1f;
			BorderStyle = FormBorderStyle.FixedToolWindow;
		}
		#endregion
	}
	}
