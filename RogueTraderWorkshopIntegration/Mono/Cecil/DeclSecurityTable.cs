using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class DeclSecurityTable : SortedTable<Row<SecurityAction, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteUInt16((ushort)rows[i].Col1);
			buffer.WriteCodedRID(rows[i].Col2, CodedIndex.HasDeclSecurity);
			buffer.WriteBlob(rows[i].Col3);
		}
	}

	public override int Compare(Row<SecurityAction, uint, uint> x, Row<SecurityAction, uint, uint> y)
	{
		return SortedTable<Row<SecurityAction, uint, uint>>.Compare(x.Col2, y.Col2);
	}
}
