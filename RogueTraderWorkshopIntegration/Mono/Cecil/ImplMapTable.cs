using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class ImplMapTable : SortedTable<Row<PInvokeAttributes, uint, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteUInt16((ushort)rows[i].Col1);
			buffer.WriteCodedRID(rows[i].Col2, CodedIndex.MemberForwarded);
			buffer.WriteString(rows[i].Col3);
			buffer.WriteRID(rows[i].Col4, Table.ModuleRef);
		}
	}

	public override int Compare(Row<PInvokeAttributes, uint, uint, uint> x, Row<PInvokeAttributes, uint, uint, uint> y)
	{
		return SortedTable<Row<PInvokeAttributes, uint, uint, uint>>.Compare(x.Col2, y.Col2);
	}
}
