using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public class MonoSymbolFile : IDisposable
{
	private List<MethodEntry> methods = new List<MethodEntry>();

	private List<SourceFileEntry> sources = new List<SourceFileEntry>();

	private List<CompileUnitEntry> comp_units = new List<CompileUnitEntry>();

	private Dictionary<int, AnonymousScopeEntry> anonymous_scopes;

	private OffsetTable ot;

	private int last_type_index;

	private int last_method_index;

	private int last_namespace_index;

	public readonly int MajorVersion = 50;

	public readonly int MinorVersion;

	public int NumLineNumbers;

	private MyBinaryReader reader;

	private Dictionary<int, SourceFileEntry> source_file_hash;

	private Dictionary<int, CompileUnitEntry> compile_unit_hash;

	private List<MethodEntry> method_list;

	private Dictionary<int, MethodEntry> method_token_hash;

	private Dictionary<string, int> source_name_hash;

	private Guid guid;

	internal int LineNumberCount;

	internal int LocalCount;

	internal int StringSize;

	internal int LineNumberSize;

	internal int ExtendedLineNumberSize;

	public int CompileUnitCount => ot.CompileUnitCount;

	public int SourceCount => ot.SourceCount;

	public int MethodCount => ot.MethodCount;

	public int TypeCount => ot.TypeCount;

	public int AnonymousScopeCount => ot.AnonymousScopeCount;

	public int NamespaceCount => last_namespace_index;

	public Guid Guid => guid;

	public OffsetTable OffsetTable => ot;

	public SourceFileEntry[] Sources
	{
		get
		{
			if (reader == null)
			{
				throw new InvalidOperationException();
			}
			SourceFileEntry[] array = new SourceFileEntry[SourceCount];
			for (int i = 0; i < SourceCount; i++)
			{
				array[i] = GetSourceFile(i + 1);
			}
			return array;
		}
	}

	public CompileUnitEntry[] CompileUnits
	{
		get
		{
			if (reader == null)
			{
				throw new InvalidOperationException();
			}
			CompileUnitEntry[] array = new CompileUnitEntry[CompileUnitCount];
			for (int i = 0; i < CompileUnitCount; i++)
			{
				array[i] = GetCompileUnit(i + 1);
			}
			return array;
		}
	}

	public MethodEntry[] Methods
	{
		get
		{
			if (reader == null)
			{
				throw new InvalidOperationException();
			}
			lock (this)
			{
				read_methods();
				MethodEntry[] array = new MethodEntry[MethodCount];
				method_list.CopyTo(array, 0);
				return array;
			}
		}
	}

	internal MyBinaryReader BinaryReader
	{
		get
		{
			if (reader == null)
			{
				throw new InvalidOperationException();
			}
			return reader;
		}
	}

	public MonoSymbolFile()
	{
		ot = new OffsetTable();
	}

	public int AddSource(SourceFileEntry source)
	{
		sources.Add(source);
		return sources.Count;
	}

	public int AddCompileUnit(CompileUnitEntry entry)
	{
		comp_units.Add(entry);
		return comp_units.Count;
	}

	public void AddMethod(MethodEntry entry)
	{
		methods.Add(entry);
	}

	public MethodEntry DefineMethod(CompileUnitEntry comp_unit, int token, ScopeVariable[] scope_vars, LocalVariableEntry[] locals, LineNumberEntry[] lines, CodeBlockEntry[] code_blocks, string real_name, MethodEntry.Flags flags, int namespace_id)
	{
		if (reader != null)
		{
			throw new InvalidOperationException();
		}
		MethodEntry methodEntry = new MethodEntry(this, comp_unit, token, scope_vars, locals, lines, code_blocks, real_name, flags, namespace_id);
		AddMethod(methodEntry);
		return methodEntry;
	}

	internal void DefineAnonymousScope(int id)
	{
		if (reader != null)
		{
			throw new InvalidOperationException();
		}
		if (anonymous_scopes == null)
		{
			anonymous_scopes = new Dictionary<int, AnonymousScopeEntry>();
		}
		anonymous_scopes.Add(id, new AnonymousScopeEntry(id));
	}

	internal void DefineCapturedVariable(int scope_id, string name, string captured_name, CapturedVariable.CapturedKind kind)
	{
		if (reader != null)
		{
			throw new InvalidOperationException();
		}
		anonymous_scopes[scope_id].AddCapturedVariable(name, captured_name, kind);
	}

	internal void DefineCapturedScope(int scope_id, int id, string captured_name)
	{
		if (reader != null)
		{
			throw new InvalidOperationException();
		}
		anonymous_scopes[scope_id].AddCapturedScope(id, captured_name);
	}

	internal int GetNextTypeIndex()
	{
		return ++last_type_index;
	}

	internal int GetNextMethodIndex()
	{
		return ++last_method_index;
	}

	internal int GetNextNamespaceIndex()
	{
		return ++last_namespace_index;
	}

	private void Write(MyBinaryWriter bw, Guid guid)
	{
		bw.Write(5037318119232611860L);
		bw.Write(MajorVersion);
		bw.Write(MinorVersion);
		bw.Write(guid.ToByteArray());
		long position = bw.BaseStream.Position;
		ot.Write(bw, MajorVersion, MinorVersion);
		methods.Sort();
		for (int i = 0; i < methods.Count; i++)
		{
			methods[i].Index = i + 1;
		}
		ot.DataSectionOffset = (int)bw.BaseStream.Position;
		foreach (SourceFileEntry source in sources)
		{
			source.WriteData(bw);
		}
		foreach (CompileUnitEntry comp_unit in comp_units)
		{
			comp_unit.WriteData(bw);
		}
		foreach (MethodEntry method in methods)
		{
			method.WriteData(this, bw);
		}
		ot.DataSectionSize = (int)bw.BaseStream.Position - ot.DataSectionOffset;
		ot.MethodTableOffset = (int)bw.BaseStream.Position;
		for (int j = 0; j < methods.Count; j++)
		{
			methods[j].Write(bw);
		}
		ot.MethodTableSize = (int)bw.BaseStream.Position - ot.MethodTableOffset;
		ot.SourceTableOffset = (int)bw.BaseStream.Position;
		for (int k = 0; k < sources.Count; k++)
		{
			sources[k].Write(bw);
		}
		ot.SourceTableSize = (int)bw.BaseStream.Position - ot.SourceTableOffset;
		ot.CompileUnitTableOffset = (int)bw.BaseStream.Position;
		for (int l = 0; l < comp_units.Count; l++)
		{
			comp_units[l].Write(bw);
		}
		ot.CompileUnitTableSize = (int)bw.BaseStream.Position - ot.CompileUnitTableOffset;
		ot.AnonymousScopeCount = ((anonymous_scopes != null) ? anonymous_scopes.Count : 0);
		ot.AnonymousScopeTableOffset = (int)bw.BaseStream.Position;
		if (anonymous_scopes != null)
		{
			foreach (AnonymousScopeEntry value in anonymous_scopes.Values)
			{
				value.Write(bw);
			}
		}
		ot.AnonymousScopeTableSize = (int)bw.BaseStream.Position - ot.AnonymousScopeTableOffset;
		ot.TypeCount = last_type_index;
		ot.MethodCount = methods.Count;
		ot.SourceCount = sources.Count;
		ot.CompileUnitCount = comp_units.Count;
		ot.TotalFileSize = (int)bw.BaseStream.Position;
		bw.Seek((int)position, SeekOrigin.Begin);
		ot.Write(bw, MajorVersion, MinorVersion);
		bw.Seek(0, SeekOrigin.End);
	}

	public void CreateSymbolFile(Guid guid, FileStream fs)
	{
		if (reader != null)
		{
			throw new InvalidOperationException();
		}
		Write(new MyBinaryWriter(fs), guid);
	}

	private MonoSymbolFile(Stream stream)
	{
		reader = new MyBinaryReader(stream);
		try
		{
			long num = reader.ReadInt64();
			int num2 = reader.ReadInt32();
			int num3 = reader.ReadInt32();
			if (num != 5037318119232611860L)
			{
				throw new MonoSymbolFileException("Symbol file is not a valid");
			}
			if (num2 != 50)
			{
				throw new MonoSymbolFileException("Symbol file has version {0} but expected {1}", num2, 50);
			}
			if (num3 != 0)
			{
				throw new MonoSymbolFileException("Symbol file has version {0}.{1} but expected {2}.{3}", num2, num3, 50, 0);
			}
			MajorVersion = num2;
			MinorVersion = num3;
			guid = new Guid(reader.ReadBytes(16));
			ot = new OffsetTable(reader, num2, num3);
		}
		catch (Exception innerException)
		{
			throw new MonoSymbolFileException("Cannot read symbol file", innerException);
		}
		source_file_hash = new Dictionary<int, SourceFileEntry>();
		compile_unit_hash = new Dictionary<int, CompileUnitEntry>();
	}

	public static MonoSymbolFile ReadSymbolFile(Assembly assembly)
	{
		string mdbFilename = assembly.Location + ".mdb";
		Guid moduleVersionId = assembly.GetModules()[0].ModuleVersionId;
		return ReadSymbolFile(mdbFilename, moduleVersionId);
	}

	public static MonoSymbolFile ReadSymbolFile(string mdbFilename)
	{
		return ReadSymbolFile(new FileStream(mdbFilename, FileMode.Open, FileAccess.Read));
	}

	public static MonoSymbolFile ReadSymbolFile(string mdbFilename, Guid assemblyGuid)
	{
		MonoSymbolFile monoSymbolFile = ReadSymbolFile(mdbFilename);
		if (assemblyGuid != monoSymbolFile.guid)
		{
			throw new MonoSymbolFileException("Symbol file `{0}' does not match assembly", mdbFilename);
		}
		return monoSymbolFile;
	}

	public static MonoSymbolFile ReadSymbolFile(Stream stream)
	{
		return new MonoSymbolFile(stream);
	}

	public SourceFileEntry GetSourceFile(int index)
	{
		if (index < 1 || index > ot.SourceCount)
		{
			throw new ArgumentException();
		}
		if (reader == null)
		{
			throw new InvalidOperationException();
		}
		lock (this)
		{
			if (source_file_hash.TryGetValue(index, out var value))
			{
				return value;
			}
			long position = reader.BaseStream.Position;
			reader.BaseStream.Position = ot.SourceTableOffset + SourceFileEntry.Size * (index - 1);
			value = new SourceFileEntry(this, reader);
			source_file_hash.Add(index, value);
			reader.BaseStream.Position = position;
			return value;
		}
	}

	public CompileUnitEntry GetCompileUnit(int index)
	{
		if (index < 1 || index > ot.CompileUnitCount)
		{
			throw new ArgumentException();
		}
		if (reader == null)
		{
			throw new InvalidOperationException();
		}
		lock (this)
		{
			if (compile_unit_hash.TryGetValue(index, out var value))
			{
				return value;
			}
			long position = reader.BaseStream.Position;
			reader.BaseStream.Position = ot.CompileUnitTableOffset + CompileUnitEntry.Size * (index - 1);
			value = new CompileUnitEntry(this, reader);
			compile_unit_hash.Add(index, value);
			reader.BaseStream.Position = position;
			return value;
		}
	}

	private void read_methods()
	{
		lock (this)
		{
			if (method_token_hash == null)
			{
				method_token_hash = new Dictionary<int, MethodEntry>();
				method_list = new List<MethodEntry>();
				long position = reader.BaseStream.Position;
				reader.BaseStream.Position = ot.MethodTableOffset;
				for (int i = 0; i < MethodCount; i++)
				{
					MethodEntry methodEntry = new MethodEntry(this, reader, i + 1);
					method_token_hash.Add(methodEntry.Token, methodEntry);
					method_list.Add(methodEntry);
				}
				reader.BaseStream.Position = position;
			}
		}
	}

	public MethodEntry GetMethodByToken(int token)
	{
		if (reader == null)
		{
			throw new InvalidOperationException();
		}
		lock (this)
		{
			read_methods();
			method_token_hash.TryGetValue(token, out var value);
			return value;
		}
	}

	public MethodEntry GetMethod(int index)
	{
		if (index < 1 || index > ot.MethodCount)
		{
			throw new ArgumentException();
		}
		if (reader == null)
		{
			throw new InvalidOperationException();
		}
		lock (this)
		{
			read_methods();
			return method_list[index - 1];
		}
	}

	public int FindSource(string file_name)
	{
		if (reader == null)
		{
			throw new InvalidOperationException();
		}
		lock (this)
		{
			if (source_name_hash == null)
			{
				source_name_hash = new Dictionary<string, int>();
				for (int i = 0; i < ot.SourceCount; i++)
				{
					SourceFileEntry sourceFile = GetSourceFile(i + 1);
					source_name_hash.Add(sourceFile.FileName, i);
				}
			}
			if (!source_name_hash.TryGetValue(file_name, out var value))
			{
				return -1;
			}
			return value;
		}
	}

	public AnonymousScopeEntry GetAnonymousScope(int id)
	{
		if (reader == null)
		{
			throw new InvalidOperationException();
		}
		lock (this)
		{
			if (anonymous_scopes != null)
			{
				anonymous_scopes.TryGetValue(id, out var value);
				return value;
			}
			anonymous_scopes = new Dictionary<int, AnonymousScopeEntry>();
			reader.BaseStream.Position = ot.AnonymousScopeTableOffset;
			for (int i = 0; i < ot.AnonymousScopeCount; i++)
			{
				AnonymousScopeEntry value = new AnonymousScopeEntry(reader);
				anonymous_scopes.Add(value.ID, value);
			}
			return anonymous_scopes[id];
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && reader != null)
		{
			reader.Close();
			reader = null;
		}
	}
}
