using System;
using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public class MonoSymbolFileException : Exception
{
	public MonoSymbolFileException()
	{
	}

	public MonoSymbolFileException(string message, params object[] args)
		: base(string.Format(message, args))
	{
	}

	public MonoSymbolFileException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
