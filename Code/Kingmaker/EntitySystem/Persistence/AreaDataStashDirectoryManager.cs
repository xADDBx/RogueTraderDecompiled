using System.Diagnostics;
using System.IO;
using Kingmaker.Utility.UnityExtensions;

namespace Kingmaker.EntitySystem.Persistence;

internal class AreaDataStashDirectoryManager
{
	private const string LockFilename = "lock";

	private const string FolderPrefix = "Areas_";

	private const FileShare LockFileShare = FileShare.None;

	private const FileAccess LockFileAccess = FileAccess.ReadWrite;

	private bool m_IsClosed;

	private FileStream m_Lock;

	public string Folder { get; private set; }

	public GameHistoryFile GameHistoryFile { get; private set; }

	public void Init()
	{
		DeleteUnusedStashFolders();
		Folder = InitStashFolder();
		string path = Path.Combine(Folder, "lock");
		m_Lock = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		GameHistoryFile = new GameHistoryFile(Folder);
		GameHistoryFile.Open();
	}

	public AreaDataStashFileAccessor AccessFile(string filename)
	{
		return new AreaDataStashFileAccessor(Folder, filename, GameHistoryFile);
	}

	public bool Exists(string filename)
	{
		using (AccessFile(filename))
		{
			return File.Exists(Path.Combine(Folder, filename));
		}
	}

	public void CloseAndDelete()
	{
		if (!m_IsClosed)
		{
			GameHistoryFile.Close();
			m_Lock.Close();
			Directory.Delete(Folder, recursive: true);
			m_IsClosed = true;
		}
	}

	public void ClearDirectory()
	{
		GameHistoryFile.Close();
		string[] directories = Directory.GetDirectories(Folder);
		for (int i = 0; i < directories.Length; i++)
		{
			Directory.Delete(directories[i], recursive: true);
		}
		directories = Directory.GetFiles(Folder);
		foreach (string path in directories)
		{
			if (!(Path.GetFileName(path) == "lock"))
			{
				File.Delete(path);
			}
		}
		GameHistoryFile.Open();
	}

	private void DeleteUnusedStashFolders()
	{
		string[] directories = Directory.GetDirectories(ApplicationPaths.persistentDataPath, "Areas_*");
		foreach (string text in directories)
		{
			FileInfo fileInfo = new FileInfo(Path.Combine(text, "lock"));
			if (!fileInfo.Exists | fileInfo.CanAccess(FileAccess.ReadWrite, FileShare.None))
			{
				Directory.Delete(text, recursive: true);
			}
		}
	}

	private string InitStashFolder()
	{
		int id = Process.GetCurrentProcess().Id;
		string text = Path.Combine(ApplicationPaths.persistentDataPath, string.Format("{0}{1}", "Areas_", id));
		PFLog.System.Log("Set AreaDataStash folder to " + text);
		Directory.CreateDirectory(text);
		return text;
	}
}
