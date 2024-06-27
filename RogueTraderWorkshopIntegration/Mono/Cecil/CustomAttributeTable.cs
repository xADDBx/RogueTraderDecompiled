using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class CustomAttributeTable : SortedTable<Row<uint, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteCodedRID(rows[i].Col1, CodedIndex.HasCustomAttribute);
			buffer.WriteCodedRID(rows[i].Col2, CodedIndex.CustomAttributeType);
			buffer.WriteBlob(rows[i].Col3);
		}
	}

	public override int Compare(Row<uint, uint, uint> x, Row<uint, uint, uint> y)
	{
		return SortedTable<Row<uint, uint, uint>>.Compare(x.Col1, y.Col1);
	}
}
