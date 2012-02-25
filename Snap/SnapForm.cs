using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Snap
{
	public partial class SnapForm : Form
	{
		#region Imports
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr GetForegroundWindow();
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int X, int Y, int cx, int cy, uint flags);
		#endregion

		#region Private Variables
		private readonly Size windowSize = SystemInformation.PrimaryMonitorMaximizedWindowSize;
		private readonly int keyOffset = 37;
		private KeyboardHook hook = new KeyboardHook();
		private HashSet<IntPtr> seen = new HashSet<IntPtr>();
		private List<Dictionary<string, int>> windows = new List<Dictionary<string, int>>();
		#endregion

		public SnapForm()
		{
			InitializeComponent();

			hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
			// Register directional keys + ctrl + windows key
			hook.RegisterHotKey(Snap.ModifierKeys.Control | Snap.ModifierKeys.Win, Keys.Left);
			hook.RegisterHotKey(Snap.ModifierKeys.Control | Snap.ModifierKeys.Win, Keys.Right);
			hook.RegisterHotKey(Snap.ModifierKeys.Control | Snap.ModifierKeys.Win, Keys.Up);
			hook.RegisterHotKey(Snap.ModifierKeys.Control | Snap.ModifierKeys.Win, Keys.Down);

			this.ShowInTaskbar = false;
		}

		void hook_KeyPressed(object sender, KeyPressedEventArgs e)
		{
			IntPtr currentWindow = GetForegroundWindow();
			if (seen.Contains(currentWindow))
			{
				// Look at the window's state, and do the appropriate action
				foreach (Dictionary<string, int> window in windows)
				{
					if (window["window"] == (int)currentWindow)
					{
						// Found the window
						window["state"] = SetWindowSizeAndPosition(currentWindow, window["state"], e.Key);
					}
				}
			}
			else
			{
				// Do the right thing and add the window
				int newState = SetWindowSizeAndPosition(currentWindow, 0, e.Key);
				windows.Add(new Dictionary<string, int>() { { "window", (int)currentWindow }, { "state", newState } });
				seen.Add(currentWindow);
			}
		}

		private void SnapForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// Do some cleanup
		}

		// Sets the new window size and position based on the current window state and pressed key
		// Returns the new window state
		private int SetWindowSizeAndPosition(IntPtr window, int windowState, Keys key)
		{
			uint ignoreZIndex = 0x0004;		// Bit flag to ignore the z-index ordering
			int action = windowState + ((int)key) - keyOffset;
			switch (action)
			{
				// Left
				case 0:     // Left
				case 25:	// Up + BottomLeftState
				case 43:	// Down + TopLeftState
					SetWindowPos(window, window, 0, 0, windowSize.Width / 2, windowSize.Height, ignoreZIndex);
					return (int)WindowStates.Left;
				// Top
				case 1:     // Up
				case 36:	// Left + TopRightState
				case 42:	// Right + TopLeftState
					SetWindowPos(window, window, 0, 0, windowSize.Width, windowSize.Height / 2, ignoreZIndex);
					return (int)WindowStates.Top;
				// Right
				case 2:     // Right
				case 21:	// Up + BottomRightState
				case 39:	// Down + TopRightState
					SetWindowPos(window, window, windowSize.Width / 2, 0, windowSize.Width / 2, windowSize.Height, ignoreZIndex);
					return (int)WindowStates.Right;
				// Bottom
				case 3:     // Down
				case 20:	// Left + BottomRightState
				case 26:	// Right + BottomLeftState
					SetWindowPos(window, window, 0, windowSize.Height/2, windowSize.Width, windowSize.Height / 2, ignoreZIndex);
					return (int)WindowStates.Bottom;
				// Maximize
				case 4:     // Left + RightState
				case 10:    // Right + LeftState
				case 17:	// Up + BottomState
				case 35:	// Bottom + TopState
					SetWindowPos(window, window, 0, 0, windowSize.Width, windowSize.Height, ignoreZIndex);
					return 0;
				// Same
				case 6:     // Right + RightState
				case 8:     // Left + LeftState
				case 19:	// Down + BottomState
				case 33:	// Up + TopState
				case 37:	// Up + TopRightState
				case 38:	// Right + TopRightState
				case 40:	// Left + TopLeftState
				case 41:	// Up + TopLeftState
				case 22:	// Right + BottomRightState
				case 23:	// Down + BottomRightState
				case 24:	// Left + BottomLeftState
				case 27:	// Down + BottomLeftState
					// Don't do anything
					return windowState;
				// Bottom Right
				case 7:     // Down + RightState
				case 18:	// Right + BottomState
					SetWindowPos(window, window, windowSize.Width / 2, windowSize.Height / 2, windowSize.Width / 2, windowSize.Height / 2, ignoreZIndex);
					return (int)WindowStates.Bottom | (int)WindowStates.Right;
				// Top Left
				case 9:     // Up + LeftState
				case 32:	// Left + TopState
					SetWindowPos(window, window, 0, 0, windowSize.Width / 2, windowSize.Height / 2, ignoreZIndex);
					return (int)WindowStates.Left | (int)WindowStates.Top;
				// Bottom Left
				case 11:	// Down + LeftState
				case 16:	// Left + BottomState
					SetWindowPos(window, window, 0, windowSize.Height / 2, windowSize.Width / 2, windowSize.Height / 2, ignoreZIndex);
					return (int)WindowStates.Bottom | (int)WindowStates.Left;
				// Top Right
				case 5:     // Up + RightState
				case 34:	// Right + TopState
					SetWindowPos(window, window, windowSize.Width / 2, 0, windowSize.Width / 2, windowSize.Height / 2, ignoreZIndex);
					return ((int)WindowStates.Top | (int)WindowStates.Right);
			}
			return -1;
		}

		private void SnapForm_Resize(object sender, EventArgs e)
		{
			if (FormWindowState.Minimized == this.WindowState)
			{
				notifyIcon.Visible = true;
				this.Hide();
			}
			else if (FormWindowState.Normal == this.WindowState)
			{
				notifyIcon.Visible = false;
			}
		}

		private void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			this.Close();
		}

		private void notifyIcon_Click(object sender, EventArgs e)
		{
			contextMenu.Visible = true;
		}
	}

	public enum WindowStates
	{
		Top = 32,
		Bottom = 16,
		Left = 8,
		Right = 4
	}
}
