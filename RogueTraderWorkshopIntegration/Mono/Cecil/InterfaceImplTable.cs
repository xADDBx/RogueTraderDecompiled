using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class InterfaceImplTable : MetadataTable<Row<uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteRID(rows[i].Col1, Table.TypeDef);
			buffer.WriteCodedRID(rows[i].Col2, CodedIndex.TypeDefOrRef);
		}
	}
}
