using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public class MethodEntry : IComparable
{
	[Flags]
	public enum Flags
	{
		LocalNamesAmbiguous = 1,
		ColumnsInfoIncluded = 2,
		EndInfoIncluded = 4
	}

	public readonly int CompileUnitIndex;

	public readonly int Token;

	public readonly int NamespaceID;

	private int DataOffset;

	private int LocalVariableTableOffset;

	private int LineNumberTableOffset;

	private int CodeBlockTableOffset;

	private int ScopeVariableTableOffset;

	private int RealNameOffset;

	private Flags flags;

	private int index;

	public readonly CompileUnitEntry CompileUnit;

	private LocalVariableEntry[] locals;

	private CodeBlockEntry[] code_blocks;

	private ScopeVariable[] scope_vars;

	private LineNumberTable lnt;

	private string real_name;

	public readonly MonoSymbolFile SymbolFile;

	public const int Size = 12;

	public Flags MethodFlags => flags;

	public int Index
	{
		get
		{
			return index;
		}
		set
		{
			index = value;
		}
	}

	internal MethodEntry(MonoSymbolFile file, MyBinaryReader reader, int index)
	{
		SymbolFile = file;
		this.index = index;
		Token = reader.ReadInt32();
		DataOffset = reader.ReadInt32();
		LineNumberTableOffset = reader.ReadInt32();
		long position = reader.BaseStream.Position;
		reader.BaseStream.Position = DataOffset;
		CompileUnitIndex = reader.ReadLeb128();
		LocalVariableTableOffset = reader.ReadLeb128();
		NamespaceID = reader.ReadLeb128();
		CodeBlockTableOffset = reader.ReadLeb128();
		ScopeVariableTableOffset = reader.ReadLeb128();
		RealNameOffset = reader.ReadLeb128();
		flags = (Flags)reader.ReadLeb128();
		reader.BaseStream.Position = position;
		CompileUnit = file.GetCompileUnit(CompileUnitIndex);
	}

	internal MethodEntry(MonoSymbolFile file, CompileUnitEntry comp_unit, int token, ScopeVariable[] scope_vars, LocalVariableEntry[] locals, LineNumberEntry[] lines, CodeBlockEntry[] code_blocks, string real_name, Flags flags, int namespace_id)
	{
		SymbolFile = file;
		this.real_name = real_name;
		this.locals = locals;
		this.code_blocks = code_blocks;
		this.scope_vars = scope_vars;
		this.flags = flags;
		index = -1;
		Token = token;
		CompileUnitIndex = comp_unit.Index;
		CompileUnit = comp_unit;
		NamespaceID = namespace_id;
		CheckLineNumberTable(lines);
		lnt = new LineNumberTable(file, lines);
		file.NumLineNumbers += lines.Length;
		int num = ((locals != null) ? locals.Length : 0);
		if (num <= 32)
		{
			for (int i = 0; i < num; i++)
			{
				string name = locals[i].Name;
				for (int j = i + 1; j < num; j++)
				{
					if (locals[j].Name == name)
					{
						flags |= Flags.LocalNamesAmbiguous;
						return;
					}
				}
			}
			return;
		}
		Dictionary<string, LocalVariableEntry> dictionary = new Dictionary<string, LocalVariableEntry>();
		for (int k = 0; k < locals.Length; k++)
		{
			LocalVariableEntry value = locals[k];
			if (dictionary.ContainsKey(value.Name))
			{
				flags |= Flags.LocalNamesAmbiguous;
				break;
			}
			dictionary.Add(value.Name, value);
		}
	}

	private static void CheckLineNumberTable(LineNumberEntry[] line_numbers)
	{
		int num = -1;
		int num2 = -1;
		if (line_numbers == null)
		{
			return;
		}
		foreach (LineNumberEntry lineNumberEntry in line_numbers)
		{
			if (lineNumberEntry.Equals(LineNumberEntry.Null))
			{
				throw new MonoSymbolFileException();
			}
			if (lineNumberEntry.Offset < num)
			{
				throw new MonoSymbolFileException();
			}
			if (lineNumberEntry.Offset > num)
			{
				num2 = lineNumberEntry.Row;
				num = lineNumberEntry.Offset;
			}
			else if (lineNumberEntry.Row > num2)
			{
				num2 = lineNumberEntry.Row;
			}
		}
	}

	internal void Write(MyBinaryWriter bw)
	{
		if (index <= 0 || DataOffset == 0)
		{
			throw new InvalidOperationException();
		}
		bw.Write(Token);
		bw.Write(DataOffset);
		bw.Write(LineNumberTableOffset);
	}

	internal void WriteData(MonoSymbolFile file, MyBinaryWriter bw)
	{
		if (index <= 0)
		{
			throw new InvalidOperationException();
		}
		LocalVariableTableOffset = (int)bw.BaseStream.Position;
		int num = ((locals != null) ? locals.Length : 0);
		bw.WriteLeb128(num);
		for (int i = 0; i < num; i++)
		{
			locals[i].Write(file, bw);
		}
		file.LocalCount += num;
		CodeBlockTableOffset = (int)bw.BaseStream.Position;
		int num2 = ((code_blocks != null) ? code_blocks.Length : 0);
		bw.WriteLeb128(num2);
		for (int j = 0; j < num2; j++)
		{
			code_blocks[j].Write(bw);
		}
		ScopeVariableTableOffset = (int)bw.BaseStream.Position;
		int num3 = ((scope_vars != null) ? scope_vars.Length : 0);
		bw.WriteLeb128(num3);
		for (int k = 0; k < num3; k++)
		{
			scope_vars[k].Write(bw);
		}
		if (real_name != null)
		{
			RealNameOffset = (int)bw.BaseStream.Position;
			bw.Write(real_name);
		}
		LineNumberEntry[] lineNumbers = lnt.LineNumbers;
		foreach (LineNumberEntry lineNumberEntry in lineNumbers)
		{
			if (lineNumberEntry.EndRow != -1 || lineNumberEntry.EndColumn != -1)
			{
				flags |= Flags.EndInfoIncluded;
			}
		}
		LineNumberTableOffset = (int)bw.BaseStream.Position;
		lnt.Write(file, bw, (flags & Flags.ColumnsInfoIncluded) != 0, (flags & Flags.EndInfoIncluded) != 0);
		DataOffset = (int)bw.BaseStream.Position;
		bw.WriteLeb128(CompileUnitIndex);
		bw.WriteLeb128(LocalVariableTableOffset);
		bw.WriteLeb128(NamespaceID);
		bw.WriteLeb128(CodeBlockTableOffset);
		bw.WriteLeb128(ScopeVariableTableOffset);
		bw.WriteLeb128(RealNameOffset);
		bw.WriteLeb128((int)flags);
	}

	public void ReadAll()
	{
		GetLineNumberTable();
		GetLocals();
		GetCodeBlocks();
		GetScopeVariables();
		GetRealName();
	}

	public LineNumberTable GetLineNumberTable()
	{
		lock (SymbolFile)
		{
			if (lnt != null)
			{
				return lnt;
			}
			if (LineNumberTableOffset == 0)
			{
				return null;
			}
			MyBinaryReader binaryReader = SymbolFile.BinaryReader;
			long position = binaryReader.BaseStream.Position;
			binaryReader.BaseStream.Position = LineNumberTableOffset;
			lnt = LineNumberTable.Read(SymbolFile, binaryReader, (flags & Flags.ColumnsInfoIncluded) != 0, (flags & Flags.EndInfoIncluded) != 0);
			binaryReader.BaseStream.Position = position;
			return lnt;
		}
	}

	public LocalVariableEntry[] GetLocals()
	{
		lock (SymbolFile)
		{
			if (locals != null)
			{
				return locals;
			}
			if (LocalVariableTableOffset == 0)
			{
				return null;
			}
			MyBinaryReader binaryReader = SymbolFile.BinaryReader;
			long position = binaryReader.BaseStream.Position;
			binaryReader.BaseStream.Position = LocalVariableTableOffset;
			int num = binaryReader.ReadLeb128();
			locals = new LocalVariableEntry[num];
			for (int i = 0; i < num; i++)
			{
				locals[i] = new LocalVariableEntry(SymbolFile, binaryReader);
			}
			binaryReader.BaseStream.Position = position;
			return locals;
		}
	}

	public CodeBlockEntry[] GetCodeBlocks()
	{
		lock (SymbolFile)
		{
			if (code_blocks != null)
			{
				return code_blocks;
			}
			if (CodeBlockTableOffset == 0)
			{
				return null;
			}
			MyBinaryReader binaryReader = SymbolFile.BinaryReader;
			long position = binaryReader.BaseStream.Position;
			binaryReader.BaseStream.Position = CodeBlockTableOffset;
			int num = binaryReader.ReadLeb128();
			code_blocks = new CodeBlockEntry[num];
			for (int i = 0; i < num; i++)
			{
				code_blocks[i] = new CodeBlockEntry(i, binaryReader);
			}
			binaryReader.BaseStream.Position = position;
			return code_blocks;
		}
	}

	public ScopeVariable[] GetScopeVariables()
	{
		lock (SymbolFile)
		{
			if (scope_vars != null)
			{
				return scope_vars;
			}
			if (ScopeVariableTableOffset == 0)
			{
				return null;
			}
			MyBinaryReader binaryReader = SymbolFile.BinaryReader;
			long position = binaryReader.BaseStream.Position;
			binaryReader.BaseStream.Position = ScopeVariableTableOffset;
			int num = binaryReader.ReadLeb128();
			scope_vars = new ScopeVariable[num];
			for (int i = 0; i < num; i++)
			{
				scope_vars[i] = new ScopeVariable(binaryReader);
			}
			binaryReader.BaseStream.Position = position;
			return scope_vars;
		}
	}

	public string GetRealName()
	{
		lock (SymbolFile)
		{
			if (real_name != null)
			{
				return real_name;
			}
			if (RealNameOffset == 0)
			{
				return null;
			}
			real_name = SymbolFile.BinaryReader.ReadString(RealNameOffset);
			return real_name;
		}
	}

	public int CompareTo(object obj)
	{
		MethodEntry methodEntry = (MethodEntry)obj;
		if (methodEntry.Token < Token)
		{
			return 1;
		}
		if (methodEntry.Token > Token)
		{
			return -1;
		}
		return 0;
	}

	public override string ToString()
	{
		return $"[Method {index}:{Token:x}:{CompileUnitIndex}:{CompileUnit}]";
	}
}
