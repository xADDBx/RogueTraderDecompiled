using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class TypeDefTable : MetadataTable<Row<TypeAttributes, uint, uint, uint, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteUInt32((uint)rows[i].Col1);
			buffer.WriteString(rows[i].Col2);
			buffer.WriteString(rows[i].Col3);
			buffer.WriteCodedRID(rows[i].Col4, CodedIndex.TypeDefOrRef);
			buffer.WriteRID(rows[i].Col5, Table.Field);
			buffer.WriteRID(rows[i].Col6, Table.Method);
		}
	}
}
