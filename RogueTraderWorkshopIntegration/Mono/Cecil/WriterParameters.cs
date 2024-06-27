using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Mono.Cecil.Cil;

namespace Mono.Cecil;

[ComVisible(false)]
public sealed class WriterParameters
{
	private uint? timestamp;

	private Stream symbol_stream;

	private ISymbolWriterProvider symbol_writer_provider;

	private bool write_symbols;

	private byte[] key_blob;

	private string key_container;

	private StrongNameKeyPair key_pair;

	public uint? Timestamp
	{
		get
		{
			return timestamp;
		}
		set
		{
			timestamp = value;
		}
	}

	public Stream SymbolStream
	{
		get
		{
			return symbol_stream;
		}
		set
		{
			symbol_stream = value;
		}
	}

	public ISymbolWriterProvider SymbolWriterProvider
	{
		get
		{
			return symbol_writer_provider;
		}
		set
		{
			symbol_writer_provider = value;
		}
	}

	public bool WriteSymbols
	{
		get
		{
			return write_symbols;
		}
		set
		{
			write_symbols = value;
		}
	}

	public bool HasStrongNameKey
	{
		get
		{
			if (key_pair == null && key_blob == null)
			{
				return key_container != null;
			}
			return true;
		}
	}

	public byte[] StrongNameKeyBlob
	{
		get
		{
			return key_blob;
		}
		set
		{
			key_blob = value;
		}
	}

	public string StrongNameKeyContainer
	{
		get
		{
			return key_container;
		}
		set
		{
			key_container = value;
		}
	}

	public StrongNameKeyPair StrongNameKeyPair
	{
		get
		{
			return key_pair;
		}
		set
		{
			key_pair = value;
		}
	}

	public bool DeterministicMvid { get; set; }
}
