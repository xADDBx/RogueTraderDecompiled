using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class DocumentTable : MetadataTable<Row<uint, uint, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteBlob(rows[i].Col1);
			buffer.WriteGuid(rows[i].Col2);
			buffer.WriteBlob(rows[i].Col3);
			buffer.WriteGuid(rows[i].Col4);
		}
	}
}
