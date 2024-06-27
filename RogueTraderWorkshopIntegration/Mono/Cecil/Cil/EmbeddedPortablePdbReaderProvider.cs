using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Mono.Cecil.PE;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public sealed class EmbeddedPortablePdbReaderProvider : ISymbolReaderProvider
{
	public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
	{
		Mixin.CheckModule(module);
		ImageDebugHeaderEntry embeddedPortablePdbEntry = module.GetDebugHeader().GetEmbeddedPortablePdbEntry();
		if (embeddedPortablePdbEntry == null)
		{
			throw new InvalidOperationException();
		}
		return new EmbeddedPortablePdbReader((PortablePdbReader)new PortablePdbReaderProvider().GetSymbolReader(module, GetPortablePdbStream(embeddedPortablePdbEntry)));
	}

	private static Stream GetPortablePdbStream(ImageDebugHeaderEntry entry)
	{
		MemoryStream stream = new MemoryStream(entry.Data);
		BinaryStreamReader binaryStreamReader = new BinaryStreamReader(stream);
		binaryStreamReader.ReadInt32();
		MemoryStream memoryStream = new MemoryStream(binaryStreamReader.ReadInt32());
		using DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress, leaveOpen: true);
		deflateStream.CopyTo(memoryStream);
		return memoryStream;
	}

	public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
	{
		throw new NotSupportedException();
	}
}
