using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Mono.Cecil.Cil;

[Serializable]
[ComVisible(false)]
public sealed class SymbolsNotFoundException : FileNotFoundException
{
	public SymbolsNotFoundException(string message)
		: base(message)
	{
	}

	private SymbolsNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
