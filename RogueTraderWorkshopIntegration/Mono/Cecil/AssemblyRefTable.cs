using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class AssemblyRefTable : MetadataTable<Row<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteUInt16(rows[i].Col1);
			buffer.WriteUInt16(rows[i].Col2);
			buffer.WriteUInt16(rows[i].Col3);
			buffer.WriteUInt16(rows[i].Col4);
			buffer.WriteUInt32((uint)rows[i].Col5);
			buffer.WriteBlob(rows[i].Col6);
			buffer.WriteString(rows[i].Col7);
			buffer.WriteString(rows[i].Col8);
			buffer.WriteBlob(rows[i].Col9);
		}
	}
}
