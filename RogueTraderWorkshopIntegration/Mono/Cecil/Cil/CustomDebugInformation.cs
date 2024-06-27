using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public abstract class CustomDebugInformation : DebugInformation
{
	private Guid identifier;

	public Guid Identifier => identifier;

	public abstract CustomDebugInformationKind Kind { get; }

	internal CustomDebugInformation(Guid identifier)
	{
		this.identifier = identifier;
		token = new MetadataToken(TokenType.CustomDebugInformation);
	}
}
