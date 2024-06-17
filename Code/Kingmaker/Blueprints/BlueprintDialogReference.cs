using System;
using Kingmaker.DialogSystem.Blueprints;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintDialogReference : BlueprintReference<BlueprintDialog>, IMemoryPackable<BlueprintDialogReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintDialogReferenceFormatter : MemoryPackFormatter<BlueprintDialogReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintDialogReference value)
		{
			BlueprintDialogReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintDialogReference value)
		{
			BlueprintDialogReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintDialogReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintDialogReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintDialogReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintDialogReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintDialogReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintDialogReference? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteString(value.guid);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintDialogReference? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string text;
		if (memberCount == 1)
		{
			if (value != null)
			{
				text = value.guid;
				text = reader.ReadString();
				goto IL_0068;
			}
			text = reader.ReadString();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintDialogReference), 1, memberCount);
				return;
			}
			text = ((value != null) ? value.guid : null);
			if (memberCount != 0)
			{
				text = reader.ReadString();
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0068;
			}
		}
		value = new BlueprintDialogReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}
