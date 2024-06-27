using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public sealed class EmbeddedPortablePdbReader : ISymbolReader, IDisposable
{
	private readonly PortablePdbReader reader;

	internal EmbeddedPortablePdbReader(PortablePdbReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException();
		}
		this.reader = reader;
	}

	public ISymbolWriterProvider GetWriterProvider()
	{
		return new EmbeddedPortablePdbWriterProvider();
	}

	public bool ProcessDebugHeader(ImageDebugHeader header)
	{
		return reader.ProcessDebugHeader(header);
	}

	public MethodDebugInformation Read(MethodDefinition method)
	{
		return reader.Read(method);
	}

	public void Dispose()
	{
		reader.Dispose();
	}
}
