using System;
using System.IO;
using System.Runtime.InteropServices;
using Mono.Cecil.PE;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public class DefaultSymbolReaderProvider : ISymbolReaderProvider
{
	private readonly bool throw_if_no_symbol;

	public DefaultSymbolReaderProvider()
		: this(throwIfNoSymbol: true)
	{
	}

	public DefaultSymbolReaderProvider(bool throwIfNoSymbol)
	{
		throw_if_no_symbol = throwIfNoSymbol;
	}

	public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
	{
		if (module.Image.HasDebugTables())
		{
			return null;
		}
		if (module.HasDebugHeader && module.GetDebugHeader().GetEmbeddedPortablePdbEntry() != null)
		{
			return new EmbeddedPortablePdbReaderProvider().GetSymbolReader(module, fileName);
		}
		if (File.Exists(Mixin.GetPdbFileName(fileName)))
		{
			if (Mixin.IsPortablePdb(Mixin.GetPdbFileName(fileName)))
			{
				return new PortablePdbReaderProvider().GetSymbolReader(module, fileName);
			}
			try
			{
				return SymbolProvider.GetReaderProvider(SymbolKind.NativePdb).GetSymbolReader(module, fileName);
			}
			catch (Exception)
			{
			}
		}
		if (File.Exists(Mixin.GetMdbFileName(fileName)))
		{
			try
			{
				return SymbolProvider.GetReaderProvider(SymbolKind.Mdb).GetSymbolReader(module, fileName);
			}
			catch (Exception)
			{
			}
		}
		if (throw_if_no_symbol)
		{
			throw new SymbolsNotFoundException($"No symbol found for file: {fileName}");
		}
		return null;
	}

	public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
	{
		if (module.Image.HasDebugTables())
		{
			return null;
		}
		if (module.HasDebugHeader && module.GetDebugHeader().GetEmbeddedPortablePdbEntry() != null)
		{
			return new EmbeddedPortablePdbReaderProvider().GetSymbolReader(module, "");
		}
		Mixin.CheckStream(symbolStream);
		Mixin.CheckReadSeek(symbolStream);
		long position = symbolStream.Position;
		BinaryStreamReader binaryStreamReader = new BinaryStreamReader(symbolStream);
		int num = binaryStreamReader.ReadInt32();
		symbolStream.Position = position;
		if (num == 1112167234)
		{
			return new PortablePdbReaderProvider().GetSymbolReader(module, symbolStream);
		}
		byte[] array = binaryStreamReader.ReadBytes("Microsoft C/C++ MSF 7.00".Length);
		symbolStream.Position = position;
		bool flag = true;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != (byte)"Microsoft C/C++ MSF 7.00"[i])
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			try
			{
				return SymbolProvider.GetReaderProvider(SymbolKind.NativePdb).GetSymbolReader(module, symbolStream);
			}
			catch (Exception)
			{
			}
		}
		long num2 = binaryStreamReader.ReadInt64();
		symbolStream.Position = position;
		if (num2 == 5037318119232611860L)
		{
			try
			{
				return SymbolProvider.GetReaderProvider(SymbolKind.Mdb).GetSymbolReader(module, symbolStream);
			}
			catch (Exception)
			{
			}
		}
		if (throw_if_no_symbol)
		{
			throw new SymbolsNotFoundException($"No symbols found in stream");
		}
		return null;
	}
}
