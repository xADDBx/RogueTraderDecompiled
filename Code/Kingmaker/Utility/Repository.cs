using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Kingmaker.Utility;

internal class Repository : IDisposable
{
	private bool m_Disposed;

	private readonly Process m_GitProcess;

	public string CommitHash => RunCommand("rev-parse HEAD");

	public string BranchName => RunCommand("rev-parse --abbrev-ref HEAD");

	public string TrackedBranchName => RunCommand("rev-parse --abbrev-ref --symbolic-full-name @{u}");

	public bool HasUnpushedCommits => !string.IsNullOrWhiteSpace(RunCommand("log @{u}..HEAD"));

	public bool HasUncommittedChanges => !string.IsNullOrWhiteSpace(RunCommand("status --porcelain"));

	private bool IsGitRepository => !string.IsNullOrWhiteSpace(RunCommand("log -1"));

	public IEnumerable<string> Log
	{
		get
		{
			int skip = 0;
			while (true)
			{
				string text = RunCommand($"log --skip={skip++} -n1");
				if (string.IsNullOrWhiteSpace(text))
				{
					break;
				}
				yield return text;
			}
		}
	}

	public static Repository GetRepositoryInfo(string path, string gitPath = null)
	{
		Repository repository = new Repository(path, gitPath);
		if (!repository.IsGitRepository)
		{
			return null;
		}
		return repository;
	}

	public void Dispose()
	{
		if (!m_Disposed)
		{
			m_Disposed = true;
			m_GitProcess.Dispose();
		}
	}

	private Repository(string path, string gitPath)
	{
		ProcessStartInfo startInfo = new ProcessStartInfo
		{
			UseShellExecute = false,
			RedirectStandardOutput = true,
			FileName = (Directory.Exists(gitPath) ? gitPath : "git.exe"),
			CreateNoWindow = true,
			WorkingDirectory = ((path != null && Directory.Exists(path)) ? path : Environment.CurrentDirectory)
		};
		m_GitProcess = new Process
		{
			StartInfo = startInfo
		};
	}

	private string RunCommand(string args)
	{
		try
		{
			m_GitProcess.StartInfo.Arguments = args;
			m_GitProcess.Start();
			string result = m_GitProcess.StandardOutput.ReadToEnd().Trim();
			m_GitProcess.WaitForExit();
			return result;
		}
		catch
		{
			return string.Empty;
		}
	}
}
