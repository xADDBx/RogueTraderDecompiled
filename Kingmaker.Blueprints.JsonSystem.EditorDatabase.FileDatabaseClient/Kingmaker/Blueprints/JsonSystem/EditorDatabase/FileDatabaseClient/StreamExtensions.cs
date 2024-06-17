using System;
using System.IO;
using System.Threading.Tasks;

namespace Kingmaker.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient;

public static class StreamExtensions
{
	public static int SureRead(this Stream stream, Span<byte> mem)
	{
		int num = 0;
		int num3;
		do
		{
			Span<byte> span = mem;
			int num2 = num;
			num3 = stream.Read(span.Slice(num2, span.Length - num2));
			num += num3;
		}
		while (num3 > 0 && num < mem.Length);
		if (num != mem.Length)
		{
			throw new IOException("Failed to read requested length");
		}
		return num;
	}

	public static async Task<int> SureReadAsync(this Stream stream, Memory<byte> mem)
	{
		int num = await stream.ReadAsync(mem);
		if (num != mem.Length)
		{
			throw new IOException("Failed to read requested length");
		}
		return num;
	}
}
