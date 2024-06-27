using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public class SourceFileEntry
{
	public readonly int Index;

	private int DataOffset;

	private MonoSymbolFile file;

	private string file_name;

	private byte[] guid;

	private byte[] hash;

	private bool creating;

	private bool auto_generated;

	private readonly string sourceFile;

	public static int Size => 8;

	public byte[] Checksum => hash;

	public string FileName
	{
		get
		{
			return file_name;
		}
		set
		{
			file_name = value;
		}
	}

	public bool AutoGenerated => auto_generated;

	public SourceFileEntry(MonoSymbolFile file, string file_name)
	{
		this.file = file;
		this.file_name = file_name;
		Index = file.AddSource(this);
		creating = true;
	}

	public SourceFileEntry(MonoSymbolFile file, string sourceFile, byte[] guid, byte[] checksum)
		: this(file, sourceFile, sourceFile, guid, checksum)
	{
	}

	public SourceFileEntry(MonoSymbolFile file, string fileName, string sourceFile, byte[] guid, byte[] checksum)
		: this(file, fileName)
	{
		this.guid = guid;
		hash = checksum;
		this.sourceFile = sourceFile;
	}

	internal void WriteData(MyBinaryWriter bw)
	{
		DataOffset = (int)bw.BaseStream.Position;
		bw.Write(file_name);
		if (guid == null)
		{
			guid = new byte[16];
		}
		if (hash == null)
		{
			try
			{
				using FileStream inputStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
				MD5 mD = MD5.Create();
				hash = mD.ComputeHash(inputStream);
			}
			catch
			{
				hash = new byte[16];
			}
		}
		bw.Write(guid);
		bw.Write(hash);
		bw.Write((byte)(auto_generated ? 1u : 0u));
	}

	internal void Write(BinaryWriter bw)
	{
		bw.Write(Index);
		bw.Write(DataOffset);
	}

	internal SourceFileEntry(MonoSymbolFile file, MyBinaryReader reader)
	{
		this.file = file;
		Index = reader.ReadInt32();
		DataOffset = reader.ReadInt32();
		int num = (int)reader.BaseStream.Position;
		reader.BaseStream.Position = DataOffset;
		sourceFile = (file_name = reader.ReadString());
		guid = reader.ReadBytes(16);
		hash = reader.ReadBytes(16);
		auto_generated = reader.ReadByte() == 1;
		reader.BaseStream.Position = num;
	}

	public void SetAutoGenerated()
	{
		if (!creating)
		{
			throw new InvalidOperationException();
		}
		auto_generated = true;
		file.OffsetTable.FileFlags |= OffsetTable.Flags.IsAspxSource;
	}

	public bool CheckChecksum()
	{
		try
		{
			using FileStream inputStream = new FileStream(sourceFile, FileMode.Open);
			byte[] array = MD5.Create().ComputeHash(inputStream);
			for (int i = 0; i < 16; i++)
			{
				if (array[i] != hash[i])
				{
					return false;
				}
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public override string ToString()
	{
		return $"SourceFileEntry ({Index}:{DataOffset})";
	}
}