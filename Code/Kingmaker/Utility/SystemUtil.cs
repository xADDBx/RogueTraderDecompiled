using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Kingmaker.EntitySystem.Persistence;

namespace Kingmaker.Utility;

public static class SystemUtil
{
	public const int WM_LBUTTONDOWN = 513;

	public const int WM_LBUTTONUP = 514;

	public const int WM_KEYDOWN = 256;

	public const int WM_KEYUP = 257;

	public const int WM_CLOSE = 16;

	public static string ExecutablePath
	{
		get
		{
			string text = Environment.CommandLine.Replace("\"", "");
			int num = text.IndexOf(".exe", StringComparison.Ordinal) + 4;
			if (num >= text.Length)
			{
				return text;
			}
			return text.Substring(0, num);
		}
	}

	public static string CommandLineArguments
	{
		get
		{
			string text = Environment.CommandLine.Replace("\"", "");
			int num = text.IndexOf(".exe", StringComparison.Ordinal) + 5;
			if (num >= text.Length)
			{
				return "";
			}
			return text.Substring(num, text.Length - num);
		}
	}

	public static void SendKey(IntPtr hwnd, int keyCode, bool extended)
	{
		uint num = MapVirtualKey((uint)keyCode, 0u);
		uint num2 = 1u | (num << 16);
		if (extended)
		{
			num2 |= 0x1000000u;
		}
		SendMessage(hwnd, 256, (uint)keyCode, num2);
		num2 |= 0xC0000000u;
		SendMessage(hwnd, 257, (uint)keyCode, num2);
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern uint MapVirtualKey(uint uCode, uint uMapType);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr GetActiveWindow();

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);

	public static void ApplicationQuit()
	{
		ApplicationQuitter.Request();
	}

	internal static void ApplicationQuitInternal()
	{
		SaveManager.WaitCommit();
		SendMessage(GetActiveWindow(), 16, 0u, 0u);
	}

	public static void WaitForPreviousProcessToFinish()
	{
		Match match = Regex.Match(CommandLineArguments, "-waitpid (\\d+)");
		if (match.Success)
		{
			try
			{
				Process.GetProcessById(int.Parse(match.Groups[1].Value)).WaitForExit();
			}
			catch (Exception)
			{
			}
		}
	}

	public static void ApplicationRestart()
	{
		SaveManager.WaitCommit();
		string commandLineArguments = CommandLineArguments;
		commandLineArguments = Regex.Replace(commandLineArguments, "-waitpid (\\d+)", "");
		commandLineArguments = commandLineArguments + " -waitpid " + Process.GetCurrentProcess().Id;
		Process.Start(ExecutablePath, commandLineArguments);
		ApplicationQuit();
	}
}
