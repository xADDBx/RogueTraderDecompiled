using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public class CompileUnitEntry : ICompileUnit
{
	public readonly int Index;

	private int DataOffset;

	private MonoSymbolFile file;

	private SourceFileEntry source;

	private List<SourceFileEntry> include_files;

	private List<NamespaceEntry> namespaces;

	private bool creating;

	public static int Size => 8;

	CompileUnitEntry ICompileUnit.Entry => this;

	public SourceFileEntry SourceFile
	{
		get
		{
			if (creating)
			{
				return source;
			}
			ReadData();
			return source;
		}
	}

	public NamespaceEntry[] Namespaces
	{
		get
		{
			ReadData();
			NamespaceEntry[] array = new NamespaceEntry[namespaces.Count];
			namespaces.CopyTo(array, 0);
			return array;
		}
	}

	public SourceFileEntry[] IncludeFiles
	{
		get
		{
			ReadData();
			if (include_files == null)
			{
				return new SourceFileEntry[0];
			}
			SourceFileEntry[] array = new SourceFileEntry[include_files.Count];
			include_files.CopyTo(array, 0);
			return array;
		}
	}

	public CompileUnitEntry(MonoSymbolFile file, SourceFileEntry source)
	{
		this.file = file;
		this.source = source;
		Index = file.AddCompileUnit(this);
		creating = true;
		namespaces = new List<NamespaceEntry>();
	}

	public void AddFile(SourceFileEntry file)
	{
		if (!creating)
		{
			throw new InvalidOperationException();
		}
		if (include_files == null)
		{
			include_files = new List<SourceFileEntry>();
		}
		include_files.Add(file);
	}

	public int DefineNamespace(string name, string[] using_clauses, int parent)
	{
		if (!creating)
		{
			throw new InvalidOperationException();
		}
		int nextNamespaceIndex = file.GetNextNamespaceIndex();
		NamespaceEntry item = new NamespaceEntry(name, nextNamespaceIndex, using_clauses, parent);
		namespaces.Add(item);
		return nextNamespaceIndex;
	}

	internal void WriteData(MyBinaryWriter bw)
	{
		DataOffset = (int)bw.BaseStream.Position;
		bw.WriteLeb128(source.Index);
		int value = ((include_files != null) ? include_files.Count : 0);
		bw.WriteLeb128(value);
		if (include_files != null)
		{
			foreach (SourceFileEntry include_file in include_files)
			{
				bw.WriteLeb128(include_file.Index);
			}
		}
		bw.WriteLeb128(namespaces.Count);
		foreach (NamespaceEntry @namespace in namespaces)
		{
			@namespace.Write(file, bw);
		}
	}

	internal void Write(BinaryWriter bw)
	{
		bw.Write(Index);
		bw.Write(DataOffset);
	}

	internal CompileUnitEntry(MonoSymbolFile file, MyBinaryReader reader)
	{
		this.file = file;
		Index = reader.ReadInt32();
		DataOffset = reader.ReadInt32();
	}

	public void ReadAll()
	{
		ReadData();
	}

	private void ReadData()
	{
		if (creating)
		{
			throw new InvalidOperationException();
		}
		lock (file)
		{
			if (namespaces != null)
			{
				return;
			}
			MyBinaryReader binaryReader = file.BinaryReader;
			int num = (int)binaryReader.BaseStream.Position;
			binaryReader.BaseStream.Position = DataOffset;
			int index = binaryReader.ReadLeb128();
			source = file.GetSourceFile(index);
			int num2 = binaryReader.ReadLeb128();
			if (num2 > 0)
			{
				include_files = new List<SourceFileEntry>();
				for (int i = 0; i < num2; i++)
				{
					include_files.Add(file.GetSourceFile(binaryReader.ReadLeb128()));
				}
			}
			int num3 = binaryReader.ReadLeb128();
			namespaces = new List<NamespaceEntry>();
			for (int j = 0; j < num3; j++)
			{
				namespaces.Add(new NamespaceEntry(file, binaryReader));
			}
			binaryReader.BaseStream.Position = num;
		}
	}
}
