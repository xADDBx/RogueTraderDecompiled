using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class EventTable : MetadataTable<Row<EventAttributes, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteUInt16((ushort)rows[i].Col1);
			buffer.WriteString(rows[i].Col2);
			buffer.WriteCodedRID(rows[i].Col3, CodedIndex.TypeDefOrRef);
		}
	}
}
