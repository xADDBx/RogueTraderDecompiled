using System.IO;
using System.Runtime.InteropServices;
using Mono.Cecil.Cil;

namespace Mono.Cecil.Pdb;

[ComVisible(false)]
public sealed class PdbWriterProvider : ISymbolWriterProvider
{
	public ISymbolWriter GetSymbolWriter(ModuleDefinition module, string fileName)
	{
		Mixin.CheckModule(module);
		Mixin.CheckFileName(fileName);
		if (HasPortablePdbSymbols(module))
		{
			return new PortablePdbWriterProvider().GetSymbolWriter(module, fileName);
		}
		return new NativePdbWriterProvider().GetSymbolWriter(module, fileName);
	}

	private static bool HasPortablePdbSymbols(ModuleDefinition module)
	{
		if (module.symbol_reader != null)
		{
			return module.symbol_reader is PortablePdbReader;
		}
		return false;
	}

	public ISymbolWriter GetSymbolWriter(ModuleDefinition module, Stream symbolStream)
	{
		Mixin.CheckModule(module);
		Mixin.CheckStream(symbolStream);
		Mixin.CheckReadSeek(symbolStream);
		if (HasPortablePdbSymbols(module))
		{
			return new PortablePdbWriterProvider().GetSymbolWriter(module, symbolStream);
		}
		return new NativePdbWriterProvider().GetSymbolWriter(module, symbolStream);
	}
}
