using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class LocalConstantTable : MetadataTable<Row<uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteString(rows[i].Col1);
			buffer.WriteBlob(rows[i].Col2);
		}
	}
}
