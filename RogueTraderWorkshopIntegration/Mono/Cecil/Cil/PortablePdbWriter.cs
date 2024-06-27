using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil.Metadata;
using Mono.Cecil.PE;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public sealed class PortablePdbWriter : ISymbolWriter, IDisposable
{
	private readonly MetadataBuilder pdb_metadata;

	private readonly ModuleDefinition module;

	private readonly ImageWriter writer;

	private readonly Disposable<Stream> final_stream;

	private MetadataBuilder module_metadata;

	internal byte[] pdb_checksum;

	internal Guid pdb_id_guid;

	internal uint pdb_id_stamp;

	private bool IsEmbedded => writer == null;

	internal PortablePdbWriter(MetadataBuilder pdb_metadata, ModuleDefinition module)
	{
		this.pdb_metadata = pdb_metadata;
		this.module = module;
		module_metadata = module.metadata_builder;
		if (module_metadata != pdb_metadata)
		{
			this.pdb_metadata.metadata_builder = module_metadata;
		}
		pdb_metadata.AddCustomDebugInformations(module);
	}

	internal PortablePdbWriter(MetadataBuilder pdb_metadata, ModuleDefinition module, ImageWriter writer, Disposable<Stream> final_stream)
		: this(pdb_metadata, module)
	{
		this.writer = writer;
		this.final_stream = final_stream;
	}

	public ISymbolReaderProvider GetReaderProvider()
	{
		return new PortablePdbReaderProvider();
	}

	public void Write(MethodDebugInformation info)
	{
		CheckMethodDebugInformationTable();
		pdb_metadata.AddMethodDebugInformation(info);
	}

	public void Write()
	{
		if (!IsEmbedded)
		{
			WritePdbFile();
			if (final_stream.value != null)
			{
				writer.BaseStream.Seek(0L, SeekOrigin.Begin);
				byte[] buffer = new byte[8192];
				CryptoService.CopyStreamChunk(writer.BaseStream, final_stream.value, buffer, (int)writer.BaseStream.Length);
			}
		}
	}

	public ImageDebugHeader GetDebugHeader()
	{
		if (IsEmbedded)
		{
			return new ImageDebugHeader();
		}
		ImageDebugDirectory imageDebugDirectory = default(ImageDebugDirectory);
		imageDebugDirectory.MajorVersion = 256;
		imageDebugDirectory.MinorVersion = 20557;
		imageDebugDirectory.Type = ImageDebugType.CodeView;
		imageDebugDirectory.TimeDateStamp = (int)pdb_id_stamp;
		ImageDebugDirectory directory = imageDebugDirectory;
		ByteBuffer byteBuffer = new ByteBuffer();
		byteBuffer.WriteUInt32(1396986706u);
		byteBuffer.WriteBytes(pdb_id_guid.ToByteArray());
		byteBuffer.WriteUInt32(1u);
		string text = writer.BaseStream.GetFileName();
		if (string.IsNullOrEmpty(text))
		{
			text = module.Assembly.Name.Name + ".pdb";
		}
		byteBuffer.WriteBytes(Encoding.UTF8.GetBytes(text));
		byteBuffer.WriteByte(0);
		byte[] array = new byte[byteBuffer.length];
		Buffer.BlockCopy(byteBuffer.buffer, 0, array, 0, byteBuffer.length);
		directory.SizeOfData = array.Length;
		ImageDebugHeaderEntry imageDebugHeaderEntry = new ImageDebugHeaderEntry(directory, array);
		imageDebugDirectory = default(ImageDebugDirectory);
		imageDebugDirectory.MajorVersion = 1;
		imageDebugDirectory.MinorVersion = 0;
		imageDebugDirectory.Type = ImageDebugType.PdbChecksum;
		imageDebugDirectory.TimeDateStamp = 0;
		ImageDebugDirectory directory2 = imageDebugDirectory;
		ByteBuffer byteBuffer2 = new ByteBuffer();
		byteBuffer2.WriteBytes(Encoding.UTF8.GetBytes("SHA256"));
		byteBuffer2.WriteByte(0);
		byteBuffer2.WriteBytes(pdb_checksum);
		byte[] array2 = new byte[byteBuffer2.length];
		Buffer.BlockCopy(byteBuffer2.buffer, 0, array2, 0, byteBuffer2.length);
		directory2.SizeOfData = array2.Length;
		ImageDebugHeaderEntry imageDebugHeaderEntry2 = new ImageDebugHeaderEntry(directory2, array2);
		return new ImageDebugHeader(new ImageDebugHeaderEntry[2] { imageDebugHeaderEntry, imageDebugHeaderEntry2 });
	}

	private void CheckMethodDebugInformationTable()
	{
		MethodDebugInformationTable table = pdb_metadata.table_heap.GetTable<MethodDebugInformationTable>(Table.MethodDebugInformation);
		if (table.length <= 0)
		{
			table.rows = new Row<uint, uint>[module_metadata.method_rid - 1];
			table.length = table.rows.Length;
		}
	}

	public void Dispose()
	{
		writer.stream.Dispose();
		final_stream.Dispose();
	}

	private void WritePdbFile()
	{
		WritePdbHeap();
		WriteTableHeap();
		writer.BuildMetadataTextMap();
		writer.WriteMetadataHeader();
		writer.WriteMetadata();
		writer.Flush();
		ComputeChecksumAndPdbId();
		WritePdbId();
	}

	private void WritePdbHeap()
	{
		PdbHeapBuffer pdb_heap = pdb_metadata.pdb_heap;
		pdb_heap.WriteBytes(20);
		pdb_heap.WriteUInt32(module_metadata.entry_point.ToUInt32());
		MetadataTable[] tables = module_metadata.table_heap.tables;
		ulong num = 0uL;
		for (int i = 0; i < tables.Length; i++)
		{
			if (tables[i] != null && tables[i].Length != 0)
			{
				num |= (ulong)(1L << i);
			}
		}
		pdb_heap.WriteUInt64(num);
		for (int j = 0; j < tables.Length; j++)
		{
			if (tables[j] != null && tables[j].Length != 0)
			{
				pdb_heap.WriteUInt32((uint)tables[j].Length);
			}
		}
	}

	private void WriteTableHeap()
	{
		pdb_metadata.table_heap.string_offsets = pdb_metadata.string_heap.WriteStrings();
		pdb_metadata.table_heap.ComputeTableInformations();
		pdb_metadata.table_heap.WriteTableHeap();
	}

	private void ComputeChecksumAndPdbId()
	{
		byte[] buffer = new byte[8192];
		writer.BaseStream.Seek(0L, SeekOrigin.Begin);
		SHA256 sHA = SHA256.Create();
		using (CryptoStream dest_stream = new CryptoStream(Stream.Null, sHA, CryptoStreamMode.Write))
		{
			CryptoService.CopyStreamChunk(writer.BaseStream, dest_stream, buffer, (int)writer.BaseStream.Length);
		}
		pdb_checksum = sHA.Hash;
		ByteBuffer byteBuffer = new ByteBuffer(pdb_checksum);
		pdb_id_guid = new Guid(byteBuffer.ReadBytes(16));
		pdb_id_stamp = byteBuffer.ReadUInt32();
	}

	private void WritePdbId()
	{
		writer.MoveToRVA(TextSegment.PdbHeap);
		writer.WriteBytes(pdb_id_guid.ToByteArray());
		writer.WriteUInt32(pdb_id_stamp);
	}
}
