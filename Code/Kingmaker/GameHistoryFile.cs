using System.IO;
using Kingmaker.EntitySystem.Persistence;

namespace Kingmaker;

public class GameHistoryFile
{
	public const string Name = "history";

	private string m_Path;

	private StreamWriter m_Writer;

	private bool m_DisableWrite;

	private readonly object m_Lock = new object();

	public bool DisableWrite
	{
		get
		{
			lock (m_Lock)
			{
				return m_DisableWrite;
			}
		}
		set
		{
			lock (m_Lock)
			{
				m_DisableWrite = value;
			}
		}
	}

	public GameHistoryFile(string path)
	{
		m_Path = Path.Combine(path, "history");
	}

	public void Append(string message)
	{
		lock (m_Lock)
		{
			if (!m_DisableWrite && m_Writer != null)
			{
				m_Writer.WriteLine(message);
			}
		}
	}

	public void Close()
	{
		lock (m_Lock)
		{
			m_Writer?.Close();
			m_Writer = null;
		}
	}

	public void Open()
	{
		lock (m_Lock)
		{
			FileStream stream = new FileStream(m_Path, FileMode.Append, FileAccess.Write, FileShare.Read, ISaver.BuffersSize, FileOptions.SequentialScan);
			m_Writer = new StreamWriter(stream, ISaver.UTF8NoBom, ISaver.BuffersSize);
		}
	}
}
