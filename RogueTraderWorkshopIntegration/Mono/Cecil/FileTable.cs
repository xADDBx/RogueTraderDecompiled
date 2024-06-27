using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class FileTable : MetadataTable<Row<FileAttributes, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteUInt32((uint)rows[i].Col1);
			buffer.WriteString(rows[i].Col2);
			buffer.WriteBlob(rows[i].Col3);
		}
	}
}
