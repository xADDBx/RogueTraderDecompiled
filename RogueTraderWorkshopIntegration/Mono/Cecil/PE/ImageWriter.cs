using System;
using System.IO;
using Mono.Cecil.Cil;
using Mono.Cecil.Metadata;

namespace Mono.Cecil.PE;

internal sealed class ImageWriter : BinaryStreamWriter
{
	private readonly ModuleDefinition module;

	private readonly MetadataBuilder metadata;

	private readonly TextMap text_map;

	internal readonly Disposable<Stream> stream;

	private readonly string runtime_version;

	private ImageDebugHeader debug_header;

	private ByteBuffer win32_resources;

	private const uint pe_header_size = 152u;

	private const uint section_header_size = 40u;

	private const uint file_alignment = 512u;

	private const uint section_alignment = 8192u;

	private const ulong image_base = 4194304uL;

	internal const uint text_rva = 8192u;

	private readonly bool pe64;

	private readonly bool has_reloc;

	internal Section text;

	internal Section rsrc;

	internal Section reloc;

	private ushort sections;

	internal long debug_header_entries_position;

	private ImageWriter(ModuleDefinition module, string runtime_version, MetadataBuilder metadata, Disposable<Stream> stream, bool metadataOnly = false)
		: base(stream.value)
	{
		this.module = module;
		this.runtime_version = runtime_version;
		text_map = metadata.text_map;
		this.stream = stream;
		this.metadata = metadata;
		if (!metadataOnly)
		{
			pe64 = module.Architecture == TargetArchitecture.AMD64 || module.Architecture == TargetArchitecture.IA64 || module.Architecture == TargetArchitecture.ARM64;
			has_reloc = module.Architecture == TargetArchitecture.I386;
			GetDebugHeader();
			GetWin32Resources();
			BuildTextMap();
			sections = (ushort)((!has_reloc) ? 1u : 2u);
		}
	}

	private void GetDebugHeader()
	{
		ISymbolWriter symbol_writer = metadata.symbol_writer;
		if (symbol_writer != null)
		{
			debug_header = symbol_writer.GetDebugHeader();
		}
		if (module.HasDebugHeader && module.GetDebugHeader().GetDeterministicEntry() != null)
		{
			debug_header = debug_header.AddDeterministicEntry();
		}
	}

	private void GetWin32Resources()
	{
		if (!module.HasImage)
		{
			return;
		}
		DataDirectory win32Resources = module.Image.Win32Resources;
		uint size = win32Resources.Size;
		if (size != 0)
		{
			win32_resources = module.Image.GetReaderAt(win32Resources.VirtualAddress, size, (uint s, BinaryStreamReader reader) => new ByteBuffer(reader.ReadBytes((int)s)));
		}
	}

	public static ImageWriter CreateWriter(ModuleDefinition module, MetadataBuilder metadata, Disposable<Stream> stream)
	{
		ImageWriter imageWriter = new ImageWriter(module, module.runtime_version, metadata, stream);
		imageWriter.BuildSections();
		return imageWriter;
	}

	public static ImageWriter CreateDebugWriter(ModuleDefinition module, MetadataBuilder metadata, Disposable<Stream> stream)
	{
		ImageWriter imageWriter = new ImageWriter(module, "PDB v1.0", metadata, stream, metadataOnly: true);
		uint length = metadata.text_map.GetLength();
		imageWriter.text = new Section
		{
			SizeOfRawData = length,
			VirtualSize = length
		};
		return imageWriter;
	}

	private void BuildSections()
	{
		bool num = win32_resources != null;
		if (num)
		{
			sections++;
		}
		text = CreateSection(".text", text_map.GetLength(), null);
		Section previous = text;
		if (num)
		{
			rsrc = CreateSection(".rsrc", (uint)win32_resources.length, previous);
			PatchWin32Resources(win32_resources);
			previous = rsrc;
		}
		if (has_reloc)
		{
			reloc = CreateSection(".reloc", 12u, previous);
		}
	}

