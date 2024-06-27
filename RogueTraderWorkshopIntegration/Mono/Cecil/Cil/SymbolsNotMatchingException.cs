using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Mono.Cecil.Cil;

[Serializable]
[ComVisible(false)]
public sealed class SymbolsNotMatchingException : InvalidOperationException
{
	public SymbolsNotMatchingException(string message)
		: base(message)
	{
	}

	private SymbolsNotMatchingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
