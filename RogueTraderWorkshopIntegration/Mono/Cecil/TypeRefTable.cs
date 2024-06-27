using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class TypeRefTable : MetadataTable<Row<uint, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteCodedRID(rows[i].Col1, CodedIndex.ResolutionScope);
			buffer.WriteString(rows[i].Col2);
			buffer.WriteString(rows[i].Col3);
		}
	}
}
