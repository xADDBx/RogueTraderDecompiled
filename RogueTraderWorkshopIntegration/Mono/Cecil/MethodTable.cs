using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class MethodTable : MetadataTable<Row<uint, MethodImplAttributes, MethodAttributes, uint, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteUInt32(rows[i].Col1);
			buffer.WriteUInt16((ushort)rows[i].Col2);
			buffer.WriteUInt16((ushort)rows[i].Col3);
			buffer.WriteString(rows[i].Col4);
			buffer.WriteBlob(rows[i].Col5);
			buffer.WriteRID(rows[i].Col6, Table.Param);
		}
	}
}
