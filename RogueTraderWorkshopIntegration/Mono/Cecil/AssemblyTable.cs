using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class AssemblyTable : OneRowTable<Row<AssemblyHashAlgorithm, ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		buffer.WriteUInt32((uint)row.Col1);
		buffer.WriteUInt16(row.Col2);
		buffer.WriteUInt16(row.Col3);
		buffer.WriteUInt16(row.Col4);
		buffer.WriteUInt16(row.Col5);
		buffer.WriteUInt32((uint)row.Col6);
		buffer.WriteBlob(row.Col7);
		buffer.WriteString(row.Col8);
		buffer.WriteString(row.Col9);
	}
}
