using System;
using System.IO;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil.PE;
using Mono.Collections.Generic;

namespace Mono.Cecil.Pdb;

internal class CustomMetadataWriter : IDisposable
{
	private readonly SymWriter sym_writer;

	private readonly MemoryStream stream;

	private readonly BinaryStreamWriter writer;

	private int count;

	private const byte version = 4;

	public CustomMetadataWriter(SymWriter sym_writer)
	{
		this.sym_writer = sym_writer;
		stream = new MemoryStream();
		writer = new BinaryStreamWriter(stream);
		writer.WriteByte(4);
		writer.WriteByte(0);
		writer.Align(4);
	}

	public void WriteUsingInfo(ImportDebugInformation import_info)
	{
		Write(CustomMetadataType.UsingInfo, delegate
		{
			writer.WriteUInt16(1);
			writer.WriteUInt16((ushort)import_info.Targets.Count);
		});
	}

	public void WriteForwardInfo(MetadataToken import_parent)
	{
		Write(CustomMetadataType.ForwardInfo, delegate
		{
			writer.WriteUInt32(import_parent.ToUInt32());
		});
	}

	public void WriteIteratorScopes(StateMachineScopeDebugInformation state_machine, MethodDebugInformation debug_info)
	{
		Write(CustomMetadataType.IteratorScopes, delegate
		{
			Collection<StateMachineScope> scopes = state_machine.Scopes;
			writer.WriteInt32(scopes.Count);
			foreach (StateMachineScope item in scopes)
			{
				int offset = item.Start.Offset;
				int num = (item.End.IsEndOfMethod ? debug_info.code_size : item.End.Offset);
				writer.WriteInt32(offset);
				writer.WriteInt32(num - 1);
			}
		});
	}

	public void WriteForwardIterator(TypeReference type)
	{
		Write(CustomMetadataType.ForwardIterator, delegate
		{
			writer.WriteBytes(Encoding.Unicode.GetBytes(type.Name));
		});
	}

	private void Write(CustomMetadataType type, Action write)
	{
		count++;
		writer.WriteByte(4);
		writer.WriteByte((byte)type);
		writer.Align(4);
		int position = writer.Position;
		writer.WriteUInt32(0u);
		write();
		writer.Align(4);
		int position2 = writer.Position;
		int value = position2 - position + 4;
		writer.Position = position;
		writer.WriteInt32(value);
		writer.Position = position2;
	}

	public void WriteCustomMetadata()
	{
		if (count != 0)
		{
			writer.BaseStream.Position = 1L;
			writer.WriteByte((byte)count);
			writer.Flush();
			sym_writer.DefineCustomMetadata("MD2", stream.ToArray());
		}
	}

	public void Dispose()
	{
		stream.Dispose();
	}
}
