using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public struct ScopeVariable
{
	public readonly int Scope;

	public readonly int Index;

	public ScopeVariable(int scope, int index)
	{
		Scope = scope;
		Index = index;
	}

	internal ScopeVariable(MyBinaryReader reader)
	{
		Scope = reader.ReadLeb128();
		Index = reader.ReadLeb128();
	}

	internal void Write(MyBinaryWriter bw)
	{
		bw.WriteLeb128(Scope);
		bw.WriteLeb128(Index);
	}

	public override string ToString()
	{
		return $"[ScopeVariable {Scope}:{Index}]";
	}
}
