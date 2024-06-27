using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public class CodeBlockEntry
{
	public enum Type
	{
		Lexical = 1,
		CompilerGenerated,
		IteratorBody,
		IteratorDispatcher
	}

	public int Index;

	public int Parent;

	public Type BlockType;

	public int StartOffset;

	public int EndOffset;

	public CodeBlockEntry(int index, int parent, Type type, int start_offset)
	{
		Index = index;
		Parent = parent;
		BlockType = type;
		StartOffset = start_offset;
	}

	internal CodeBlockEntry(int index, MyBinaryReader reader)
	{
		Index = index;
		int num = reader.ReadLeb128();
		BlockType = (Type)(num & 0x3F);
		Parent = reader.ReadLeb128();
		StartOffset = reader.ReadLeb128();
		EndOffset = reader.ReadLeb128();
		if (((uint)num & 0x40u) != 0)
		{
			int num2 = reader.ReadInt16();
			reader.BaseStream.Position += num2;
		}
	}

	public void Close(int end_offset)
	{
		EndOffset = end_offset;
	}

	internal void Write(MyBinaryWriter bw)
	{
		bw.WriteLeb128((int)BlockType);
		bw.WriteLeb128(Parent);
		bw.WriteLeb128(StartOffset);
		bw.WriteLeb128(EndOffset);
	}

	public override string ToString()
	{
		return $"[CodeBlock {Index}:{Parent}:{BlockType}:{StartOffset}:{EndOffset}]";
	}
}
