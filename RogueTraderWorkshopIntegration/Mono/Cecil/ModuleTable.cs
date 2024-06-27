using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class ModuleTable : OneRowTable<Row<uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		buffer.WriteUInt16(0);
		buffer.WriteString(row.Col1);
		buffer.WriteGuid(row.Col2);
		buffer.WriteUInt16(0);
		buffer.WriteUInt16(0);
	}
}
