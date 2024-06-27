using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Kingmaker.Utility.Reporting.Base;

public class ReportVersionManager
{
	public static string GetCommitOrRevision(bool shortHash = false)
	{
		try
		{
			return GetCommitFromVersionFile();
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
	}

	public static string GetBuildInfo()
	{
		string empty = string.Empty;
		if (!string.IsNullOrEmpty(empty))
		{
			return empty;
		}
		try
		{
			return File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Version.info"));
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
	}

	private static string GetCommitFromVersionFile()
	{
		try
		{
			return File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Version.info")).Split(' ')[2];
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
	}

	private static string StartCommandlineCommand(string cmd)
	{
		try
		{
			Process process = new Process();
			process.StartInfo = new ProcessStartInfo
			{
				WorkingDirectory = Application.dataPath,
				FileName = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "powershell" : "/bin/bash"),
				Arguments = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ("/C \"" + cmd + "\"") : ("-c \"" + cmd + "\"")),
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			process.Start();
			string result = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			return result;
		}
		catch
		{
			return string.Empty;
		}
	}
}
