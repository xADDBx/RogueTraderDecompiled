using System;

namespace Mono.Cecil.Pdb;

internal class SymDocumentWriter
{
	private readonly ISymUnmanagedDocumentWriter writer;

	public ISymUnmanagedDocumentWriter Writer => writer;

	public SymDocumentWriter(ISymUnmanagedDocumentWriter writer)
	{
		this.writer = writer;
	}

	public void SetSource(byte[] source)
	{
		writer.SetSource((uint)source.Length, source);
	}

	public void SetCheckSum(Guid hashAlgo, byte[] checkSum)
	{
		writer.SetCheckSum(hashAlgo, (uint)checkSum.Length, checkSum);
	}
}
