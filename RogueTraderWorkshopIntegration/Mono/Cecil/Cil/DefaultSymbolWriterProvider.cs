using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public class DefaultSymbolWriterProvider : ISymbolWriterProvider
{
	public ISymbolWriter GetSymbolWriter(ModuleDefinition module, string fileName)
	{
		ISymbolReader symbolReader = module.SymbolReader;
		if (symbolReader == null)
		{
			throw new InvalidOperationException();
		}
		if (module.Image != null && module.Image.HasDebugTables())
		{
			return null;
		}
		return symbolReader.GetWriterProvider().GetSymbolWriter(module, fileName);
	}

	public ISymbolWriter GetSymbolWriter(ModuleDefinition module, Stream symbolStream)
	{
		throw new NotSupportedException();
	}
}
