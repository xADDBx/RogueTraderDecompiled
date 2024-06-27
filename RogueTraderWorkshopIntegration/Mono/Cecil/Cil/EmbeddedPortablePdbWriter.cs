using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Mono.Cecil.PE;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public sealed class EmbeddedPortablePdbWriter : ISymbolWriter, IDisposable
{
	private readonly Stream stream;

	private readonly PortablePdbWriter writer;

	internal EmbeddedPortablePdbWriter(Stream stream, PortablePdbWriter writer)
	{
		this.stream = stream;
		this.writer = writer;
	}

	public ISymbolReaderProvider GetReaderProvider()
	{
		return new EmbeddedPortablePdbReaderProvider();
	}

	public void Write(MethodDebugInformation info)
	{
		writer.Write(info);
	}

	public ImageDebugHeader GetDebugHeader()
	{
		ImageDebugHeader debugHeader = writer.GetDebugHeader();
		ImageDebugDirectory imageDebugDirectory = default(ImageDebugDirectory);
		imageDebugDirectory.Type = ImageDebugType.EmbeddedPortablePdb;
		imageDebugDirectory.MajorVersion = 256;
		imageDebugDirectory.MinorVersion = 256;
		ImageDebugDirectory directory = imageDebugDirectory;
		MemoryStream memoryStream = new MemoryStream();
		BinaryStreamWriter binaryStreamWriter = new BinaryStreamWriter(memoryStream);
		binaryStreamWriter.WriteByte(77);
		binaryStreamWriter.WriteByte(80);
		binaryStreamWriter.WriteByte(68);
		binaryStreamWriter.WriteByte(66);
		binaryStreamWriter.WriteInt32((int)stream.Length);
		stream.Position = 0L;
		using (DeflateStream destination = new DeflateStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
		{
			stream.CopyTo(destination);
		}
		directory.SizeOfData = (int)memoryStream.Length;
		ImageDebugHeaderEntry[] array = new ImageDebugHeaderEntry[debugHeader.Entries.Length + 1];
		for (int i = 0; i < debugHeader.Entries.Length; i++)
		{
			array[i] = debugHeader.Entries[i];
		}
		array[^1] = new ImageDebugHeaderEntry(directory, memoryStream.ToArray());
		return new ImageDebugHeader(array);
	}

	public void Write()
	{
		writer.Write();
	}

	public void Dispose()
	{
		writer.Dispose();
	}
}
