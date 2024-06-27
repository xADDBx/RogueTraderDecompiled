using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class MethodSemanticsTable : SortedTable<Row<MethodSemanticsAttributes, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteUInt16((ushort)rows[i].Col1);
			buffer.WriteRID(rows[i].Col2, Table.Method);
			buffer.WriteCodedRID(rows[i].Col3, CodedIndex.HasSemantics);
		}
	}

	public override int Compare(Row<MethodSemanticsAttributes, uint, uint> x, Row<MethodSemanticsAttributes, uint, uint> y)
	{
		return SortedTable<Row<MethodSemanticsAttributes, uint, uint>>.Compare(x.Col3, y.Col3);
	}
}
