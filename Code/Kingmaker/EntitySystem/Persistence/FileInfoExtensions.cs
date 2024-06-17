using System.IO;

namespace Kingmaker.EntitySystem.Persistence;

internal static class FileInfoExtensions
{
	public static bool CanAccess(this FileInfo file, FileAccess access, FileShare share)
	{
		try
		{
			using (file.Open(FileMode.Open, access, share))
			{
			}
		}
		catch (IOException)
		{
			return false;
		}
		return true;
	}
}
