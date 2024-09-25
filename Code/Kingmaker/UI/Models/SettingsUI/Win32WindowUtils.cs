using System;
using System.Runtime.InteropServices;

namespace Kingmaker.UI.Models.SettingsUI;

public static class Win32WindowUtils
{
	public struct Rect
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;

		public int Width => Right - Left;

		public int Height => Bottom - Top;
	}

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

	[DllImport("user32.dll")]
	private static extern bool GetClientRect(IntPtr hWnd, out Rect lpRect);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetActiveWindow();

	public static Rect GetWindowRect()
	{
		GetWindowRect(GetActiveWindow(), out var lpRect);
		return lpRect;
	}

	public static Rect GetClientRect()
	{
		GetClientRect(GetActiveWindow(), out var lpRect);
		return lpRect;
	}
}
