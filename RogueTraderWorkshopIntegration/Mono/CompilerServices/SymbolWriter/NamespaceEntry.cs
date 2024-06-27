using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public struct NamespaceEntry
{
	public readonly string Name;

	public readonly int Index;

	public readonly int Parent;

	public readonly string[] UsingClauses;

	public NamespaceEntry(string name, int index, string[] using_clauses, int parent)
	{
		Name = name;
		Index = index;
		Parent = parent;
		UsingClauses = ((using_clauses != null) ? using_clauses : new string[0]);
	}

	internal NamespaceEntry(MonoSymbolFile file, MyBinaryReader reader)
	{
		Name = reader.ReadString();
		Index = reader.ReadLeb128();
		Parent = reader.ReadLeb128();
		int num = reader.ReadLeb128();
		UsingClauses = new string[num];
		for (int i = 0; i < num; i++)
		{
			UsingClauses[i] = reader.ReadString();
		}
	}

	internal void Write(MonoSymbolFile file, MyBinaryWriter bw)
	{
		bw.Write(Name);
		bw.WriteLeb128(Index);
		bw.WriteLeb128(Parent);
		bw.WriteLeb128(UsingClauses.Length);
		string[] usingClauses = UsingClauses;
		foreach (string value in usingClauses)
		{
			bw.Write(value);
		}
	}

	public override string ToString()
	{
		return $"[Namespace {Name}:{Index}:{Parent}]";
	}
}