	private Section CreateSection(string name, uint size, Section previous)
	{
		return new Section
		{
			Name = name,
			VirtualAddress = ((previous != null) ? (previous.VirtualAddress + Align(previous.VirtualSize, 8192u)) : 8192u),
			VirtualSize = size,
			PointerToRawData = ((previous != null) ? (previous.PointerToRawData + previous.SizeOfRawData) : Align(GetHeaderSize(), 512u)),
			SizeOfRawData = Align(size, 512u)
		};
	}

	private static uint Align(uint value, uint align)
	{
		align--;
		return (value + align) & ~align;
	}

	private void WriteDOSHeader()
	{
		Write(new byte[128]
		{
			77, 90, 144, 0, 3, 0, 0, 0, 4, 0,
			0, 0, 255, 255, 0, 0, 184, 0, 0, 0,
			0, 0, 0, 0, 64, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			128, 0, 0, 0, 14, 31, 186, 14, 0, 180,
			9, 205, 33, 184, 1, 76, 205, 33, 84, 104,
			105, 115, 32, 112, 114, 111, 103, 114, 97, 109,
			32, 99, 97, 110, 110, 111, 116, 32, 98, 101,
			32, 114, 117, 110, 32, 105, 110, 32, 68, 79,
			83, 32, 109, 111, 100, 101, 46, 13, 13, 10,
			36, 0, 0, 0, 0, 0, 0, 0
		});
	}

	private ushort SizeOfOptionalHeader()
	{
		return (ushort)((!pe64) ? 224u : 240u);
	}

	private void WritePEFileHeader()
	{
		WriteUInt32(17744u);
		WriteUInt16((ushort)module.Architecture);
		WriteUInt16(sections);
		WriteUInt32(metadata.timestamp);
		WriteUInt32(0u);
		WriteUInt32(0u);
		WriteUInt16(SizeOfOptionalHeader());
		ushort num = (ushort)(2u | ((!pe64) ? 256u : 32u));
		if (module.Kind == ModuleKind.Dll || module.Kind == ModuleKind.NetModule)
		{
			num = (ushort)(num | 0x2000u);
		}
		if (module.Image != null && (module.Image.Characteristics & 0x20u) != 0)
		{
			num = (ushort)(num | 0x20u);
		}
		WriteUInt16(num);
	}

	private Section LastSection()
	{
		if (reloc != null)
		{
			return reloc;
		}
		if (rsrc != null)
		{
			return rsrc;
		}
		return text;
	}

