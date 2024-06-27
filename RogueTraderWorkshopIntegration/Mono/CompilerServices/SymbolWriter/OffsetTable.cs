using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public class OffsetTable
{
	[Flags]
	public enum Flags
	{
		IsAspxSource = 1,
		WindowsFileNames = 2
	}

	public const int MajorVersion = 50;

	public const int MinorVersion = 0;

	public const long Magic = 5037318119232611860L;

	public int TotalFileSize;

	public int DataSectionOffset;

	public int DataSectionSize;

	public int CompileUnitCount;

	public int CompileUnitTableOffset;

	public int CompileUnitTableSize;

	public int SourceCount;

	public int SourceTableOffset;

	public int SourceTableSize;

	public int MethodCount;

	public int MethodTableOffset;

	public int MethodTableSize;

	public int TypeCount;

	public int AnonymousScopeCount;

	public int AnonymousScopeTableOffset;

	public int AnonymousScopeTableSize;

	public Flags FileFlags;

	public int LineNumberTable_LineBase = -1;

	public int LineNumberTable_LineRange = 8;

	public int LineNumberTable_OpcodeBase = 9;

	internal OffsetTable()
	{
		int platform = (int)Environment.OSVersion.Platform;
		if (platform != 4 && platform != 128)
		{
			FileFlags |= Flags.WindowsFileNames;
		}
	}

	internal OffsetTable(BinaryReader reader, int major_version, int minor_version)
	{
		TotalFileSize = reader.ReadInt32();
		DataSectionOffset = reader.ReadInt32();
		DataSectionSize = reader.ReadInt32();
		CompileUnitCount = reader.ReadInt32();
		CompileUnitTableOffset = reader.ReadInt32();
		CompileUnitTableSize = reader.ReadInt32();
		SourceCount = reader.ReadInt32();
		SourceTableOffset = reader.ReadInt32();
		SourceTableSize = reader.ReadInt32();
		MethodCount = reader.ReadInt32();
		MethodTableOffset = reader.ReadInt32();
		MethodTableSize = reader.ReadInt32();
		TypeCount = reader.ReadInt32();
		AnonymousScopeCount = reader.ReadInt32();
		AnonymousScopeTableOffset = reader.ReadInt32();
		AnonymousScopeTableSize = reader.ReadInt32();
		LineNumberTable_LineBase = reader.ReadInt32();
		LineNumberTable_LineRange = reader.ReadInt32();
		LineNumberTable_OpcodeBase = reader.ReadInt32();
		FileFlags = (Flags)reader.ReadInt32();
	}

	internal void Write(BinaryWriter bw, int major_version, int minor_version)
	{
		bw.Write(TotalFileSize);
		bw.Write(DataSectionOffset);
		bw.Write(DataSectionSize);
		bw.Write(CompileUnitCount);
		bw.Write(CompileUnitTableOffset);
		bw.Write(CompileUnitTableSize);
		bw.Write(SourceCount);
		bw.Write(SourceTableOffset);
		bw.Write(SourceTableSize);
		bw.Write(MethodCount);
		bw.Write(MethodTableOffset);
		bw.Write(MethodTableSize);
		bw.Write(TypeCount);
		bw.Write(AnonymousScopeCount);
		bw.Write(AnonymousScopeTableOffset);
		bw.Write(AnonymousScopeTableSize);
		bw.Write(LineNumberTable_LineBase);
		bw.Write(LineNumberTable_LineRange);
		bw.Write(LineNumberTable_OpcodeBase);
		bw.Write((int)FileFlags);
	}

	public override string ToString()
	{
		return $"OffsetTable [{TotalFileSize} - {DataSectionOffset}:{DataSectionSize} - {SourceCount}:{SourceTableOffset}:{SourceTableSize} - {MethodCount}:{MethodTableOffset}:{MethodTableSize} - {TypeCount}]";
	}
}
