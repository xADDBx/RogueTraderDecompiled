using System.IO;
using System.Runtime.InteropServices;
using Mono.Cecil.PE;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public sealed class PortablePdbWriterProvider : ISymbolWriterProvider
{
	public ISymbolWriter GetSymbolWriter(ModuleDefinition module, string fileName)
	{
		Mixin.CheckModule(module);
		Mixin.CheckFileName(fileName);
		FileStream value = File.Open(Mixin.GetPdbFileName(fileName), FileMode.OpenOrCreate, FileAccess.ReadWrite);
		return GetSymbolWriter(module, Disposable.Owned((Stream)value), Disposable.NotOwned<Stream>(null));
	}

	public ISymbolWriter GetSymbolWriter(ModuleDefinition module, Stream symbolStream)
	{
		Mixin.CheckModule(module);
		Mixin.CheckStream(symbolStream);
		return GetSymbolWriter(module, Disposable.Owned((Stream)new MemoryStream()), Disposable.NotOwned(symbolStream));
	}

	private ISymbolWriter GetSymbolWriter(ModuleDefinition module, Disposable<Stream> stream, Disposable<Stream> final_stream)
	{
		MetadataBuilder metadataBuilder = new MetadataBuilder(module, this);
		ImageWriter writer = ImageWriter.CreateDebugWriter(module, metadataBuilder, stream);
		return new PortablePdbWriter(metadataBuilder, module, writer, final_stream);
	}
}
