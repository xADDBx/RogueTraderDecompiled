using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Kingmaker.EntitySystem.Persistence;

public class ZipSaver : ISaver, IDisposable
{
	private string m_FileName;

	private ZipArchive m_ZipFile;

	private ISaver.Mode m_Mode;

	public string FileName => m_FileName;

	private ZipArchive ZipFile
	{
		get
		{
			if (m_ZipFile != null)
			{
				return m_ZipFile;
			}
			if (m_Mode == ISaver.Mode.None)
			{
				return null;
			}
			FileStream fileStream = File.Open(m_FileName, (m_Mode == ISaver.Mode.Read) ? FileMode.Open : FileMode.OpenOrCreate, (m_Mode == ISaver.Mode.Read) ? FileAccess.Read : FileAccess.ReadWrite, (m_Mode == ISaver.Mode.Read) ? FileShare.Read : FileShare.None);
			try
			{
				m_ZipFile = new ZipArchive(fileStream, (m_Mode != ISaver.Mode.Read) ? ZipArchiveMode.Update : ZipArchiveMode.Read);
			}
			catch (Exception)
			{
				fileStream?.Dispose();
				throw;
			}
			return m_ZipFile;
		}
	}

	public ZipSaver(string fileName, ISaver.Mode mode = ISaver.Mode.None)
	{
		m_FileName = fileName;
		m_Mode = mode;
	}

	public string ReadHeader()
	{
		return ReadJson("header");
	}

	public string ReadJson(string name)
	{
		ZipArchiveEntry zipArchiveEntry = FindEntry(name + ".json");
		if (zipArchiveEntry == null)
		{
			return null;
		}
		using StreamReader streamReader = new StreamReader(zipArchiveEntry.Open());
		return streamReader.ReadToEnd();
	}

	public byte[] ReadBytes(string name)
	{
		ZipArchiveEntry zipArchiveEntry = FindEntry(name);
		if (zipArchiveEntry == null)
		{
			return null;
		}
		using Stream stream = zipArchiveEntry.Open();
		using MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		memoryStream.Position = 0L;
		return memoryStream.ToArray();
	}

	public async Task<byte[]> ReadBytesAsync(string name)
	{
		ZipArchiveEntry zipArchiveEntry = FindEntry(name);
		if (zipArchiveEntry == null)
		{
			return null;
		}
		byte[] result;
		await using (Stream stream = zipArchiveEntry.Open())
		{
			using MemoryStream ms = new MemoryStream();
			await stream.CopyToAsync(ms);
			ms.Position = 0L;
			result = ms.ToArray();
		}
		return result;
	}

	public void SaveJson(string name, string json)
	{
		name += ".json";
		ZipArchiveEntry zipArchiveEntry = FindEntry(name);
		if (zipArchiveEntry == null)
		{
			zipArchiveEntry = ZipFile.CreateEntry(name);
		}
		using Stream stream = zipArchiveEntry.Open();
		using StreamWriter streamWriter = new StreamWriter(stream);
		streamWriter.Write(json);
		stream.SetLength(stream.Position);
	}

	public void SaveBytes(string name, byte[] bytes)
	{
		ZipArchiveEntry zipArchiveEntry = FindEntry(name);
		if (zipArchiveEntry == null)
		{
			zipArchiveEntry = ZipFile.CreateEntry(name);
		}
		using Stream stream = zipArchiveEntry.Open();
		stream.Write(bytes);
		stream.SetLength(stream.Position);
	}

	public bool CopyFromStash(string fileName)
	{
		if (!AreaDataStash.Exists(fileName))
		{
			PFLog.Default.Warning("Cannot save file [" + fileName + "]: failed to find it in area data stash");
			return false;
		}
		using AreaDataStashFileAccessor areaDataStashFileAccessor = AreaDataStash.AccessFile(fileName);
		string fileName2 = Path.GetFileName(areaDataStashFileAccessor.Path);
		ZipFile.CreateEntryFromFile(areaDataStashFileAccessor.Path, fileName2);
		return true;
	}

	public void CopyToStash(string fileName)
	{
		using AreaDataStashFileAccessor areaDataStashFileAccessor = AreaDataStash.AccessFile(fileName);
		using FileStream destination = new FileStream(areaDataStashFileAccessor.Path, FileMode.Create);
		ZipFile.Entries.SingleOrDefault((ZipArchiveEntry v) => v.FullName == fileName)?.Open().CopyTo(destination);
	}

	public void Clear()
	{
		m_ZipFile?.Dispose();
		m_ZipFile = null;
		try
		{
			File.Delete(m_FileName);
		}
		catch (IOException ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	public void Save()
	{
		m_ZipFile?.Dispose();
		m_ZipFile = null;
	}

	public List<string> GetAllFiles()
	{
		return ZipFile.Entries.Select((ZipArchiveEntry s) => s.FullName).Distinct().ToList();
	}

	public void SetMode(ISaver.Mode mode)
	{
		if (m_Mode != mode)
		{
			m_ZipFile?.Dispose();
			m_ZipFile = null;
			m_Mode = mode;
		}
	}

	public ISaver Clone()
	{
		if (m_Mode == ISaver.Mode.Write)
		{
			throw new Exception("Cant clone saver in write mode");
		}
		return new ZipSaver(m_FileName, m_Mode);
	}

	public void Dispose()
	{
		m_ZipFile?.Dispose();
		m_ZipFile = null;
	}

	public ulong GetFileSize()
	{
		return (ulong)new FileInfo(m_FileName).Length;
	}

	public void MoveTo(string newFileName)
	{
		if (File.Exists(newFileName))
		{
			File.Delete(newFileName);
		}
		File.Move(m_FileName, newFileName);
		m_FileName = newFileName;
	}

	private ZipArchiveEntry FindEntry(string fullName)
	{
		return ZipFile.Entries.SingleOrDefault((ZipArchiveEntry v) => v.FullName == fullName);
	}
}
