using System;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints.Items.Augments;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintAugmentSlotReference : BlueprintReference<BlueprintAugmentSlot>, IMemoryPackable<BlueprintAugmentSlotReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintAugmentSlotReferenceFormatter : MemoryPackFormatter<BlueprintAugmentSlotReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintAugmentSlotReference value)
		{
			BlueprintAugmentSlotReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintAugmentSlotReference value)
		{
			BlueprintAugmentSlotReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintAugmentSlotReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintAugmentSlotReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintAugmentSlotReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintAugmentSlotReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintAugmentSlotReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintAugmentSlotReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintAugmentSlotReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintAugmentSlotReference), 1, memberCount);
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
		value = new BlueprintAugmentSlotReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}
