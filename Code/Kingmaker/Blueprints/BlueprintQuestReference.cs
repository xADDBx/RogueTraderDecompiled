using System;
using Kingmaker.Blueprints.Quests;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintQuestReference : BlueprintReference<BlueprintQuest>, IMemoryPackable<BlueprintQuestReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintQuestReferenceFormatter : MemoryPackFormatter<BlueprintQuestReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintQuestReference value)
		{
			BlueprintQuestReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintQuestReference value)
		{
			BlueprintQuestReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintQuestReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintQuestReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintQuestReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintQuestReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintQuestReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintQuestReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintQuestReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintQuestReference), 1, memberCount);
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
		value = new BlueprintQuestReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}
