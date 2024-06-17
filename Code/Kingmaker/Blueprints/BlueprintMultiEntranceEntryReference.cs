using System;
using Kingmaker.Globalmap.Blueprints;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintMultiEntranceEntryReference : BlueprintReference<BlueprintMultiEntranceEntry>, IMemoryPackable<BlueprintMultiEntranceEntryReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintMultiEntranceEntryReferenceFormatter : MemoryPackFormatter<BlueprintMultiEntranceEntryReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintMultiEntranceEntryReference value)
		{
			BlueprintMultiEntranceEntryReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintMultiEntranceEntryReference value)
		{
			BlueprintMultiEntranceEntryReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintMultiEntranceEntryReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintMultiEntranceEntryReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintMultiEntranceEntryReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintMultiEntranceEntryReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintMultiEntranceEntryReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintMultiEntranceEntryReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintMultiEntranceEntryReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintMultiEntranceEntryReference), 1, memberCount);
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
		value = new BlueprintMultiEntranceEntryReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}
