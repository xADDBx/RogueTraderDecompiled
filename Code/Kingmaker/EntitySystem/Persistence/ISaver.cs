using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kingmaker.EntitySystem.Persistence;

public interface ISaver : IDisposable
{
	public enum Mode
	{
		None,
		Read,
		Write
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
}
