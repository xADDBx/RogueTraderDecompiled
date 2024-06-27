using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class GenericParamTable : MetadataTable<Row<ushort, GenericParameterAttributes, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteUInt16(rows[i].Col1);
			buffer.WriteUInt16((ushort)rows[i].Col2);
			buffer.WriteCodedRID(rows[i].Col3, CodedIndex.TypeOrMethodDef);
			buffer.WriteString(rows[i].Col4);
		}
	}
}
