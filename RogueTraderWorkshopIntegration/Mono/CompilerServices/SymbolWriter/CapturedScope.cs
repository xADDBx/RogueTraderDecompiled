using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public struct CapturedScope
{
	public readonly int Scope;

	public readonly string CapturedName;

	public CapturedScope(int scope, string captured_name)
	{
		Scope = scope;
		CapturedName = captured_name;
	}

	internal CapturedScope(MyBinaryReader reader)
	{
		Scope = reader.ReadLeb128();
		CapturedName = reader.ReadString();
	}

	internal void Write(MyBinaryWriter bw)
	{
		bw.WriteLeb128(Scope);
		bw.Write(CapturedName);
	}

	public override string ToString()
	{
		return $"[CapturedScope {Scope}:{CapturedName}]";
	}
}
