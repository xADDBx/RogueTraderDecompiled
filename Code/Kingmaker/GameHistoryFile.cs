using System.IO;

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
			m_Writer = new StreamWriter(m_Path, append: true);
		}
	}
}
