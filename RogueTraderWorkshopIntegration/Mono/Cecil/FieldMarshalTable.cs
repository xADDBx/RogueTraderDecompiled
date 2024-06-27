using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class FieldMarshalTable : SortedTable<Row<uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteCodedRID(rows[i].Col1, CodedIndex.HasFieldMarshal);
			buffer.WriteBlob(rows[i].Col2);
		}
	}

	public override int Compare(Row<uint, uint> x, Row<uint, uint> y)
	{
		return SortedTable<Row<uint, uint>>.Compare(x.Col1, y.Col1);
	}
}
