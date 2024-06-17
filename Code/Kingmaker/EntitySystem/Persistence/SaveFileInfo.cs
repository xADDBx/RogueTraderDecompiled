using System;

namespace Kingmaker.EntitySystem.Persistence;

internal class SaveFileInfo : IComparable<SaveFileInfo>
{
	public int FileId;

	public string FileName;

	public int FileSize;

	public long FileTimestamp;

	public int CompareTo(SaveFileInfo other)
	{
		if (this == other)
		{
			return 0;
		}
		if (other == null)
		{
			return 1;
		}
		return FileTimestamp.CompareTo(other.FileTimestamp);
	}
}
