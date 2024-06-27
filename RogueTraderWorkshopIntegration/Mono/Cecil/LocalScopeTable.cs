using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class LocalScopeTable : MetadataTable<Row<uint, uint, uint, uint, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteRID(rows[i].Col1, Table.Method);
			buffer.WriteRID(rows[i].Col2, Table.ImportScope);
			buffer.WriteRID(rows[i].Col3, Table.LocalVariable);
			buffer.WriteRID(rows[i].Col4, Table.LocalConstant);
			buffer.WriteUInt32(rows[i].Col5);
			buffer.WriteUInt32(rows[i].Col6);
		}
	}
}
