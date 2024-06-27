namespace Mono.Cecil.Metadata;

internal sealed class UserStringHeapBuffer : StringHeapBuffer
{
	public override uint GetStringIndex(string @string)
	{
		if (strings.TryGetValue(@string, out var value))
		{
			return value;
		}
		value = (uint)position;
		WriteString(@string);
		strings.Add(@string, value);
		return value;
	}

	protected override void WriteString(string @string)
	{
		WriteCompressedUInt32((uint)(@string.Length * 2 + 1));
		byte b = 0;
		foreach (char c in @string)
		{
			WriteUInt16(c);
			if (b != 1 && (c < ' ' || c > '~') && (c > '~' || (c >= '\u0001' && c <= '\b') || (c >= '\u000e' && c <= '\u001f') || c == '\'' || c == '-'))
			{
				b = 1;
			}
		}
		WriteByte(b);
	}
}
