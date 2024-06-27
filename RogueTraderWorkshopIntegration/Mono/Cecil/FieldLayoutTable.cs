using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class FieldLayoutTable : SortedTable<Row<uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteUInt32(rows[i].Col1);
			buffer.WriteRID(rows[i].Col2, Table.Field);
		}
	}

	public override int Compare(Row<uint, uint> x, Row<uint, uint> y)
	{
		return SortedTable<Row<uint, uint>>.Compare(x.Col2, y.Col2);
	}
}
