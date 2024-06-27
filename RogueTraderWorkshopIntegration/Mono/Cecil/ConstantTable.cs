using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class ConstantTable : SortedTable<Row<ElementType, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteUInt16((ushort)rows[i].Col1);
			buffer.WriteCodedRID(rows[i].Col2, CodedIndex.HasConstant);
			buffer.WriteBlob(rows[i].Col3);
		}
	}

	public override int Compare(Row<ElementType, uint, uint> x, Row<ElementType, uint, uint> y)
	{
		return SortedTable<Row<ElementType, uint, uint>>.Compare(x.Col2, y.Col2);
	}
}
