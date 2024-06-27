using System.IO;
using System.Runtime.InteropServices;
using Mono.Cecil.PE;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public sealed class PortablePdbReaderProvider : ISymbolReaderProvider
{
	public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
	{
		Mixin.CheckModule(module);
		Mixin.CheckFileName(fileName);
		FileStream fileStream = File.OpenRead(Mixin.GetPdbFileName(fileName));
		return GetSymbolReader(module, Disposable.Owned((Stream)fileStream), fileStream.Name);
	}

	public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
	{
		Mixin.CheckModule(module);
		Mixin.CheckStream(symbolStream);
		return GetSymbolReader(module, Disposable.NotOwned(symbolStream), symbolStream.GetFileName());
	}

	private ISymbolReader GetSymbolReader(ModuleDefinition module, Disposable<Stream> symbolStream, string fileName)
	{
		uint pdb_heap_offset;
		return new PortablePdbReader(ImageReader.ReadPortablePdb(symbolStream, fileName, out pdb_heap_offset), module);
	}
}
