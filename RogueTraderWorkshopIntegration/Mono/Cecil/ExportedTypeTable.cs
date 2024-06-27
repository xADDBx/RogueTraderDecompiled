using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class ExportedTypeTable : MetadataTable<Row<TypeAttributes, uint, uint, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteUInt32((uint)rows[i].Col1);
			buffer.WriteUInt32(rows[i].Col2);
			buffer.WriteString(rows[i].Col3);
			buffer.WriteString(rows[i].Col4);
			buffer.WriteCodedRID(rows[i].Col5, CodedIndex.Implementation);
		}
	}
}
