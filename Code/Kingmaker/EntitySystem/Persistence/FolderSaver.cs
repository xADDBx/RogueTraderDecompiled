using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Persistence;

public class FolderSaver : ISaver, IDisposable
{
	private string m_FolderName;

	public FolderSaver(string folderName)
	{
		m_FolderName = folderName;
	}

	public string ReadHeader()
	{
		string text = ReadJson("header");
		if (text == null)
		{
			PFLog.Default.Warning("Save folder {0} looks broken: no header file", m_FolderName);
		}
		return text;
	}

	public string ReadJson(string name)
	{
		string path = Path.Combine(m_FolderName, name + ".json");
		if (File.Exists(path))
		{
			return File.ReadAllText(path);
		}
		return null;
	}

	public byte[] ReadBytes(string name)
	{
		string path = Path.Combine(m_FolderName, name);
		if (File.Exists(path))
		{
			return File.ReadAllBytes(path);
		}
		return null;
	}

	public Task<byte[]> ReadBytesAsync(string name)
	{
		string path = Path.Combine(m_FolderName, name);
		if (File.Exists(path))
		{
			return File.ReadAllBytesAsync(path);
		}
		return null;
	}

	public void SaveJson(string name, string json)
	{
		using StreamWriter streamWriter = new StreamWriter(Path.Combine(m_FolderName, name + ".json"));
		streamWriter.Write(json);
	}

	public void SaveBytes(string name, byte[] bytes)
	{
		using FileStream fileStream = new FileStream(Path.Combine(m_FolderName, name), FileMode.Create);
		fileStream.Write(bytes, 0, bytes.Length);
	}

	public bool CopyFromStash(string fileName)
	{
		using AreaDataStashFileAccessor areaDataStashFileAccessor = AreaDataStash.AccessFile(fileName);
		File.Copy(areaDataStashFileAccessor.Path, Path.Combine(m_FolderName, fileName));
		return true;
	}

	public void CopyToStash(string fileName)
	{
		using AreaDataStashFileAccessor areaDataStashFileAccessor = AreaDataStash.AccessFile(fileName);
		File.Copy(Path.Combine(m_FolderName, fileName), areaDataStashFileAccessor.Path, overwrite: true);
	}

	public void Clear()
	{
		if (!Directory.Exists(m_FolderName))
		{
			Directory.CreateDirectory(m_FolderName);
		}
		else
		{
			Directory.GetFiles(m_FolderName).ForEach(File.Delete);
		}
	}

	public void Save()
	{
	}

	public List<string> GetAllFiles()
	{
		return Directory.GetFiles(m_FolderName).Select(Path.GetFileName).ToList();
	}

	public void SetMode(ISaver.Mode mode)
	{
	}

	public ISaver Clone()
	{
		return new FolderSaver(m_FolderName);
	}

	public void Dispose()
	{
	}

	public void MoveTo(string newFolderName)
	{
		if (Directory.Exists(newFolderName))
		{
			Directory.Delete(newFolderName, recursive: true);
		}
		Directory.Move(m_FolderName, newFolderName);
		m_FolderName = newFolderName;
	}
}