	private void WriteOptionalHeaders()
	{
		WriteUInt16((ushort)((!pe64) ? 267u : 523u));
		WriteUInt16(module.linker_version);
		WriteUInt32(text.SizeOfRawData);
		WriteUInt32(((reloc != null) ? reloc.SizeOfRawData : 0) + ((rsrc != null) ? rsrc.SizeOfRawData : 0));
		WriteUInt32(0u);
		Range range = text_map.GetRange(TextSegment.StartupStub);
		WriteUInt32((range.Length != 0) ? range.Start : 0u);
		WriteUInt32(8192u);
		if (!pe64)
		{
			WriteUInt32(0u);
			WriteUInt32(4194304u);
		}
		else
		{
			WriteUInt64(4194304uL);
		}
		WriteUInt32(8192u);
		WriteUInt32(512u);
		WriteUInt16(4);
		WriteUInt16(0);
		WriteUInt16(0);
		WriteUInt16(0);
		WriteUInt16(module.subsystem_major);
		WriteUInt16(module.subsystem_minor);
		WriteUInt32(0u);
		Section section = LastSection();
		WriteUInt32(section.VirtualAddress + Align(section.VirtualSize, 8192u));
		WriteUInt32(text.PointerToRawData);
		WriteUInt32(0u);
		WriteUInt16(GetSubSystem());
		WriteUInt16((ushort)module.Characteristics);
		if (!pe64)
		{
			WriteUInt32(1048576u);
			WriteUInt32(4096u);
			WriteUInt32(1048576u);
			WriteUInt32(4096u);
		}
		else
		{
			WriteUInt64(4194304uL);
			WriteUInt64(16384uL);
			WriteUInt64(1048576uL);
			WriteUInt64(8192uL);
		}
		WriteUInt32(0u);
		WriteUInt32(16u);
		WriteZeroDataDirectory();
		WriteDataDirectory(text_map.GetDataDirectory(TextSegment.ImportDirectory));
		if (rsrc != null)
		{
			WriteUInt32(rsrc.VirtualAddress);
			WriteUInt32(rsrc.VirtualSize);
		}
		else
		{
			WriteZeroDataDirectory();
		}
		WriteZeroDataDirectory();
		WriteZeroDataDirectory();
		WriteUInt32((reloc != null) ? reloc.VirtualAddress : 0u);
		WriteUInt32((reloc != null) ? reloc.VirtualSize : 0u);
		if (text_map.GetLength(TextSegment.DebugDirectory) > 0)
		{
			WriteUInt32(text_map.GetRVA(TextSegment.DebugDirectory));
			WriteUInt32((uint)(debug_header.Entries.Length * 28));
		}
		else
		{
			WriteZeroDataDirectory();
		}
		WriteZeroDataDirectory();
		WriteZeroDataDirectory();
		WriteZeroDataDirectory();
		WriteZeroDataDirectory();
		WriteZeroDataDirectory();
		WriteDataDirectory(text_map.GetDataDirectory(TextSegment.ImportAddressTable));
		WriteZeroDataDirectory();
		WriteDataDirectory(text_map.GetDataDirectory(TextSegment.CLIHeader));
		WriteZeroDataDirectory();
	}

	private void WriteZeroDataDirectory()
	{
		WriteUInt32(0u);
		WriteUInt32(0u);
	}

