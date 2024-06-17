using System;
using System.IO;

namespace Kingmaker.EntitySystem.Persistence;

internal class AreaDataStashFileAccessor : IDisposable
{
	public readonly string Path;

	private readonly bool m_IsHistoryFile;

	private readonly GameHistoryFile m_GameHistoryFile;

	public AreaDataStashFileAccessor(string folder, string filename, GameHistoryFile gameHistoryFile)
	{
		m_GameHistoryFile = gameHistoryFile;
		m_IsHistoryFile = filename == "history";
		Path = System.IO.Path.Combine(folder, filename);
		if (m_IsHistoryFile)
		{
			m_GameHistoryFile.Close();
		}
	}

	public void Dispose()
	{
		if (m_IsHistoryFile)
		{
			m_GameHistoryFile.Open();
		}
	}
}
