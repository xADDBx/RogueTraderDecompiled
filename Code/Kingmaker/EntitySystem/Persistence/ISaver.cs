using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kingmaker.Utility.UnityExtensions;

namespace Kingmaker.EntitySystem.Persistence;

public interface ISaver : IDisposable
{
	public enum Mode
	{
		None,
		Read,
		Write,
		WriteOnly
	}

	string ReadHeader();

	string ReadJson(string name);

	byte[] ReadBytes(string name);

	Task<byte[]> ReadBytesAsync(string name);

	void SaveJson(string name, string json);

	void SaveBytes(string name, byte[] bytes);

	bool CopyFromStash(string fileName);

	void CopyToStash(string fileName);

	void Clear();

	void Save();

	List<string> GetAllFiles();

	void SetMode(Mode mode);

	ISaver Clone();

	void MoveTo(string newName);

	static string GetNewStagingFileName()
	{
		string text = ApplicationPaths.temporaryCachePath + "/save-staging.zip";
		try
		{
			if (File.Exists(text))
			{
				File.Delete(text);
			}
		}
		catch (IOException)
		{
			text = $"{ApplicationPaths.temporaryCachePath}/save-staging-{Guid.NewGuid():N}.zip";
		}
		return text;
	}
}