	private ushort GetSubSystem()
	{
		switch (module.Kind)
		{
		case ModuleKind.Dll:
		case ModuleKind.Console:
		case ModuleKind.NetModule:
			return 3;
		case ModuleKind.Windows:
			return 2;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void WriteSectionHeaders()
	{
		WriteSection(text, 1610612768u);
		if (rsrc != null)
		{
			WriteSection(rsrc, 1073741888u);
		}
		if (reloc != null)
		{
			WriteSection(reloc, 1107296320u);
		}
	}

	private void WriteSection(Section section, uint characteristics)
	{
		byte[] array = new byte[8];
		string name = section.Name;
		for (int i = 0; i < name.Length; i++)
		{
			array[i] = (byte)name[i];
		}
		WriteBytes(array);
		WriteUInt32(section.VirtualSize);
		WriteUInt32(section.VirtualAddress);
		WriteUInt32(section.SizeOfRawData);
		WriteUInt32(section.PointerToRawData);
		WriteUInt32(0u);
		WriteUInt32(0u);
		WriteUInt16(0);
		WriteUInt16(0);
		WriteUInt32(characteristics);
	}

	private uint GetRVAFileOffset(Section section, uint rva)
	{
		return section.PointerToRawData + rva - section.VirtualAddress;
	}

	private void MoveTo(uint pointer)
	{
		BaseStream.Seek(pointer, SeekOrigin.Begin);
	}

	private void MoveToRVA(Section section, uint rva)
	{
		BaseStream.Seek(GetRVAFileOffset(section, rva), SeekOrigin.Begin);
	}

	internal void MoveToRVA(TextSegment segment)
	{
		MoveToRVA(text, text_map.GetRVA(segment));
	}

	private void WriteRVA(uint rva)
	{
		if (!pe64)
		{
			WriteUInt32(rva);
		}
		else
		{
			WriteUInt64(rva);
		}
	}

	private void PrepareSection(Section section)
	{
		MoveTo(section.PointerToRawData);
		if (section.SizeOfRawData <= 4096)
		{
			Write(new byte[section.SizeOfRawData]);
			MoveTo(section.PointerToRawData);
			return;
		}
		int i = 0;
		byte[] buffer = new byte[4096];
		int num;
		for (; i != section.SizeOfRawData; i += num)
		{
			num = System.Math.Min((int)section.SizeOfRawData - i, 4096);
			Write(buffer, 0, num);
		}
		MoveTo(section.PointerToRawData);
	}

	private void WriteText()
	{
		PrepareSection(text);
		if (has_reloc)
		{
			WriteRVA(text_map.GetRVA(TextSegment.ImportHintNameTable));
			WriteRVA(0u);
		}
		WriteUInt32(72u);
		WriteUInt16(2);
		WriteUInt16((ushort)((module.Runtime > TargetRuntime.Net_1_1) ? 5u : 0u));
		WriteUInt32(text_map.GetRVA(TextSegment.MetadataHeader));
		WriteUInt32(GetMetadataLength());
		WriteUInt32((uint)module.Attributes);
		WriteUInt32(metadata.entry_point.ToUInt32());
		WriteDataDirectory(text_map.GetDataDirectory(TextSegment.Resources));
		WriteDataDirectory(text_map.GetDataDirectory(TextSegment.StrongNameSignature));
		WriteZeroDataDirectory();
		WriteZeroDataDirectory();
		WriteZeroDataDirectory();
		WriteZeroDataDirectory();
		MoveToRVA(TextSegment.Code);
		WriteBuffer(metadata.code);
		MoveToRVA(TextSegment.Resources);
		WriteBuffer(metadata.resources);
		if (metadata.data.length > 0)
		{
			MoveToRVA(TextSegment.Data);
			WriteBuffer(metadata.data);
		}
		MoveToRVA(TextSegment.MetadataHeader);
		WriteMetadataHeader();
		WriteMetadata();
		if (text_map.GetLength(TextSegment.DebugDirectory) > 0)
		{
			MoveToRVA(TextSegment.DebugDirectory);
			WriteDebugDirectory();
		}
		if (has_reloc)
		{
			MoveToRVA(TextSegment.ImportDirectory);
			WriteImportDirectory();
			MoveToRVA(TextSegment.StartupStub);
			WriteStartupStub();
		}
	}

	private uint GetMetadataLength()
	{
		return text_map.GetRVA(TextSegment.DebugDirectory) - text_map.GetRVA(TextSegment.MetadataHeader);
	}

	public void WriteMetadataHeader()
	{
		WriteUInt32(1112167234u);
		WriteUInt16(1);
		WriteUInt16(1);
		WriteUInt32(0u);
		byte[] zeroTerminatedString = GetZeroTerminatedString(runtime_version);
		WriteUInt32((uint)zeroTerminatedString.Length);
		WriteBytes(zeroTerminatedString);
		WriteUInt16(0);
		WriteUInt16(GetStreamCount());
		uint offset = text_map.GetRVA(TextSegment.TableHeap) - text_map.GetRVA(TextSegment.MetadataHeader);
		WriteStreamHeader(ref offset, TextSegment.TableHeap, "#~");
		WriteStreamHeader(ref offset, TextSegment.StringHeap, "#Strings");
		WriteStreamHeader(ref offset, TextSegment.UserStringHeap, "#US");
		WriteStreamHeader(ref offset, TextSegment.GuidHeap, "#GUID");
		WriteStreamHeader(ref offset, TextSegment.BlobHeap, "#Blob");
		WriteStreamHeader(ref offset, TextSegment.PdbHeap, "#Pdb");
	}

	private ushort GetStreamCount()
	{
		return (ushort)((uint)(2 + ((!metadata.user_string_heap.IsEmpty) ? 1 : 0) + ((!metadata.guid_heap.IsEmpty) ? 1 : 0) + ((!metadata.blob_heap.IsEmpty) ? 1 : 0)) + ((metadata.pdb_heap != null) ? 1u : 0u));
	}

	private void WriteStreamHeader(ref uint offset, TextSegment heap, string name)
	{
		uint length = (uint)text_map.GetLength(heap);
		if (length != 0)
		{
			WriteUInt32(offset);
			WriteUInt32(length);
			WriteBytes(GetZeroTerminatedString(name));
			offset += length;
		}
	}

	private static int GetZeroTerminatedStringLength(string @string)
	{
		return (@string.Length + 1 + 3) & -4;
	}

	private static byte[] GetZeroTerminatedString(string @string)
	{
		return GetString(@string, GetZeroTerminatedStringLength(@string));
	}

	private static byte[] GetSimpleString(string @string)
	{
		return GetString(@string, @string.Length);
	}

	private static byte[] GetString(string @string, int length)
	{
		byte[] array = new byte[length];
		for (int i = 0; i < @string.Length; i++)
		{
			array[i] = (byte)@string[i];
		}
		return array;
	}

	public void WriteMetadata()
	{
		WriteHeap(TextSegment.TableHeap, metadata.table_heap);
		WriteHeap(TextSegment.StringHeap, metadata.string_heap);
		WriteHeap(TextSegment.UserStringHeap, metadata.user_string_heap);
		WriteHeap(TextSegment.GuidHeap, metadata.guid_heap);
		WriteHeap(TextSegment.BlobHeap, metadata.blob_heap);
		WriteHeap(TextSegment.PdbHeap, metadata.pdb_heap);
	}

	private void WriteHeap(TextSegment heap, HeapBuffer buffer)
	{
		if (buffer != null && !buffer.IsEmpty)
		{
			MoveToRVA(heap);
			WriteBuffer(buffer);
		}
	}

	private void WriteDebugDirectory()
	{
		int num = (int)BaseStream.Position + debug_header.Entries.Length * 28;
		for (int i = 0; i < debug_header.Entries.Length; i++)
		{
			ImageDebugHeaderEntry imageDebugHeaderEntry = debug_header.Entries[i];
			ImageDebugDirectory directory = imageDebugHeaderEntry.Directory;
			WriteInt32(directory.Characteristics);
			WriteInt32(directory.TimeDateStamp);
			WriteInt16(directory.MajorVersion);
			WriteInt16(directory.MinorVersion);
			WriteInt32((int)directory.Type);
			WriteInt32(directory.SizeOfData);
			WriteInt32(directory.AddressOfRawData);
			WriteInt32(num);
			num += imageDebugHeaderEntry.Data.Length;
		}
		debug_header_entries_position = BaseStream.Position;
		for (int j = 0; j < debug_header.Entries.Length; j++)
		{
			ImageDebugHeaderEntry imageDebugHeaderEntry2 = debug_header.Entries[j];
			WriteBytes(imageDebugHeaderEntry2.Data);
		}
	}

	private void WriteImportDirectory()
	{
		WriteUInt32(text_map.GetRVA(TextSegment.ImportDirectory) + 40);
		WriteUInt32(0u);
		WriteUInt32(0u);
		WriteUInt32(text_map.GetRVA(TextSegment.ImportHintNameTable) + 14);
		WriteUInt32(text_map.GetRVA(TextSegment.ImportAddressTable));
		Advance(20);
		WriteUInt32(text_map.GetRVA(TextSegment.ImportHintNameTable));
		MoveToRVA(TextSegment.ImportHintNameTable);
		WriteUInt16(0);
		WriteBytes(GetRuntimeMain());
		WriteByte(0);
		WriteBytes(GetSimpleString("mscoree.dll"));
		WriteUInt16(0);
	}

	private byte[] GetRuntimeMain()
	{
		if (module.Kind != 0 && module.Kind != ModuleKind.NetModule)
		{
			return GetSimpleString("_CorExeMain");
		}
		return GetSimpleString("_CorDllMain");
	}

	private void WriteStartupStub()
	{
		if (module.Architecture == TargetArchitecture.I386)
		{
			WriteUInt16(9727);
			WriteUInt32(4194304 + text_map.GetRVA(TextSegment.ImportAddressTable));
			return;
		}
		throw new NotSupportedException();
	}

	private void WriteRsrc()
	{
		PrepareSection(rsrc);
		WriteBuffer(win32_resources);
	}

	private void WriteReloc()
	{
		PrepareSection(reloc);
		uint rVA = text_map.GetRVA(TextSegment.StartupStub);
		rVA += (uint)((module.Architecture == TargetArchitecture.IA64) ? 32 : 2);
		uint num = rVA & 0xFFFFF000u;
		WriteUInt32(num);
		WriteUInt32(12u);
		if (module.Architecture == TargetArchitecture.I386)
		{
			WriteUInt32(12288 + rVA - num);
			return;
		}
		throw new NotSupportedException();
	}

	public void WriteImage()
	{
		WriteDOSHeader();
		WritePEFileHeader();
		WriteOptionalHeaders();
		WriteSectionHeaders();
		WriteText();
		if (rsrc != null)
		{
			WriteRsrc();
		}
		if (reloc != null)
		{
			WriteReloc();
		}
		Flush();
	}

	private void BuildTextMap()
	{
		TextMap textMap = text_map;
		textMap.AddMap(TextSegment.Code, metadata.code.length, (!pe64) ? 4 : 16);
		textMap.AddMap(TextSegment.Resources, metadata.resources.length, 8);
		textMap.AddMap(TextSegment.Data, metadata.data.length, metadata.data.BufferAlign);
		if (metadata.data.length > 0)
		{
			metadata.table_heap.FixupData(textMap.GetRVA(TextSegment.Data));
		}
		textMap.AddMap(TextSegment.StrongNameSignature, GetStrongNameLength(), 4);
		BuildMetadataTextMap();
		int length = 0;
		if (debug_header != null && debug_header.HasEntries)
		{
			int num = debug_header.Entries.Length * 28;
			int num2 = (int)textMap.GetNextRVA(TextSegment.BlobHeap) + num;
			int num3 = 0;
			for (int i = 0; i < debug_header.Entries.Length; i++)
			{
				ImageDebugHeaderEntry imageDebugHeaderEntry = debug_header.Entries[i];
				ImageDebugDirectory directory = imageDebugHeaderEntry.Directory;
				directory.AddressOfRawData = ((imageDebugHeaderEntry.Data.Length != 0) ? num2 : 0);
				imageDebugHeaderEntry.Directory = directory;
				num3 += imageDebugHeaderEntry.Data.Length;
				num2 += imageDebugHeaderEntry.Data.Length;
			}
			length = num + num3;
		}
		textMap.AddMap(TextSegment.DebugDirectory, length, 4);
		if (!has_reloc)
		{
			uint nextRVA = textMap.GetNextRVA(TextSegment.DebugDirectory);
			textMap.AddMap(TextSegment.ImportDirectory, new Range(nextRVA, 0u));
			textMap.AddMap(TextSegment.ImportHintNameTable, new Range(nextRVA, 0u));
			textMap.AddMap(TextSegment.StartupStub, new Range(nextRVA, 0u));
			return;
		}
		uint nextRVA2 = textMap.GetNextRVA(TextSegment.DebugDirectory);
		uint num4 = nextRVA2 + 48;
		num4 = (num4 + 15) & 0xFFFFFFF0u;
		uint num5 = num4 - nextRVA2 + 27;
		uint num6 = nextRVA2 + num5;
		num6 = ((module.Architecture == TargetArchitecture.IA64) ? ((num6 + 15) & 0xFFFFFFF0u) : (2 + ((num6 + 3) & 0xFFFFFFFCu)));
		textMap.AddMap(TextSegment.ImportDirectory, new Range(nextRVA2, num5));
		textMap.AddMap(TextSegment.ImportHintNameTable, new Range(num4, 0u));
		textMap.AddMap(TextSegment.StartupStub, new Range(num6, GetStartupStubLength()));
	}

	public void BuildMetadataTextMap()
	{
		TextMap textMap = text_map;
		textMap.AddMap(TextSegment.MetadataHeader, GetMetadataHeaderLength(module.RuntimeVersion));
		textMap.AddMap(TextSegment.TableHeap, metadata.table_heap.length, 4);
		textMap.AddMap(TextSegment.StringHeap, metadata.string_heap.length, 4);
		textMap.AddMap(TextSegment.UserStringHeap, (!metadata.user_string_heap.IsEmpty) ? metadata.user_string_heap.length : 0, 4);
		textMap.AddMap(TextSegment.GuidHeap, metadata.guid_heap.length, 4);
		textMap.AddMap(TextSegment.BlobHeap, (!metadata.blob_heap.IsEmpty) ? metadata.blob_heap.length : 0, 4);
		textMap.AddMap(TextSegment.PdbHeap, (metadata.pdb_heap != null) ? metadata.pdb_heap.length : 0, 4);
	}

	private uint GetStartupStubLength()
	{
		if (module.Architecture == TargetArchitecture.I386)
		{
			return 6u;
		}
		throw new NotSupportedException();
	}

	private int GetMetadataHeaderLength(string runtimeVersion)
	{
		return 20 + GetZeroTerminatedStringLength(runtimeVersion) + 12 + 20 + ((!metadata.user_string_heap.IsEmpty) ? 12 : 0) + 16 + ((!metadata.blob_heap.IsEmpty) ? 16 : 0) + ((metadata.pdb_heap != null) ? 16 : 0);
	}

	private int GetStrongNameLength()
	{
		if (module.kind == ModuleKind.NetModule || module.Assembly == null)
		{
			return 0;
		}
		byte[] publicKey = module.Assembly.Name.PublicKey;
		if (publicKey.IsNullOrEmpty())
		{
			return 0;
		}
		int num = publicKey.Length;
		if (num > 32)
		{
			return num - 32;
		}
		return 128;
	}

	public DataDirectory GetStrongNameSignatureDirectory()
	{
		return text_map.GetDataDirectory(TextSegment.StrongNameSignature);
	}

	public uint GetHeaderSize()
	{
		return (uint)(152 + SizeOfOptionalHeader() + sections * 40);
	}

	private void PatchWin32Resources(ByteBuffer resources)
	{
		PatchResourceDirectoryTable(resources);
	}

	private void PatchResourceDirectoryTable(ByteBuffer resources)
	{
		resources.Advance(12);
		int num = resources.ReadUInt16() + resources.ReadUInt16();
		for (int i = 0; i < num; i++)
		{
			PatchResourceDirectoryEntry(resources);
		}
	}

	private void PatchResourceDirectoryEntry(ByteBuffer resources)
	{
		resources.Advance(4);
		uint num = resources.ReadUInt32();
		int position = resources.position;
		resources.position = (int)(num & 0x7FFFFFFF);
		if ((num & 0x80000000u) != 0)
		{
			PatchResourceDirectoryTable(resources);
		}
		else
		{
			PatchResourceDataEntry(resources);
		}
		resources.position = position;
	}

	private void PatchResourceDataEntry(ByteBuffer resources)
	{
		uint num = resources.ReadUInt32();
		resources.position -= 4;
		resources.WriteUInt32(num - module.Image.Win32Resources.VirtualAddress + rsrc.VirtualAddress);
	}
}
