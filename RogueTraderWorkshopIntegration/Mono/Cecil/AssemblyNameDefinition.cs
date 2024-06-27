using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public sealed class AssemblyNameDefinition : AssemblyNameReference
{
	public override byte[] Hash => Empty<byte>.Array;

	internal AssemblyNameDefinition()
	{
		token = new MetadataToken(TokenType.Assembly, 1);
	}

	public AssemblyNameDefinition(string name, Version version)
		: base(name, version)
	{
		token = new MetadataToken(TokenType.Assembly, 1);
	}
}
