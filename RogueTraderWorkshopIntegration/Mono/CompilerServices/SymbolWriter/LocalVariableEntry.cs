using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public struct LocalVariableEntry
{
	public readonly int Index;

	public readonly string Name;

	public readonly int BlockIndex;

	public LocalVariableEntry(int index, string name, int block)
	{
		Index = index;
		Name = name;
		BlockIndex = block;
	}

	internal LocalVariableEntry(MonoSymbolFile file, MyBinaryReader reader)
	{
		Index = reader.ReadLeb128();
		Name = reader.ReadString();
		BlockIndex = reader.ReadLeb128();
	}

	internal void Write(MonoSymbolFile file, MyBinaryWriter bw)
	{
		bw.WriteLeb128(Index);
		bw.Write(Name);
		bw.WriteLeb128(BlockIndex);
	}

	public override string ToString()
	{
		return $"[LocalVariable {Name}:{Index}:{BlockIndex - 1}]";
	}
}
