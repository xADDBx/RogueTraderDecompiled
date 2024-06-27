using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public class AnonymousScopeEntry
{
	public readonly int ID;

	private List<CapturedVariable> captured_vars = new List<CapturedVariable>();

	private List<CapturedScope> captured_scopes = new List<CapturedScope>();

	public CapturedVariable[] CapturedVariables
	{
		get
		{
			CapturedVariable[] array = new CapturedVariable[captured_vars.Count];
			captured_vars.CopyTo(array, 0);
			return array;
		}
	}

	public CapturedScope[] CapturedScopes
	{
		get
		{
			CapturedScope[] array = new CapturedScope[captured_scopes.Count];
			captured_scopes.CopyTo(array, 0);
			return array;
		}
	}

	public AnonymousScopeEntry(int id)
	{
		ID = id;
	}

	internal AnonymousScopeEntry(MyBinaryReader reader)
	{
		ID = reader.ReadLeb128();
		int num = reader.ReadLeb128();
		for (int i = 0; i < num; i++)
		{
			captured_vars.Add(new CapturedVariable(reader));
		}
		int num2 = reader.ReadLeb128();
		for (int j = 0; j < num2; j++)
		{
			captured_scopes.Add(new CapturedScope(reader));
		}
	}

	internal void AddCapturedVariable(string name, string captured_name, CapturedVariable.CapturedKind kind)
	{
		captured_vars.Add(new CapturedVariable(name, captured_name, kind));
	}

	internal void AddCapturedScope(int scope, string captured_name)
	{
		captured_scopes.Add(new CapturedScope(scope, captured_name));
	}

	internal void Write(MyBinaryWriter bw)
	{
		bw.WriteLeb128(ID);
		bw.WriteLeb128(captured_vars.Count);
		foreach (CapturedVariable captured_var in captured_vars)
		{
			captured_var.Write(bw);
		}
		bw.WriteLeb128(captured_scopes.Count);
		foreach (CapturedScope captured_scope in captured_scopes)
		{
			captured_scope.Write(bw);
		}
	}

	public override string ToString()
	{
		return $"[AnonymousScope {ID}]";
	}
}
